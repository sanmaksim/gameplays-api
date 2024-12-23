import { clearCredentials } from '../slices/authSlice';
import { Container, Form } from 'react-bootstrap';
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
import React, { useContext, useState } from 'react';

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
    const { isLoginPageContext } = useContext(PageContext);

    const navigate = useNavigate();
    const dispatch = useDispatch();

    const [logout] = useLogoutMutation();

    const { userInfo } = useSelector((state: RootState) => state.auth);

    

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

    const [query, setQuery] = useState('');

    const handleKeyDown = (evt: React.KeyboardEvent) => {
        if (evt.key === 'Enter') {
            evt.preventDefault();
            fetchGameData();
        }
    };

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
        <Navbar expand="lg" bg="dark" data-bs-theme="dark">
            <Container>
                <Nav style={{ width: '150px' }}>
                    <Navbar.Brand>
                        <Link className="text-light fw-bold text-decoration-none" to="/">
                            <FontAwesomeIcon icon={faGamepad} />
                            <span className="ms-1">Gameplays</span>
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
                            type="text" 
                            value={query} 
                            onChange={(evt) => {setQuery(evt.target.value)}} 
                            onKeyDown={handleKeyDown}
                            placeholder="Search Games" 
                            aria-label="Search" 
                        />
                    </Form>
                </Nav>
                <Nav className="d-flex justify-content-end" style={{ width: '150px' }}>
                    {userInfo ? (
                        <NavDropdown title={userInfo.username} id="user-dropdown">
                            <Link className="dropdown-item" to="/user/profile">Profile</Link>
                            <Link className="dropdown-item" to="/user/games">Games</Link>
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
