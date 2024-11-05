import { clearCredentials } from '../slices/authSlice';
import { Container } from 'react-bootstrap';
import { Controller } from 'react-bootstrap-icons';
import { Link, useNavigate } from 'react-router-dom';
import { LinkContainer } from 'react-router-bootstrap';
import { PageContext } from '../contexts/PageContext';
import { RootState } from '../store';
import { useContext } from 'react';
import { toast } from 'react-toastify';
import { useLogoutMutation } from '../slices/usersApiSlice';
import { useSelector, useDispatch } from 'react-redux';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import NavDropdown from 'react-bootstrap/NavDropdown';

function NavBar() {
    const { isLoginPageContext } = useContext(PageContext);

    const navigate = useNavigate();
    const dispatch = useDispatch();

    const [logout] = useLogoutMutation();

    const { userInfo } = useSelector((state: RootState) => state.auth);

    let isActive = false;
    const handleSelect = () => isActive ? isActive = false : isActive = true;

    const logoutHandler = async () => {
        try {
            await logout('').unwrap();
            dispatch(clearCredentials());
            navigate('/');
        } catch (error: any) {
            toast.error(error.data);
        }
    }

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
                        {userInfo ? (
                            <>
                                <NavDropdown title={userInfo.username} id="basic-nav-dropdown">
                                    <Link className="dropdown-item" to="/user/profile">Profile</Link>
                                    <Link className="dropdown-item" to="/user/settings">Settings</Link>
                                    <Link className="dropdown-item" to="/help">Help</Link>
                                    <Link className="dropdown-item" to="/user/logout" onClick={logoutHandler}>Logout</Link>
                                </NavDropdown>

                                <Nav onSelect={handleSelect}>
                                    <LinkContainer to="/user/games">
                                        <Nav.Link active={isActive} >My Games</Nav.Link>
                                    </LinkContainer>
                                </Nav>
                            </>
                        ) : isLoginPageContext ? (
                            <Link to="/user/register">
                                <button
                                    type="button"
                                    className="btn btn-primary btn-med px-2">Sign up</button>
                            </Link>
                        ) : (
                            <Link to="/user/login">
                                <button
                                    type="button"
                                    className="btn btn-secondary btn-med px-2">Sign in</button>
                            </Link>
                        )}

                        {/* <NavDropdown title="Games" id="basic-nav-dropdown" menuVariant='dark'>
                            <Link className="dropdown-item" to="/games/all">All Games</Link>
                            <Link className="dropdown-item" to="/games/popular">Popular Games</Link>
                            <Link className="dropdown-item" to="/games/top">Top 100 Games</Link>
                            <NavDropdown.Divider />
                            <Link className="dropdown-item" to="/games/recommended">Recommended Games</Link>
                        </NavDropdown> */}

                    </Nav>
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
}

export default NavBar;
