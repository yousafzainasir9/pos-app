import React, { useState, useEffect } from 'react';
import {
  Container,
  Card,
  Form,
  Button,
  Row,
  Col,
  Alert,
  Spinner,
  Tabs,
  Tab,
  Badge
} from 'react-bootstrap';
import { FaStore, FaSave, FaClock, FaDollarSign } from 'react-icons/fa';
import { toast } from 'react-toastify';
import storeService, { StoreDetail, UpdateStoreDto } from '@/services/store.service';
import { useAuth } from '@/contexts/AuthContext';

const StoreSettingsPage: React.FC = () => {
  const { user } = useAuth();
  const [store, setStore] = useState<StoreDetail | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const [formData, setFormData] = useState<UpdateStoreDto>({
    name: '',
    address: '',
    city: '',
    state: '',
    postalCode: '',
    country: '',
    phone: '',
    email: '',
    taxNumber: '',
    taxRate: 0,
    currency: 'USD',
    openingTime: '',
    closingTime: ''
  });

  useEffect(() => {
    loadStore();
  }, []);

  const loadStore = async () => {
    setIsLoading(true);
    try {
      const response = await storeService.getCurrentStore();
      if (response.success && response.data) {
        const storeData = response.data;
        setStore(storeData);
        setFormData({
          name: storeData.name,
          address: storeData.address || '',
          city: storeData.city || '',
          state: storeData.state || '',
          postalCode: storeData.postalCode || '',
          country: storeData.country || '',
          phone: storeData.phone || '',
          email: storeData.email || '',
          taxNumber: storeData.taxNumber || '',
          taxRate: storeData.taxRate,
          currency: storeData.currency,
          openingTime: storeData.openingTime || '',
          closingTime: storeData.closingTime || ''
        });
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to load store information');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!store) {
      toast.error('Store not loaded');
      return;
    }

    setIsSaving(true);
    try {
      const response = await storeService.updateStore(store.id, formData);
      if (response.success) {
        toast.success('Store settings updated successfully');
        loadStore();
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to update store settings');
    } finally {
      setIsSaving(false);
    }
  };

  const handleInputChange = (field: keyof UpdateStoreDto, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  if (isLoading) {
    return (
      <Container fluid>
        <div className="text-center py-5">
          <Spinner animation="border" />
          <p className="mt-3">Loading store information...</p>
        </div>
      </Container>
    );
  }

  if (!store) {
    return (
      <Container fluid>
        <Alert variant="danger">
          Failed to load store information. Please try again.
        </Alert>
      </Container>
    );
  }

  const canEdit = user?.role === 'Admin';

  return (
    <Container fluid>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>
          <FaStore className="me-2" />
          Store Settings
        </h2>
        <div>
          <Badge bg="info" className="me-2">
            Store Code: {store.code}
          </Badge>
          {store.isActive ? (
            <Badge bg="success">Active</Badge>
          ) : (
            <Badge bg="secondary">Inactive</Badge>
          )}
        </div>
      </div>

      {!canEdit && (
        <Alert variant="warning" className="mb-4">
          You have read-only access. Only administrators can modify store settings.
        </Alert>
      )}

      <Form onSubmit={handleSubmit}>
        <Tabs defaultActiveKey="general" className="mb-4">
          {/* General Information Tab */}
          <Tab eventKey="general" title="General Information">
            <Card className="mb-4">
              <Card.Header>
                <h5 className="mb-0">Basic Information</h5>
              </Card.Header>
              <Card.Body>
                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Store Name *</Form.Label>
                      <Form.Control
                        type="text"
                        value={formData.name}
                        onChange={(e) => handleInputChange('name', e.target.value)}
                        required
                        disabled={!canEdit}
                      />
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Store Code</Form.Label>
                      <Form.Control
                        type="text"
                        value={store.code}
                        disabled
                      />
                      <Form.Text className="text-muted">
                        Store code cannot be changed
                      </Form.Text>
                    </Form.Group>
                  </Col>
                </Row>

                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Email</Form.Label>
                      <Form.Control
                        type="email"
                        value={formData.email}
                        onChange={(e) => handleInputChange('email', e.target.value)}
                        disabled={!canEdit}
                      />
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Phone</Form.Label>
                      <Form.Control
                        type="tel"
                        value={formData.phone}
                        onChange={(e) => handleInputChange('phone', e.target.value)}
                        disabled={!canEdit}
                      />
                    </Form.Group>
                  </Col>
                </Row>
              </Card.Body>
            </Card>

            <Card className="mb-4">
              <Card.Header>
                <h5 className="mb-0">Address</h5>
              </Card.Header>
              <Card.Body>
                <Form.Group className="mb-3">
                  <Form.Label>Street Address</Form.Label>
                  <Form.Control
                    type="text"
                    value={formData.address}
                    onChange={(e) => handleInputChange('address', e.target.value)}
                    disabled={!canEdit}
                  />
                </Form.Group>

                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>City</Form.Label>
                      <Form.Control
                        type="text"
                        value={formData.city}
                        onChange={(e) => handleInputChange('city', e.target.value)}
                        disabled={!canEdit}
                      />
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>State/Province</Form.Label>
                      <Form.Control
                        type="text"
                        value={formData.state}
                        onChange={(e) => handleInputChange('state', e.target.value)}
                        disabled={!canEdit}
                      />
                    </Form.Group>
                  </Col>
                </Row>

                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Postal Code</Form.Label>
                      <Form.Control
                        type="text"
                        value={formData.postalCode}
                        onChange={(e) => handleInputChange('postalCode', e.target.value)}
                        disabled={!canEdit}
                      />
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Country</Form.Label>
                      <Form.Control
                        type="text"
                        value={formData.country}
                        onChange={(e) => handleInputChange('country', e.target.value)}
                        disabled={!canEdit}
                      />
                    </Form.Group>
                  </Col>
                </Row>
              </Card.Body>
            </Card>
          </Tab>

          {/* Tax & Financial Tab */}
          <Tab eventKey="financial" title={<><FaDollarSign className="me-1" />Financial</>}>
            <Card className="mb-4">
              <Card.Header>
                <h5 className="mb-0">Tax Settings</h5>
              </Card.Header>
              <Card.Body>
                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Tax Number/VAT ID</Form.Label>
                      <Form.Control
                        type="text"
                        value={formData.taxNumber}
                        onChange={(e) => handleInputChange('taxNumber', e.target.value)}
                        disabled={!canEdit}
                      />
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Tax Rate (%)</Form.Label>
                      <Form.Control
                        type="number"
                        step="0.01"
                        min="0"
                        max="100"
                        value={formData.taxRate}
                        onChange={(e) => handleInputChange('taxRate', parseFloat(e.target.value))}
                        disabled={!canEdit}
                      />
                      <Form.Text className="text-muted">
                        Default tax rate applied to all sales
                      </Form.Text>
                    </Form.Group>
                  </Col>
                </Row>
              </Card.Body>
            </Card>

            <Card className="mb-4">
              <Card.Header>
                <h5 className="mb-0">Currency Settings</h5>
              </Card.Header>
              <Card.Body>
                <Form.Group className="mb-3">
                  <Form.Label>Currency</Form.Label>
                  <Form.Select
                    value={formData.currency}
                    onChange={(e) => handleInputChange('currency', e.target.value)}
                    disabled={!canEdit}
                  >
                    <option value="USD">USD - US Dollar</option>
                    <option value="EUR">EUR - Euro</option>
                    <option value="GBP">GBP - British Pound</option>
                    <option value="CAD">CAD - Canadian Dollar</option>
                    <option value="AUD">AUD - Australian Dollar</option>
                    <option value="JPY">JPY - Japanese Yen</option>
                    <option value="CNY">CNY - Chinese Yuan</option>
                    <option value="INR">INR - Indian Rupee</option>
                    <option value="PKR">PKR - Pakistani Rupee</option>
                  </Form.Select>
                  <Form.Text className="text-muted">
                    Currency used for all transactions
                  </Form.Text>
                </Form.Group>
              </Card.Body>
            </Card>
          </Tab>

          {/* Operating Hours Tab */}
          <Tab eventKey="hours" title={<><FaClock className="me-1" />Operating Hours</>}>
            <Card className="mb-4">
              <Card.Header>
                <h5 className="mb-0">Business Hours</h5>
              </Card.Header>
              <Card.Body>
                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Opening Time</Form.Label>
                      <Form.Control
                        type="time"
                        value={formData.openingTime}
                        onChange={(e) => handleInputChange('openingTime', e.target.value)}
                        disabled={!canEdit}
                      />
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Closing Time</Form.Label>
                      <Form.Control
                        type="time"
                        value={formData.closingTime}
                        onChange={(e) => handleInputChange('closingTime', e.target.value)}
                        disabled={!canEdit}
                      />
                    </Form.Group>
                  </Col>
                </Row>
                <Alert variant="info">
                  <small>
                    These hours are displayed on receipts and can be used for automated reports.
                    The system does not enforce these hours.
                  </small>
                </Alert>
              </Card.Body>
            </Card>
          </Tab>

          {/* Statistics Tab */}
          <Tab eventKey="stats" title="Statistics">
            <Card className="mb-4">
              <Card.Header>
                <h5 className="mb-0">Store Statistics</h5>
              </Card.Header>
              <Card.Body>
                <Row>
                  <Col md={4}>
                    <div className="text-center p-3 border rounded mb-3">
                      <h3 className="text-primary">{store.activeUserCount}</h3>
                      <p className="text-muted mb-0">Active Users</p>
                    </div>
                  </Col>
                  <Col md={4}>
                    <div className="text-center p-3 border rounded mb-3">
                      <h3 className="text-success">{store.isActive ? 'Active' : 'Inactive'}</h3>
                      <p className="text-muted mb-0">Store Status</p>
                    </div>
                  </Col>
                  <Col md={4}>
                    <div className="text-center p-3 border rounded mb-3">
                      <h3 className="text-info">{store.code}</h3>
                      <p className="text-muted mb-0">Store Code</p>
                    </div>
                  </Col>
                </Row>

                <hr />

                <h6 className="mb-3">Store Information</h6>
                <Row>
                  <Col md={6}>
                    <dl className="row">
                      <dt className="col-sm-4">Store ID:</dt>
                      <dd className="col-sm-8">{store.id}</dd>
                      
                      <dt className="col-sm-4">Created:</dt>
                      <dd className="col-sm-8">
                        {new Date().toLocaleDateString()}
                      </dd>
                      
                      <dt className="col-sm-4">Currency:</dt>
                      <dd className="col-sm-8">{store.currency}</dd>
                    </dl>
                  </Col>
                  <Col md={6}>
                    <dl className="row">
                      <dt className="col-sm-4">Tax Rate:</dt>
                      <dd className="col-sm-8">{(store.taxRate * 100).toFixed(2)}%</dd>
                      
                      <dt className="col-sm-4">Hours:</dt>
                      <dd className="col-sm-8">
                        {store.openingTime && store.closingTime
                          ? `${store.openingTime} - ${store.closingTime}`
                          : 'Not Set'}
                      </dd>
                      
                      <dt className="col-sm-4">Status:</dt>
                      <dd className="col-sm-8">
                        {store.isActive ? (
                          <Badge bg="success">Active</Badge>
                        ) : (
                          <Badge bg="secondary">Inactive</Badge>
                        )}
                      </dd>
                    </dl>
                  </Col>
                </Row>
              </Card.Body>
            </Card>
          </Tab>
        </Tabs>

        {canEdit && (
          <div className="d-flex justify-content-end gap-2 mb-4">
            <Button variant="secondary" onClick={loadStore} disabled={isSaving}>
              Reset
            </Button>
            <Button variant="primary" type="submit" disabled={isSaving}>
              {isSaving ? (
                <>
                  <Spinner animation="border" size="sm" className="me-2" />
                  Saving...
                </>
              ) : (
                <>
                  <FaSave className="me-2" />
                  Save Changes
                </>
              )}
            </Button>
          </div>
        )}
      </Form>
    </Container>
  );
};

export default StoreSettingsPage;
