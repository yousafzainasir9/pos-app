import React, { useState, useEffect } from 'react';
import { Table, Card, Badge, Button, Form, Row, Col } from 'react-bootstrap';
import { FaEye, FaPrint, FaFilter, FaReceipt } from 'react-icons/fa';
import { format } from 'date-fns';
import { Order, OrderStatus } from '@/types';
import orderService from '@/services/order.service';
import { toast } from 'react-toastify';

const OrdersPage: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [filter, setFilter] = useState({
    status: '',
    fromDate: '',
    toDate: ''
  });

  useEffect(() => {
    loadOrders();
  }, []);

  const loadOrders = async () => {
    setIsLoading(true);
    try {
      const params: any = {};
      if (filter.status) params.status = parseInt(filter.status);
      if (filter.fromDate) params.fromDate = new Date(filter.fromDate);
      if (filter.toDate) params.toDate = new Date(filter.toDate);

      const data = await orderService.getOrders(params);
      setOrders(data);
    } catch (error) {
      console.error('Failed to load orders:', error);
      toast.error('Failed to load orders');
    } finally {
      setIsLoading(false);
    }
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

  const handleFilterChange = (key: string, value: string) => {
    setFilter(prev => ({ ...prev, [key]: value }));
  };

  const handleApplyFilter = () => {
    loadOrders();
  };

  const handleClearFilter = () => {
    setFilter({ status: '', fromDate: '', toDate: '' });
    loadOrders();
  };

  const handleViewOrder = (orderId: number) => {
    window.open(`/orders/${orderId}`, '_blank');
  };

  const handlePrintReceipt = (orderId: number) => {
    window.open(`/orders/${orderId}/receipt`, '_blank');
  };

  return (
    <div>
      <h2 className="mb-4">Orders</h2>

      {/* Filters */}
      <Card className="mb-4">
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

      {/* Orders Table */}
      <Card>
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
                    <td>{format(new Date(order.orderDate), 'dd/MM/yyyy HH:mm')}</td>
                    <td>{order.customerName || 'Walk-in'}</td>
                    <td>
                      <Badge bg="secondary">
                        {order.orderType === 1 ? 'Dine In' : 
                         order.orderType === 2 ? 'Take Away' :
                         order.orderType === 3 ? 'Delivery' : 'Pickup'}
                      </Badge>
                    </td>
                    <td>{getStatusBadge(order.status)}</td>
                    <td className="fw-bold">${order.totalAmount.toFixed(2)}</td>
                    <td>${order.paidAmount.toFixed(2)}</td>
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
              <FaReceipt size={48} className="mb-3" />
              <p>No orders found</p>
            </div>
          )}
        </Card.Body>
      </Card>
    </div>
  );
};

export default OrdersPage;
