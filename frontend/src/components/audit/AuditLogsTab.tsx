import React, { useState, useEffect } from 'react';
import { Table, Form, Row, Col, Button, Badge, Pagination, Spinner, Modal } from 'react-bootstrap';
import { FaSearch, FaDownload, FaEye, FaFilter, FaTimes } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { format } from 'date-fns';
import auditService, { AuditLog, AuditLogSearchRequest, PagedResult } from '../../services/audit.service';

const AuditLogsTab: React.FC = () => {
  const [logs, setLogs] = useState<PagedResult<AuditLog> | null>(null);
  const [loading, setLoading] = useState(false);
  const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [filters, setFilters] = useState<AuditLogSearchRequest>({
    page: 1,
    pageSize: 50
  });
  const [showFilters, setShowFilters] = useState(false);

  useEffect(() => {
    loadLogs();
  }, [filters.page]);

  const loadLogs = async () => {
    try {
      setLoading(true);
      const result = await auditService.getAuditLogs(filters);
      setLogs(result);
    } catch (error) {
      console.error('Failed to load audit logs:', error);
      toast.error('Failed to load audit logs');
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
      const blob = await auditService.exportAuditLogs(filters);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `audit-logs-${format(new Date(), 'yyyy-MM-dd-HHmmss')}.csv`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      toast.success('Audit logs exported successfully');
    } catch (error) {
      console.error('Failed to export:', error);
      toast.error('Failed to export audit logs');
    }
  };

  const viewDetails = async (id: number) => {
    try {
      const log = await auditService.getAuditLogById(id);
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
                <Form.Label className="small">Action</Form.Label>
                <Form.Control
                  type="text"
                  size="sm"
                  placeholder="e.g., CREATE, UPDATE, DELETE"
                  value={filters.action || ''}
                  onChange={(e) => setFilters({ ...filters, action: e.target.value })}
                />
              </Form.Group>
            </Col>
            <Col md={3}>
              <Form.Group className="mb-2">
                <Form.Label className="small">Entity</Form.Label>
                <Form.Control
                  type="text"
                  size="sm"
                  placeholder="e.g., Product, Order, User"
                  value={filters.entityName || ''}
                  onChange={(e) => setFilters({ ...filters, entityName: e.target.value })}
                />
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
          <p className="mt-2 text-muted">Loading audit logs...</p>
        </div>
      ) : logs && logs.items.length > 0 ? (
        <>
          <div className="table-responsive">
            <Table striped hover size="sm">
              <thead>
                <tr>
                  <th>Timestamp</th>
                  <th>User</th>
                  <th>Action</th>
                  <th>Entity</th>
                  <th>Entity ID</th>
                  <th>Store</th>
                  <th>IP Address</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {logs.items.map((log) => (
                  <tr key={log.id}>
                    <td className="small">
                      {format(new Date(log.timestamp), 'yyyy-MM-dd HH:mm:ss')}
                    </td>
                    <td className="small">{log.userName || '-'}</td>
                    <td>
                      <Badge bg={getActionBadgeColor(log.action)}>
                        {log.action}
                      </Badge>
                    </td>
                    <td className="small">{log.entityName}</td>
                    <td className="small">{log.entityId || '-'}</td>
                    <td className="small">{log.storeName || '-'}</td>
                    <td className="small">{log.ipAddress || '-'}</td>
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
          <p className="text-muted">No audit logs found</p>
        </div>
      )}

      {/* Detail Modal */}
      <Modal show={showDetailModal} onHide={() => setShowDetailModal(false)} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>Audit Log Details</Modal.Title>
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
                  <strong>User:</strong><br />
                  {selectedLog.userName || 'N/A'}
                </Col>
              </Row>
              <Row className="mb-3">
                <Col md={6}>
                  <strong>Action:</strong><br />
                  <Badge bg={getActionBadgeColor(selectedLog.action)}>
                    {selectedLog.action}
                  </Badge>
                </Col>
                <Col md={6}>
                  <strong>Entity:</strong><br />
                  {selectedLog.entityName} {selectedLog.entityId && `#${selectedLog.entityId}`}
                </Col>
              </Row>
              <Row className="mb-3">
                <Col md={6}>
                  <strong>Store:</strong><br />
                  {selectedLog.storeName || 'N/A'}
                </Col>
                <Col md={6}>
                  <strong>IP Address:</strong><br />
                  {selectedLog.ipAddress || 'N/A'}
                </Col>
              </Row>
              {selectedLog.details && (
                <Row className="mb-3">
                  <Col md={12}>
                    <strong>Details:</strong><br />
                    <div className="bg-light p-2 rounded small">
                      {selectedLog.details}
                    </div>
                  </Col>
                </Row>
              )}
              {selectedLog.oldValues && (
                <Row className="mb-3">
                  <Col md={12}>
                    <strong>Old Values:</strong><br />
                    <pre className="bg-light p-2 rounded small">
                      {JSON.stringify(JSON.parse(selectedLog.oldValues), null, 2)}
                    </pre>
                  </Col>
                </Row>
              )}
              {selectedLog.newValues && (
                <Row className="mb-3">
                  <Col md={12}>
                    <strong>New Values:</strong><br />
                    <pre className="bg-light p-2 rounded small">
                      {JSON.stringify(JSON.parse(selectedLog.newValues), null, 2)}
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

const getActionBadgeColor = (action: string): string => {
  switch (action.toUpperCase()) {
    case 'CREATE':
    case 'INSERT':
      return 'success';
    case 'UPDATE':
    case 'MODIFY':
      return 'primary';
    case 'DELETE':
    case 'REMOVE':
      return 'danger';
    default:
      return 'secondary';
  }
};

export default AuditLogsTab;
