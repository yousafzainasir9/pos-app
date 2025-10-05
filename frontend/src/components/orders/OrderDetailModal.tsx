import React, { useState, useEffect } from 'react';
import { Modal, Button, Table, Badge, Row, Col, Card, Spinner } from 'react-bootstrap';
import { format } from 'date-fns';
import { FaPrint, FaTimes } from 'react-icons/fa';
import { Order, OrderStatus, PaymentMethod } from '@/types';
import orderService from '@/services/order.service';
import { toast } from 'react-toastify';

interface OrderDetailModalProps {
  show: boolean;
  onHide: () => void;
  orderId: number;
  onPrint?: (order: Order) => void;
}

const OrderDetailModal: React.FC<OrderDetailModalProps> = ({ show, onHide, orderId, onPrint }) => {
  const [order, setOrder] = useState<Order | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (show && orderId) {
      loadOrderDetails();
    }
  }, [show, orderId]);

  const loadOrderDetails = async () => {
    setIsLoading(true);
    try {
      const orderData = await orderService.getOrder(orderId);
      setOrder(orderData);
    } catch (error) {
      console.error('Failed to load order details:', error);
      toast.error('Failed to load order details');
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
      [OrderStatus.Refunded]: { variant: 'secondary', label: 'Refunded' },
      [OrderStatus.PartiallyRefunded]: { variant: 'warning', label: 'Partially Refunded' },
      [OrderStatus.OnHold]: { variant: 'dark', label: 'On Hold' }
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

  const getPaymentMethodLabel = (method: PaymentMethod) => {
    const methodMap = {
      [PaymentMethod.Cash]: 'Cash',
      [PaymentMethod.CreditCard]: 'Credit Card',
      [PaymentMethod.DebitCard]: 'Debit Card',
      [PaymentMethod.MobilePayment]: 'Mobile Payment',
      [PaymentMethod.GiftCard]: 'Gift Card',
      [PaymentMethod.LoyaltyPoints]: 'Loyalty Points',
      [PaymentMethod.Other]: 'Other'
    };

    return methodMap[method] || 'Unknown';
  };

  const handlePrint = () => {
    if (order && onPrint) {
      onPrint(order);
    }
  };

  return (
    <Modal show={show} onHide={onHide} size="lg" centered>
      <Modal.Header closeButton>
        <Modal.Title>
          Order Details
          {order && (
            <span className="ms-3">
              <Badge bg="dark">{order.orderNumber}</Badge>
            </span>
          )}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {isLoading ? (
          <div className="text-center py-5">
            <Spinner animation="border" variant="primary" />
            <p className="mt-3 text-muted">Loading order details...</p>
          </div>
        ) : order ? (
          <>
            {/* Order Information */}
            <Card className="mb-3">
              <Card.Header className="bg-light">
                <strong>Order Information</strong>
              </Card.Header>
              <Card.Body>
                <Row>
                  <Col md={6}>
                    <p className="mb-2">
                      <strong>Order Number:</strong> {order.orderNumber}
                    </p>
                    <p className="mb-2">
                      <strong>Date:</strong> {format(new Date(order.orderDate), 'MMM dd, yyyy HH:mm')}
                    </p>
                    <p className="mb-2">
                      <strong>Type:</strong> {getOrderTypeBadge(order.orderType)}
                    </p>
                    <p className="mb-2">
                      <strong>Status:</strong> {getStatusBadge(order.status)}
                    </p>
                  </Col>
                  <Col md={6}>
                    <p className="mb-2">
                      <strong>Customer:</strong> {order.customerName || 'Walk-in'}
                    </p>
                    <p className="mb-2">
                      <strong>Cashier:</strong> {order.cashierName || 'N/A'}
                    </p>
                    <p className="mb-2">
                      <strong>Store:</strong> {order.storeName || 'N/A'}
                    </p>
                    {order.tableNumber && (
                      <p className="mb-2">
                        <strong>Table:</strong> {order.tableNumber}
                      </p>
                    )}
                    {order.notes && (
                      <p className="mb-2">
                        <strong>Notes:</strong> {order.notes}
                      </p>
                    )}
                  </Col>
                </Row>
              </Card.Body>
            </Card>

            {/* Order Items */}
            <Card className="mb-3">
              <Card.Header className="bg-light">
                <strong>Order Items</strong>
              </Card.Header>
              <Card.Body className="p-0">
                <Table responsive hover className="mb-0">
                  <thead>
                    <tr>
                      <th>Item</th>
                      <th>SKU</th>
                      <th className="text-end">Qty</th>
                      <th className="text-end">Price</th>
                      <th className="text-end">Discount</th>
                      <th className="text-end">Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    {order.items && order.items.length > 0 ? (
                      order.items.map((item) => (
                        <tr key={item.id} className={item.isVoided ? 'text-decoration-line-through text-muted' : ''}>
                          <td>
                            {item.productName}
                            {item.isVoided && <Badge bg="danger" className="ms-2">Voided</Badge>}
                          </td>
                          <td>{item.productSKU || '-'}</td>
                          <td className="text-end">{item.quantity}</td>
                          <td className="text-end">${item.unitPriceIncGst.toFixed(2)}</td>
                          <td className="text-end">${item.discountAmount.toFixed(2)}</td>
                          <td className="text-end fw-bold">${item.totalAmount.toFixed(2)}</td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={6} className="text-center text-muted py-3">
                          No items in this order
                        </td>
                      </tr>
                    )}
                  </tbody>
                </Table>
              </Card.Body>
            </Card>

            {/* Order Totals */}
            <Card className="mb-3">
              <Card.Header className="bg-light">
                <strong>Order Totals</strong>
              </Card.Header>
              <Card.Body>
                <Row className="mb-2">
                  <Col className="text-end text-muted">Subtotal:</Col>
                  <Col xs="auto" className="fw-bold">${order.subTotal.toFixed(2)}</Col>
                </Row>
                <Row className="mb-2">
                  <Col className="text-end text-muted">Discount:</Col>
                  <Col xs="auto" className="text-danger">-${order.discountAmount.toFixed(2)}</Col>
                </Row>
                <Row className="mb-2">
                  <Col className="text-end text-muted">Tax (GST):</Col>
                  <Col xs="auto">${order.taxAmount.toFixed(2)}</Col>
                </Row>
                <Row className="border-top pt-2">
                  <Col className="text-end"><strong>Total:</strong></Col>
                  <Col xs="auto" className="fs-5 fw-bold text-primary">${order.totalAmount.toFixed(2)}</Col>
                </Row>
                <Row className="mb-2">
                  <Col className="text-end text-muted">Paid:</Col>
                  <Col xs="auto" className="text-success">${order.paidAmount.toFixed(2)}</Col>
                </Row>
                {order.changeAmount > 0 && (
                  <Row>
                    <Col className="text-end text-muted">Change:</Col>
                    <Col xs="auto">${order.changeAmount.toFixed(2)}</Col>
                  </Row>
                )}
              </Card.Body>
            </Card>

            {/* Payments */}
            {order.payments && order.payments.length > 0 && (
              <Card>
                <Card.Header className="bg-light">
                  <strong>Payment Information</strong>
                </Card.Header>
                <Card.Body className="p-0">
                  <Table responsive hover className="mb-0">
                    <thead>
                      <tr>
                        <th>Method</th>
                        <th>Reference</th>
                        <th>Date</th>
                        <th className="text-end">Amount</th>
                        <th>Status</th>
                      </tr>
                    </thead>
                    <tbody>
                      {order.payments.map((payment) => (
                        <tr key={payment.id}>
                          <td>{getPaymentMethodLabel(payment.paymentMethod)}</td>
                          <td>{payment.referenceNumber || '-'}</td>
                          <td>{format(new Date(payment.paymentDate), 'MMM dd, HH:mm')}</td>
                          <td className="text-end fw-bold">${payment.amount.toFixed(2)}</td>
                          <td>
                            <Badge bg={payment.status === 2 ? 'success' : 'secondary'}>
                              {payment.status === 2 ? 'Completed' : 'Pending'}
                            </Badge>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </Table>
                </Card.Body>
              </Card>
            )}
          </>
        ) : (
          <div className="text-center py-5 text-muted">
            <p>Order not found</p>
          </div>
        )}
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={onHide}>
          <FaTimes className="me-2" />
          Close
        </Button>
        {order && (
          <Button variant="primary" onClick={handlePrint}>
            <FaPrint className="me-2" />
            Print Receipt
          </Button>
        )}
      </Modal.Footer>
    </Modal>
  );
};

export default OrderDetailModal;
