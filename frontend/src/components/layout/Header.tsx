import React from 'react';
import { Navbar, Nav, Container, Badge, Dropdown } from 'react-bootstrap';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { FaShoppingCart, FaUser, FaCashRegister, FaSignOutAlt, FaPalette, FaShieldAlt } from 'react-icons/fa';
import { useAuth } from '@/contexts/AuthContext';
import { useCart } from '@/contexts/CartContext';
import { useShift } from '@/contexts/ShiftContext';
import { useTheme } from '@/theme/ThemeContext';

const Header: React.FC = () => {
  const { user, logout } = useAuth();
  const { totalItems } = useCart();
  const { isShiftOpen, currentShift } = useShift();
  const { theme } = useTheme();
  const navigate = useNavigate();
  const location = useLocation();

  // Helper function to check if a path is active
  const isActive = (path: string) => {
    return location.pathname === path || location.pathname.startsWith(path + '/');
  };

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <Navbar className="navbar-theme" expand="lg" sticky="top">
      <Container fluid>
        <Navbar.Brand as={Link} to="/" className="fw-bold logo-text">
          <span className="logo-icon">🍪</span> {theme.companyName}
        </Navbar.Brand>
        
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            {/* POS access for Manager and Cashier only */}
            {user?.role !== 'Admin' && (
              <Nav.Link 
                as={Link} 
                to="/pos"
                active={isActive('/pos')}
              >
                POS
              </Nav.Link>
            )}
            <Nav.Link 
              as={Link} 
              to="/orders"
              active={isActive('/orders')}
            >
              Orders
            </Nav.Link>
            <Nav.Link 
              as={Link} 
              to="/products"
              active={isActive('/products')}
            >
              Products
            </Nav.Link>
            {user?.role === 'Admin' || user?.role === 'Manager' ? (
              <>
                <Nav.Link 
                  as={Link} 
                  to="/reports"
                  active={isActive('/reports')}
                >
                  Reports
                </Nav.Link>
                <Nav.Link 
                  as={Link} 
                  to="/admin"
                  active={isActive('/admin')}
                >
                  Admin
                </Nav.Link>
              </>
            ) : null}
          </Nav>

          <Nav className="ms-auto align-items-center">
            {/* Shift Status - Only for Manager and Cashier */}
            {user?.role !== 'Admin' && (
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
            )}

            {/* Cart - Only for Manager and Cashier */}
            {user?.role !== 'Admin' && (
              <Nav.Link 
                as={Link} 
                to="/cart" 
                className="position-relative me-3"
                active={isActive('/cart')}
              >
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
            )}

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
                {user?.role === 'Admin' && (
                  <Dropdown.Item as={Link} to="/theme">
                    <FaPalette className="me-2" />
                    Theme Settings
                  </Dropdown.Item>
                )}
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
