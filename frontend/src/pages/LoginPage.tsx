import React, { useState } from 'react';
import { Container, Row, Col, Card, Form, Button, Tab, Tabs } from 'react-bootstrap';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { loginSchema, pinLoginSchema, LoginFormData, PinLoginFormData } from '@/schemas';
import { FaUser, FaLock, FaSignInAlt, FaKeyboard } from 'react-icons/fa';

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { login, pinLogin } = useAuth();
  const [isLoading, setIsLoading] = useState(false);

  // Username/Password form
  const {
    register: registerLogin,
    handleSubmit: handleSubmitLogin,
    formState: { errors: loginErrors }
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema)
  });

  // PIN form
  const {
    register: registerPin,
    handleSubmit: handleSubmitPin,
    formState: { errors: pinErrors },
    setValue
  } = useForm<PinLoginFormData>({
    resolver: zodResolver(pinLoginSchema),
    defaultValues: {
      storeId: 1 // Default store
    }
  });

  const handleLogin = async (data: LoginFormData) => {
    setIsLoading(true);
    try {
      const response = await login(data);
      // Redirect based on user role
      if (response?.role === 'Admin') {
        navigate('/orders');
      } else {
        navigate('/pos');
      }
    } catch (error) {
      console.error('Login failed:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handlePinLogin = async (data: PinLoginFormData) => {
    setIsLoading(true);
    try {
      const response = await pinLogin(data);
      // Redirect based on user role
      if (response?.role === 'Admin') {
        navigate('/orders');
      } else {
        navigate('/pos');
      }
    } catch (error) {
      console.error('PIN login failed:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handlePinPadClick = (digit: string) => {
    const currentPin = document.querySelector<HTMLInputElement>('input[name="pin"]')?.value || '';
    if (digit === 'C') {
      setValue('pin', '');
    } else if (digit === '←') {
      setValue('pin', currentPin.slice(0, -1));
    } else if (currentPin.length < 4) {
      setValue('pin', currentPin + digit);
    }
  };

  return (
    <Container className="d-flex align-items-center justify-content-center min-vh-100">
      <Row className="w-100">
        <Col md={6} lg={5} className="mx-auto">
          <Card className="shadow">
            <Card.Body className="p-5">
              <div className="text-center mb-4">
                <h1 className="h3 mb-3 fw-bold">Cookie Barrel POS</h1>
                <p className="text-muted">Sign in to continue</p>
              </div>

              <Tabs defaultActiveKey="password" className="mb-4">
                <Tab eventKey="password" title="Username Login">
                  <Form onSubmit={handleSubmitLogin(handleLogin)}>
                    <Form.Group className="mb-3">
                      <Form.Label>Username</Form.Label>
                      <div className="input-group">
                        <span className="input-group-text">
                          <FaUser />
                        </span>
                        <Form.Control
                          type="text"
                          placeholder="Enter username"
                          {...registerLogin('username')}
                          isInvalid={!!loginErrors.username}
                          disabled={isLoading}
                        />
                        <Form.Control.Feedback type="invalid">
                          {loginErrors.username?.message}
                        </Form.Control.Feedback>
                      </div>
                    </Form.Group>

                    <Form.Group className="mb-4">
                      <Form.Label>Password</Form.Label>
                      <div className="input-group">
                        <span className="input-group-text">
                          <FaLock />
                        </span>
                        <Form.Control
                          type="password"
                          placeholder="Enter password"
                          {...registerLogin('password')}
                          isInvalid={!!loginErrors.password}
                          disabled={isLoading}
                        />
                        <Form.Control.Feedback type="invalid">
                          {loginErrors.password?.message}
                        </Form.Control.Feedback>
                      </div>
                    </Form.Group>

                    <Button
                      variant="primary"
                      type="submit"
                      className="w-100"
                      size="lg"
                      disabled={isLoading}
                    >
                      {isLoading ? (
                        <>
                          <span 
                            className="spinner-border spinner-border-sm me-2" 
                            style={{ width: '1.2rem', height: '1.2rem' }}
                          />
                          Signing in...
                        </>
                      ) : (
                        <>
                          <FaSignInAlt className="me-2" />
                          Sign In
                        </>
                      )}
                    </Button>
                  </Form>

                  <div className="mt-3 text-center text-muted small">
                    Demo Accounts:<br />
                    Admin: admin / Admin123!<br />
                    Manager: manager1 / Manager123!<br />
                    Cashier: cashier2 / Cashier123!<br />
                    Customer: customer / Customer123!
                  </div>
                </Tab>

                <Tab eventKey="pin" title="PIN Login">
                  <Form onSubmit={handleSubmitPin(handlePinLogin)}>
                    <Form.Group className="mb-3">
                      <Form.Label>Store</Form.Label>
                      <Form.Select
                        {...registerPin('storeId', { valueAsNumber: true })}
                        isInvalid={!!pinErrors.storeId}
                        disabled={isLoading}
                      >
                        <option value="1">Cookie Barrel Main</option>
                        <option value="2">Cookie Barrel Westfield</option>
                        <option value="3">Cookie Barrel Airport</option>
                      </Form.Select>
                      <Form.Control.Feedback type="invalid">
                        {pinErrors.storeId?.message}
                      </Form.Control.Feedback>
                    </Form.Group>

                    <Form.Group className="mb-3">
                      <Form.Label>PIN</Form.Label>
                      <div className="input-group">
                        <span className="input-group-text">
                          <FaKeyboard />
                        </span>
                        <Form.Control
                          type="password"
                          placeholder="Enter 4-digit PIN"
                          maxLength={4}
                          {...registerPin('pin')}
                          isInvalid={!!pinErrors.pin}
                          disabled={isLoading}
                          className="text-center fs-3"
                        />
                        <Form.Control.Feedback type="invalid">
                          {pinErrors.pin?.message}
                        </Form.Control.Feedback>
                      </div>
                    </Form.Group>

                    {/* PIN Pad */}
                    <div className="mb-3">
                      <Row className="g-2">
                        {['1', '2', '3', '4', '5', '6', '7', '8', '9', 'C', '0', '←'].map((digit) => (
                          <Col xs={4} key={digit}>
                            <Button
                              variant="outline-secondary"
                              className="w-100 py-3 fs-5"
                              onClick={() => handlePinPadClick(digit)}
                              type="button"
                              disabled={isLoading}
                            >
                              {digit}
                            </Button>
                          </Col>
                        ))}
                      </Row>
                    </div>

                    <Button
                      variant="primary"
                      type="submit"
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
                          Signing in...
                        </>
                      ) : (
                        <>
                          <FaSignInAlt className="me-2" />
                          Sign In with PIN
                        </>
                      )}
                    </Button>
                  </Form>

                  <div className="mt-3 text-center text-muted small">
                    Demo PINs:<br />
                    Admin: 9999<br />
                    Manager (Store 1): 1001<br />
                    Cashier (Store 1): 2002, 2003, 2004
                  </div>
                </Tab>
              </Tabs>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default LoginPage;
