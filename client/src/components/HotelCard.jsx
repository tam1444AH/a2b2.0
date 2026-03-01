import React, { useState } from 'react';
import { Card, Button, Toast, ToastContainer, Spinner, Modal, Form, InputGroup} from 'react-bootstrap';
import { FaStar, FaRegStar } from 'react-icons/fa';

const HotelCard = ({ hotel }) => {
  const [isSaving, setIsSaving] = useState(false);
  const [isSaved, setIsSaved] = useState(false);
  const [toast, setToast] = useState(false);
  const [toastType, setToastType] = useState('');
  const [toastMessage, setToastMessage] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [nights, setNights] = useState(1);
  const [cardNumber, setCardNumber] = useState('');
  const [expirationDate, setExpirationDate] = useState('');
  const [totalCost, setTotalCost] = useState(200);
  const getRandomPrice = () => Math.floor(Math.random() * (1000 - 100 + 1)) + 100;

  const hotelPrice = 200;

  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleToast = (type, message) => {
    setToastType(type);
    setToastMessage(message);
    setToast(true);
  };

  const handleToastClose = () => {
    setToast(false);
  };

  const handleModalClose = () => setShowModal(false);
  const handleModalShow = () => setShowModal(true);

  const calculateTotalCost = (nights) => {
    setTotalCost(hotelPrice * nights);
  };

  const handleBookHotel = async () => {
    if (cardNumber.length !== 16 || !expirationDate.match(/^\d{4}-\d{2}$/)) {
      handleToast('danger', 'Invalid card details.');
      return;
    }

    if (!nights || nights <= 0) {
      handleToast('danger', 'Enter the number of nights you wish to stay.');
      return;
    }

    try {
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/book-hotel`, {
          method: "POST",
          headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${localStorage.getItem("authToken")}`,
          },
          body: JSON.stringify({
              hotelName: hotel.name,
              hotelDistance: hotel.distance.value,
              hotelIataCode: hotel.iataCode,
              hotelCountryCode: hotel.address.countryCode,
              hotelRating: hotel.rating,
              numNights: nights,
              totalCost: totalCost,
          }),
      });

      setIsSubmitting(true);

      if (response.ok) {
          const data = await response.json();
          handleToast("success", data.message || "Hotel successfully booked!");
          handleModalClose();
      } else {
          const error = await response.json();
          handleToast("danger", error.message || "Failed to book hotel.");
      }
    } catch (error) {
        handleToast("danger", "An error occurred while booking the hotel.");
    } finally {
      setIsSubmitting(false);
    }
  };
  
  const renderStars = (rating) => {
    const stars = [];
    for (let i = 1; i <= 5; i++) {
      stars.push(
        i <= rating ? (
          <FaStar key={i} className="text-warning" />
        ) : (
          <FaRegStar key={i} className="text-warning" />
        )
      );
    }
    return stars;
  };

  const handleSaveHotel = async () => {
    setIsSaving(true);

    try {
      const response = await fetch(`http://localhost:5030/api/hotels/save`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("authToken")}`,
        },
        body: JSON.stringify({
          HotelName: hotel.hotelName,
          HotelDistance: hotel.hotelDistance,
          HotelRating: hotel.hotelRating,
          Price: getRandomPrice(),
          HotelCountryCode: hotel.hotelCountryCode,
          HotelIataCode: hotel.hotelIataCode
        }),
      });
      
      if (response.ok) {
        const data = await response.json();
        handleToast('success', data.message || 'Hotel successfully saved.');
        setIsSaved(true);
      } else {
        const error = await response.json();
        handleToast('danger', error.message || 'Failed to save hotel.');
      }
    } catch (error) {
        handleToast('danger', 'An error occurred while saving the hotel.');
    } finally {
        setIsSaving(false);
    }
  };

  return (
    <div className="col-lg-4 col-md-6 col-sm-12 mb-4">
      <Card className="shadow-sm h-100">
        <Card.Body className="d-flex flex-column justify-content-between">
          
          <p className="mb-3 text-center fw-bold text-truncate" style={{ fontSize: '1.125rem' }}>{hotel.hotelName}</p>

          
          <div className="d-flex justify-content-between align-items-center mb-3 fs-5">
            <span className="text-dark fw-medium">
              {hotel.hotelDistance} {hotel.hotelDistanceUnit}
            </span>
            <span className="badge bg-dark text-white">{hotel.hotelCountryCode}</span>
          </div>

          
          <Card.Text className="text-center fs-5 fw-bold text-danger">
            ${getRandomPrice()}
          </Card.Text>

          
          <div className="d-flex justify-content-center mb-3 fs-5">
            {renderStars(hotel.hotelRating)}
          </div>
        </Card.Body>

        
        <Card.Footer className="d-flex justify-content-between">
          <Button 
            variant="danger"
            size="sm"
            className="fw-medium"
            disabled={isSaving || isSaved} 
            onClick={handleSaveHotel}
          >
            {isSaved ? 'Saved' : isSaving ? (
              <>
                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />{" "}Saving
              </>
            ) : (
              'Save Hotel'
            )}
          </Button>
          <Button variant="danger" size="sm" className="fw-medium" onClick={handleModalShow}>
            Book Hotel
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

      <Modal show={showModal} onHide={handleModalClose}>
        <Modal.Header closeButton>
          <Modal.Title>Book Hotel: {hotel.hotelName}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3" controlId="nights">
              <Form.Label>Number of Nights</Form.Label>
              <Form.Control
                className="form-control"
                type="number"
                min="1"
                value={nights}
                onChange={(e) => {
                  const value = parseInt(e.target.value, 10);
                  if (!isNaN(value) && value > 0) {
                    setNights(value);
                    calculateTotalCost(value);
                  } else {
                    
                    setNights(1);
                    calculateTotalCost(1);
                  }
                }}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3" controlId="cardNumber">
              <Form.Label>Card Number</Form.Label>
              <Form.Control
                type="text"
                maxLength="16"
                placeholder="Enter 16-digit card number"
                value={cardNumber}
                onChange={(e) => setCardNumber(e.target.value)}
              />
            </Form.Group>
            <Form.Group className="mb-3" controlId="expirationDate">
              <Form.Label>Expiration Date</Form.Label>
              <Form.Control
                type="month"
                value={expirationDate}
                onChange={(e) => setExpirationDate(e.target.value)}
              />
            </Form.Group>
            <div className="text-center fw-bold fs-5">
              Total Cost: ${totalCost || hotelPrice * nights}
            </div>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleModalClose}>
            Close
          </Button>
          <Button variant="primary" onClick={handleBookHotel} className="fw-medium" disabled={isSubmitting}>
            {isSubmitting ? (
              <>
                  <Spinner animation="border" size="sm" className="me-2" role="status" />
                  Confirming
              </>
            ) : (
              'Book Now'
            )}
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
};

export default HotelCard;
