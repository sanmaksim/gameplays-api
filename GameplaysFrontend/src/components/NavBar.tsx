import { clearCredentials } from '../slices/authSlice';
import { Container, Dropdown, Form, NavbarCollapse } from 'react-bootstrap';
import { faGamepad } from '@fortawesome/free-solid-svg-icons/faGamepad';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Link, useNavigate } from 'react-router-dom';
import { PageContext } from '../contexts/PageContext';
import { RootState } from '../store';
import { toast } from 'react-toastify';
import { useLogoutMutation } from '../slices/usersApiSlice';
import { useSelector, useDispatch } from 'react-redux';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import NavDropdown from 'react-bootstrap/NavDropdown';
import ProfileIcon from './../assets/profile_icon.jpg';
import React, { useContext, useEffect, useState } from 'react';

interface SearchResults {
	results: [
        {
            deck: string,
            id: number,
            image: {
                icon_url: string
            },
            name: string,
            original_release_date: string,
            platforms: {
                id: number,
                name: string
            }
        }
    ]
}

function NavBar() {
    // use page context for displaying login/logout buttons
    const { isLoginPageContext, isRegisterPageContext } = useContext(PageContext);

    // for window mgmt
    const [isBelowMobileThreshold, setIsBelowMobileThreshold] = useState(false);
    const threshold = 768; // 768px for mobile views

    // for navbar toggle
    const [expanded, setExpanded] = useState(false);
    const handleToggle = () => setExpanded((prev) => !prev);

    useEffect(() => {
        // Set initial state based on current window size
        const handleResize = () => {
            if (window.innerWidth < threshold) {
                setIsBelowMobileThreshold(true);
                setExpanded(false); // Close navbar on mobile resize
            } else {
                setIsBelowMobileThreshold(false);
            }
        };
    
        // Add resize event listener
        window.addEventListener('resize', handleResize);
        handleResize();
    
        // Cleanup event listener on component unmount
        return () => window.removeEventListener('resize', handleResize);
      }, []);

    // for redux state mgmt
    const { userInfo } = useSelector((state: RootState) => state.auth);
    const [logout] = useLogoutMutation();
    const dispatch = useDispatch();
    
    // for logout redirect
    const navigate = useNavigate();

    const logoutHandler = async () => {
        try {
            const response = await logout('').unwrap();
            dispatch(clearCredentials());
            navigate('/');
            toast.success(response.message);
        } catch (error: any) {
            toast.error(error.data.message);
        }
    };

    // to capture search query
    const [query, setQuery] = useState('');

    // run search when Enter is pressed
    const handleKeyDown = (evt: React.KeyboardEvent) => {
        if (evt.key === 'Enter') {
            evt.preventDefault();
            fetchGameData();
        }
    };

    // proxy search query via local API
    const fetchGameData = async () => {
        try {
            const response = await fetch(`https://localhost:5001/api/games/${query}`);
            if (!response.ok) {
                throw new Error(`Error: ${response.statusText}`);
            }
            const data: SearchResults = await response.json();
            navigate('/search', { state: { data: data } });
        } catch (err: any) {
            navigate('/search', { state: { data: err } });
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
                
                {!isLoginPageContext && !isRegisterPageContext ? (
                    <Form className="w-50">
                        <Form.Control
                            type="text" 
                            value={query} 
                            onChange={(evt) => {setQuery(evt.target.value)}} 
                            onKeyDown={handleKeyDown}
                            placeholder="Search Games" 
                            aria-label="Search" 
                        />
                    </Form>
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
                                <NavDropdown.Item onClick={logoutHandler}>Logout</NavDropdown.Item>
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
                        <Dropdown.Item className="dropdown-item text-light" onClick={logoutHandler}>Logout</Dropdown.Item>
                    </NavbarCollapse>
                ) : null }

            </Container>
        </Navbar>
    );
}

export default NavBar;
