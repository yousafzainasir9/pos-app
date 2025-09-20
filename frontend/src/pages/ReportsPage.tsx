import React, { useState, useEffect } from 'react';
import {
  Container,
  Row,
  Col,
  Card,
  Button,
  Form,
  Table,
  Badge,
  Alert,
  Tabs,
  Tab,
  ProgressBar,
  Spinner,
  Modal
} from 'react-bootstrap';
import {
  FaChartBar,
  FaChartLine,
  FaChartPie,
  FaDownload,
  FaCalendarAlt,
  FaDollarSign,
  FaShoppingCart,
  FaBox,
  FaExclamationTriangle,
  FaSyncAlt,
  FaClock,
  FaFileExport,
  FaTimes
} from 'react-icons/fa';
import { format, startOfWeek, endOfWeek, startOfMonth, endOfMonth, subDays } from 'date-fns';
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import reportService, { SalesReportData, ProductPerformanceData, ShiftReportData } from '@/services/report.service';
import { toast } from 'react-toastify';
import { useAuth } from '@/contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import './ReportsPage.css';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];

const ReportsPage: React.FC = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [activeReport, setActiveReport] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [dateRange, setDateRange] = useState({
    startDate: format(startOfWeek(new Date()), 'yyyy-MM-dd'),
    endDate: format(endOfWeek(new Date()), 'yyyy-MM-dd')
  });

  // Report data states
  const [salesData, setSalesData] = useState<SalesReportData | null>(null);
  const [productData, setProductData] = useState<ProductPerformanceData | null>(null);
  const [shiftData, setShiftData] = useState<ShiftReportData | null>(null);

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
  }, [isAuthenticated, navigate]);

  const loadSalesReport = async () => {
    setIsLoading(true);
    try {
      const data = await reportService.getSalesReport(dateRange.startDate, dateRange.endDate);
      setSalesData(data);
    } catch (error) {
      console.error('Error loading sales report:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const loadProductPerformance = async () => {
    setIsLoading(true);
    try {
      const data = await reportService.getProductPerformance(dateRange.startDate, dateRange.endDate);
      setProductData(data);
    } catch (error) {
      console.error('Error loading product performance:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const loadShiftReport = async () => {
    setIsLoading(true);
    try {
      const data = await reportService.getShiftReport();
      setShiftData(data);
    } catch (error) {
      console.error('Error loading shift report:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleViewReport = async (reportType: string) => {
    setActiveReport(reportType);
    switch (reportType) {
      case 'sales':
        if (!salesData) await loadSalesReport();
        break;
      case 'products':
        if (!productData) await loadProductPerformance();
        break;
      case 'shifts':
        if (!shiftData) await loadShiftReport();
        break;
    }
  };

  const handleCloseModal = () => {
    setActiveReport(null);
  };

  const handleDateRangeChange = (preset?: string) => {
    const today = new Date();
    let start, end;

    switch (preset) {
      case 'today':
        start = end = format(today, 'yyyy-MM-dd');
        break;
      case 'yesterday':
        start = end = format(subDays(today, 1), 'yyyy-MM-dd');
        break;
      case 'thisWeek':
        start = format(startOfWeek(today), 'yyyy-MM-dd');
        end = format(endOfWeek(today), 'yyyy-MM-dd');
        break;
      case 'thisMonth':
        start = format(startOfMonth(today), 'yyyy-MM-dd');
        end = format(endOfMonth(today), 'yyyy-MM-dd');
        break;
      case 'last7Days':
        start = format(subDays(today, 7), 'yyyy-MM-dd');
        end = format(today, 'yyyy-MM-dd');
        break;
      case 'last30Days':
        start = format(subDays(today, 30), 'yyyy-MM-dd');
        end = format(today, 'yyyy-MM-dd');
        break;
      default:
        return;
    }

    setDateRange({ startDate: start, endDate: end });
  };

  const handleExportData = async (reportType: string) => {
    try {
      const blob = await reportService.exportReport(reportType, dateRange.startDate, dateRange.endDate);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${reportType}-report-${format(new Date(), 'yyyy-MM-dd')}.csv`;
      link.click();
      window.URL.revokeObjectURL(url);
      toast.success('Report exported successfully');
    } catch (error) {
      toast.error('Failed to export report');
    }
  };

  return (
    <Container fluid className="reports-page">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Reports & Analytics</h2>
        <Button 
          variant="primary" 
          onClick={() => handleExportData('sales')}
        >
          <FaFileExport className="me-2" />
          Export Data
        </Button>
      </div>

      {/* Report Cards */}
      <Row className="mb-4">
        <Col md={4}>
          <Card className="text-center h-100 report-card">
            <Card.Body className="d-flex flex-column">
              <div className="report-icon mb-3">
                <FaChartBar size={48} className="text-primary" />
              </div>
              <h4>Sales Report</h4>
              <p className="text-muted flex-grow-1">View daily, weekly, and monthly sales</p>
              <Button 
                variant="outline-primary" 
                onClick={() => handleViewReport('sales')}
              >
                View Report
              </Button>
            </Card.Body>
          </Card>
        </Col>
        <Col md={4}>
          <Card className="text-center h-100 report-card">
            <Card.Body className="d-flex flex-column">
              <div className="report-icon mb-3">
                <FaChartLine size={48} className="text-success" />
              </div>
              <h4>Product Performance</h4>
              <p className="text-muted flex-grow-1">Top selling products and categories</p>
              <Button 
                variant="outline-success" 
                onClick={() => handleViewReport('products')}
              >
                View Report
              </Button>
            </Card.Body>
          </Card>
        </Col>
        <Col md={4}>
          <Card className="text-center h-100 report-card">
            <Card.Body className="d-flex flex-column">
              <div className="report-icon mb-3">
                <FaChartPie size={48} className="text-info" />
              </div>
              <h4>Shift Reports</h4>
              <p className="text-muted flex-grow-1">Detailed shift summaries and reconciliation</p>
              <Button 
                variant="outline-info" 
                onClick={() => handleViewReport('shifts')}
              >
                View Report
              </Button>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Quick Stats Summary */}
      <Card className="mt-4">
        <Card.Body>
          <h5 className="mb-4">Today's Overview</h5>
          <Row>
            <Col md={3} sm={6} className="mb-3">
              <div className="text-center">
                <h3 className="text-primary">$4,250</h3>
                <p className="text-muted mb-0">Today's Sales</p>
              </div>
            </Col>
            <Col md={3} sm={6} className="mb-3">
              <div className="text-center">
                <h3 className="text-success">42</h3>
                <p className="text-muted mb-0">Orders Today</p>
              </div>
            </Col>
            <Col md={3} sm={6} className="mb-3">
              <div className="text-center">
                <h3 className="text-info">156</h3>
                <p className="text-muted mb-0">Products Sold</p>
              </div>
            </Col>
            <Col md={3} sm={6} className="mb-3">
              <div className="text-center">
                <h3 className="text-warning">8</h3>
                <p className="text-muted mb-0">Low Stock Items</p>
              </div>
            </Col>
          </Row>
          <hr />
          <Row className="mt-4">
            <Col md={6}>
              <h6 className="text-muted mb-3">Recent Activity</h6>
              <div className="activity-list">
                <div className="d-flex align-items-center mb-2">
                  <Badge bg="success" className="me-2">Sale</Badge>
                  <span className="text-muted small">Order #1234 completed - $85.50</span>
                  <span className="text-muted small ms-auto">2 min ago</span>
                </div>
                <div className="d-flex align-items-center mb-2">
                  <Badge bg="warning" className="me-2">Alert</Badge>
                  <span className="text-muted small">Low stock: Chocolate Chip Cookies</span>
                  <span className="text-muted small ms-auto">15 min ago</span>
                </div>
                <div className="d-flex align-items-center mb-2">
                  <Badge bg="info" className="me-2">Shift</Badge>
                  <span className="text-muted small">Morning shift started by John Doe</span>
                  <span className="text-muted small ms-auto">2 hours ago</span>
                </div>
              </div>
            </Col>
            <Col md={6}>
              <h6 className="text-muted mb-3">Performance Indicators</h6>
              <div className="mb-3">
                <div className="d-flex justify-content-between mb-1">
                  <small>Sales Target Progress</small>
                  <small>85%</small>
                </div>
                <ProgressBar now={85} variant="success" style={{ height: '8px' }} />
              </div>
              <div className="mb-3">
                <div className="d-flex justify-content-between mb-1">
                  <small>Inventory Health</small>
                  <small>72%</small>
                </div>
                <ProgressBar now={72} variant="info" style={{ height: '8px' }} />
              </div>
              <div className="mb-3">
                <div className="d-flex justify-content-between mb-1">
                  <small>Customer Satisfaction</small>
                  <small>94%</small>
                </div>
                <ProgressBar now={94} variant="primary" style={{ height: '8px' }} />
              </div>
            </Col>
          </Row>
        </Card.Body>
      </Card>

      {/* Sales Report Modal */}
      <Modal show={activeReport === 'sales'} onHide={handleCloseModal} size="xl">
        <Modal.Header>
          <Modal.Title>Sales Report</Modal.Title>
          <Button variant="link" className="text-dark ms-auto" onClick={handleCloseModal}>
            <FaTimes />
          </Button>
        </Modal.Header>
        <Modal.Body style={{ maxHeight: '80vh', overflowY: 'auto' }}>
          {/* Date Range Selector */}
          <Card className="mb-4">
            <Card.Body>
              <Row className="align-items-end">
                <Col md={3}>
                  <Form.Group>
                    <Form.Label>Start Date</Form.Label>
                    <Form.Control
                      type="date"
                      value={dateRange.startDate}
                      onChange={(e) => setDateRange({ ...dateRange, startDate: e.target.value })}
                    />
                  </Form.Group>
                </Col>
                <Col md={3}>
                  <Form.Group>
                    <Form.Label>End Date</Form.Label>
                    <Form.Control
                      type="date"
                      value={dateRange.endDate}
                      onChange={(e) => setDateRange({ ...dateRange, endDate: e.target.value })}
                    />
                  </Form.Group>
                </Col>
                <Col md={6}>
                  <div className="d-flex flex-wrap gap-2">
                    <Button size="sm" variant="outline-secondary" onClick={() => handleDateRangeChange('today')}>
                      Today
                    </Button>
                    <Button size="sm" variant="outline-secondary" onClick={() => handleDateRangeChange('thisWeek')}>
                      This Week
                    </Button>
                    <Button size="sm" variant="outline-secondary" onClick={() => handleDateRangeChange('thisMonth')}>
                      This Month
                    </Button>
                    <Button size="sm" variant="primary" onClick={loadSalesReport}>
                      Refresh
                    </Button>
                  </div>
                </Col>
              </Row>
            </Card.Body>
          </Card>

          {isLoading ? (
            <div className="text-center py-5">
              <Spinner animation="border" />
            </div>
          ) : salesData ? (
            <>
              {/* Sales Summary Cards */}
              <Row className="mb-4">
                <Col md={3}>
                  <Card className="text-center">
                    <Card.Body>
                      <FaDollarSign size={30} className="text-success mb-2" />
                      <h3>${salesData.totalSales.toFixed(2)}</h3>
                      <p className="text-muted mb-0">Total Sales</p>
                    </Card.Body>
                  </Card>
                </Col>
                <Col md={3}>
                  <Card className="text-center">
                    <Card.Body>
                      <FaShoppingCart size={30} className="text-primary mb-2" />
                      <h3>{salesData.totalOrders}</h3>
                      <p className="text-muted mb-0">Total Orders</p>
                    </Card.Body>
                  </Card>
                </Col>
                <Col md={3}>
                  <Card className="text-center">
                    <Card.Body>
                      <FaChartLine size={30} className="text-info mb-2" />
                      <h3>${(salesData.totalSales / salesData.totalOrders || 0).toFixed(2)}</h3>
                      <p className="text-muted mb-0">Average Order Value</p>
                    </Card.Body>
                  </Card>
                </Col>
                <Col md={3}>
                  <Card className="text-center">
                    <Card.Body>
                      <FaCalendarAlt size={30} className="text-warning mb-2" />
                      <h3>${(salesData.totalSales / salesData.salesByDay.length || 0).toFixed(2)}</h3>
                      <p className="text-muted mb-0">Daily Average</p>
                    </Card.Body>
                  </Card>
                </Col>
              </Row>

              {/* Sales Chart */}
              <Card className="mb-4">
                <Card.Header>
                  <h5 className="mb-0">Sales Trend</h5>
                </Card.Header>
                <Card.Body>
                  <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={salesData.salesByDay}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" />
                      <YAxis yAxisId="left" />
                      <YAxis yAxisId="right" orientation="right" />
                      <Tooltip />
                      <Legend />
                      <Line yAxisId="left" type="monotone" dataKey="sales" stroke="#0088FE" name="Sales ($)" />
                      <Line yAxisId="right" type="monotone" dataKey="orders" stroke="#00C49F" name="Orders" />
                    </LineChart>
                  </ResponsiveContainer>
                </Card.Body>
              </Card>

              <Row>
                {/* Top Products */}
                <Col md={6}>
                  <Card>
                    <Card.Header>
                      <h5 className="mb-0">Top Selling Products</h5>
                    </Card.Header>
                    <Card.Body>
                      <Table hover size="sm">
                        <thead>
                          <tr>
                            <th>Product</th>
                            <th className="text-end">Qty Sold</th>
                            <th className="text-end">Revenue</th>
                          </tr>
                        </thead>
                        <tbody>
                          {salesData.topProducts.map((product, index) => (
                            <tr key={product.productId}>
                              <td>
                                <Badge bg={index < 3 ? 'success' : 'secondary'} className="me-2">
                                  #{index + 1}
                                </Badge>
                                {product.productName}
                              </td>
                              <td className="text-end">{product.quantitySold}</td>
                              <td className="text-end">${product.revenue.toFixed(2)}</td>
                            </tr>
                          ))}
                        </tbody>
                      </Table>
                    </Card.Body>
                  </Card>
                </Col>

                {/* Payment Methods */}
                <Col md={6}>
                  <Card>
                    <Card.Header>
                      <h5 className="mb-0">Payment Methods</h5>
                    </Card.Header>
                    <Card.Body>
                      <ResponsiveContainer width="100%" height={250}>
                        <PieChart>
                          <Pie
                            data={salesData.paymentMethodBreakdown}
                            dataKey="total"
                            nameKey="method"
                            cx="50%"
                            cy="50%"
                            outerRadius={80}
                            label
                          >
                            {salesData.paymentMethodBreakdown.map((entry, index) => (
                              <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                            ))}
                          </Pie>
                          <Tooltip />
                        </PieChart>
                      </ResponsiveContainer>
                    </Card.Body>
                  </Card>
                </Col>
              </Row>
            </>
          ) : (
            <Alert variant="info">Click refresh to load sales data</Alert>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleCloseModal}>Close</Button>
          <Button variant="primary" onClick={() => handleExportData('sales')}>
            <FaDownload className="me-2" />
            Export to CSV
          </Button>
        </Modal.Footer>
      </Modal>

      {/* Product Performance Modal */}
      <Modal show={activeReport === 'products'} onHide={handleCloseModal} size="xl">
        <Modal.Header>
          <Modal.Title>Product Performance</Modal.Title>
          <Button variant="link" className="text-dark ms-auto" onClick={handleCloseModal}>
            <FaTimes />
          </Button>
        </Modal.Header>
        <Modal.Body style={{ maxHeight: '80vh', overflowY: 'auto' }}>
          {isLoading ? (
            <div className="text-center py-5">
              <Spinner animation="border" />
            </div>
          ) : productData ? (
            <>
              {/* Category Performance */}
              <Card className="mb-4">
                <Card.Header>
                  <h5 className="mb-0">Category Performance</h5>
                </Card.Header>
                <Card.Body>
                  <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={productData.categoryPerformance}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="categoryName" />
                      <YAxis />
                      <Tooltip />
                      <Legend />
                      <Bar dataKey="totalRevenue" fill="#0088FE" name="Revenue ($)" />
                      <Bar dataKey="totalSales" fill="#00C49F" name="Units Sold" />
                    </BarChart>
                  </ResponsiveContainer>
                </Card.Body>
              </Card>

              <Row>
                {/* Top Products */}
                <Col md={8}>
                  <Card>
                    <Card.Header>
                      <h5 className="mb-0">Top Performing Products</h5>
                    </Card.Header>
                    <Card.Body>
                      <Table responsive hover>
                        <thead>
                          <tr>
                            <th>Product</th>
                            <th>SKU</th>
                            <th className="text-end">Qty Sold</th>
                            <th className="text-end">Revenue</th>
                            <th className="text-end">Margin</th>
                          </tr>
                        </thead>
                        <tbody>
                          {productData.topSellingProducts.map(product => (
                            <tr key={product.id}>
                              <td>{product.name}</td>
                              <td>{product.sku}</td>
                              <td className="text-end">{product.quantitySold}</td>
                              <td className="text-end">${product.revenue.toFixed(2)}</td>
                              <td className="text-end">
                                <ProgressBar 
                                  now={product.profitMargin} 
                                  label={`${product.profitMargin}%`}
                                  variant={product.profitMargin > 60 ? 'success' : product.profitMargin > 40 ? 'warning' : 'danger'}
                                />
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </Table>
                    </Card.Body>
                  </Card>
                </Col>

                {/* Low Stock Alert */}
                <Col md={4}>
                  <Card>
                    <Card.Header className="bg-warning text-dark">
                      <h5 className="mb-0">
                        <FaExclamationTriangle className="me-2" />
                        Low Stock Alert
                      </h5>
                    </Card.Header>
                    <Card.Body>
                      {productData.lowStockProducts.map(product => (
                        <div key={product.id} className="mb-3 pb-3 border-bottom">
                          <div className="d-flex justify-content-between">
                            <strong>{product.name}</strong>
                            <Badge bg="danger">{product.currentStock} left</Badge>
                          </div>
                          <small className="text-muted">
                            Threshold: {product.threshold}
                          </small>
                        </div>
                      ))}
                    </Card.Body>
                  </Card>
                </Col>
              </Row>
            </>
          ) : (
            <Alert variant="info">Loading product performance data...</Alert>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleCloseModal}>Close</Button>
          <Button variant="primary" onClick={() => handleExportData('products')}>
            <FaDownload className="me-2" />
            Export to CSV
          </Button>
        </Modal.Footer>
      </Modal>

      {/* Shift Reports Modal */}
      <Modal show={activeReport === 'shifts'} onHide={handleCloseModal} size="xl">
        <Modal.Header>
          <Modal.Title>Shift Reports</Modal.Title>
          <Button variant="link" className="text-dark ms-auto" onClick={handleCloseModal}>
            <FaTimes />
          </Button>
        </Modal.Header>
        <Modal.Body style={{ maxHeight: '80vh', overflowY: 'auto' }}>
          {isLoading ? (
            <div className="text-center py-5">
              <Spinner animation="border" />
            </div>
          ) : shiftData ? (
            <>
              <Card className="mb-4">
                <Card.Header>
                  <h5 className="mb-0">Current Shift Summary</h5>
                </Card.Header>
                <Card.Body>
                  <Row>
                    <Col md={6}>
                      <dl className="row">
                        <dt className="col-sm-4">Shift Number:</dt>
                        <dd className="col-sm-8">{shiftData.shiftNumber}</dd>
                        <dt className="col-sm-4">Cashier:</dt>
                        <dd className="col-sm-8">{shiftData.cashierName}</dd>
                        <dt className="col-sm-4">Start Time:</dt>
                        <dd className="col-sm-8">{format(new Date(shiftData.startTime), 'PPpp')}</dd>
                        <dt className="col-sm-4">Status:</dt>
                        <dd className="col-sm-8">
                          <Badge bg={shiftData.endTime ? 'secondary' : 'success'}>
                            {shiftData.endTime ? 'Closed' : 'Active'}
                          </Badge>
                        </dd>
                      </dl>
                    </Col>
                    <Col md={6}>
                      <Row>
                        <Col sm={6}>
                          <Card className="text-center mb-3">
                            <Card.Body>
                              <h4>${shiftData.totalSales.toFixed(2)}</h4>
                              <small className="text-muted">Total Sales</small>
                            </Card.Body>
                          </Card>
                        </Col>
                        <Col sm={6}>
                          <Card className="text-center mb-3">
                            <Card.Body>
                              <h4>{shiftData.totalOrders}</h4>
                              <small className="text-muted">Total Orders</small>
                            </Card.Body>
                          </Card>
                        </Col>
                      </Row>
                    </Col>
                  </Row>

                  <hr />
                  <h6>Cash Reconciliation</h6>
                  <Row>
                    <Col md={3}>
                      <div className="text-center">
                        <small className="text-muted">Starting Cash</small>
                        <h5>${shiftData.startingCash.toFixed(2)}</h5>
                      </div>
                    </Col>
                    <Col md={3}>
                      <div className="text-center">
                        <small className="text-muted">Cash Sales</small>
                        <h5 className="text-success">+${shiftData.cashSales.toFixed(2)}</h5>
                      </div>
                    </Col>
                    <Col md={3}>
                      <div className="text-center">
                        <small className="text-muted">Expected Cash</small>
                        <h5>${shiftData.expectedCash.toFixed(2)}</h5>
                      </div>
                    </Col>
                    <Col md={3}>
                      <div className="text-center">
                        <small className="text-muted">Card Sales</small>
                        <h5 className="text-info">${shiftData.cardSales.toFixed(2)}</h5>
                      </div>
                    </Col>
                  </Row>
                </Card.Body>
              </Card>

              {/* Recent Transactions */}
              <Card>
                <Card.Header>
                  <h5 className="mb-0">Recent Transactions</h5>
                </Card.Header>
                <Card.Body>
                  <Table responsive hover size="sm">
                    <thead>
                      <tr>
                        <th>Order #</th>
                        <th>Time</th>
                        <th>Payment Method</th>
                        <th className="text-end">Amount</th>
                      </tr>
                    </thead>
                    <tbody>
                      {shiftData.transactions.slice(0, 10).map((transaction, index) => (
                        <tr key={index}>
                          <td>{transaction.orderNumber}</td>
                          <td>{format(new Date(transaction.time), 'HH:mm:ss')}</td>
                          <td>
                            <Badge bg={
                              transaction.paymentMethod === 'Cash' ? 'success' :
                              transaction.paymentMethod === 'Credit Card' ? 'primary' : 'info'
                            }>
                              {transaction.paymentMethod}
                            </Badge>
                          </td>
                          <td className="text-end">${transaction.amount.toFixed(2)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </Table>
                </Card.Body>
              </Card>
            </>
          ) : (
            <Alert variant="info">Loading shift report data...</Alert>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleCloseModal}>Close</Button>
          <Button variant="primary" onClick={() => handleExportData('shifts')}>
            <FaDownload className="me-2" />
            Export to CSV
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default ReportsPage;
