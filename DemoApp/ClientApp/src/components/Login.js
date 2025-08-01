import React, { Component } from 'react';
import { Button, Container, Row, Col, Card, CardBody, CardTitle } from 'reactstrap';
import OAuthService from '../services/OAuthService';

export class Login extends Component {
  static displayName = Login.name;

  constructor(props) {
    super(props);
    this.state = {
      loading: false,
      error: null
    };
  }

  handleLogin = async () => {
    this.setState({ loading: true, error: null });
    
    try {
      await OAuthService.startLogin();
    } catch (error) {
      this.setState({ error: error.message, loading: false });
    }
  }

  render() {
    const { loading, error } = this.state;

    return (
      <Container className="mt-5">
        <Row className="justify-content-center">
          <Col md={6}>
            <Card>
              <CardBody className="text-center">
                <CardTitle tag="h2">OAuth2 Demo Login</CardTitle>
                <p className="mb-4">
                  Click the button below to start the OAuth2 authorization flow.
                  You will be redirected to the OAuth2 server for authentication.
                </p>
                
                {error && (
                  <div className="alert alert-danger">
                    {error}
                  </div>
                )}
                
                <Button 
                  color="primary" 
                  size="lg" 
                  onClick={this.handleLogin}
                  disabled={loading}
                >
                  {loading ? 'Redirecting...' : 'Login with OAuth2'}
                </Button>
                
                <div className="mt-4">
                  <h5>Test Accounts:</h5>
                  <div className="text-left">
                    <p><strong>User with 2FA:</strong><br />
                    Username: john.doe<br />
                    Password: password123</p>
                    
                    <p><strong>User without 2FA:</strong><br />
                    Username: jane.smith<br />
                    Password: password456</p>
                    
                    <p><strong>Admin User:</strong><br />
                    Username: admin<br />
                    Password: admin123</p>
                  </div>
                </div>
              </CardBody>
            </Card>
          </Col>
        </Row>
      </Container>
    );
  }
}
