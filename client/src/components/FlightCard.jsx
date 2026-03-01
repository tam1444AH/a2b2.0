import React, { useState } from 'react';
import { Card, Button, Toast, ToastContainer, Spinner, Form, InputGroup } from 'react-bootstrap';
import { IoAirplaneSharp } from 'react-icons/io5';
import Modal from 'react-bootstrap/Modal';


const FlightCard = ({ flight }) => {
  const [isSaving, setIsSaving] = useState(false);
  const [isSaved, setIsSaved] = useState(false);
  const [toast, setToast] = useState(false);
  const [toastType, setToastType] = useState('');
  const [toastMessage, setToastMessage] = useState('');

  const [show, setShow] = useState(false);
  const [numTickets, setNumTickets] = useState(1);
  const [cardNumber, setCardNumber] = useState('');
  const [expirationDate, setExpirationDate] = useState('');
  const [totalCost, setTotalCost] = useState(0);

  const [isSubmitting, setIsSubmitting] = useState(false);

  const getRandomPrice = () => Math.floor(Math.random() * (500 - 100 + 1)) + 100;

  const handleClose = () => setShow(false);
  const handleShow = () => {
    setTotalCost(150 * numTickets);
    setShow(true);
  };

  const handleToast = (type, message) => {
    setToastType(type);
    setToastMessage(message);
    setToast(true);
  };

  const handleToastClose = () => {
    setToast(false);
  };

  const handleNumTicketsChange = (e) => {
    const value = Math.max(1, parseInt(e.target.value) || 1);
    setNumTickets(value);
    setTotalCost(value * 150);
  };

  const handleCardNumberChange = (e) => {
    const value = e.target.value.replace(/\D/g, ''); // Remove non-numeric characters
    setCardNumber(value);
  };

  const handleExpirationDateChange = (e) => {
    setExpirationDate(e.target.value);
  };


  const handleBookFlight = async () => {
    if (cardNumber.length !== 16 || !expirationDate.match(/^\d{4}-\d{2}$/)) {
      handleToast('danger', 'Please enter valid card details.');
      return;
    }

    const bookingDetails = {
      FlightName: `${flight.airline.name} ${flight.flight.number}`,
      FlightDate: flight.flight_date,
      DepartureIata: flight.departure.iata,
      ArrivalIata: flight.arrival.iata,
      DepartureTime: new Date(flight.departure.scheduled).toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit',
        hour12: true,
      }),
      ArrivalTime: new Date(flight.arrival.scheduled).toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit',
        hour12: true,
      }),
      NumTickets: numTickets,
      TotalCost: totalCost,
    };

    setIsSubmitting(true);

    try {
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/book-flight`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("authToken")}`,
        },
        body: JSON.stringify(bookingDetails),
      });
  
      if (response.ok) {
        const data = await response.json();
        handleToast('success', data.message || 'Flight successfully booked!');
        handleClose();
      } else {
        const error = await response.json();
        handleToast('danger', error || 'Failed to book flight.');
      }
    } catch (error) {
      handleToast('danger', 'An error occurred while booking the flight. Please make sure you are logged in.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSaveFlight = async () => {
    setIsSaving(true);

    try {
      const response = await fetch(`http://localhost:5030/api/flights/save`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("authToken")}`,
        },
        body: JSON.stringify({
          FlightName: `${flight.airlineName} ${flight.flightNumber}`,
          DepartureTime: new Date(flight.departureScheduled).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: true }),
          ArrivalTime: new Date(flight.arrivalScheduled).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: true }),
          FlightDate: flight.flightDate,
          DepartureIata: flight.departureIata,
          ArrivalIata: flight.arrivalIata,
          Price: getRandomPrice()
        }),
      });

      if (response.ok) {
        const data = await response.json();
        handleToast('success', data.message || 'Flight successfully saved.');
        setIsSaved(true);
      } else {
        const error = await response.json();
        handleToast('danger', error.message || 'Failed to save flight.');
      }

    } catch (error) {
        handleToast('danger', 'An error occurred while saving the flight. Make sure you are logged in.');
    } finally {
        setIsSaving(false);
    }
  };

  return (
    <div className="col-lg-4 col-md-6 col-sm-12 mb-4">
      <Card className="shadow-sm h-100">
        <Card.Body className="d-flex flex-column justify-content-between">
          
          <div className="d-flex justify-content-between align-items-center mb-3">
            <h5 className="mb-0 fs-5">{new Date(flight.departureScheduled).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: true })}</h5>
            <IoAirplaneSharp className="text-danger fs-2" />
            <h5 className="mb-0 fs-5">{new Date(flight.arrivalScheduled).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: true })}</h5>
          </div>

          
          <div className="d-flex justify-content-between align-items-center mb-3">
            <span className="fw-bold text-muted fs-5">{flight.depatureIata}</span>
            <div className="flex-grow-1 mx-2 border-top border-dark"></div>
            <span className="fw-bold text-muted fs-5">{flight.arrivalIata}</span>
          </div>

          
          <Card.Text className="text-center mb-3" style={{ fontSize: '1.125rem' }}>
            <strong>{flight.airlineName} {flight.flightNumber}</strong>
          </Card.Text>

          
          <div className="d-flex justify-content-between mb-3">
            <span className="badge bg-dark text-white">{flight.flightStatus}</span>
            <span className="badge bg-dark text-white">{flight.flightDate}</span>
          </div>

          <span className="text-black fw-medium fs-4">${getRandomPrice()}</span>
        </Card.Body>

        
        <Card.Footer className="d-flex justify-content-between">
          <Button
            variant="danger"
            size="sm"
            className="fw-medium"
            disabled={isSaving || isSaved} 
            onClick={handleSaveFlight}
          >
            {isSaved ? 'Saved' : isSaving ? (
              <>
                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />{"  "} 
                Saving
              </>
            ) : (
              'Save Flight'
            )}
          </Button>
          <Button variant="danger" 
            size="sm" 
            className='fw-medium'
            onClick={handleShow}
          >
              Book Flight
          </Button>
        </Card.Footer>
      </Card>

      <ToastContainer position="top-end" className="p-3">
        <Toast
          show={toast}
          onClose={handleToastClose}
          delay={3000}
          autohide
          bg={toastType}
        >
          <Toast.Header closeButton>
            <strong className="me-auto text-dark">
              {toastType === 'success'
                ? 'Success!'
                : toastType === 'danger'
                ? 'Error'
                : 'Info'}
            </strong>
          </Toast.Header>
          <Toast.Body className="text-white text-start">{toastMessage}</Toast.Body>
        </Toast>
      </ToastContainer>

      <Modal show={show} onHide={handleClose} centered>
        <Modal.Header closeButton>
          <Modal.Title>Book Flight: {flight.airlineName} {flight.flightNumber}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3" controlId="numTickets">
              <Form.Label>Number of Tickets</Form.Label>
              <InputGroup>
                <Form.Control
                  type="number"
                  min="1"
                  value={numTickets}
                  onChange={handleNumTicketsChange}
                  required
                />
                <InputGroup.Text>Tickets</InputGroup.Text>
              </InputGroup>
            </Form.Group>

            <Form.Group className="mb-3" controlId="cardNumber">
              <Form.Label>Card Number</Form.Label>
              <Form.Control
                type="text"
                maxLength="16"
                value={cardNumber}
                onChange={handleCardNumberChange}
                placeholder="Enter 16-digit card number"
                required
              />
            </Form.Group>

            <Form.Group className="mb-3" controlId="expirationDate">
              <Form.Label>Expiration Date</Form.Label>
              <Form.Control
                type="month"
                value={expirationDate}
                onChange={handleExpirationDateChange}
                required
              />
            </Form.Group>

            <div className="text-center fw-bold fs-5">
              Total Cost: ${totalCost}
            </div>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleBookFlight} className="fw-medium" disabled={isSubmitting}>
            {isSubmitting ? (
              <>
                  <Spinner animation="border" size="sm" className="me-2" role="status" />
                  Confirming
              </>
            ) : (
              'Confirm Booking'
            )}
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
};

export default FlightCard;
