import { ChangeEvent, FormEvent, useState } from 'react';
import Alert from 'react-bootstrap/esm/Alert';
import Button from 'react-bootstrap/esm/Button';
import Card from 'react-bootstrap/esm/Card';
import Form from 'react-bootstrap/esm/Form';
import { registerUser } from '../services/UserDataService';
import { UserType } from '../types/DataType';
import { useNavigate } from 'react-router-dom';

function RegisterPage() {
    const [un, setUn] = useState('');
    const [mail, setMail] = useState('');
    const [pwd, setPwd] = useState('');
    const [unTouched, setUnTouched] = useState(false);
    const [mailTouched, setMailTouched] = useState(false);
    const [pwdTouched, setPwdTouched] = useState(false);
    const [showAlert, setShowAlert] = useState(false);

    const handleUnInputChange = (evt: ChangeEvent<HTMLInputElement>) => {
        setUn(evt.target.value);
        setUnTouched(true);
    };

    const handleMailInputChange = (evt: ChangeEvent<HTMLInputElement>) => {
        setMail(evt.target.value);
        setMailTouched(true);
    };

    const handlePwdInputChange = (evt: ChangeEvent<HTMLInputElement>) => {
        setPwd(evt.target.value);
        setPwdTouched(true);
    };

    const formData: UserType = {
        username: un,
        email: mail,
        password: pwd
    };

    const navigate = useNavigate();

    const submitForm = async (evt: FormEvent<HTMLFormElement>) => {
        evt.preventDefault();
        let result: void;

        try {
            const response = await registerUser(formData);
            if (response.ok) {
                result = navigate('/');
            }
        } catch (error) {
            setShowAlert(true);
        }

        return result;
    };

    return (
        <div className="mx-auto mt-5">
            <Card style={{ width: '20rem' }}>
                <Card.Body>

                    <Card.Title className="lg mb-3">Sign up with Gameplays</Card.Title>

                    { showAlert && (<Alert variant="danger">Invalid username or password.</Alert>) }

                    <Form onSubmit={ submitForm }>

                        <Form.Group className="mb-3" controlId="formBasicUsername">
                            <Form.Label>Username</Form.Label>
                            <Form.Control
                                type="username"
                                value={ un }
                                onChange={ handleUnInputChange }
                                required
                                isInvalid={ !un && unTouched } />
                            <Form.Control.Feedback type="invalid">Please enter a valid username.</Form.Control.Feedback>
                        </Form.Group>

                        <Form.Group className="mb-3" controlId="formBasicEmail">
                            <Form.Label>Email</Form.Label>
                            <Form.Control
                                type="email"
                                value={ mail }
                                onChange={ handleMailInputChange }
                                required
                                isInvalid={ !mail && mailTouched } />
                            <Form.Control.Feedback type="invalid">Please enter a valid email address.</Form.Control.Feedback>
                            <Form.Text className="text-muted">
                                We'll never share your email with anyone else.
                            </Form.Text>
                        </Form.Group>

                        <Form.Group className="mb-3" controlId="formBasicPassword">
                            <Form.Label>Password</Form.Label>
                            <Form.Control
                                type="password"
                                value={ pwd }
                                onChange={ handlePwdInputChange }
                                required
                                isInvalid={ !pwd && pwdTouched } />
                            <Form.Control.Feedback type="invalid">Please enter a valid password.</Form.Control.Feedback>
                        </Form.Group>

                        <Button variant="primary" type="submit">
                            Register
                        </Button>

                    </Form>

                </Card.Body>
            </Card>
        </div>
    );
};

export default RegisterPage;
