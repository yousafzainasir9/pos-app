import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Tab, Nav, Table, Form, Badge, Alert, ProgressBar, Modal } from 'react-bootstrap';
import { FaDatabase, FaDownload, FaUpload, FaArchive, FaTrash, FaPlus, FaFileExport } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { format } from 'date-fns';
import dataManagementService, { BackupDto, DataStatisticsDto, ImportResultDto } from '../services/dataManagement.service';

const DataManagementPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview');
  const [statistics, setStatistics] = useState<DataStatisticsDto | null>(null);
  const [backups, setBackups] = useState<BackupDto[]>([]);
  const [archives, setArchives] = useState<BackupDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [showBackupModal, setShowBackupModal] = useState(false);
  const [showImportModal, setShowImportModal] = useState(false);
  const [importType, setImportType] = useState<'products' | 'customers'>('products');
  
  // Backup form
  const [backupName, setBackupName] = useState('');
  const [includeAuditLogs, setIncludeAuditLogs] = useState(true);
  const [includeSecurityLogs, setIncludeSecurityLogs] = useState(true);

  // Import form
  const [importFile, setImportFile] = useState<File | null>(null);
  const [updateExisting, setUpdateExisting] = useState(false);
  const [importResult, setImportResult] = useState<ImportResultDto | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [stats, backupsList, archivesList] = await Promise.all([
        dataManagementService.getStatistics(),
        dataManagementService.getBackups(),
        dataManagementService.getArchives()
      ]);
      
      setStatistics(stats);
      setBackups(backupsList);
      setArchives(archivesList);
    } catch (error) {
      console.error('Failed to load data:', error);
      toast.error('Failed to load data management information');
    }
  };

  const handleCreateBackup = async () => {
    try {
      setLoading(true);
      const backup = await dataManagementService.createBackup({
        backupName,
        includeAuditLogs,
        includeSecurityLogs
      });
      
      toast.success(`Backup created: ${backup.fileName}`);
      setShowBackupModal(false);
      setBackupName('');
      await loadData();
    } catch (error) {
      console.error('Failed to create backup:', error);
      toast.error('Failed to create backup');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteBackup = async (fileName: string) => {
    if (!confirm(`Delete backup ${fileName}?`)) return;
    
    try {
      await dataManagementService.deleteBackup(fileName);
      toast.success('Backup deleted');
      await loadData();
    } catch (error) {
      console.error('Failed to delete backup:', error);
      toast.error('Failed to delete backup');
    }
  };

  const handleImport = async () => {
    if (!importFile) {
      toast.error('Please select a file');
      return;
    }

    try {
      setLoading(true);
      const fileContent = await importFile.text();
      const fileType = importFile.name.endsWith('.json') ? 'json' : 'csv';

      let result: ImportResultDto;
      if (importType === 'products') {
        result = await dataManagementService.importProducts(fileContent, fileType, updateExisting);
      } else {
        result = await dataManagementService.importCustomers(fileContent, fileType, updateExisting);
      }

      setImportResult(result);
      toast.success(`Import completed: ${result.successfulImports} successful, ${result.failedImports} failed`);
    } catch (error) {
      console.error('Failed to import:', error);
      toast.error('Failed to import data');
    } finally {
      setLoading(false);
    }
  };

  const handleExport = async (entityType: string, format: string) => {
    try {
      const blob = await dataManagementService.exportData({
        entityType,
        format
      });

      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${entityType}_${format}_${format(new Date(), 'yyyyMMdd_HHmmss')}.${format}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      
      toast.success('Export completed');
    } catch (error) {
      console.error('Failed to export:', error);
      toast.error('Failed to export data');
    }
  };

  return (
    <Container fluid className="py-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="mb-1">
            <FaDatabase className="me-2" />
            Data Management
          </h2>
          <p className="text-muted mb-0">Backup, import, export, and archive your data</p>
        </div>
      </div>

      {/* Statistics Cards */}
      {statistics && (
        <Row className="mb-4">
          <Col md={3}>
            <Card className="border-0 shadow-sm">
              <Card.Body>
                <h6 className="text-muted small">Total Products</h6>
                <h3>{statistics.totalProducts.toLocaleString()}</h3>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="border-0 shadow-sm">
              <Card.Body>
                <h6 className="text-muted small">Total Customers</h6>
                <h3>{statistics.totalCustomers.toLocaleString()}</h3>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="border-0 shadow-sm">
              <Card.Body>
                <h6 className="text-muted small">Total Orders</h6>
                <h3>{statistics.totalOrders.toLocaleString()}</h3>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="border-0 shadow-sm">
              <Card.Body>
                <h6 className="text-muted small">Backups</h6>
                <h3>{statistics.backupCount}</h3>
                <small className="text-muted">
                  Last: {statistics.lastBackupDate !== '0001-01-01T00:00:00' 
                    ? format(new Date(statistics.lastBackupDate), 'PPp')
                    : 'Never'}
                </small>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}

      {/* Tabs */}
      <Card className="border-0 shadow-sm">
        <Card.Body>
          <Tab.Container activeKey={activeTab} onSelect={(k) => setActiveTab(k || 'overview')}>
            <Nav variant="tabs" className="mb-4">
              <Nav.Item>
                <Nav.Link eventKey="overview">Overview</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="backup">Backup & Restore</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="import">Import Data</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="export">Export Data</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="archive">Archives</Nav.Link>
              </Nav.Item>
            </Nav>

            <Tab.Content>
              {/* Overview Tab */}
              <Tab.Pane eventKey="overview">
                <Row>
                  <Col md={6}>
                    <h5>Quick Actions</h5>
                    <div className="d-grid gap-2">
                      <Button variant="primary" onClick={() => setShowBackupModal(true)}>
                        <FaDatabase className="me-2" />
                        Create Backup
                      </Button>
                      <Button variant="success" onClick={() => setShowImportModal(true)}>
                        <FaUpload className="me-2" />
                        Import Data
                      </Button>
                      <Button variant="info" onClick={() => handleExport('products', 'csv')}>
                        <FaDownload className="me-2" />
                        Export Products
                      </Button>
                    </div>
                  </Col>
                  <Col md={6}>
                    <h5>Database Health</h5>
                    <Alert variant="success">
                      <strong>System Status:</strong> Healthy
                    </Alert>
                    <div className="mb-2">
                      <small className="text-muted">Database Size:</small>
                      <ProgressBar now={30} label="30%" />
                    </div>
                  </Col>
                </Row>
              </Tab.Pane>

              {/* Backup Tab */}
              <Tab.Pane eventKey="backup">
                <div className="d-flex justify-content-between mb-3">
                  <h5>Database Backups</h5>
                  <Button variant="primary" onClick={() => setShowBackupModal(true)}>
                    <FaPlus className="me-2" />
                    Create Backup
                  </Button>
                </div>
                
                <Table striped hover>
                  <thead>
                    <tr>
                      <th>File Name</th>
                      <th>Created</th>
                      <th>Size</th>
                      <th>Type</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {backups.map((backup) => (
                      <tr key={backup.fileName}>
                        <td>{backup.fileName}</td>
                        <td>{format(new Date(backup.createdAt), 'PPp')}</td>
                        <td>{backup.sizeFormatted}</td>
                        <td><Badge bg="primary">{backup.backupType}</Badge></td>
                        <td>
                          <Button
                            variant="outline-danger"
                            size="sm"
                            onClick={() => handleDeleteBackup(backup.fileName)}
                          >
                            <FaTrash />
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </Tab.Pane>

              {/* Import Tab */}
              <Tab.Pane eventKey="import">
                <h5>Import Data</h5>
                <Form>
                  <Form.Group className="mb-3">
                    <Form.Label>Import Type</Form.Label>
                    <Form.Select 
                      value={importType} 
                      onChange={(e) => setImportType(e.target.value as any)}
                    >
                      <option value="products">Products</option>
                      <option value="customers">Customers</option>
                    </Form.Select>
                  </Form.Group>

                  <Form.Group className="mb-3">
                    <Form.Label>File (CSV or JSON)</Form.Label>
                    <Form.Control
                      type="file"
                      accept=".csv,.json"
                      onChange={(e: any) => setImportFile(e.target.files?.[0] || null)}
                    />
                  </Form.Group>

                  <Form.Group className="mb-3">
                    <Form.Check
                      type="checkbox"
                      label="Update existing records"
                      checked={updateExisting}
                      onChange={(e) => setUpdateExisting(e.target.checked)}
                    />
                  </Form.Group>

                  <Button variant="primary" onClick={handleImport} disabled={!importFile || loading}>
                    <FaUpload className="me-2" />
                    {loading ? 'Importing...' : 'Import'}
                  </Button>
                </Form>

                {importResult && (
                  <Alert variant="info" className="mt-3">
                    <h6>Import Results:</h6>
                    <p>Total: {importResult.totalRecords}</p>
                    <p className="text-success">Successful: {importResult.successfulImports}</p>
                    <p className="text-warning">Updated: {importResult.updatedRecords}</p>
                    <p className="text-danger">Failed: {importResult.failedImports}</p>
                    {importResult.errors.length > 0 && (
                      <div>
                        <strong>Errors:</strong>
                        <ul>
                          {importResult.errors.slice(0, 5).map((err, i) => <li key={i}>{err}</li>)}
                        </ul>
                      </div>
                    )}
                  </Alert>
                )}
              </Tab.Pane>

              {/* Export Tab */}
              <Tab.Pane eventKey="export">
                <h5>Export Data</h5>
                <Row>
                  <Col md={4}>
                    <Card className="mb-3">
                      <Card.Body>
                        <h6>Products</h6>
                        <div className="d-grid gap-2">
                          <Button size="sm" onClick={() => handleExport('products', 'csv')}>
                            <FaFileExport className="me-2" />
                            Export as CSV
                          </Button>
                          <Button size="sm" onClick={() => handleExport('products', 'json')}>
                            <FaFileExport className="me-2" />
                            Export as JSON
                          </Button>
                        </div>
                      </Card.Body>
                    </Card>
                  </Col>
                  <Col md={4}>
                    <Card className="mb-3">
                      <Card.Body>
                        <h6>Customers</h6>
                        <div className="d-grid gap-2">
                          <Button size="sm" onClick={() => handleExport('customers', 'csv')}>
                            <FaFileExport className="me-2" />
                            Export as CSV
                          </Button>
                          <Button size="sm" onClick={() => handleExport('customers', 'json')}>
                            <FaFileExport className="me-2" />
                            Export as JSON
                          </Button>
                        </div>
                      </Card.Body>
                    </Card>
                  </Col>
                  <Col md={4}>
                    <Card className="mb-3">
                      <Card.Body>
                        <h6>Orders</h6>
                        <div className="d-grid gap-2">
                          <Button size="sm" onClick={() => handleExport('orders', 'csv')}>
                            <FaFileExport className="me-2" />
                            Export as CSV
                          </Button>
                          <Button size="sm" onClick={() => handleExport('orders', 'json')}>
                            <FaFileExport className="me-2" />
                            Export as JSON
                          </Button>
                        </div>
                      </Card.Body>
                    </Card>
                  </Col>
                </Row>
              </Tab.Pane>

              {/* Archives Tab */}
              <Tab.Pane eventKey="archive">
                <h5>Data Archives</h5>
                <Table striped hover>
                  <thead>
                    <tr>
                      <th>File Name</th>
                      <th>Created</th>
                      <th>Size</th>
                      <th>Type</th>
                    </tr>
                  </thead>
                  <tbody>
                    {archives.map((archive) => (
                      <tr key={archive.fileName}>
                        <td>{archive.fileName}</td>
                        <td>{format(new Date(archive.createdAt), 'PPp')}</td>
                        <td>{archive.sizeFormatted}</td>
                        <td><Badge bg="secondary">{archive.backupType}</Badge></td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </Tab.Pane>
            </Tab.Content>
          </Tab.Container>
        </Card.Body>
      </Card>

      {/* Create Backup Modal */}
      <Modal show={showBackupModal} onHide={() => setShowBackupModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Create Database Backup</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Backup Name (Optional)</Form.Label>
              <Form.Control
                type="text"
                placeholder="Leave empty for auto-generated name"
                value={backupName}
                onChange={(e) => setBackupName(e.target.value)}
              />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Check
                type="checkbox"
                label="Include Audit Logs"
                checked={includeAuditLogs}
                onChange={(e) => setIncludeAuditLogs(e.target.checked)}
              />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Check
                type="checkbox"
                label="Include Security Logs"
                checked={includeSecurityLogs}
                onChange={(e) => setIncludeSecurityLogs(e.target.checked)}
              />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowBackupModal(false)}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleCreateBackup} disabled={loading}>
            {loading ? 'Creating...' : 'Create Backup'}
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default DataManagementPage;
