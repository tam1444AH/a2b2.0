import React, { useState } from "react";
import { Table, Button, Form, InputGroup } from "react-bootstrap";
import { IoTrash, IoBed } from "react-icons/io5";
import { FaStar, FaRegStar } from 'react-icons/fa';
import Toast from 'react-bootstrap/Toast';
import ToastContainer from 'react-bootstrap/ToastContainer';
import Modal from 'react-bootstrap/Modal';
import Spinner from 'react-bootstrap/Spinner';

const SavedHotelsTable = ({ hotels, setHotels }) => {

  const [toast, setToast] = useState(false);
  const [toastType, setToastType] = useState('');
  const [toastMessage, setToastMessage] = useState('');

  const [show, setShow] = useState(false);
  const [numNights, setNumNights] = useState(1);
  const [cardNumber, setCardNumber] = useState('');
  const [expirationDate, setExpirationDate] = useState('');
  const [totalCost, setTotalCost] = useState(0);

  const [selectedHotel, setSelectedHotel] = useState(null);

  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleClose = () => setShow(false);
  const handleShow = (hotel) => {
    setSelectedHotel(hotel); 
    setTotalCost(hotel.price * numNights);
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

  const handleNumNightsChange = (e) => {
    const value = Math.max(1, parseInt(e.target.value) || 1);
    setNumNights(value);
    if (selectedHotel) {
      setTotalCost(value * selectedHotel.price);
    }
  };

  const handleCardNumberChange = (e) => {
    const value = e.target.value.replace(/\D/g, ''); // Remove non-numeric characters
    setCardNumber(value);
  };

  const handleExpirationDateChange = (e) => {
    setExpirationDate(e.target.value);
  };

  const handleBookHotel = async (hotel) => {
    if (!hotel) {
      handleToast('danger', 'No hotel selected for booking.');
      return;
    }

    if (!numNights || numNights <= 0) {
      handleToast('danger', 'Enter the number of nights you wish to stay.');
      return;
    }

    if (cardNumber.length !== 16 || !expirationDate.match(/^\d{4}-\d{2}$/)) {
      handleToast('danger', 'Please enter valid card details.');
      return;
    }

    const bookingDetails = {
      HotelName: hotel.hotelName,
      HotelDistance: hotel.hotelDistance,
      HotelIataCode: hotel.hotelIataCode,
      HotelCountryCode: hotel.hotelCountryCode,
      HotelRating: hotel.hotelRating,
      NumNights: numNights,
      TotalCost: totalCost,
    }

    setIsSubmitting(true);

    try {
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/book-hotel`, {
          method: "POST",
          headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${localStorage.getItem("authToken")}`,
          },
          body: JSON.stringify(bookingDetails),
      });

      if (response.ok) {
          const data = await response.json();
          handleToast("success", data.message || "Hotel successfully booked!");
          handleClose();
      } else {
          const error = await response.json();
          console.error("Error:", error);
          handleToast("danger", error.message || "Failed to book hotel.");
      }
    } catch (error) {
      console.error("Error:", error);
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

  if (hotels.length === 0) {
    return (
      <div
        className="d-flex flex-column justify-content-center align-items-center text-muted bg-light rounded-4 text-center"
        style={{ height: "60vh", padding: "0 1rem", overflow: "hidden", wordBreak: "break-word" }}
      >
        <IoBed className="fs-1 mb-1" />
        <p className="fs-4 p-1">
          Save a hotel for it to appear here.
        </p>
      </div>
    );
  }

  const handleDeleteHotel = async (hotelId) => {
    try {
      const response = await fetch(`http://localhost:5030/api/hotels/delete/${hotelId}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("authToken")}`,
        },
      });

      if (response.ok) {
        setHotels((prevHotels) => prevHotels.filter((hotel) => hotel.id !== hotelId));
      } else {
        const error = await response.json();
        console.error("Error:", error);
        handleToast('danger', "Failed to delete hotel.");
      }
    } catch (error) {
      console.error("Error:", error);
      handleToast('danger', 'An error occurred while deleting the hotel.');
    }
  };

  return (
    <div className="table-responsive mb-4 rounded-4 bg-light" style={{ height: "60vh", overflowY: "auto" }}>
      <Table bordered hover className="shadow-sm bg-light">
        <thead className="bg-dark text-white">
          <tr>
            <th>Hotel</th>
            <th>Distance</th>
            <th>Price</th>
            <th>Rating</th>
            <th>IATA</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {hotels.map((hotel, index) => (
            <tr key={index}>
              <td>{hotel.hotelName}</td>
              <td>{hotel.hotelDistance} MI</td>
              <td>${hotel.price}</td>
              <td>{renderStars(hotel.hotelRating)}</td>
              <td>{hotel.hotelIataCode}</td>
              <td className="d-flex gap-2 justify-content-around">
                <Button 
                  variant="danger" 
                  size="lg"
                  onClick={() => handleDeleteHotel(hotel.id)}
                >
                  <IoTrash />
                </Button>
                <Button variant="primary" size="lg" onClick={() => handleShow(hotel)}>
                  <IoBed />
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </Table>

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
          <Modal.Title>Book Hotel: {selectedHotel?.hotelName || "Loading..."} </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3" controlId="numTickets">
              <Form.Label>Number of Nights</Form.Label>
              <InputGroup>
                <Form.Control
                  type="number"
                  min="1"
                  value={numNights}
                  onChange={handleNumNightsChange}
                  required
                />
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
          <Button variant="primary" onClick={() => handleBookHotel(selectedHotel)} className="fw-medium" disabled={isSubmitting}>
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

export default SavedHotelsTable;
