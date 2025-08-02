import React, { Component } from 'react';
import { Route, Switch } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Login } from './components/Login';
import { Callback } from './components/Callback';
import { Profile } from './components/Profile';
import { ApiTest } from './components/ApiTest';
import OAuthService from './services/OAuthService';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  constructor(props) {
    super(props);
    this.state = {
      isLoggedIn: OAuthService.isLoggedIn()
    };
  }

  componentDidMount() {
    // Listen for storage changes to update login state
    window.addEventListener('storage', this.handleStorageChange);
  }

  componentWillUnmount() {
    window.removeEventListener('storage', this.handleStorageChange);
  }

  handleStorageChange = () => {
    this.setState({ isLoggedIn: OAuthService.isLoggedIn() });
  }

  updateLoginState = () => {
    this.setState({ isLoggedIn: OAuthService.isLoggedIn() });
  }

  render() {
    return (
      <Layout isLoggedIn={this.state.isLoggedIn} updateLoginState={this.updateLoginState}>
        <Switch>
          <Route exact path='/' component={Home} />
          <Route path='/login' component={Login} />
          <Route path='/callback' render={(props) => <Callback {...props} updateLoginState={this.updateLoginState} />} />
          <Route path='/profile' component={Profile} />
          <Route path='/api-test' component={ApiTest} />
        </Switch>
      </Layout>
    );
  }
}
