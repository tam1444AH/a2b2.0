import React, { useState, useContext } from "react";
import { IoTrash } from "react-icons/io5";
import Toast from "react-bootstrap/Toast";
import ToastContainer from "react-bootstrap/ToastContainer";
import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";
import { AuthContext } from '../context/AuthProvider';

const DeleteButton = () => {
  const [showModal, setShowModal] = useState(false);

  const [toast, setToast] = useState(false);
  const [toastType, setToastType] = useState('');
  const [toastMessage, setToastMessage] = useState('');

  const { logOut } = useContext(AuthContext);

  const handleToast = (type, message) => {
    setToastType(type);
    setToastMessage(message);
    setToast(true);
  };

  const handleToastClose = () => {
    setToast(false);
  };

  const handleDeleteAccount = async () => {
    try {
      const response = await fetch(`http://localhost:5030/api/auth/delete`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("authToken")}`,
        },
      });

      if (response.ok) {
        setShowModal(false);
        logOut();
      } else {
        handleToast('danger', "Failed to delete account.");
      }
    } catch (error) {
      setToastMessage(`Error: ${error.message}`);
      handleToast('danger', "Failed to delete account.");
    }
  };

  return (
    <div>

      <Button
        className="fw-bold d-flex align-items-center"
        variant="danger"
        onClick={() => setShowModal(true)}
        size="lg"
      >
        Delete Account <IoTrash className="ms-2" />
      </Button>

      <Modal show={showModal} onHide={() => setShowModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Confirm Account Deletion</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Are you sure you want to delete your account? This action cannot be undone.
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowModal(false)}>
            No
          </Button>
          <Button variant="danger" onClick={handleDeleteAccount} className="fw-medium">
            Yes, Delete
          </Button>
        </Modal.Footer>
      </Modal>

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
    </div>
  );
};

export default DeleteButton;
