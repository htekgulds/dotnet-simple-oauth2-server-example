class OAuthService {
    constructor() {
        this.clientId = 'demo-web-app';
        this.clientSecret = 'demo-web-app-secret';
        this.redirectUri = window.location.origin + '/callback';
        this.scope = 'openid profile email api';
        this.authServerUrl = 'https://localhost:7000';
        this.apiUrl = window.location.origin;
    }

    // Generate PKCE challenge
    generateCodeChallenge() {
        const codeVerifier = this.generateRandomString(128);
        sessionStorage.setItem('code_verifier', codeVerifier);
        
        return crypto.subtle.digest('SHA-256', new TextEncoder().encode(codeVerifier))
            .then(hash => {
                const base64 = btoa(String.fromCharCode(...new Uint8Array(hash)));
                return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
            });
    }

    generateRandomString(length) {
        const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
        let result = '';
        for (let i = 0; i < length; i++) {
            result += chars.charAt(Math.floor(Math.random() * chars.length));
        }
        return result;
    }

    // Start OAuth flow
    async startLogin() {
        const state = this.generateRandomString(32);
        const codeChallenge = await this.generateCodeChallenge();
        
        sessionStorage.setItem('oauth_state', state);

        const params = new URLSearchParams({
            response_type: 'code',
            client_id: this.clientId,
            redirect_uri: this.redirectUri,
            scope: this.scope,
            state: state,
            code_challenge: codeChallenge,
            code_challenge_method: 'S256'
        });

        // First get the authorize endpoint
        const authorizeResponse = await fetch(`${this.authServerUrl}/oauth2/authorize?${params}`);
        const authorizeData = await authorizeResponse.json();
        
        if (authorizeData.login_url) {
            // Redirect to login page
            window.location.href = this.authServerUrl + authorizeData.login_url;
        } else {
            throw new Error('Failed to get login URL');
        }
    }

    // Handle callback
    async handleCallback() {
        const urlParams = new URLSearchParams(window.location.search);
        const code = urlParams.get('code');
        const state = urlParams.get('state');
        const error = urlParams.get('error');

        if (error) {
            throw new Error(`OAuth error: ${error}`);
        }

        if (!code) {
            throw new Error('No authorization code received');
        }

        const storedState = sessionStorage.getItem('oauth_state');
        if (state !== storedState) {
            throw new Error('Invalid state parameter');
        }

        // Exchange code for tokens
        const codeVerifier = sessionStorage.getItem('code_verifier');
        const tokenData = await this.exchangeCodeForTokens(code, codeVerifier);
        
        // Store tokens
        localStorage.setItem('access_token', tokenData.access_token);
        localStorage.setItem('refresh_token', tokenData.refresh_token);
        localStorage.setItem('token_expires_at', Date.now() + (tokenData.expires_in * 1000));

        // Clean up
        sessionStorage.removeItem('oauth_state');
        sessionStorage.removeItem('code_verifier');

        return tokenData;
    }

    async exchangeCodeForTokens(code, codeVerifier) {
        const formData = new FormData();
        formData.append('grant_type', 'authorization_code');
        formData.append('code', code);
        formData.append('redirect_uri', this.redirectUri);
        formData.append('client_id', this.clientId);
        formData.append('client_secret', this.clientSecret);
        formData.append('code_verifier', codeVerifier);

        const response = await fetch(`${this.authServerUrl}/oauth2/token`, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(`Token exchange failed: ${error.error_description || error.error}`);
        }

        return await response.json();
    }

    // Get access token
    getAccessToken() {
        const token = localStorage.getItem('access_token');
        const expiresAt = localStorage.getItem('token_expires_at');
        
        if (!token || !expiresAt) {
            return null;
        }

        if (Date.now() >= parseInt(expiresAt)) {
            this.logout();
            return null;
        }

        return token;
    }

    // Check if user is logged in
    isLoggedIn() {
        return this.getAccessToken() !== null;
    }

    // Logout
    logout() {
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        localStorage.removeItem('token_expires_at');
    }

    // Make authenticated API call
    async apiCall(endpoint, options = {}) {
        const token = this.getAccessToken();
        
        if (!token) {
            throw new Error('No access token available');
        }

        const defaultOptions = {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        };

        const finalOptions = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers
            }
        };

        const response = await fetch(`${this.apiUrl}${endpoint}`, finalOptions);
        
        if (response.status === 401) {
            this.logout();
            throw new Error('Unauthorized - please login again');
        }

        return response;
    }
}

export default new OAuthService();
