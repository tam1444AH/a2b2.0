import React, { useContext } from 'react'
import Container from 'react-bootstrap/Container'
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import { IoAirplaneSharp } from "react-icons/io5";
import { AuthContext } from '../context/AuthProvider';
import { Button } from 'react-bootstrap';


export const NavigationBar = () => {
  const { isLoggedIn, logOut } = useContext(AuthContext);
  
  return (
    <>
      <Navbar collapseOnSelect expand="lg" bg="dark" data-bs-theme="dark">
        <Container>
          <Navbar.Brand href="/">A<IoAirplaneSharp />B</Navbar.Brand>
          <Navbar.Toggle aria-controls="responsive-navbar-nav" />
          <Navbar.Collapse id="responsive-navbar-nav">
            <Nav className="me-auto">
              {isLoggedIn ? (
                <>
                  <Nav.Link href='/hotels'>Hotels</Nav.Link>
                  <Nav.Link href='/profile'>Profile</Nav.Link>
                </>
              ) : (
                <>
                  <Nav.Link href='/login'>Login</Nav.Link>
                  <Nav.Link href='/signup'>Sign Up</Nav.Link>
                </>
              )}
            </Nav>
            {isLoggedIn && (
              <Button variant='outline-light' onClick={logOut}>
                Log Out
              </Button>
            )}
          </Navbar.Collapse>
        </Container>
      </Navbar>
    </>
  );
};
