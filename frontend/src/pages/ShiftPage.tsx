import React, { useState } from 'react';
import { Card, Button, Form, Row, Col, Alert, Badge } from 'react-bootstrap';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { openShiftSchema, closeShiftSchema, OpenShiftFormData, CloseShiftFormData } from '@/schemas';
import { useShift } from '@/contexts/ShiftContext';
import { useAuth } from '@/contexts/AuthContext';
import { FaCashRegister, FaPlay, FaStop, FaClock, FaDollarSign } from 'react-icons/fa';
import { format } from 'date-fns';

const ShiftPage: React.FC = () => {
  const { currentShift, isShiftOpen, openShift, closeShift, isLoading } = useShift();
  const { user } = useAuth();
  const [showCloseModal, setShowCloseModal] = useState(false);

  const {
    register: registerOpen,
    handleSubmit: handleSubmitOpen,
    formState: { errors: openErrors },
    reset: resetOpen
  } = useForm<OpenShiftFormData>({
    resolver: zodResolver(openShiftSchema),
    defaultValues: {
      startingCash: 0
    }
  });

  const {
    register: registerClose,
    handleSubmit: handleSubmitClose,
    formState: { errors: closeErrors },
    reset: resetClose
  } = useForm<CloseShiftFormData>({
    resolver: zodResolver(closeShiftSchema),
    defaultValues: {
      endingCash: 0
    }
  });

  const handleOpenShift = async (data: OpenShiftFormData) => {
    try {
      await openShift(data);
      resetOpen();
    } catch (error) {
      console.error('Failed to open shift:', error);
    }
  };

  const handleCloseShift = async (data: CloseShiftFormData) => {
    try {
      await closeShift(data);
      resetClose();
      setShowCloseModal(false);
    } catch (error) {
      console.error('Failed to close shift:', error);
    }
  };

  return (
    <div>
      <h2 className="mb-4">
        <FaCashRegister className="me-2" />
        Shift Management
      </h2>

      <Row>
        <Col lg={6}>
          {/* Current Shift Status */}
          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">Current Shift Status</h5>
            </Card.Header>
            <Card.Body>
              {isShiftOpen && currentShift ? (
                <div>
                  <div className={`shift-status open mb-3`}>
                    <div className="d-flex justify-content-between align-items-center">
                      <div>
                        <h6 className="mb-1">Shift #{currentShift.shiftNumber}</h6>
                        <div className="text-muted">
                          <FaClock className="me-1" />
                          Started: {format(new Date(currentShift.startTime), 'dd/MM/yyyy HH:mm')}
                        </div>
                      </div>
                      <Badge bg="success" className="fs-6">OPEN</Badge>
                    </div>
                  </div>

                  <Row className="mb-3">
                    <Col>
                      <div className="text-center">
                        <div className="text-muted small">Starting Cash</div>
                        <div className="fs-5 fw-bold">${currentShift.startingCash.toFixed(2)}</div>
                      </div>
                    </Col>
                    <Col>
                      <div className="text-center">
                        <div className="text-muted small">Total Orders</div>
                        <div className="fs-5 fw-bold">{currentShift.totalOrders}</div>
                      </div>
                    </Col>
                    <Col>
                      <div className="text-center">
                        <div className="text-muted small">Total Sales</div>
                        <div className="fs-5 fw-bold text-success">
                          ${currentShift.totalSales.toFixed(2)}
                        </div>
                      </div>
                    </Col>
                  </Row>

                  <Alert variant="info">
                    <small>
                      Shift is active. Remember to close your shift at the end of your work day.
                    </small>
                  </Alert>

                  <Button
                    variant="danger"
                    className="w-100"
                    onClick={() => setShowCloseModal(true)}
                    disabled={isLoading}
                  >
                    <FaStop className="me-2" />
                    Close Shift
                  </Button>
                </div>
              ) : (
                <div>
                  <Alert variant="warning">
                    <FaCashRegister className="me-2" />
                    No active shift. Open a shift to start processing orders.
                  </Alert>

                  <Form onSubmit={handleSubmitOpen(handleOpenShift)}>
                    <Form.Group className="mb-3">
                      <Form.Label>Starting Cash Amount</Form.Label>
                      <div className="input-group">
                        <span className="input-group-text">$</span>
                        <Form.Control
                          type="number"
                          step="0.01"
                          {...registerOpen('startingCash', { valueAsNumber: true })}
                          isInvalid={!!openErrors.startingCash}
                          disabled={isLoading}
                        />
                        <Form.Control.Feedback type="invalid">
                          {openErrors.startingCash?.message}
                        </Form.Control.Feedback>
                      </div>
                      <Form.Text className="text-muted">
                        Enter the amount of cash in the register to start the shift
                      </Form.Text>
                    </Form.Group>

                    <Form.Group className="mb-3">
                      <Form.Label>Notes (Optional)</Form.Label>
                      <Form.Control
                        as="textarea"
                        rows={2}
                        {...registerOpen('notes')}
                        disabled={isLoading}
                        placeholder="Any notes for this shift..."
                      />
                    </Form.Group>

                    <Button
                      type="submit"
                      variant="success"
                      className="w-100"
                      size="lg"
                      disabled={isLoading}
                    >
                      {isLoading ? (
                        <>
                          <span 
                            className="spinner-border spinner-border-sm me-2" 
                            style={{ width: '1rem', height: '1rem' }}
                          />
                          Opening Shift...
                        </>
                      ) : (
                        <>
                          <FaPlay className="me-2" />
                          Open Shift
                        </>
                      )}
                    </Button>
                  </Form>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>

        <Col lg={6}>
          {/* Shift Information */}
          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">Shift Information</h5>
            </Card.Header>
            <Card.Body>
              <dl className="row mb-0">
                <dt className="col-sm-4">Cashier:</dt>
                <dd className="col-sm-8">{user?.firstName} {user?.lastName}</dd>

                <dt className="col-sm-4">Store:</dt>
                <dd className="col-sm-8">{user?.storeName || 'N/A'}</dd>

                <dt className="col-sm-4">Current Time:</dt>
                <dd className="col-sm-8">{format(new Date(), 'dd/MM/yyyy HH:mm:ss')}</dd>

                <dt className="col-sm-4">Shift Status:</dt>
                <dd className="col-sm-8">
                  {isShiftOpen ? (
                    <Badge bg="success">Open</Badge>
                  ) : (
                    <Badge bg="secondary">Closed</Badge>
                  )}
                </dd>
              </dl>
            </Card.Body>
          </Card>

          {/* Quick Actions */}
          <Card>
            <Card.Header>
              <h5 className="mb-0">Quick Actions</h5>
            </Card.Header>
            <Card.Body>
              <div className="d-grid gap-2">
                <Button
                  variant="primary"
                  href="/pos"
                  disabled={!isShiftOpen}
                >
                  Go to POS
                </Button>
                <Button
                  variant="secondary"
                  href="/orders"
                  disabled={!isShiftOpen}
                >
                  View Orders
                </Button>
                <Button
                  variant="info"
                  href="/reports"
                  disabled={!isShiftOpen}
                >
                  View Reports
                </Button>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Close Shift Modal */}
      {showCloseModal && (
        <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Close Shift</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowCloseModal(false)}
                  disabled={isLoading}
                />
              </div>
              <div className="modal-body">
                <Form onSubmit={handleSubmitClose(handleCloseShift)}>
                  <Alert variant="info">
                    Please count the cash in the register and enter the total amount.
                  </Alert>

                  <Form.Group className="mb-3">
                    <Form.Label>Ending Cash Amount</Form.Label>
                    <div className="input-group">
                      <span className="input-group-text">$</span>
                      <Form.Control
                        type="number"
                        step="0.01"
                        {...registerClose('endingCash', { valueAsNumber: true })}
                        isInvalid={!!closeErrors.endingCash}
                        disabled={isLoading}
                        autoFocus
                      />
                      <Form.Control.Feedback type="invalid">
                        {closeErrors.endingCash?.message}
                      </Form.Control.Feedback>
                    </div>
                  </Form.Group>

                  <Form.Group className="mb-3">
                    <Form.Label>Notes (Optional)</Form.Label>
                    <Form.Control
                      as="textarea"
                      rows={2}
                      {...registerClose('notes')}
                      disabled={isLoading}
                      placeholder="Any notes about this shift..."
                    />
                  </Form.Group>

                  <div className="d-flex justify-content-end gap-2">
                    <Button
                      variant="secondary"
                      onClick={() => setShowCloseModal(false)}
                      disabled={isLoading}
                    >
                      Cancel
                    </Button>
                    <Button
                      type="submit"
                      variant="danger"
                      disabled={isLoading}
                    >
                      {isLoading ? (
                        <>
                          <span 
                            className="spinner-border spinner-border-sm me-2" 
                            style={{ width: '1rem', height: '1rem' }}
                          />
                          Closing...
                        </>
                      ) : (
                        <>
                          <FaStop className="me-2" />
                          Close Shift
                        </>
                      )}
                    </Button>
                  </div>
                </Form>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ShiftPage;
