import React, { useState, useEffect } from "react";
import ProfileBanner from "../components/ProfileBanner";
import flights from "../data/flights";
import hotels from "../data/hotels";
import SavedFlightsTable from "../components/SavedFlightsTable";
import SavedHotelsTable from "../components/SavedHotelsTable";
import { IoAirplane, IoBed } from "react-icons/io5";
import Toast from 'react-bootstrap/Toast';
import ToastContainer from 'react-bootstrap/ToastContainer';
import DeleteButton from "../components/DeleteButton";
import Footer from "../components/Footer";


const ProfilePage = () => {
  const [greeting, setGreeting] = useState("");

  const [fetchedFlights, setFetchedFlights] = useState([]);
  const [fetchedHotels, setFetchedHotels] = useState([]);

  const [toast, setToast] = useState(false);
  const [toastMessage, setToastMessage] = useState('');

  useEffect(() => {
    const hour = new Date().getHours();
    if (hour >= 5 && hour < 12) {
      setGreeting("Good morning,");
    } else if (hour >= 12 && hour < 17) {
      setGreeting("Good afternoon,");
    } else if (hour >= 17 && hour < 24) {
      setGreeting("Good evening,");
    } else {
      setGreeting("Hello,");
    }
  }, []);

  const handleToast = (message) => {
    setToastMessage(message);
    setToast(true);
  };
  
  const handleToastClose = () => {
    setToast(false);
  };

  useEffect(() => {
    const fetchSavedFlights = async () => {
      try {
        const response = await fetch(`http://localhost:5030/api/flights/saved`, {
          method: "GET",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("authToken")}`, 
          },
        });
        if (response.ok) {
          const data = await response.json();
          setFetchedFlights(data);
        } else {
          console.error("Failed to fetch saved flights.");
          handleToast("Failed to fetch saved flights.");
        }
      } catch (error) {
        console.error("Error fetching saved flights:", error);
        handleToast(error.message);
      }
    };

    const fetchSavedHotels = async () => {
      try {
        const response = await fetch(`http://localhost:5030/api/hotels/saved`, {
          method: "GET",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("authToken")}`,
          },
        });
        if (response.ok) {
          const data = await response.json();
          setFetchedHotels(data);
        } else {
          console.error("Failed to fetch saved hotels.");
          handleToast("Failed to fetch saved hotels.");
        }
      } catch (error) {
        console.error("Error fetching saved hotels:", error);
        handleToast(error.message);
      }
    };

    fetchSavedFlights();
    fetchSavedHotels();
  }, []);

  return (
    <div className="d-flex flex-column min-vh-100 bg-black text-white">
      <div className="flex-grow-1 d-flex flex-column justify-content-evenly align-items-center p-3">
        <ProfileBanner greeting={greeting} />
        <div className="container py-4">
          <div className="row g-4">
            <div className="col-lg-6">
              <p className="text-center text-white mb-3 fs-4 fw-medium">Saved Flights <IoAirplane /></p>
              <SavedFlightsTable flights={fetchedFlights} setFlights={setFetchedFlights} />
            </div>
            <div className="col-lg-6">
              <p className="text-center text-white mb-3 fs-4 fw-medium">Saved Hotels <IoBed /></p>
              <SavedHotelsTable hotels={fetchedHotels} setHotels={setFetchedHotels} />
            </div>
            <div className="text-center mt-4 d-flex justify-content-center">
              <DeleteButton />
            </div>
          </div>
        </div>

        <ToastContainer position="top-end" className="p-3" aria-live="assertive">
          <Toast
            show={toast}
            onClose={handleToastClose}
            delay={3000}
            autohide
            bg='danger'
          >
            <Toast.Header closeButton>
              <strong className="me-auto text-dark">
                Error
              </strong>
            </Toast.Header>
            <Toast.Body className="text-white">{toastMessage}</Toast.Body>
          </Toast>
        </ToastContainer>
      </div>
      <Footer/>
    </div>
  );
};

export default ProfilePage;
