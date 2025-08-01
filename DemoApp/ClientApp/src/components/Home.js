import React, { Component } from 'react';
import { Button, Card, CardBody, CardTitle, Row, Col } from 'reactstrap';
import { Link } from 'react-router-dom';
import OAuthService from '../services/OAuthService';

export class Home extends Component {
  static displayName = Home.name;

  render() {
    const isLoggedIn = OAuthService.isLoggedIn();

    return (
      <div>
        <h1>OAuth2 Demo Application</h1>
        <p className="lead">Welcome to the OAuth2 demonstration application built with:</p>
        
        <Row className="mb-4">
          <Col md={4}>
            <Card>
              <CardBody>
                <CardTitle tag="h5">🔐 OAuth2 Server</CardTitle>
                <p>Complete OAuth2 authorization server with JWT tokens, PKCE, and 2FA support.</p>
              </CardBody>
            </Card>
          </Col>
          <Col md={4}>
            <Card>
              <CardBody>
                <CardTitle tag="h5">⚛️ React Client</CardTitle>
                <p>Modern React application with OAuth2 integration and protected routes.</p>
              </CardBody>
            </Card>
          </Col>
          <Col md={4}>
            <Card>
              <CardBody>
                <CardTitle tag="h5">🔧 Mock Services</CardTitle>
                <p>User service and SMS service for testing authentication flows.</p>
              </CardBody>
            </Card>
          </Col>
        </Row>

        <div className="mb-4">
          <h3>Authentication Status</h3>
          {isLoggedIn ? (
            <div className="alert alert-success">
              <h4>✅ You are logged in!</h4>
              <p>You can now access protected resources and test the API endpoints.</p>
              <Link to="/profile" className="btn btn-success mr-2">View Profile</Link>
              <Link to="/api-test" className="btn btn-outline-success">Test APIs</Link>
            </div>
          ) : (
            <div className="alert alert-info">
              <h4>🔑 You are not logged in</h4>
              <p>Click the button below to start the OAuth2 authentication flow.</p>
              <Link to="/login" className="btn btn-primary">Login with OAuth2</Link>
            </div>
          )}
        </div>

        <div className="mb-4">
          <h3>Features Demonstrated</h3>
          <ul className="list-group">
            <li className="list-group-item">✅ OAuth2 Authorization Code Flow with PKCE</li>
            <li className="list-group-item">✅ OAuth2 Client Credentials Flow</li>
            <li className="list-group-item">✅ JWT Access and Refresh Tokens</li>
            <li className="list-group-item">✅ Two-Factor Authentication (2FA) with SMS</li>
            <li className="list-group-item">✅ Protected API Endpoints</li>
            <li className="list-group-item">✅ React OAuth2 Client Integration</li>
            <li className="list-group-item">✅ Multiple OAuth2 Clients Configuration</li>
          </ul>
        </div>

        <div className="mb-4">
          <h3>Test Accounts</h3>
          <div className="row">
            <div className="col-md-4">
              <div className="card">
                <div className="card-body">
                  <h5 className="card-title">User with 2FA</h5>
                  <p><strong>Username:</strong> john.doe</p>
                  <p><strong>Password:</strong> password123</p>
                  <small className="text-muted">This user has 2FA enabled</small>
                </div>
              </div>
            </div>
            <div className="col-md-4">
              <div className="card">
                <div className="card-body">
                  <h5 className="card-title">Regular User</h5>
                  <p><strong>Username:</strong> jane.smith</p>
                  <p><strong>Password:</strong> password456</p>
                  <small className="text-muted">This user has 2FA disabled</small>
                </div>
              </div>
            </div>
            <div className="col-md-4">
              <div className="card">
                <div className="card-body">
                  <h5 className="card-title">Admin User</h5>
                  <p><strong>Username:</strong> admin</p>
                  <p><strong>Password:</strong> admin123</p>
                  <small className="text-muted">This user has admin privileges</small>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="mb-4">
          <h3>Quick Actions</h3>
          <Link to="/api-test" className="btn btn-outline-primary mr-2">Test API Endpoints</Link>
          {!isLoggedIn && <Link to="/login" className="btn btn-primary">Start OAuth2 Flow</Link>}
        </div>
      </div>
    );
  }
}
