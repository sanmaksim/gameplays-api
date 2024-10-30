import { AuthContext } from '../contexts/AuthContext';
import { Container } from 'react-bootstrap';
import { Controller } from 'react-bootstrap-icons';
import { Link } from 'react-router-dom';
import { LinkContainer } from 'react-router-bootstrap';
import { PageContext } from '../contexts/PageContext';
import { useContext } from 'react';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import NavDropdown from 'react-bootstrap/NavDropdown';

function NavBar() {
    const { isLoggedInContext, setIsLoggedOutContext } = useContext(AuthContext);
    const { isLoginPageContext, isRegisterPageContext } = useContext(PageContext);

    let isActive = false;
    const handleSelect = () => isActive ? isActive = false : isActive = true;

    return (
        <Navbar expand="lg" bg="dark" data-bs-theme="dark">
            <Container>
                <Navbar.Brand>
                    <Link to="/">
                        <Controller className="text-light fw-bold" size={30} />
                    </Link>
                </Navbar.Brand>
                <Navbar.Toggle aria-controls="basic-navbar-nav" />
                <Navbar.Collapse id="basic-navbar-nav">
                    <Nav className="ms-auto">
                        {isLoggedInContext ? (
                            <>
                                <NavDropdown title="User" id="basic-nav-dropdown">
                                    <Link className="dropdown-item" to="/user/profile">Profile</Link>
                                    <Link className="dropdown-item" to="/user/settings">Settings</Link>
                                    <Link className="dropdown-item" to="/help">Help</Link>
                                    <Link className="dropdown-item" to="/user/logout" onClick={setIsLoggedOutContext}>Logout</Link>
                                </NavDropdown>

                                <Nav onSelect={handleSelect}>
                                    <LinkContainer to="/user/games">
                                        <Nav.Link active={isActive} >My Games</Nav.Link>
                                    </LinkContainer>
                                </Nav>
                            </>
                        ) : (
                            <></>
                        )}
                        {/* <NavDropdown title="Games" id="basic-nav-dropdown" menuVariant='dark'>
                            <Link className="dropdown-item" to="/games/all">All Games</Link>
                            <Link className="dropdown-item" to="/games/popular">Popular Games</Link>
                            <Link className="dropdown-item" to="/games/top">Top 100 Games</Link>
                            <NavDropdown.Divider />
                            <Link className="dropdown-item" to="/games/recommended">Recommended Games</Link>
                        </NavDropdown> */}
                        {!isLoggedInContext && isRegisterPageContext ? (
                            <Link to="/user/login">
                                <button
                                    type="button"
                                    className="btn btn-secondary btn-med px-2">Sign in</button>
                            </Link>
                        ) : !isLoggedInContext && isLoginPageContext ? (
                            <Link to="/user/register">
                                <button
                                    type="button"
                                    className="btn btn-primary btn-med px-2">Sign up</button>
                            </Link>
                        ) : !isLoggedInContext ? (
                            <Link to="/user/login">
                                <button
                                    type="button"
                                    className="btn btn-secondary btn-med px-2">Sign in</button>
                            </Link>
                        ) : (
                            <></>
                        )}
                    </Nav>
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
}

export default NavBar;
