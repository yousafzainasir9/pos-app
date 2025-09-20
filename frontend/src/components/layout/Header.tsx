import React from 'react';
import { Navbar, Nav, Container, Badge, Dropdown } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { FaShoppingCart, FaUser, FaCashRegister, FaSignOutAlt } from 'react-icons/fa';
import { useAuth } from '@/contexts/AuthContext';
import { useCart } from '@/contexts/CartContext';
import { useShift } from '@/contexts/ShiftContext';

const Header: React.FC = () => {
  const { user, logout } = useAuth();
  const { totalItems } = useCart();
  const { isShiftOpen, currentShift } = useShift();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <Navbar bg="dark" variant="dark" expand="lg" sticky="top">
      <Container fluid>
        <Navbar.Brand as={Link} to="/" className="fw-bold">
          Cookie Barrel POS
        </Navbar.Brand>
        
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            <Nav.Link as={Link} to="/pos">
              POS
            </Nav.Link>
            <Nav.Link as={Link} to="/orders">
              Orders
            </Nav.Link>
            <Nav.Link as={Link} to="/products">
              Products
            </Nav.Link>
            {user?.role === 'Admin' || user?.role === 'Manager' ? (
              <>
                <Nav.Link as={Link} to="/reports">
                  Reports
                </Nav.Link>
                <Nav.Link as={Link} to="/admin">
                  Admin
                </Nav.Link>
              </>
            ) : null}
          </Nav>

          <Nav className="ms-auto align-items-center">
            {/* Shift Status */}
            <div className="me-3">
              {isShiftOpen ? (
                <Badge bg="success" className="p-2">
                  <FaCashRegister className="me-1" />
                  Shift: {currentShift?.shiftNumber}
                </Badge>
              ) : (
                <Badge bg="warning" className="p-2">
                  <FaCashRegister className="me-1" />
                  No Active Shift
                </Badge>
              )}
            </div>

            {/* Cart */}
            <Nav.Link as={Link} to="/cart" className="position-relative me-3">
              <FaShoppingCart size={20} />
              {totalItems > 0 && (
                <Badge 
                  bg="danger" 
                  className="position-absolute top-0 start-100 translate-middle"
                  style={{ fontSize: '0.7rem' }}
                >
                  {totalItems}
                </Badge>
              )}
            </Nav.Link>

            {/* User Menu */}
            <Dropdown align="end">
              <Dropdown.Toggle variant="outline-light" size="sm">
                <FaUser className="me-2" />
                {user?.firstName} {user?.lastName}
              </Dropdown.Toggle>
              <Dropdown.Menu>
                <Dropdown.Item disabled>
                  Role: {user?.role}
                </Dropdown.Item>
                <Dropdown.Item disabled>
                  Store: {user?.storeName || 'N/A'}
                </Dropdown.Item>
                <Dropdown.Divider />
                <Dropdown.Item as={Link} to="/shift">
                  Manage Shift
                </Dropdown.Item>
                <Dropdown.Item as={Link} to="/profile">
                  Profile
                </Dropdown.Item>
                <Dropdown.Divider />
                <Dropdown.Item onClick={handleLogout}>
                  <FaSignOutAlt className="me-2" />
                  Logout
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default Header;
