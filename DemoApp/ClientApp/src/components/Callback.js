import React, { Component } from 'react';
import { Container, Row, Col, Card, CardBody, Spinner } from 'reactstrap';
import OAuthService from '../services/OAuthService';

export class Callback extends Component {
  static displayName = Callback.name;

  constructor(props) {
    super(props);
    this.state = {
      loading: true,
      error: null,
      success: false
    };
  }

  async componentDidMount() {
    try {
      await OAuthService.handleCallback();
      this.setState({ loading: false, success: true });
      this.props.updateLoginState();
      
      // Redirect to home after successful login
      setTimeout(() => {
        this.props.history.push('/');
      }, 2000);
    } catch (error) {
      this.setState({ loading: false, error: error.message });
    }
  }

  render() {
    const { loading, error, success } = this.state;

    return (
      <Container className="mt-5">
        <Row className="justify-content-center">
          <Col md={6}>
            <Card>
              <CardBody className="text-center">
                {loading && (
                  <>
                    <Spinner color="primary" />
                    <p className="mt-3">Processing authentication...</p>
                  </>
                )}
                
                {error && (
                  <>
                    <div className="alert alert-danger">
                      <h4>Authentication Failed</h4>
                      <p>{error}</p>
                    </div>
                    <a href="/login" className="btn btn-primary">Try Again</a>
                  </>
                )}
                
                {success && (
                  <>
                    <div className="alert alert-success">
                      <h4>Authentication Successful!</h4>
                      <p>You have been successfully logged in. Redirecting to home page...</p>
                    </div>
                  </>
                )}
              </CardBody>
            </Card>
          </Col>
        </Row>
      </Container>
    );
  }
}
