import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Nav, Tab, Badge, Button, Alert } from 'react-bootstrap';
import { FaShieldAlt, FaHistory, FaChartLine, FaExclamationTriangle, FaDownload } from 'react-icons/fa';
import { toast } from 'react-toastify';
import auditService, { AuditStatistics } from '../services/audit.service';
import AuditLogsTab from '../components/audit/AuditLogsTab';
import SecurityLogsTab from '../components/audit/SecurityLogsTab';
import SecurityDashboard from '../components/audit/SecurityDashboard';

const SecurityAuditPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('dashboard');
  const [statistics, setStatistics] = useState<AuditStatistics | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadStatistics();
  }, []);

  const loadStatistics = async () => {
    try {
      setLoading(true);
      const stats = await auditService.getStatistics(30);
      setStatistics(stats);
    } catch (error) {
      console.error('Failed to load statistics:', error);
      toast.error('Failed to load statistics');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container fluid className="py-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="mb-1">
            <FaShieldAlt className="me-2" />
            Security & Audit
          </h2>
          <p className="text-muted mb-0">Monitor system activity and security events</p>
        </div>
      </div>

      {/* Alert Section */}
      {statistics && (
        <Row className="mb-4">
          {statistics.failedLoginAttempts24h > 10 && (
            <Col md={12}>
              <Alert variant="warning" className="d-flex align-items-center">
                <FaExclamationTriangle className="me-2" size={20} />
                <div>
                  <strong>High Failed Login Attempts:</strong> {statistics.failedLoginAttempts24h} failed login attempts in the last 24 hours
                </div>
              </Alert>
            </Col>
          )}
          {statistics.criticalEvents7d > 0 && (
            <Col md={12}>
              <Alert variant="danger" className="d-flex align-items-center">
                <FaExclamationTriangle className="me-2" size={20} />
                <div>
                  <strong>Critical Security Events:</strong> {statistics.criticalEvents7d} critical security events in the last 7 days
                </div>
              </Alert>
            </Col>
          )}
        </Row>
      )}

      {/* Quick Stats */}
      {statistics && (
        <Row className="mb-4">
          <Col md={3}>
            <Card className="border-0 shadow-sm">
              <Card.Body>
                <div className="d-flex justify-content-between align-items-center">
                  <div>
                    <p className="text-muted mb-1 small">Total Audit Logs</p>
                    <h4 className="mb-0">{statistics.totalAuditLogs.toLocaleString()}</h4>
                  </div>
                  <div className="bg-primary bg-opacity-10 p-3 rounded">
                    <FaHistory size={24} className="text-primary" />
                  </div>
                </div>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="border-0 shadow-sm">
              <Card.Body>
                <div className="d-flex justify-content-between align-items-center">
                  <div>
                    <p className="text-muted mb-1 small">Security Logs</p>
                    <h4 className="mb-0">{statistics.totalSecurityLogs.toLocaleString()}</h4>
                  </div>
                  <div className="bg-success bg-opacity-10 p-3 rounded">
                    <FaShieldAlt size={24} className="text-success" />
                  </div>
                </div>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="border-0 shadow-sm">
              <Card.Body>
                <div className="d-flex justify-content-between align-items-center">
                  <div>
                    <p className="text-muted mb-1 small">Failed Logins (24h)</p>
                    <h4 className="mb-0">
                      {statistics.failedLoginAttempts24h}
                      {statistics.failedLoginAttempts24h > 10 && (
                        <Badge bg="warning" className="ms-2">High</Badge>
                      )}
                    </h4>
                  </div>
                  <div className="bg-warning bg-opacity-10 p-3 rounded">
                    <FaExclamationTriangle size={24} className="text-warning" />
                  </div>
                </div>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="border-0 shadow-sm">
              <Card.Body>
                <div className="d-flex justify-content-between align-items-center">
                  <div>
                    <p className="text-muted mb-1 small">Critical Events (7d)</p>
                    <h4 className="mb-0">
                      {statistics.criticalEvents7d}
                      {statistics.criticalEvents7d > 0 && (
                        <Badge bg="danger" className="ms-2">!</Badge>
                      )}
                    </h4>
                  </div>
                  <div className="bg-danger bg-opacity-10 p-3 rounded">
                    <FaExclamationTriangle size={24} className="text-danger" />
                  </div>
                </div>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}

      {/* Tabs */}
      <Card className="border-0 shadow-sm">
        <Card.Body>
          <Tab.Container activeKey={activeTab} onSelect={(k) => setActiveTab(k || 'dashboard')}>
            <Nav variant="tabs" className="mb-4">
              <Nav.Item>
                <Nav.Link eventKey="dashboard">
                  <FaChartLine className="me-2" />
                  Dashboard
                </Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="audit-logs">
                  <FaHistory className="me-2" />
                  Audit Logs
                </Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="security-logs">
                  <FaShieldAlt className="me-2" />
                  Security Logs
                </Nav.Link>
              </Nav.Item>
            </Nav>

            <Tab.Content>
              <Tab.Pane eventKey="dashboard">
                <SecurityDashboard statistics={statistics} onRefresh={loadStatistics} />
              </Tab.Pane>
              <Tab.Pane eventKey="audit-logs">
                <AuditLogsTab />
              </Tab.Pane>
              <Tab.Pane eventKey="security-logs">
                <SecurityLogsTab />
              </Tab.Pane>
            </Tab.Content>
          </Tab.Container>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default SecurityAuditPage;
