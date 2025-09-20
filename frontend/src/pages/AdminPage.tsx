import React from 'react';
import { Card, Row, Col, ListGroup } from 'react-bootstrap';
import { FaUsers, FaStore, FaCog, FaDatabase, FaShieldAlt } from 'react-icons/fa';
import { useAuth } from '@/contexts/AuthContext';
import { Navigate } from 'react-router-dom';

const AdminPage: React.FC = () => {
  const { user } = useAuth();

  // Only allow Admin and Manager roles
  if (user?.role !== 'Admin' && user?.role !== 'Manager') {
    return <Navigate to="/" replace />;
  }

  return (
    <div>
      <h2 className="mb-4">Administration</h2>

      <Row className="g-4">
        <Col md={6} lg={4}>
          <Card className="h-100">
            <Card.Body>
              <div className="d-flex align-items-center mb-3">
                <FaUsers size={32} className="text-primary me-3" />
                <div>
                  <h5 className="mb-0">User Management</h5>
                  <small className="text-muted">Manage staff accounts</small>
                </div>
              </div>
              <ListGroup variant="flush">
                <ListGroup.Item action>Add New User</ListGroup.Item>
                <ListGroup.Item action>View All Users</ListGroup.Item>
                <ListGroup.Item action>Role Permissions</ListGroup.Item>
              </ListGroup>
            </Card.Body>
          </Card>
        </Col>

        <Col md={6} lg={4}>
          <Card className="h-100">
            <Card.Body>
              <div className="d-flex align-items-center mb-3">
                <FaStore size={32} className="text-success me-3" />
                <div>
                  <h5 className="mb-0">Store Settings</h5>
                  <small className="text-muted">Configure store details</small>
                </div>
              </div>
              <ListGroup variant="flush">
                <ListGroup.Item action>Store Information</ListGroup.Item>
                <ListGroup.Item action>Tax Settings</ListGroup.Item>
                <ListGroup.Item action>Operating Hours</ListGroup.Item>
              </ListGroup>
            </Card.Body>
          </Card>
        </Col>

        <Col md={6} lg={4}>
          <Card className="h-100">
            <Card.Body>
              <div className="d-flex align-items-center mb-3">
                <FaDatabase size={32} className="text-info me-3" />
                <div>
                  <h5 className="mb-0">Data Management</h5>
                  <small className="text-muted">Import/Export data</small>
                </div>
              </div>
              <ListGroup variant="flush">
                <ListGroup.Item action>Backup Database</ListGroup.Item>
                <ListGroup.Item action>Import Products</ListGroup.Item>
                <ListGroup.Item action>Export Reports</ListGroup.Item>
              </ListGroup>
            </Card.Body>
          </Card>
        </Col>

        <Col md={6} lg={4}>
          <Card className="h-100">
            <Card.Body>
              <div className="d-flex align-items-center mb-3">
                <FaCog size={32} className="text-warning me-3" />
                <div>
                  <h5 className="mb-0">System Settings</h5>
                  <small className="text-muted">Configure system</small>
                </div>
              </div>
              <ListGroup variant="flush">
                <ListGroup.Item action>General Settings</ListGroup.Item>
                <ListGroup.Item action>Receipt Template</ListGroup.Item>
                <ListGroup.Item action>Email Settings</ListGroup.Item>
              </ListGroup>
            </Card.Body>
          </Card>
        </Col>

        <Col md={6} lg={4}>
          <Card className="h-100">
            <Card.Body>
              <div className="d-flex align-items-center mb-3">
                <FaShieldAlt size={32} className="text-danger me-3" />
                <div>
                  <h5 className="mb-0">Security</h5>
                  <small className="text-muted">Security settings</small>
                </div>
              </div>
              <ListGroup variant="flush">
                <ListGroup.Item action>Audit Logs</ListGroup.Item>
                <ListGroup.Item action>Access Control</ListGroup.Item>
                <ListGroup.Item action>Password Policy</ListGroup.Item>
              </ListGroup>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {user?.role === 'Manager' && (
        <div className="alert alert-info mt-4">
          <strong>Note:</strong> As a Manager, you have limited administrative access. 
          Contact your system administrator for full access.
        </div>
      )}
    </div>
  );
};

export default AdminPage;
