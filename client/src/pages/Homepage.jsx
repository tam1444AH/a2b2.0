import React, { useState } from 'react'
import IATAForm from '../components/IATAForm'
import FlightsBox from '../components/FlightsBox'
import Toast from 'react-bootstrap/Toast';
import ToastContainer from 'react-bootstrap/ToastContainer';
import flights from '../data/flights';
import Footer from '../components/Footer';

const Homepage = () => {
  // const [flightsH, setFlightsH] = useState([]);
  const [flights, setFlights] = useState([]);

  const [toast, setToast] = useState(false);
  const [toastType, setToastType] = useState('');
  const [toastMessage, setToastMessage] = useState('');
  
  const handleToast = (type, message) => {
    setToastType(type);
    setToastMessage(message);
    setToast(true);
  };
  
  const handleToastClose = () => {
    setToast(false);
  };


  const handleSearch = async (from, to) => {
    try {
      const response = await fetch(`http://localhost:5030/api/flights/${from}-${to}`);

      if (!response.ok) {
        const error = await response.json();
        handleToast('error', error.detail || "Error fetching flights.");
        return;
      }

      const data = await response.json();
      console.log(data);
      // const data = flights.flights;

      // handleToast('dark', 'No flights found for the specified route.');
      // return;

      if (data.message == 'No flights found for the specified route.') {
        handleToast('dark', 'No flights found for the specified route.');
        return;
      }

      // setFlightsH(data);
      // setFlights(data);
      // handleToast('success', `${data.length} flights found!`);
  

    } catch (err) {

      console.error("Error fetching flights:", err);
      handleToast('error', err.message);

    }
  };

  return (
    <>
      <div className="d-flex flex-column min-vh-100 bg-black text-white">
        <div className='flex-grow-1 d-flex flex-column justify-content-evenly align-items-center p-3'>
          <IATAForm onSearch={handleSearch} />
          <FlightsBox flights={flights} />
          {/* <FlightsBox flights={flightsH} /> */}
          <ToastContainer position="top-end" className="p-3" aria-live="assertive">
            <Toast
              show={toast}
              onClose={handleToastClose}
              delay={3000}
              autohide
              bg={
                toastType === 'success'
                  ? 'success'
                  : toastType === 'error'
                  ? 'danger'
                  : 'dark'
              }
            >
              <Toast.Header closeButton>
                <strong className="me-auto text-dark">
                  {toastType === 'success'
                    ? 'Success!'
                    : toastType === 'error'
                    ? 'Error'
                    : 'Info'}
                </strong>
              </Toast.Header>
              <Toast.Body className="text-white">{toastMessage}</Toast.Body>
            </Toast>
          </ToastContainer>
        </div> 
        <Footer />
      </div>
    </>
  )
}

export default Homepage;