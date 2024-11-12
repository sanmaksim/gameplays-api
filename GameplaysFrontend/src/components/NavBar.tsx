import { clearCredentials } from '../slices/authSlice';
import { Container, Form } from 'react-bootstrap';
import { Controller } from 'react-bootstrap-icons';
import { Link, useNavigate } from 'react-router-dom';
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

    // let isActive = false;
    // const handleSelect = () => isActive ? isActive = false : isActive = true;

    const logoutHandler = async () => {
        try {
            const response = await logout('').unwrap();
            dispatch(clearCredentials());
            navigate('/');
            toast.success(response.message);
        } catch (error: any) {
            toast.error(error.data.message);
        }
    }

    return (
        <Navbar expand="lg" bg="dark" data-bs-theme="dark">
            <Container>
                <Nav>
                    <Navbar.Brand>
                        <Link className="text-light fw-bold text-decoration-none" to="/">
                            <Controller size={30} />
                            {/* <span className="ms-1">Gameplays</span> */}
                        </Link>
                    </Navbar.Brand>
                </Nav>
                <Nav className="mx-auto" style={{ width: '50%' }}>
                    <NavDropdown title="Games" id="user-dropdown" menuVariant='dark'>
                        <Link className="dropdown-item" to="/games">All Games</Link>
                        <Link className="dropdown-item" to="/games/popular">Popular Games</Link>
                        <Link className="dropdown-item" to="/games/top">Top 250 Games</Link>
                        <Link className="dropdown-item" to="/games/bottom">Bottom 100 Games</Link>
                    </NavDropdown>
                    <Form style={{ width: '100%' }}>
                        <Form.Control
                            type="search"
                            placeholder="Search Games"
                            aria-label="Search"
                        />
                    </Form>
                </Nav>
                <Nav>
                    {userInfo ? (
                        <NavDropdown title={userInfo.username} id="user-dropdown">
                            <Link className="dropdown-item" to={`/user/${userInfo.username}`}>Profile</Link>
                            <Link className="dropdown-item" to={`/user/${userInfo.username}/games`}>Games</Link>
                            <Link className="dropdown-item" to="/help">Help</Link>
                            <Nav.Item className="dropdown-item" onClick={logoutHandler}>Logout</Nav.Item>
                        </NavDropdown>
                    ) : isLoginPageContext ? (
                        <Link to="/register">
                            <button
                                type="button"
                                className="btn btn-primary btn-med px-2">Sign up</button>
                        </Link>
                    ) : (
                        <Link to="/login">
                            <button
                                type="button"
                                className="btn btn-secondary btn-med px-2">Sign in</button>
                        </Link>
                    )}
                </Nav>
            </Container>
        </Navbar>
    );
}

export default NavBar;
