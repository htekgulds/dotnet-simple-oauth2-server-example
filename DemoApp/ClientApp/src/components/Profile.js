import React, { Component } from 'react';
import { Container, Row, Col, Card, CardBody, CardTitle, Button } from 'reactstrap';
import OAuthService from '../services/OAuthService';

export class Profile extends Component {
  static displayName = Profile.name;

  constructor(props) {
    super(props);
    this.state = {
      user: null,
      loading: true,
      error: null
    };
  }

  async componentDidMount() {
    if (!OAuthService.isLoggedIn()) {
      this.props.history.push('/login');
      return;
    }

    try {
      const response = await OAuthService.apiCall('/api/protected');
      const data = await response.json();
      this.setState({ user: data.user, loading: false });
    } catch (error) {
      this.setState({ error: error.message, loading: false });
    }
  }

  handleLogout = () => {
    OAuthService.logout();
    this.props.history.push('/');
  }

  render() {
    const { user, loading, error } = this.state;

    if (loading) {
      return (
        <Container className="mt-5">
          <div className="text-center">Loading profile...</div>
        </Container>
      );
    }

    if (error) {
      return (
        <Container className="mt-5">
          <div className="alert alert-danger">
            Error loading profile: {error}
          </div>
        </Container>
      );
    }

    return (
      <Container className="mt-5">
        <Row className="justify-content-center">
          <Col md={8}>
            <Card>
              <CardBody>
                <CardTitle tag="h2">User Profile</CardTitle>
                
                {user && (
                  <div>
                    <div className="row mb-3">
                      <div className="col-sm-3"><strong>User ID:</strong></div>
                      <div className="col-sm-9">{user.id}</div>
                    </div>
                    
                    <div className="row mb-3">
                      <div className="col-sm-3"><strong>Username:</strong></div>
                      <div className="col-sm-9">{user.username}</div>
                    </div>
                    
                    <div className="row mb-3">
                      <div className="col-sm-3"><strong>Email:</strong></div>
                      <div className="col-sm-9">{user.email}</div>
                    </div>
                    
                    <div className="row mb-3">
                      <div className="col-sm-3"><strong>Scopes:</strong></div>
                      <div className="col-sm-9">
                        {user.scopes && user.scopes.length > 0 ? (
                          <ul className="list-unstyled">
                            {user.scopes.map((scope, index) => (
                              <li key={index}>
                                <span className="badge badge-primary mr-1">{scope}</span>
                              </li>
                            ))}
                          </ul>
                        ) : (
                          <span className="text-muted">No scopes</span>
                        )}
                      </div>
                    </div>
                    
                    <div className="mt-4">
                      <Button color="danger" onClick={this.handleLogout}>
                        Logout
                      </Button>
                    </div>
                  </div>
                )}
              </CardBody>
            </Card>
          </Col>
        </Row>
      </Container>
    );
  }
}"
