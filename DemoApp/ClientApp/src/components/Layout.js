import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';

export class Layout extends Component {
  static displayName = Layout.name;

  render() {
    return (
      <div>
        <NavMenu 
          isLoggedIn={this.props.isLoggedIn} 
          updateLoginState={this.props.updateLoginState} 
        />
        <Container>
          {this.props.children}
        </Container>
      </div>
    );
  }
}
