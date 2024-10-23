import { AuthContext } from '../contexts/AuthContext';
import { authUser } from '../services/UserDataService';
import { ChangeEvent, FormEvent, useContext, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { UserType } from '../types/DataType';
import Alert from 'react-bootstrap/esm/Alert';
import Button from 'react-bootstrap/esm/Button';
import Card from 'react-bootstrap/esm/Card';
import Form from 'react-bootstrap/esm/Form';

function LoginPage() {
    const { setIsLoggedInContext } = useContext(AuthContext);

    const [cred, setCred] = useState('');
    const [pwd, setPwd] = useState('');
    const [credTouched, setCredTouched] = useState(false);
    const [pwdTouched, setPwdTouched] = useState(false);
    const [showAlert, setShowAlert] = useState(false);

    const handleCredInputChange = (evt: ChangeEvent<HTMLInputElement>) => {
        setCred(evt.target.value);
        setCredTouched(true);
    };

    const handlePwdInputChange = (evt: ChangeEvent<HTMLInputElement>) => {
        setPwd(evt.target.value);
        setPwdTouched(true);
    };

    let credential = {};

    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (emailRegex.test(cred)) {
        credential = { email: cred };
    } else {
        credential = { username: cred };
    }

    const formData: UserType = {
        // spread operator copies properties
        // from one object to another
        ...credential,
        password: pwd
    };

    const navigate = useNavigate();

    const submitForm = async (evt: FormEvent<HTMLFormElement>) => {
        evt.preventDefault();
        
        try {
            const user = await authUser(formData);
            if (user.userId) {
                navigate(`/user/${user.userId}`);
                setIsLoggedInContext();
            }
        } catch (error) {
            setShowAlert(true);
        }
    };

    return (
        <div className="mx-auto mt-5">
            <Card style={{ width: '20rem' }}>
                <Card.Body>

                    <Card.Title className="lg mb-3">Please sign in</Card.Title>

                    { showAlert && (<Alert variant="danger">Incorrect username or password.</Alert>) }

                    <Form onSubmit={ submitForm }>

                        <Form.Group className="mb-3" controlId="formBasicUsername">
                            <Form.Label>Username or Email</Form.Label>
                            <Form.Control
                                type="username"
                                placeholder="Email address or username"
                                value={ cred }
                                onChange={ handleCredInputChange }
                                required
                                isInvalid={ !cred && credTouched } />
                            <Form.Control.Feedback type="invalid">Please enter your login.</Form.Control.Feedback>
                        </Form.Group>

                        <Form.Group className="mb-3" controlId="formBasicPassword">
                            <Form.Label>Password</Form.Label>
                            <Form.Control
                                type="password"
                                placeholder="Password"
                                value={ pwd }
                                onChange={ handlePwdInputChange }
                                required
                                isInvalid={ !pwd && pwdTouched } />
                            <Form.Control.Feedback type="invalid">Please enter your password.</Form.Control.Feedback>
                        </Form.Group>

                        {/* <Form.Group className="mb-3" controlId="formBasicCheckbox">
                            <Form.Check type="checkbox" label="Keep me signed in" />
                        </Form.Group> */}

                        <Button variant="secondary" type="submit">
                            Sign in
                        </Button>

                    </Form>

                </Card.Body>
            </Card>
        </div>
    );
}

export default LoginPage;
