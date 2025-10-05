import React, { useState, useEffect } from 'react';
import { Table, Form, Row, Col, Button, Badge, Pagination, Spinner, Modal } from 'react-bootstrap';
import { FaSearch, FaDownload, FaEye, FaFilter, FaTimes } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { format } from 'date-fns';
import auditService, { SecurityLog, SecurityLogSearchRequest, PagedResult } from '../../services/audit.service';

const SecurityLogsTab: React.FC = () => {
  const [logs, setLogs] = useState<PagedResult<SecurityLog> | null>(null);
  const [loading, setLoading] = useState(false);
  const [selectedLog, setSelectedLog] = useState<SecurityLog | null>(null);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [eventTypes, setEventTypes] = useState<Record<number, string>>({});
  const [severityLevels, setSeverityLevels] = useState<Record<number, string>>({});
  const [filters, setFilters] = useState<SecurityLogSearchRequest>({
    page: 1,
    pageSize: 50
  });
  const [showFilters, setShowFilters] = useState(false);

  useEffect(() => {
    loadEnums();
    loadLogs();
  }, [filters.page]);

  const loadEnums = async () => {
    try {
      const [types, levels] = await Promise.all([
        auditService.getSecurityEventTypes(),
        auditService.getSecuritySeverityLevels()
      ]);
      setEventTypes(types);
      setSeverityLevels(levels);
    } catch (error) {
      console.error('Failed to load enums:', error);
    }
  };

  const loadLogs = async () => {
    try {
      setLoading(true);
      const result = await auditService.getSecurityLogs(filters);
      setLogs(result);
    } catch (error) {
      console.error('Failed to load security logs:', error);
      toast.error('Failed to load security logs');
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = () => {
    setFilters({ ...filters, page: 1 });
    loadLogs();
  };

  const handleClearFilters = () => {
    setFilters({ page: 1, pageSize: 50 });
    setTimeout(() => loadLogs(), 100);
  };

  const handleExport = async () => {
    try {
      const blob = await auditService.exportSecurityLogs(filters);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `security-logs-${format(new Date(), 'yyyy-MM-dd-HHmmss')}.csv`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      toast.success('Security logs exported successfully');
    } catch (error) {
      console.error('Failed to export:', error);
      toast.error('Failed to export security logs');
    }
  };

  const viewDetails = async (id: number) => {
    try {
      const log = await auditService.getSecurityLogById(id);
      setSelectedLog(log);
      setShowDetailModal(true);
    } catch (error) {
      console.error('Failed to load log details:', error);
      toast.error('Failed to load log details');
    }
  };

  const renderPagination = () => {
    if (!logs || logs.totalPages <= 1) return null;

    const items = [];
    const maxPages = 5;
    let startPage = Math.max(1, logs.page - Math.floor(maxPages / 2));
    let endPage = Math.min(logs.totalPages, startPage + maxPages - 1);

    if (endPage - startPage < maxPages - 1) {
      startPage = Math.max(1, endPage - maxPages + 1);
    }

    items.push(
      <Pagination.First
        key="first"
        disabled={!logs.hasPrevious}
        onClick={() => setFilters({ ...filters, page: 1 })}
      />
    );
    items.push(
      <Pagination.Prev
        key="prev"
        disabled={!logs.hasPrevious}
        onClick={() => setFilters({ ...filters, page: logs.page - 1 })}
      />
    );

    for (let page = startPage; page <= endPage; page++) {
      items.push(
        <Pagination.Item
          key={page}
          active={page === logs.page}
          onClick={() => setFilters({ ...filters, page })}
        >
          {page}
        </Pagination.Item>
      );
    }

    items.push(
      <Pagination.Next
        key="next"
        disabled={!logs.hasNext}
        onClick={() => setFilters({ ...filters, page: logs.page + 1 })}
      />
    );
    items.push(
      <Pagination.Last
        key="last"
        disabled={!logs.hasNext}
        onClick={() => setFilters({ ...filters, page: logs.totalPages })}
      />
    );

    return <Pagination className="mb-0">{items}</Pagination>;
  };

  return (
    <>
      {/* Filters */}
      <div className="mb-3">
        <Button
          variant="outline-secondary"
          size="sm"
          onClick={() => setShowFilters(!showFilters)}
          className="me-2"
        >
          <FaFilter className="me-1" />
          {showFilters ? 'Hide' : 'Show'} Filters
        </Button>
        <Button variant="outline-primary" size="sm" onClick={handleExport}>
          <FaDownload className="me-1" />
          Export CSV
        </Button>
      </div>

      {showFilters && (
        <div className="bg-light p-3 rounded mb-3">
          <Row>
            <Col md={3}>
              <Form.Group className="mb-2">
                <Form.Label className="small">Start Date</Form.Label>
                <Form.Control
                  type="datetime-local"
                  size="sm"
                  value={filters.startDate || ''}
                  onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
                />
              </Form.Group>
            </Col>
            <Col md={3}>
              <Form.Group className="mb-2">
                <Form.Label className="small">End Date</Form.Label>
                <Form.Control
                  type="datetime-local"
                  size="sm"
                  value={filters.endDate || ''}
                  onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
                />
              </Form.Group>
            </Col>
            <Col md={3}>
              <Form.Group className="mb-2">
                <Form.Label className="small">Event Type</Form.Label>
                <Form.Select
                  size="sm"
                  value={filters.eventType || ''}
                  onChange={(e) => setFilters({ ...filters, eventType: e.target.value ? parseInt(e.target.value) : undefined })}
                >
                  <option value="">All Types</option>
                  {Object.entries(eventTypes).map(([key, value]) => (
                    <option key={key} value={key}>{value}</option>
                  ))}
                </Form.Select>
              </Form.Group>
            </Col>
            <Col md={3}>
              <Form.Group className="mb-2">
                <Form.Label className="small">Severity</Form.Label>
                <Form.Select
                  size="sm"
                  value={filters.severity || ''}
                  onChange={(e) => setFilters({ ...filters, severity: e.target.value ? parseInt(e.target.value) : undefined })}
                >
                  <option value="">All Severities</option>
                  {Object.entries(severityLevels).map(([key, value]) => (
                    <option key={key} value={key}>{value}</option>
                  ))}
                </Form.Select>
              </Form.Group>
            </Col>
          </Row>
          <Row>
            <Col md={3}>
              <Form.Group className="mb-2">
                <Form.Label className="small">Success</Form.Label>
                <Form.Select
                  size="sm"
                  value={filters.success !== undefined ? filters.success.toString() : ''}
                  onChange={(e) => setFilters({ ...filters, success: e.target.value === '' ? undefined : e.target.value === 'true' })}
                >
                  <option value="">All</option>
                  <option value="true">Success</option>
                  <option value="false">Failed</option>
                </Form.Select>
              </Form.Group>
            </Col>
          </Row>
          <div className="mt-2">
            <Button variant="primary" size="sm" onClick={handleSearch} className="me-2">
              <FaSearch className="me-1" />
              Search
            </Button>
            <Button variant="outline-secondary" size="sm" onClick={handleClearFilters}>
              <FaTimes className="me-1" />
              Clear Filters
            </Button>
          </div>
        </div>
      )}

      {/* Table */}
      {loading ? (
        <div className="text-center py-5">
          <Spinner animation="border" variant="primary" />
          <p className="mt-2 text-muted">Loading security logs...</p>
        </div>
      ) : logs && logs.items.length > 0 ? (
        <>
          <div className="table-responsive">
            <Table striped hover size="sm">
              <thead>
                <tr>
                  <th>Timestamp</th>
                  <th>Event Type</th>
                  <th>Severity</th>
                  <th>User</th>
                  <th>Description</th>
                  <th>IP Address</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {logs.items.map((log) => (
                  <tr key={log.id}>
                    <td className="small">
                      {format(new Date(log.timestamp), 'yyyy-MM-dd HH:mm:ss')}
                    </td>
                    <td>
                      <Badge bg={getEventTypeBadgeColor(log.eventTypeName)}>
                        {log.eventTypeName}
                      </Badge>
                    </td>
                    <td>
                      <Badge bg={getSeverityBadgeColor(log.severityName)}>
                        {log.severityName}
                      </Badge>
                    </td>
                    <td className="small">{log.userName || '-'}</td>
                    <td className="small">{log.description}</td>
                    <td className="small">{log.ipAddress || '-'}</td>
                    <td>
                      <Badge bg={log.success ? 'success' : 'danger'}>
                        {log.success ? 'Success' : 'Failed'}
                      </Badge>
                    </td>
                    <td>
                      <Button
                        variant="outline-primary"
                        size="sm"
                        onClick={() => viewDetails(log.id)}
                      >
                        <FaEye />
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
          </div>

          <div className="d-flex justify-content-between align-items-center mt-3">
            <div className="text-muted small">
              Showing {((logs.page - 1) * logs.pageSize) + 1} to{' '}
              {Math.min(logs.page * logs.pageSize, logs.totalCount)} of{' '}
              {logs.totalCount} entries
            </div>
            {renderPagination()}
          </div>
        </>
      ) : (
        <div className="text-center py-5">
          <p className="text-muted">No security logs found</p>
        </div>
      )}

      {/* Detail Modal */}
      <Modal show={showDetailModal} onHide={() => setShowDetailModal(false)} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>Security Log Details</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {selectedLog && (
            <div>
              <Row className="mb-3">
                <Col md={6}>
                  <strong>Timestamp:</strong><br />
                  {format(new Date(selectedLog.timestamp), 'PPpp')}
                </Col>
                <Col md={6}>
                  <strong>Event Type:</strong><br />
                  <Badge bg={getEventTypeBadgeColor(selectedLog.eventTypeName)}>
                    {selectedLog.eventTypeName}
                  </Badge>
                </Col>
              </Row>
              <Row className="mb-3">
                <Col md={6}>
                  <strong>Severity:</strong><br />
                  <Badge bg={getSeverityBadgeColor(selectedLog.severityName)}>
                    {selectedLog.severityName}
                  </Badge>
                </Col>
                <Col md={6}>
                  <strong>Status:</strong><br />
                  <Badge bg={selectedLog.success ? 'success' : 'danger'}>
                    {selectedLog.success ? 'Success' : 'Failed'}
                  </Badge>
                </Col>
              </Row>
              <Row className="mb-3">
                <Col md={6}>
                  <strong>User:</strong><br />
                  {selectedLog.userName || 'N/A'}
                </Col>
                <Col md={6}>
                  <strong>IP Address:</strong><br />
                  {selectedLog.ipAddress || 'N/A'}
                </Col>
              </Row>
              <Row className="mb-3">
                <Col md={12}>
                  <strong>Description:</strong><br />
                  <div className="bg-light p-2 rounded">
                    {selectedLog.description}
                  </div>
                </Col>
              </Row>
              {selectedLog.userAgent && (
                <Row className="mb-3">
                  <Col md={12}>
                    <strong>User Agent:</strong><br />
                    <div className="bg-light p-2 rounded small">
                      {selectedLog.userAgent}
                    </div>
                  </Col>
                </Row>
              )}
              {selectedLog.metadata && (
                <Row className="mb-3">
                  <Col md={12}>
                    <strong>Metadata:</strong><br />
                    <pre className="bg-light p-2 rounded small">
                      {JSON.stringify(JSON.parse(selectedLog.metadata), null, 2)}
                    </pre>
                  </Col>
                </Row>
              )}
            </div>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowDetailModal(false)}>
            Close
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

const getEventTypeBadgeColor = (eventType: string): string => {
  switch (eventType) {
    case 'Login':
      return 'success';
    case 'Logout':
      return 'info';
    case 'LoginFailed':
      return 'warning';
    case 'UnauthorizedAccess':
    case 'AccountLocked':
      return 'danger';
    case 'PasswordChanged':
    case 'PasswordReset':
      return 'primary';
    default:
      return 'secondary';
  }
};

const getSeverityBadgeColor = (severity: string): string => {
  switch (severity) {
    case 'Info':
      return 'info';
    case 'Warning':
      return 'warning';
    case 'Critical':
      return 'danger';
    case 'Emergency':
      return 'dark';
    default:
      return 'secondary';
  }
};

export default SecurityLogsTab;
