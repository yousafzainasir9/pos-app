import React from 'react';
import { 
  Container, 
  Card, 
  Row, 
  Col, 
  ListGroup, 
  Badge
} from 'react-bootstrap';
import { 
  FaUsers, 
  FaStore, 
  FaCog, 
  FaDatabase, 
  FaShieldAlt,
  FaCheckCircle,
  FaTimesCircle
} from 'react-icons/fa';
import { useAuth } from '@/contexts/AuthContext';
import { Navigate, useNavigate } from 'react-router-dom';

const AdminPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  // Only allow Admin and Manager roles
  if (user?.role !== 'Admin' && user?.role !== 'Manager') {
    return <Navigate to="/" replace />;
  }

  return (
    <Container fluid>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Administration</h2>
        <Badge bg="success" className="fs-6">
          <FaCheckCircle className="me-2" />
          Fully Functional
        </Badge>
      </div>

      <Row className="g-4">
        {/* User Management - WORKING */}
        <Col md={6} lg={4}>
          <Card 
            className="h-100 border-success" 
            style={{ cursor: 'pointer' }} 
            onClick={() => navigate('/admin/users')}
          >
            <Card.Body>
              <div className="d-flex align-items-center mb-3">
                <FaUsers size={32} className="text-primary me-3" />
                <div className="flex-grow-1">
                  <h5 className="mb-0">User Management</h5>
                  <small className="text-muted">Manage staff accounts</small>
                </div>
                <FaCheckCircle size={20} className="text-success" />
              </div>
              <ListGroup variant="flush">
                <ListGroup.Item action>
                  Add New User
                </ListGroup.Item>
                <ListGroup.Item action>
                  View All Users
                </ListGroup.Item>
                <ListGroup.Item action>
                  Edit User Details
                </ListGroup.Item>
                <ListGroup.Item action>
                  Reset Passwords & PINs
                </ListGroup.Item>
              </ListGroup>
              <div className="mt-3 text-center">
                <Badge bg="success">Fully Implemented</Badge>
              </div>
            </Card.Body>
          </Card>
        </Col>

        {/* Store Settings - WORKING */}
        <Col md={6} lg={4}>
          <Card 
            className="h-100 border-success" 
            style={{ cursor: 'pointer' }} 
            onClick={() => navigate('/admin/store-settings')}
          >
            <Card.Body>
              <div className="d-flex align-items-center mb-3">
                <FaStore size={32} className="text-success me-3" />
                <div className="flex-grow-1">
                  <h5 className="mb-0">Store Settings</h5>
                  <small className="text-muted">Configure store details</small>
                </div>
                <FaCheckCircle size={20} className="text-success" />
              </div>
              <ListGroup variant="flush">
                <ListGroup.Item action>Store Information</ListGroup.Item>
                <ListGroup.Item action>Tax Settings</ListGroup.Item>
                <ListGroup.Item action>Operating Hours</ListGroup.Item>
                <ListGroup.Item action>Currency & Format</ListGroup.Item>
              </ListGroup>
              <div className="mt-3 text-center">
                <Badge bg="success">Fully Implemented</Badge>
              </div>
            </Card.Body>
          </Card>
        </Col>

        {/* Theme Settings - WORKING */}
        <Col md={6} lg={4}>
          <Card 
            className="h-100 border-success" 
            style={{ cursor: 'pointer' }} 
            onClick={() => navigate('/theme-settings')}
          >
            <Card.Body>
              <div className="d-flex align-items-center mb-3">
                <FaCog size={32} className="text-primary me-3" />
                <div className="flex-grow-1">
                  <h5 className="mb-0">Theme Settings</h5>
                  <small className="text-muted">Customize appearance</small>
                </div>
                <FaCheckCircle size={20} className="text-success" />
              </div>
              <ListGroup variant="flush">
                <ListGroup.Item action>
                  Customize Colors
                </ListGroup.Item>
                <ListGroup.Item action>
                  Layout Options
                </ListGroup.Item>
                <ListGroup.Item action>
                  Font Settings
                </ListGroup.Item>
              </ListGroup>
              <div className="mt-3 text-center">
                <Badge bg="success">Fully Implemented</Badge>
              </div>
            </Card.Body>
          </Card>
        </Col>

        {/* Data Management - Coming Soon */}
        <Col md={6} lg={4}>
          <Card className="h-100 border-warning opacity-75">
            <Card.Body>
              <div className="d-flex align-items-center mb-3">
                <FaDatabase size={32} className="text-info me-3" />
                <div className="flex-grow-1">
                  <h5 className="mb-0">Data Management</h5>
                  <small className="text-muted">Import/Export data</small>
                </div>
                <FaTimesCircle size={20} className="text-warning" />
              </div>
              <ListGroup variant="flush">
                <ListGroup.Item>Backup Database</ListGroup.Item>
                <ListGroup.Item>Import Products</ListGroup.Item>
                <ListGroup.Item>Export Reports</ListGroup.Item>
                <ListGroup.Item>Data Archiving</ListGroup.Item>
              </ListGroup>
              <div className="mt-3 text-center">
                <Badge bg="warning">Coming Soon</Badge>
              </div>
            </Card.Body>
          </Card>
        </Col>

        {/* System Settings - Coming Soon */}
        <Col md={6} lg={4}>
          <Card className="h-100 border-warning opacity-75">
            <Card.Body>
              <div className="d-flex align-items-center mb-3">
                <FaCog size={32} className="text-warning me-3" />
                <div className="flex-grow-1">
                  <h5 className="mb-0">System Settings</h5>
                  <small className="text-muted">Configure system</small>
                </div>
                <FaTimesCircle size={20} className="text-warning" />
              </div>
              <ListGroup variant="flush">
                <ListGroup.Item>General Settings</ListGroup.Item>
                <ListGroup.Item>Receipt Template</ListGroup.Item>
                <ListGroup.Item>Email Settings</ListGroup.Item>
                <ListGroup.Item>Default Values</ListGroup.Item>
              </ListGroup>
              <div className="mt-3 text-center">
                <Badge bg="warning">Coming Soon</Badge>
              </div>
            </Card.Body>
          </Card>
        </Col>

        {/* Security & Audit - WORKING (Admin Only) */}
        {user?.role === 'Admin' && (
          <Col md={6} lg={4}>
            <Card 
              className="h-100 border-success" 
              style={{ cursor: 'pointer' }} 
              onClick={() => navigate('/admin/security')}
            >
              <Card.Body>
                <div className="d-flex align-items-center mb-3">
                  <FaShieldAlt size={32} className="text-danger me-3" />
                  <div className="flex-grow-1">
                    <h5 className="mb-0">Security & Audit</h5>
                    <small className="text-muted">Monitor system activity</small>
                  </div>
                  <FaCheckCircle size={20} className="text-success" />
                </div>
                <ListGroup variant="flush">
                  <ListGroup.Item action>Audit Logs</ListGroup.Item>
                  <ListGroup.Item action>Security Logs</ListGroup.Item>
                  <ListGroup.Item action>Analytics Dashboard</ListGroup.Item>
                  <ListGroup.Item action>Export Reports</ListGroup.Item>
                </ListGroup>
                <div className="mt-3 text-center">
                  <Badge bg="success">Fully Implemented</Badge>
                </div>
              </Card.Body>
            </Card>
          </Col>
        )}
      </Row>

      {user?.role === 'Manager' && (
        <div className="alert alert-info mt-4">
          <strong>Note:</strong> As a Manager, you have limited administrative access. 
          Contact your system administrator for full access to all features.
        </div>
      )}
    </Container>
  );
};

export default AdminPage;
