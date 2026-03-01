import React, { useState } from 'react';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';
import Col from 'react-bootstrap/Col';
import Container from 'react-bootstrap/Container';
import Toast from 'react-bootstrap/Toast';
import ToastContainer from 'react-bootstrap/ToastContainer';
import hotels from '../data/hotels';
import Spinner from 'react-bootstrap/Spinner';

const HotelsForm = ({ setHotels }) => {
    const [to, setTo] = useState('');
    const [dist, setDist] = useState('');
    const [stars, setStars] = useState('');
    const [toastMessage, setToastMessage] = useState('');
    const [toastType, setToastType] = useState('');
    const [showToast, setShowToast] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false); 

    const validateInputs = () => {
        const iataRegex = /^[A-Z]{3}$/;


        if (!iataRegex.test(to)) {
            setToastMessage('Please enter a valid IATA code (3 uppercase letters).');
            setToastType('error');
            setShowToast(true);
            return false;
        }

        if (dist < 1) {
            setToastMessage('Distance must be at least 1 mile.');
            setToastType('error');
            setShowToast(true);
            return false;
        }

        if (!stars) {
            setToastMessage('Please select a rating.');
            setToastType('error');
            setShowToast(true);
            return false;
        }

        return true;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateInputs()) return;

        setIsSubmitting(true);

        try {

            const response = await fetch(`http://localhost:5030/api/hotels/${to}-${dist}-${stars}`, {
                method: 'GET',
                headers: {     
                    Authorization: `Bearer ${localStorage.getItem("authToken")}`,
                    'Content-Type': 'application/json',
                },
            });
            console.log(response);


            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.detail || 'Something went wrong');
            }

            const data = await response.json();

            
            setHotels(data);    


            setToastType('success');
            setToastMessage('Hotels fetched successfully!');
            
            setShowToast(true);

        } catch (err) {
            setToastMessage(err.message);
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
            <Col xs={12} sm={10} md={8} lg={6} className="p-4 bg-dark rounded mb-4 mx-auto">
                <Form onSubmit={handleSubmit}>
                    <Form.Group>
                        <h3 className="text-center mb-4 text-white">Enter the following:</h3>
                    </Form.Group>
                    <Form.Group className="mb-4" controlId="formGroupArrival">
                        <Form.Control
                        type="text"
                        placeholder="IATA code of airport of arrival"
                        className="p-2 text-truncate"
                        required
                        value={to}
                        onChange={(e) => setTo(e.target.value.toUpperCase())}
                        />
                    </Form.Group>
                    <Form.Group className="mb-4" controlId="formMaxDist">
                        <Form.Control
                        type="number"
                        placeholder="Maximum distance from airport (in miles)"
                        className="p-2 text-truncate"
                        required
                        value={dist}
                        onChange={(e) => setDist(e.target.value)}
                        />
                    </Form.Group>
                    <Form.Group className="mb-4" controlId="formRating">
                        <Form.Select
                            value={stars}
                            onChange={(e) => setStars(e.target.value)}
                            required
                        >
                            <option>Rating</option>
                            <option value="1">1</option>
                            <option value="2">2</option>
                            <option value="3">3</option>
                            <option value="4">4</option>
                            <option value="5">5</option>
                        </Form.Select>
                    </Form.Group>
                    <div className="d-flex justify-content-center">
                        <Button variant="danger" type="submit" className="fw-medium" disabled={isSubmitting}>
                            {isSubmitting ? (
                                <>
                                    <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" className="me-2" />
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

export default HotelsForm;