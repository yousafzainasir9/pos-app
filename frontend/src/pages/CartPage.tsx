import React, { useState } from 'react';
import { Row, Col, Card, Button, Table, Form, Modal } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { useCart } from '@/contexts/CartContext';
import { useShift } from '@/contexts/ShiftContext';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { paymentSchema, PaymentFormData } from '@/schemas';
import { FaTrash, FaPlus, FaMinus, FaShoppingCart, FaCreditCard, FaMoneyBill } from 'react-icons/fa';
import { PaymentMethod, OrderType } from '@/types';
import orderService from '@/services/order.service';
import { toast } from 'react-toastify';

const CartPage: React.FC = () => {
  const navigate = useNavigate();
  const { items, totalAmount, taxAmount, subTotal, updateQuantity, removeItem, clearCart } = useCart();
  const { isShiftOpen } = useShift();
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [currentOrderId, setCurrentOrderId] = useState<number | null>(null);
  
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setValue,
    watch
  } = useForm<PaymentFormData>({
    resolver: zodResolver(paymentSchema),
    defaultValues: {
      paymentMethod: PaymentMethod.Cash
    }
  });

  const selectedPaymentMethod = watch('paymentMethod');

  const handleCheckout = async () => {
    if (!isShiftOpen) {
      toast.error('Please open a shift before processing orders');
      navigate('/shift');
      return;
    }

    if (items.length === 0) {
      toast.error('Cart is empty');
      return;
    }

    setIsProcessing(true);
    try {
      const orderData = {
        orderType: OrderType.DineIn,
        items: items.map(item => ({
          productId: item.productId,
          quantity: item.quantity,
          discountAmount: item.discountAmount,
          notes: item.notes
        })),
        discountAmount: 0
      };

      const response = await orderService.createOrder(orderData);
      setCurrentOrderId(response.orderId);
      toast.success(`Order ${response.orderNumber} created`);
      setShowPaymentModal(true);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to create order');
    } finally {
      setIsProcessing(false);
    }
  };

  const handlePayment = async (data: PaymentFormData) => {
    if (!currentOrderId) return;

    setIsProcessing(true);
    try {
      const paymentData = {
        ...data,
        orderId: currentOrderId
      };

      const response = await orderService.processPayment(paymentData);
      
      toast.success('Payment processed successfully!');
      
      if (response.changeAmount > 0) {
        toast.info(`Change due: $${response.changeAmount.toFixed(2)}`, {
          autoClose: 5000
        });
      }

      // Clear cart and close modal
      clearCart();
      setShowPaymentModal(false);
      reset();
      
      // Navigate to order details
      navigate(`/orders/${currentOrderId}`);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Payment failed');
    } finally {
      setIsProcessing(false);
    }
  };

  if (items.length === 0) {
    return (
      <div className="text-center py-5">
        <FaShoppingCart size={64} className="text-muted mb-3" />
        <h3>Your cart is empty</h3>
        <p className="text-muted">Add some products to get started</p>
        <Button variant="primary" onClick={() => navigate('/pos')}>
          Browse Products
        </Button>
      </div>
    );
  }

  return (
    <>
      <Row>
        <Col lg={8}>
          <Card>
            <Card.Header className="d-flex justify-content-between align-items-center">
              <h4 className="mb-0">Shopping Cart ({items.length} items)</h4>
              <Button variant="outline-danger" size="sm" onClick={clearCart}>
                Clear Cart
              </Button>
            </Card.Header>
            <Card.Body>
              <Table responsive>
                <thead>
                  <tr>
                    <th>Product</th>
                    <th>Price</th>
                    <th>Quantity</th>
                    <th>Subtotal</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {items.map(item => (
                    <tr key={item.productId}>
                      <td>
                        <div>
                          <div className="fw-bold">{item.productName}</div>
                          {item.sku && <small className="text-muted d-block">SKU: {item.sku}</small>}
                          {item.notes && (
                            <small className="text-primary d-block">
                              <strong>Note:</strong> {item.notes}
                            </small>
                          )}
                        </div>
                      </td>
                      <td>${item.unitPrice.toFixed(2)}</td>
                      <td>
                        <div className="quantity-controls">
                          <Button
                            variant="outline-secondary"
                            size="sm"
                            onClick={() => updateQuantity(item.productId, item.quantity - 1)}
                          >
                            <FaMinus />
                          </Button>
                          <span className="mx-2">{item.quantity}</span>
                          <Button
                            variant="outline-secondary"
                            size="sm"
                            onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                          >
                            <FaPlus />
                          </Button>
                        </div>
                      </td>
                      <td className="fw-bold">${item.subtotal.toFixed(2)}</td>
                      <td>
                        <Button
                          variant="outline-danger"
                          size="sm"
                          onClick={() => removeItem(item.productId)}
                        >
                          <FaTrash />
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </Card.Body>
          </Card>
        </Col>

        <Col lg={4}>
          <Card>
            <Card.Header>
              <h4 className="mb-0">Order Summary</h4>
            </Card.Header>
            <Card.Body>
              <div className="d-flex justify-content-between mb-2">
                <span>Subtotal (ex GST):</span>
                <span>${subTotal.toFixed(2)}</span>
              </div>
              <div className="d-flex justify-content-between mb-2">
                <span>GST (10%):</span>
                <span>${taxAmount.toFixed(2)}</span>
              </div>
              <hr />
              <div className="d-flex justify-content-between mb-3">
                <strong>Total (inc GST):</strong>
                <strong className="text-primary fs-4">${totalAmount.toFixed(2)}</strong>
              </div>
              
              <div className="d-grid gap-2">
                <Button
                  variant="success"
                  size="lg"
                  onClick={handleCheckout}
                  disabled={!isShiftOpen || isProcessing}
                >
                  {isProcessing ? (
                    <>
                      <span 
                        className="spinner-border spinner-border-sm me-2" 
                        style={{ width: '1rem', height: '1rem' }}
                      />
                      Processing...
                    </>
                  ) : (
                    <>
                      <FaCreditCard className="me-2" />
                      Proceed to Payment
                    </>
                  )}
                </Button>
                <Button
                  variant="outline-secondary"
                  onClick={() => navigate('/pos')}
                >
                  Continue Shopping
                </Button>
              </div>

              {!isShiftOpen && (
                <div className="alert alert-warning mt-3 mb-0">
                  <small>Please open a shift to process orders</small>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Payment Modal */}
      <Modal show={showPaymentModal} onHide={() => !isProcessing && setShowPaymentModal(false)} size="lg">
        <Modal.Header closeButton={!isProcessing}>
          <Modal.Title>Process Payment</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form onSubmit={handleSubmit(handlePayment)}>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Payment Method</Form.Label>
                  <Form.Select
                    {...register('paymentMethod', { valueAsNumber: true })}
                    disabled={isProcessing}
                  >
                    <option value={PaymentMethod.Cash}>Cash</option>
                    <option value={PaymentMethod.CreditCard}>Credit Card</option>
                    <option value={PaymentMethod.DebitCard}>Debit Card</option>
                    <option value={PaymentMethod.MobilePayment}>Mobile Payment</option>
                  </Form.Select>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Amount</Form.Label>
                  <Form.Control
                    type="number"
                    step="0.01"
                    {...register('amount', { valueAsNumber: true })}
                    defaultValue={totalAmount}
                    isInvalid={!!errors.amount}
                    disabled={isProcessing}
                  />
                  <Form.Control.Feedback type="invalid">
                    {errors.amount?.message}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
            </Row>

            {(selectedPaymentMethod === PaymentMethod.CreditCard || 
              selectedPaymentMethod === PaymentMethod.DebitCard) && (
              <Row>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Card Last 4 Digits</Form.Label>
                    <Form.Control
                      type="text"
                      maxLength={4}
                      {...register('cardLastFourDigits')}
                      isInvalid={!!errors.cardLastFourDigits}
                      disabled={isProcessing}
                    />
                    <Form.Control.Feedback type="invalid">
                      {errors.cardLastFourDigits?.message}
                    </Form.Control.Feedback>
                  </Form.Group>
                </Col>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Card Type</Form.Label>
                    <Form.Select
                      {...register('cardType')}
                      disabled={isProcessing}
                    >
                      <option value="">Select...</option>
                      <option value="Visa">Visa</option>
                      <option value="Mastercard">Mastercard</option>
                      <option value="Amex">American Express</option>
                    </Form.Select>
                  </Form.Group>
                </Col>
              </Row>
            )}

            <Form.Group className="mb-3">
              <Form.Label>Reference Number (Optional)</Form.Label>
              <Form.Control
                type="text"
                {...register('referenceNumber')}
                disabled={isProcessing}
              />
            </Form.Group>

            <div className="alert alert-info">
              <strong>Order Total: ${totalAmount.toFixed(2)}</strong>
            </div>

            <div className="d-flex justify-content-end gap-2">
              <Button
                variant="secondary"
                onClick={() => setShowPaymentModal(false)}
                disabled={isProcessing}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="success"
                disabled={isProcessing}
              >
                {isProcessing ? (
                  <>
                    <span 
                      className="spinner-border spinner-border-sm me-2" 
                      style={{ width: '1rem', height: '1rem' }}
                    />
                    Processing...
                  </>
                ) : (
                  <>
                    <FaMoneyBill className="me-2" />
                    Process Payment
                  </>
                )}
              </Button>
            </div>
          </Form>
        </Modal.Body>
      </Modal>
    </>
  );
};

export default CartPage;
