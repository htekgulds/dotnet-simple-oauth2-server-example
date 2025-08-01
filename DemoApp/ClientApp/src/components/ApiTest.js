import React, { Component } from 'react';
import { Container, Row, Col, Card, CardBody, CardTitle, Button, Alert } from 'reactstrap';
import OAuthService from '../services/OAuthService';

export class ApiTest extends Component {
  static displayName = ApiTest.name;

  constructor(props) {
    super(props);
    this.state = {
      results: {},
      loading: {},
      errors: {}
    };
  }

  testApi = async (endpoint, name) => {
    this.setState(prevState => ({
      loading: { ...prevState.loading, [name]: true },
      errors: { ...prevState.errors, [name]: null }
    }));

    try {
      const response = await OAuthService.apiCall(endpoint);
      const data = await response.json();
      
      this.setState(prevState => ({
        results: { ...prevState.results, [name]: data },
        loading: { ...prevState.loading, [name]: false }
      }));
    } catch (error) {
      this.setState(prevState => ({
        errors: { ...prevState.errors, [name]: error.message },
        loading: { ...prevState.loading, [name]: false }
      }));
    }
  }

  testPublicApi = () => {
    this.testApi('/api/public', 'public');
  }

  testProtectedApi = () => {
    this.testApi('/api/protected', 'protected');
  }

  testAdminApi = () => {
    this.testApi('/api/admin', 'admin');
  }

  renderApiResult = (name, title) => {
    const { results, loading, errors } = this.state;
    const result = results[name];
    const isLoading = loading[name];
    const error = errors[name];

    return (
      <Card className="mb-3">
        <CardBody>
          <CardTitle tag="h5">{title}</CardTitle>
          
          {isLoading && <div>Loading...</div>}
          
          {error && (
            <Alert color="danger">
              <strong>Error:</strong> {error}
            </Alert>
          )}
          
          {result && (
            <Alert color="success">
              <strong>Success:</strong>
              <pre className="mt-2 mb-0">
                {JSON.stringify(result, null, 2)}
              </pre>
            </Alert>
          )}
        </CardBody>
      </Card>
    );
  }

  render() {
    const isLoggedIn = OAuthService.isLoggedIn();

    return (
      <Container className="mt-5">
        <Row>
          <Col md={12}>
            <h2>API Testing</h2>
            <p>Test different API endpoints to see how OAuth2 authentication works.</p>
            
            <div className="mb-4">
              <Button 
                color="primary" 
                className="mr-2 mb-2" 
                onClick={this.testPublicApi}
                disabled={this.state.loading.public}
              >
                Test Public API
              </Button>
              
              <Button 
                color="success" 
                className="mr-2 mb-2" 
                onClick={this.testProtectedApi}
                disabled={!isLoggedIn || this.state.loading.protected}
              >
                Test Protected API
              </Button>
              
              <Button 
                color="warning" 
                className="mr-2 mb-2" 
                onClick={this.testAdminApi}
                disabled={!isLoggedIn || this.state.loading.admin}
              >
                Test Admin API
              </Button>
            </div>

            {!isLoggedIn && (
              <Alert color="info">
                <strong>Note:</strong> You need to be logged in to test protected and admin APIs.
                <a href="/login" className="btn btn-primary btn-sm ml-2">Login</a>
              </Alert>
            )}

            {this.renderApiResult('public', 'Public API (No Authentication Required)')}
            {this.renderApiResult('protected', 'Protected API (Authentication Required)')}
            {this.renderApiResult('admin', 'Admin API (Admin Scope Required)')}
          </Col>
        </Row>
      </Container>
    );
  }
}
