import { clearCredentials } from '../slices/authSlice';
import { Container, Dropdown, NavbarCollapse } from 'react-bootstrap';
import { faGamepad } from '@fortawesome/free-solid-svg-icons/faGamepad';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Link, useNavigate } from 'react-router-dom';
import { PageContext } from '../contexts/PageContext';
import { RootState } from '../store';
import { toast } from 'react-toastify';
import { useContext, useEffect, useState } from 'react';
import { useLogoutMutation } from '../slices/usersApiSlice';
import { useSelector, useDispatch } from 'react-redux';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import NavDropdown from 'react-bootstrap/NavDropdown';
import ProfileIcon from './../assets/profile_icon.jpg';
import SearchBar from './SearchBar';

function NavBar() {
    // setup page context
    const { isLoginPageContext, isRegisterPageContext } = useContext(PageContext);

    // setup redux
    const { userInfo } = useSelector((state: RootState) => state.auth);
    const [logout] = useLogoutMutation();
    const dispatch = useDispatch();
    
    const navigate = useNavigate();

    // toggle navbar expanded at mobile screen size threshold
    const [expanded, setExpanded] = useState(false);
    const [isBelowMobileThreshold, setIsBelowMobileThreshold] = useState(false);
    const threshold = 768; // 768px for mobile views
    const handleToggle = () => setExpanded((prev) => !prev);
    useEffect(() => {
        // set initial state based on current window size
        const handleResize = () => {
            if (window.innerWidth < threshold) {
                setIsBelowMobileThreshold(true);
                setExpanded(false); // close navbar on mobile resize
            } else {
                setIsBelowMobileThreshold(false);
            }
        };
    
        // add resize event listener
        window.addEventListener('resize', handleResize);
        handleResize();
    
        // cleanup event listener on component unmount
        return () => window.removeEventListener('resize', handleResize);
      }, []);


    const handleLogout = async () => {
        try {
            const response = await logout('').unwrap();
            dispatch(clearCredentials());
            navigate('/');
            toast.success(response.message);
        } catch (error: any) {
            toast.error(error.data.message);
        }
    };

    return (
        <Navbar bg="dark" data-bs-theme="dark" collapseOnSelect expand="lg" expanded={expanded} onToggle={handleToggle}>
            <Container>

                <Navbar.Brand>
                    <Link className="text-light fw-bold text-decoration-none" to="/">
                        <FontAwesomeIcon icon={faGamepad} />
                        <span className="d-none d-md-inline ms-1">Gameplays</span>
                    </Link>
                </Navbar.Brand>
                
                {/* do not show the search bar on login and register pages */}
                {!isLoginPageContext && !isRegisterPageContext ? (
                    <SearchBar />
                ) : null }

                {userInfo ? (
                    <>
                        {/* only show nav toggle on mobile view */}
                        {isBelowMobileThreshold ? (
                            <Navbar.Toggle id="navbar-nav">
                                <img src={ProfileIcon} className="rounded-circle" alt="Profile Icon" width='40' height='40' />
                            </Navbar.Toggle>
                        ) : (
                            <NavDropdown title={<img src={ProfileIcon} className="rounded-circle" alt="Profile Icon" width='40' height='40' />} align='end' style={{ color: 'white' }}>
                                <Link className="dropdown-item" to="/user/profile">Profile</Link>
                                <Link className="dropdown-item" to="/user/games">Games</Link>
                                <Link className="dropdown-item" to="/help">Help</Link>
                                <NavDropdown.Item onClick={handleLogout}>Logout</NavDropdown.Item>
                            </NavDropdown>
                        )}
                    </>
                ) : isLoginPageContext ? (
                    <Nav.Link href="#">
                        <Link to="/register">
                            <button
                                type="button"
                                className="btn btn-primary btn-med px-2">
                                    Sign up
                            </button>
                        </Link>
                    </Nav.Link>
                ) : (
                    <Nav.Link href="#">
                        <Link to="/login">
                            <button
                                type="button"
                                className="btn btn-secondary btn-med px-2">
                                    Sign in
                            </button>
                        </Link>
                    </Nav.Link>
                )}

                {isBelowMobileThreshold ? (
                    <NavbarCollapse id="navbar-nav">
                        <Dropdown.Item><Link className="dropdown-item text-light" to="/user/profile">Profile</Link></Dropdown.Item>
                        <Dropdown.Item><Link className="dropdown-item text-light" to="/user/games">Games</Link></Dropdown.Item>
                        <Dropdown.Item><Link className="dropdown-item text-light" to="/help">Help</Link></Dropdown.Item>
                        <Dropdown.Item className="dropdown-item text-light" onClick={handleLogout}>Logout</Dropdown.Item>
                    </NavbarCollapse>
                ) : null }

            </Container>
        </Navbar>
    );
}

export default NavBar;
