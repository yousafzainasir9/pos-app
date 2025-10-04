import React, { useState, useEffect } from 'react';
import { Table, Card, Badge, Button, Form, Row, Col, Pagination } from 'react-bootstrap';
import { FaEye, FaPrint, FaFilter, FaReceipt, FaTimes, FaCheck } from 'react-icons/fa';
import { format } from 'date-fns';
import { Order, OrderStatus } from '@/types';
import orderService from '@/services/order.service';
import { toast } from 'react-toastify';

const OrdersPage: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingSummary, setIsLoadingSummary] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(20);
  const [pagination, setPagination] = useState({
    totalCount: 0,
    totalPages: 0,
    hasNext: false,
    hasPrevious: false
  });
  const [summary, setSummary] = useState({
    totalOrders: 0,
    totalSales: 0,
    pendingOrders: 0,
    processingOrders: 0
  });
  
  // Initialize with today's date
  const today = format(new Date(), 'yyyy-MM-dd');
  const [filter, setFilter] = useState({
    status: '',
    fromDate: today,
    toDate: today
  });

  useEffect(() => {
    // Load both summary and orders on initial mount
    loadSummary();
    loadOrders();
  }, []);

  useEffect(() => {
    // Load orders when page changes (but keep same filters)
    loadOrders();
  }, [currentPage]);

  const loadSummary = async () => {
    setIsLoadingSummary(true);
    try {
      const params: any = {};
      if (filter.status) params.status = parseInt(filter.status);
      if (filter.fromDate) params.fromDate = filter.fromDate;
      if (filter.toDate) params.toDate = filter.toDate;

      const summaryData = await orderService.getOrdersSummary(params);
      setSummary(summaryData);
    } catch (error) {
      console.error('Failed to load summary:', error);
      setSummary({
        totalOrders: 0,
        totalSales: 0,
        pendingOrders: 0,
        processingOrders: 0
      });
    } finally {
      setIsLoadingSummary(false);
    }
  };

  const loadOrders = async () => {
    setIsLoading(true);
    try {
      // Pass parameters correctly to the service
      const params: any = {
        page: currentPage,
        pageSize: pageSize
      };
      if (filter.status) params.status = parseInt(filter.status);
      if (filter.fromDate) params.fromDate = filter.fromDate;
      if (filter.toDate) params.toDate = filter.toDate;

      const response = await orderService.getOrders(params);
      setOrders(response.data);
      setPagination({
        totalCount: response.pagination.totalCount,
        totalPages: response.pagination.totalPages,
        hasNext: response.pagination.hasNext,
        hasPrevious: response.pagination.hasPrevious
      });
    } catch (error) {
      console.error('Failed to load orders:', error);
      setOrders([]);
      setPagination({
        totalCount: 0,
        totalPages: 0,
        hasNext: false,
        hasPrevious: false
      });
    } finally {
      setIsLoading(false);
    }
  };

  const loadMockOrders = () => {
    // Generate mock orders for today
    const mockOrders: Order[] = [
      {
        id: 1,
        orderNumber: 'ORD-2024-001',
        orderDate: new Date().toISOString(),
        customerName: 'John Doe',
        customerId: 1,
        orderType: 1, // Dine In
        status: OrderStatus.Completed,
        subTotal: 45.00,
        discountAmount: 0,
        taxAmount: 4.50,
        totalAmount: 49.50,
        paidAmount: 49.50,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 2,
        orderNumber: 'ORD-2024-002',
        orderDate: new Date(Date.now() - 3600000).toISOString(), // 1 hour ago
        customerName: 'Jane Smith',
        customerId: 2,
        orderType: 2, // Take Away
        status: OrderStatus.Processing,
        subTotal: 32.00,
        discountAmount: 3.20,
        taxAmount: 2.88,
        totalAmount: 31.68,
        paidAmount: 31.68,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 3,
        orderNumber: 'ORD-2024-003',
        orderDate: new Date(Date.now() - 7200000).toISOString(), // 2 hours ago
        customerName: null,
        customerId: null,
        orderType: 1, // Dine In
        status: OrderStatus.Completed,
        subTotal: 18.50,
        discountAmount: 0,
        taxAmount: 1.85,
        totalAmount: 20.35,
        paidAmount: 20.35,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: 'Table 5',
        userId: 1,
        storeId: 1
      },
      {
        id: 4,
        orderNumber: 'ORD-2024-004',
        orderDate: new Date(Date.now() - 1800000).toISOString(), // 30 min ago
        customerName: 'Mike Johnson',
        customerId: 3,
        orderType: 3, // Delivery
        status: OrderStatus.Pending,
        subTotal: 65.00,
        discountAmount: 0,
        taxAmount: 6.50,
        totalAmount: 71.50,
        paidAmount: 71.50,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: 'Delivery to 123 Main St',
        userId: 1,
        storeId: 1
      },
      {
        id: 5,
        orderNumber: 'ORD-2024-005',
        orderDate: new Date(Date.now() - 900000).toISOString(), // 15 min ago
        customerName: 'Sarah Wilson',
        customerId: 4,
        orderType: 1, // Dine In
        status: OrderStatus.Processing,
        subTotal: 28.00,
        discountAmount: 2.80,
        taxAmount: 2.52,
        totalAmount: 27.72,
        paidAmount: 30.00,
        changeAmount: 2.28,
        items: [],
        payments: [],
        notes: 'Table 2',
        userId: 1,
        storeId: 1
      }
    ];

    // Filter mock orders based on selected filters
    let filteredOrders = [...mockOrders];
    
    if (filter.status) {
      filteredOrders = filteredOrders.filter(o => o.status === parseInt(filter.status));
    }

    setOrders(filteredOrders);
  };

  const getStatusBadge = (status: OrderStatus) => {
    const statusMap = {
      [OrderStatus.Pending]: { variant: 'warning', label: 'Pending' },
      [OrderStatus.Processing]: { variant: 'info', label: 'Processing' },
      [OrderStatus.Completed]: { variant: 'success', label: 'Completed' },
      [OrderStatus.Cancelled]: { variant: 'danger', label: 'Cancelled' },
      [OrderStatus.Refunded]: { variant: 'secondary', label: 'Refunded' }
    };

    const config = statusMap[status] || { variant: 'secondary', label: 'Unknown' };
    return <Badge bg={config.variant}>{config.label}</Badge>;
  };

  const getOrderTypeBadge = (type: number) => {
    const typeMap: { [key: number]: { variant: string; label: string } } = {
      1: { variant: 'primary', label: 'Dine In' },
      2: { variant: 'warning', label: 'Take Away' },
      3: { variant: 'info', label: 'Delivery' },
      4: { variant: 'secondary', label: 'Pickup' }
    };

    const config = typeMap[type] || { variant: 'secondary', label: 'Unknown' };
    return <Badge bg={config.variant}>{config.label}</Badge>;
  };

  const handleFilterChange = (key: string, value: string) => {
    setFilter(prev => ({ ...prev, [key]: value }));
  };

  const handleApplyFilter = async () => {
    setCurrentPage(1); // Reset to first page when applying filter
    // Load both summary and orders with new filters
    await Promise.all([
      loadSummary(),
      loadOrders()
    ]);
  };

  const handleClearFilter = async () => {
    const today = format(new Date(), 'yyyy-MM-dd');
    setFilter({ status: '', fromDate: today, toDate: today });
    setCurrentPage(1); // Reset to first page
    // Small delay to ensure state is updated before reloading
    setTimeout(async () => {
      await Promise.all([
        loadSummary(),
        loadOrders()
      ]);
    }, 50);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleViewOrder = (orderId: number) => {
    // For now, just show a toast as the order detail page might not be implemented
    toast.info(`View order #${orderId} - Feature coming soon!`);
  };

  const handlePrintReceipt = (orderId: number) => {
    toast.info(`Print receipt for order #${orderId} - Feature coming soon!`);
  };

  const getPaymentStatus = (order: Order) => {
    if (order.paidAmount >= order.totalAmount) {
      return <Badge bg="success"><FaCheck /> Paid</Badge>;
    } else if (order.paidAmount > 0) {
      return <Badge bg="warning">Partial</Badge>;
    } else {
      return <Badge bg="danger"><FaTimes /> Unpaid</Badge>;
    }
  };

  return (
    <div className="orders-page">
      <h2 className="mb-4">Orders</h2>

      {/* Filters */}
      <Card className="mb-4 shadow-sm">
        <Card.Body>
          <Row className="align-items-end">
            <Col md={3}>
              <Form.Group>
                <Form.Label>Status</Form.Label>
                <Form.Select
                  value={filter.status}
                  onChange={(e) => handleFilterChange('status', e.target.value)}
                >
                  <option value="">All Status</option>
                  <option value={OrderStatus.Pending}>Pending</option>
                  <option value={OrderStatus.Processing}>Processing</option>
                  <option value={OrderStatus.Completed}>Completed</option>
                  <option value={OrderStatus.Cancelled}>Cancelled</option>
                  <option value={OrderStatus.Refunded}>Refunded</option>
                </Form.Select>
              </Form.Group>
            </Col>
            <Col md={3}>
              <Form.Group>
                <Form.Label>From Date</Form.Label>
                <Form.Control
                  type="date"
                  value={filter.fromDate}
                  onChange={(e) => handleFilterChange('fromDate', e.target.value)}
                />
              </Form.Group>
            </Col>
            <Col md={3}>
              <Form.Group>
                <Form.Label>To Date</Form.Label>
                <Form.Control
                  type="date"
                  value={filter.toDate}
                  onChange={(e) => handleFilterChange('toDate', e.target.value)}
                />
              </Form.Group>
            </Col>
            <Col md={3}>
              <div className="d-flex gap-2">
                <Button variant="primary" onClick={handleApplyFilter}>
                  <FaFilter className="me-2" />
                  Apply
                </Button>
                <Button variant="outline-secondary" onClick={handleClearFilter}>
                  Clear
                </Button>
              </div>
            </Col>
          </Row>
        </Card.Body>
      </Card>

      {/* Orders Summary */}
      <Row className="mb-4">
        <Col md={3}>
          <Card className="text-center">
            <Card.Body>
              {isLoadingSummary ? (
                <div className="spinner-border spinner-border-sm text-primary" />
              ) : (
                <h4 className="text-primary">{summary.totalOrders}</h4>
              )}
              <p className="text-muted mb-0">Total Orders</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="text-center">
            <Card.Body>
              {isLoadingSummary ? (
                <div className="spinner-border spinner-border-sm text-success" />
              ) : (
                <h4 className="text-success">${summary.totalSales.toFixed(2)}</h4>
              )}
              <p className="text-muted mb-0">Total Sales</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="text-center">
            <Card.Body>
              {isLoadingSummary ? (
                <div className="spinner-border spinner-border-sm text-warning" />
              ) : (
                <h4 className="text-warning">{summary.pendingOrders}</h4>
              )}
              <p className="text-muted mb-0">Pending Orders</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="text-center">
            <Card.Body>
              {isLoadingSummary ? (
                <div className="spinner-border spinner-border-sm text-info" />
              ) : (
                <h4 className="text-info">{summary.processingOrders}</h4>
              )}
              <p className="text-muted mb-0">Processing</p>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Orders Table */}
      <Card className="shadow-sm">
        <Card.Body>
          {isLoading ? (
            <div className="text-center py-5">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
            </div>
          ) : (
            <Table responsive hover>
              <thead>
                <tr>
                  <th>Order #</th>
                  <th>Date</th>
                  <th>Customer</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Total</th>
                  <th>Paid</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {orders.map(order => (
                  <tr key={order.id}>
                    <td>
                      <strong>{order.orderNumber}</strong>
                    </td>
                    <td>{format(new Date(order.orderDate), 'MM/dd/yyyy HH:mm')}</td>
                    <td>{order.customerName || 'Walk-in'}</td>
                    <td>{getOrderTypeBadge(order.orderType)}</td>
                    <td>{getStatusBadge(order.status)}</td>
                    <td className="fw-bold">${order.totalAmount.toFixed(2)}</td>
                    <td>{getPaymentStatus(order)}</td>
                    <td>
                      <div className="d-flex gap-1">
                        <Button
                          variant="outline-primary"
                          size="sm"
                          onClick={() => handleViewOrder(order.id)}
                          title="View Order"
                        >
                          <FaEye />
                        </Button>
                        <Button
                          variant="outline-secondary"
                          size="sm"
                          onClick={() => handlePrintReceipt(order.id)}
                          title="Print Receipt"
                        >
                          <FaPrint />
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}

          {!isLoading && orders.length === 0 && (
            <div className="text-center py-5 text-muted">
              <FaReceipt size={48} className="mb-3 opacity-50" />
              <p>No orders found for the selected filters</p>
              <small>Try adjusting your date range or status filter</small>
            </div>
          )}

          {/* Pagination */}
          {!isLoading && orders.length > 0 && pagination.totalPages > 1 && (
            <div className="d-flex justify-content-between align-items-center mt-3">
              <div className="text-muted">
                Showing {((currentPage - 1) * pageSize) + 1} to {Math.min(currentPage * pageSize, pagination.totalCount)} of {pagination.totalCount} orders
              </div>
              <Pagination className="mb-0">
                <Pagination.First 
                  onClick={() => handlePageChange(1)} 
                  disabled={!pagination.hasPrevious}
                />
                <Pagination.Prev 
                  onClick={() => handlePageChange(currentPage - 1)} 
                  disabled={!pagination.hasPrevious}
                />
                
                {/* Show page numbers */}
                {(() => {
                  const pages = [];
                  const maxPagesToShow = 5;
                  let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
                  let endPage = Math.min(pagination.totalPages, startPage + maxPagesToShow - 1);
                  
                  // Adjust start if we're near the end
                  if (endPage - startPage < maxPagesToShow - 1) {
                    startPage = Math.max(1, endPage - maxPagesToShow + 1);
                  }
                  
                  // Add first page and ellipsis if needed
                  if (startPage > 1) {
                    pages.push(
                      <Pagination.Item key={1} onClick={() => handlePageChange(1)}>
                        1
                      </Pagination.Item>
                    );
                    if (startPage > 2) {
                      pages.push(<Pagination.Ellipsis key="ellipsis-start" disabled />);
                    }
                  }
                  
                  // Add page numbers
                  for (let i = startPage; i <= endPage; i++) {
                    pages.push(
                      <Pagination.Item
                        key={i}
                        active={i === currentPage}
                        onClick={() => handlePageChange(i)}
                      >
                        {i}
                      </Pagination.Item>
                    );
                  }
                  
                  // Add ellipsis and last page if needed
                  if (endPage < pagination.totalPages) {
                    if (endPage < pagination.totalPages - 1) {
                      pages.push(<Pagination.Ellipsis key="ellipsis-end" disabled />);
                    }
                    pages.push(
                      <Pagination.Item
                        key={pagination.totalPages}
                        onClick={() => handlePageChange(pagination.totalPages)}
                      >
                        {pagination.totalPages}
                      </Pagination.Item>
                    );
                  }
                  
                  return pages;
                })()}
                
                <Pagination.Next 
                  onClick={() => handlePageChange(currentPage + 1)} 
                  disabled={!pagination.hasNext}
                />
                <Pagination.Last 
                  onClick={() => handlePageChange(pagination.totalPages)} 
                  disabled={!pagination.hasNext}
                />
              </Pagination>
            </div>
          )}
        </Card.Body>
      </Card>
    </div>
  );
};

export default OrdersPage;
