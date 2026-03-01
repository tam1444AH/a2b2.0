import React, { useContext, useState } from 'react'
import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';
import Col from 'react-bootstrap/Col';
import Toast from 'react-bootstrap/Toast';
import ToastContainer from 'react-bootstrap/ToastContainer';
import axios from 'axios';
import { AuthContext } from '../context/AuthProvider';
import Spinner from 'react-bootstrap/Spinner';


const LoginForm = () => {
    const { setIsLoggedIn } = useContext(AuthContext);
    
    const [formData, setFormData] = useState({
        email: '',
        password: '',
    });

    const [toastMessage, setToastMessage] = useState('');
    const [toastType, setToastType] = useState('');
    const [showToast, setShowToast] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);

        if (formData.password.length < 8) {
            setToastType('error');
            setToastMessage('Password length must be at least 8 characters.');
            setShowToast(true);
            setIsSubmitting(false);
            return;
        }

        try {
            // const response = await axios.post(`${import.meta.env.VITE_API_BASE_URL}/login`, formData);
            // const { message, token } = response.data;
            const response = await axios.post(`http://localhost:5030/api/auth/login`, formData);
            const { token } = response.data;
            localStorage.setItem('authToken', token);

            setToastMessage('Login successful!');
            setToastType('success');
            setShowToast(true);
            setIsLoggedIn(true);
            
        } catch (error) {
            console.log(error);
            setToastMessage(
                error.response?.data?.message || 'Incorrect password or email. Please try again.'
            );
            setToastType('error');
            setShowToast(true);
        } finally {
            setIsSubmitting(false);
        }
    };



    return (
        <Container>
            <ToastContainer position="top-end" className="p-3">
                <Toast
                    bg={toastType === 'success' 
                        ? 'success' 
                        : toastType === 'error' 
                        ? 'danger'
                        : 'dark'}
                    show={showToast}
                    onClose={() => setShowToast(false)}
                    delay={3000}
                    autohide
                >
                    <Toast.Header closeButton>
                        <strong className="me-auto text-dark">
                            {toastType === 'success'
                                ? 'Success' 
                                : toastType === 'error'
                                ? 'Error'
                                : 'Info'}
                        </strong>
                    </Toast.Header>
                    <Toast.Body className="text-white">{toastMessage}</Toast.Body>
                </Toast>
            </ToastContainer>
            <Col xs={12} sm={10} md={8} lg={6} className="p-4 bg-dark rounded mb-4 mx-auto mt-4">
                <Form onSubmit={handleSubmit}>
                    <Form.Group>
                        <h3 className="text-center mb-4 text-white">Login:</h3>
                    </Form.Group>
                    <Form.Group className="mb-4" controlId="formGroupEmail">
                        <Form.Label>Email address</Form.Label>
                        <Form.Control
                            type="email"
                            placeholder="Enter your email"
                            className="p-2 text-truncate"
                            name='email'
                            value={formData.email}
                            onChange={handleChange}
                        />
                    </Form.Group>
                    <Form.Group className="mb-4" controlId="formGroupPassword">
                        <Form.Label>Password</Form.Label>
                        <Form.Control
                            type="password"
                            placeholder="Enter your password"
                            className="p-2 text-truncate"
                            name='password'
                            value={formData.password}
                            onChange={handleChange}
                        />
                    </Form.Group>
                    <div className="d-flex justify-content-center">
                        <Button variant="danger" type="submit" className="fw-medium d-flex align-items-center justify-content-center" disabled={isSubmitting}>
                            {isSubmitting ? (
                                <>
                                    <Spinner animation="border" size="sm" className="me-2" role="status" />
                                    Submitting
                                </>
                            ) : (
                                'Submit'
                            )}
                        </Button>
                    </div>
                </Form>
            </Col>
        </Container>
  )
}

export default LoginForm;