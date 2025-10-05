import EnhancedReceiptSettings from '../components/EnhancedReceiptSettings';import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Tab, Nav, Form, Alert, Badge, Spinner } from 'react-bootstrap';
import { FaCog, FaSave, FaUndo, FaEnvelope, FaEye, FaFileExport, FaFileImport, FaHistory } from 'react-icons/fa';
import { toast } from 'react-toastify';
import systemSettingsService, { 
  ReceiptSettingsDto, 
  EmailSettingsDto, 
  DefaultValuesDto 
} from '../services/systemSettings.service';
import { clearSettingsCache } from '@/hooks/useSystemSettings';

// Enhanced Receipt Settings with new fields
interface EnhancedReceiptSettings extends ReceiptSettingsDto {
  showBarcode?: boolean;
  showQRCode?: boolean;
  showCustomerInfo?: boolean;
  printMarginTop?: number;
  printMarginBottom?: number;
  printMarginLeft?: number;
  printMarginRight?: number;
}

// Enhanced Email Settings
interface EnhancedEmailSettings extends EmailSettingsDto {
  enableLowStockAlerts?: boolean;
  enableDailySalesReport?: boolean;
}

// Enhanced Default Values
interface EnhancedDefaultValues extends DefaultValuesDto {
  autoOpenCashDrawer?: boolean;
  enableBarcodeLookup?: boolean;
  enableQuickSale?: boolean;
  passwordMinLength?: number;
  requireStrongPassword?: boolean;
}

const SystemSettingsPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('receipt');
  const [loading, setLoading] = useState(false);
  const [showPreview, setShowPreview] = useState(false);
  
  // Enhanced Settings state
  const [receiptSettings, setReceiptSettings] = useState<EnhancedReceiptSettings>({
    headerText: '',
    footerText: '',
    showLogo: true,
    showTaxDetails: true,
    showItemDetails: true,
    showBarcode: false,
    showQRCode: false,
    showCustomerInfo: true,
    paperSize: '80mm',
    fontSize: 12,
    receiptTemplate: 'standard',
    printMarginTop: 0,
    printMarginBottom: 10,
    printMarginLeft: 5,
    printMarginRight: 5
  });

  const [emailSettings, setEmailSettings] = useState<EnhancedEmailSettings>({
    smtpHost: '',
    smtpPort: 587,
    smtpUsername: '',
    smtpPassword: '',
    smtpUseSsl: true,
    fromEmail: '',
    fromName: '',
    enableEmailReceipts: false,
    enableEmailNotifications: false,
    enableLowStockAlerts: false,
    enableDailySalesReport: false
  });

  const [defaultValues, setDefaultValues] = useState<EnhancedDefaultValues>({
    defaultTaxRate: 10.0,
    defaultLowStockThreshold: 10,
    defaultOrderStatus: 'Pending',
    defaultPaymentMethod: 'Cash',
    sessionTimeoutMinutes: 60,
    receiptPrintCopies: 1,
    autoPrintReceipt: true,
    requireCustomerForOrder: false,
    autoOpenCashDrawer: true,
    enableBarcodeLookup: true,
    enableQuickSale: true,
    passwordMinLength: 6,
    requireStrongPassword: false
  });

  const [testEmail, setTestEmail] = useState('');
  const [dateTimePreview, setDateTimePreview] = useState('');

  useEffect(() => {
    loadSettings();
    updateDateTimePreview();
  }, []);

  const updateDateTimePreview = () => {
    const now = new Date();
    setDateTimePreview(now.toLocaleString());
  };

  const loadSettings = async () => {
    try {
      setLoading(true);
      const [receipt, email, defaults] = await Promise.all([
        systemSettingsService.getReceiptSettings(),
        systemSettingsService.getEmailSettings(),
        systemSettingsService.getDefaultValues()
      ]);

      // 🔍 DEBUG: Log loaded settings
      console.log('📥 [SystemSettings] Loaded from backend:', {
        receiptTemplate: receipt.receiptTemplate,
        paperSize: receipt.paperSize,
        fontSize: receipt.fontSize,
        timestamp: new Date().toISOString()
      });

      // Properly merge loaded settings with defaults, prioritizing loaded values
      setReceiptSettings(prev => {
        const newSettings = {
          ...prev,
          ...receipt,
          // Ensure critical fields are set
          receiptTemplate: receipt.receiptTemplate || prev.receiptTemplate,
          paperSize: receipt.paperSize || prev.paperSize,
          fontSize: receipt.fontSize || prev.fontSize
        };
        
        // 🔍 DEBUG: Log merged state
        console.log('✅ [SystemSettings] Merged state:', {
          receiptTemplate: newSettings.receiptTemplate,
          paperSize: newSettings.paperSize,
          fontSize: newSettings.fontSize
        });
        
        return newSettings;
      });
      
      setEmailSettings(prev => ({ ...prev, ...email }));
      setDefaultValues(prev => ({ ...prev, ...defaults }));
    } catch (error) {
      console.error('❌ [SystemSettings] Failed to load:', error);
      toast.error('Failed to load settings');
    } finally {
      setLoading(false);
    }
  };

  const handleSaveReceipt = async (settingsToSave?: EnhancedReceiptSettings) => {
    try {
      setLoading(true);
      
      // Use settings passed from child component, or fall back to parent state
      const finalSettings = settingsToSave || receiptSettings;
      
      // 🔍 DEBUG: Log what we're saving
      console.log('💾 [SystemSettings] Saving receipt settings:', {
        receiptTemplate: finalSettings.receiptTemplate,
        paperSize: finalSettings.paperSize,
        fontSize: finalSettings.fontSize,
        source: settingsToSave ? 'from child component' : 'from parent state'
      });
      
      await systemSettingsService.updateReceiptSettings(finalSettings);
      
      // Update parent state with the saved settings
      setReceiptSettings(finalSettings);
      
      clearSettingsCache(); // Clear cache so new settings take effect immediately
      
      // 🆕 CRITICAL: Reload settings to verify save
      console.log('🔄 [SystemSettings] Reloading to verify save...');
      await loadSettings();
      
      toast.success('Receipt settings saved successfully');
    } catch (error: any) {
      console.error('❌ [SystemSettings] Save failed:', error);
      toast.error(error.response?.data?.message || 'Failed to save receipt settings');
    } finally {
      setLoading(false);
    }
  };

  const handleSaveEmail = async () => {
    // Validate email settings
    if (emailSettings.enableEmailReceipts || emailSettings.enableEmailNotifications) {
      if (!emailSettings.smtpHost || !emailSettings.fromEmail) {
        toast.error('SMTP Host and From Email are required when email features are enabled');
        return;
      }
    }

    try {
      setLoading(true);
      await systemSettingsService.updateEmailSettings(emailSettings);
      clearSettingsCache(); // Clear cache so new settings take effect immediately
      toast.success('Email settings saved successfully');
    } catch (error: any) {
      console.error('Failed to save:', error);
      toast.error(error.response?.data?.message || 'Failed to save email settings');
    } finally {
      setLoading(false);
    }
  };

  const handleTestEmail = async () => {
    if (!testEmail) {
      toast.error('Please enter a test email address');
      return;
    }

    if (!emailSettings.smtpHost || !emailSettings.fromEmail) {
      toast.error('Please configure SMTP settings first');
      return;
    }

    try {
      setLoading(true);
      const success = await systemSettingsService.testEmailSettings(emailSettings, testEmail);
      if (success) {
        toast.success('Test email sent successfully! Check your inbox.');
      } else {
        toast.error('Failed to send test email. Please check your SMTP settings.');
      }
    } catch (error: any) {
      console.error('Failed to test email:', error);
      toast.error(error.response?.data?.message || 'Failed to send test email');
    } finally {
      setLoading(false);
    }
  };

  const handleSaveDefaults = async () => {
    // Validate password settings
    if (defaultValues.passwordMinLength && (defaultValues.passwordMinLength < 4 || defaultValues.passwordMinLength > 20)) {
      toast.error('Password minimum length must be between 4 and 20 characters');
      return;
    }

    try {
      setLoading(true);
      await systemSettingsService.updateDefaultValues(defaultValues);
      clearSettingsCache(); // Clear cache so new settings take effect immediately
      toast.success('Default values saved successfully');
    } catch (error: any) {
      console.error('Failed to save:', error);
      toast.error(error.response?.data?.message || 'Failed to save default values');
    } finally {
      setLoading(false);
    }
  };

  const handleReset = async () => {
    if (!window.confirm('Are you sure you want to reset all settings to defaults? This cannot be undone.')) return;

    try {
      setLoading(true);
      await systemSettingsService.resetToDefaults();
      clearSettingsCache(); // Clear cache so reset takes effect immediately
      toast.success('Settings reset to defaults');
      await loadSettings();
    } catch (error) {
      console.error('Failed to reset:', error);
      toast.error('Failed to reset settings');
    } finally {
      setLoading(false);
    }
  };

  const handleExportSettings = async () => {
    try {
      const allSettings = {
        receipt: receiptSettings,
        email: { ...emailSettings, smtpPassword: '***ENCRYPTED***' }, // Don't export password
        defaults: defaultValues,
        exportedAt: new Date().toISOString()
      };
      
      const dataStr = JSON.stringify(allSettings, null, 2);
      const dataBlob = new Blob([dataStr], { type: 'application/json' });
      const url = URL.createObjectURL(dataBlob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `system-settings-${new Date().toISOString().split('T')[0]}.json`;
      link.click();
      URL.revokeObjectURL(url);
      
      toast.success('Settings exported successfully');
    } catch (error) {
      console.error('Failed to export:', error);
      toast.error('Failed to export settings');
    }
  };

  const handleImportSettings = () => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';
    input.onchange = async (e: any) => {
      try {
        const file = e.target.files[0];
        const text = await file.text();
        const imported = JSON.parse(text);
        
        if (imported.receipt) setReceiptSettings({ ...receiptSettings, ...imported.receipt });
        if (imported.defaults) setDefaultValues({ ...defaultValues, ...imported.defaults });
        // Don't import email password for security
        
        toast.success('Settings imported successfully. Please review and save.');
      } catch (error) {
        console.error('Failed to import:', error);
        toast.error('Failed to import settings. Please check the file format.');
      }
    };
    input.click();
  };

  if (loading && !receiptSettings.headerText) {
    return (
      <Container fluid className="py-5 text-center">
        <Spinner animation="border" />
        <p className="mt-3">Loading settings...</p>
      </Container>
    );
  }

  return (
    <Container fluid className="py-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="mb-1">
            <FaCog className="me-2" />
            System Settings
          </h2>
          <p className="text-muted mb-0">Configure system</p>
        </div>
        <div>
          <Button variant="outline-info" onClick={handleExportSettings} className="me-2" size="sm">
            <FaFileExport className="me-2" />
            Export
          </Button>
          <Button variant="outline-info" onClick={handleImportSettings} className="me-2" size="sm">
            <FaFileImport className="me-2" />
            Import
          </Button>
          <Button variant="outline-danger" onClick={handleReset} size="sm">
            <FaUndo className="me-2" />
            Reset to Defaults
          </Button>
        </div>
      </div>

      <Alert variant="info" className="mb-4">
        <strong>Note:</strong> Company information, currency, and tax settings are managed in <strong>Store Settings</strong>. 
        Theme and appearance settings are in <strong>Theme Settings</strong>.
      </Alert>

      <Card className="border-0 shadow-sm">
        <Card.Body>
          <Tab.Container activeKey={activeTab} onSelect={(k) => setActiveTab(k || 'receipt')}>
            <Nav variant="tabs" className="mb-4">
              <Nav.Item>
                <Nav.Link eventKey="receipt">
                  Receipt Template
                  <Badge bg="success" className="ms-2">✓</Badge>
                </Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="email">
                  Email Settings
                  {emailSettings.enableEmailReceipts && <Badge bg="success" className="ms-2">Active</Badge>}
                </Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="defaults">
                  Default Values
                  <Badge bg="success" className="ms-2">✓</Badge>
                </Nav.Link>
              </Nav.Item>
            </Nav>

            <Tab.Content>
              {/* Receipt Settings - Using Enhanced Component */}
              <Tab.Pane eventKey="receipt">
                <EnhancedReceiptSettings 
                  key={receiptSettings.receiptTemplate}
                  initialSettings={receiptSettings}
                  onSave={handleSaveReceipt}
                />
              </Tab.Pane>

              {/* Email Settings */}
              <Tab.Pane eventKey="email">
                <Alert variant="info" className="mb-4">
                  <strong>📧 Email Configuration</strong><br />
                  Configure SMTP settings to send email receipts and notifications. Your password will be encrypted before storage.
                </Alert>

                <Row>
                  <Col lg={6}>
                    <Card className="mb-4">
                      <Card.Header>
                        <h6 className="mb-0">SMTP Server</h6>
                      </Card.Header>
                      <Card.Body>
                        <Form.Group className="mb-3">
                          <Form.Label>SMTP Host *</Form.Label>
                          <Form.Control
                            type="text"
                            value={emailSettings.smtpHost}
                            onChange={(e) => setEmailSettings({...emailSettings, smtpHost: e.target.value})}
                            placeholder="smtp.gmail.com"
                            required
                          />
                          <Form.Text className="text-muted">
                            Common: smtp.gmail.com, smtp.office365.com, smtp.sendgrid.net
                          </Form.Text>
                        </Form.Group>

                        <Row>
                          <Col md={6}>
                            <Form.Group className="mb-3">
                              <Form.Label>SMTP Port *</Form.Label>
                              <Form.Control
                                type="number"
                                value={emailSettings.smtpPort}
                                onChange={(e) => setEmailSettings({...emailSettings, smtpPort: parseInt(e.target.value)})}
                                required
                              />
                              <Form.Text className="text-muted">
                                Common: 587 (TLS), 465 (SSL), 25
                              </Form.Text>
                            </Form.Group>
                          </Col>
                          <Col md={6}>
                            <Form.Group className="mb-3">
                              <Form.Label>Security</Form.Label>
                              <Form.Check
                                type="checkbox"
                                label="Use SSL/TLS"
                                checked={emailSettings.smtpUseSsl}
                                onChange={(e) => setEmailSettings({...emailSettings, smtpUseSsl: e.target.checked})}
                                className="mt-2"
                              />
                            </Form.Group>
                          </Col>
                        </Row>

                        <Form.Group className="mb-3">
                          <Form.Label>SMTP Username</Form.Label>
                          <Form.Control
                            type="text"
                            value={emailSettings.smtpUsername}
                            onChange={(e) => setEmailSettings({...emailSettings, smtpUsername: e.target.value})}
                            placeholder="your-email@example.com"
                          />
                        </Form.Group>

                        <Form.Group className="mb-3">
                          <Form.Label>SMTP Password 🔒</Form.Label>
                          <Form.Control
                            type="password"
                            value={emailSettings.smtpPassword}
                            onChange={(e) => setEmailSettings({...emailSettings, smtpPassword: e.target.value})}
                            placeholder="••••••••"
                          />
                          <Form.Text className="text-muted">
                            ✅ Password will be encrypted before storage
                          </Form.Text>
                        </Form.Group>
                      </Card.Body>
                    </Card>
                  </Col>

                  <Col lg={6}>
                    <Card className="mb-4">
                      <Card.Header>
                        <h6 className="mb-0">Sender Information</h6>
                      </Card.Header>
                      <Card.Body>
                        <Form.Group className="mb-3">
                          <Form.Label>From Email *</Form.Label>
                          <Form.Control
                            type="email"
                            value={emailSettings.fromEmail}
                            onChange={(e) => setEmailSettings({...emailSettings, fromEmail: e.target.value})}
                            placeholder="noreply@yourstore.com"
                            required
                          />
                        </Form.Group>

                        <Form.Group className="mb-3">
                          <Form.Label>From Name</Form.Label>
                          <Form.Control
                            type="text"
                            value={emailSettings.fromName}
                            onChange={(e) => setEmailSettings({...emailSettings, fromName: e.target.value})}
                            placeholder="Your Store Name"
                          />
                        </Form.Group>
                      </Card.Body>
                    </Card>

                    <Card className="mb-4">
                      <Card.Header>
                        <h6 className="mb-0">Email Features</h6>
                      </Card.Header>
                      <Card.Body>
                        <Form.Check
                          type="checkbox"
                          label="✉️ Enable Email Receipts"
                          checked={emailSettings.enableEmailReceipts}
                          onChange={(e) => setEmailSettings({...emailSettings, enableEmailReceipts: e.target.checked})}
                          className="mb-2"
                        />
                        <Form.Check
                          type="checkbox"
                          label="🔔 Enable Email Notifications"
                          checked={emailSettings.enableEmailNotifications}
                          onChange={(e) => setEmailSettings({...emailSettings, enableEmailNotifications: e.target.checked})}
                          className="mb-2"
                        />
                        <Form.Check
                          type="checkbox"
                          label="📦 Low Stock Alerts"
                          checked={emailSettings.enableLowStockAlerts}
                          onChange={(e) => setEmailSettings({...emailSettings, enableLowStockAlerts: e.target.checked})}
                          className="mb-2"
                        />
                        <Form.Check
                          type="checkbox"
                          label="📊 Daily Sales Report"
                          checked={emailSettings.enableDailySalesReport}
                          onChange={(e) => setEmailSettings({...emailSettings, enableDailySalesReport: e.target.checked})}
                        />
                      </Card.Body>
                    </Card>

                    <Card className="bg-light">
                      <Card.Body>
                        <h6 className="mb-3">🧪 Test Email Configuration</h6>
                        <Row>
                          <Col md={8}>
                            <Form.Control
                              type="email"
                              placeholder="Enter test email address"
                              value={testEmail}
                              onChange={(e) => setTestEmail(e.target.value)}
                            />
                          </Col>
                          <Col md={4}>
                            <Button 
                              variant="outline-primary" 
                              onClick={handleTestEmail} 
                              disabled={loading}
                              className="w-100"
                            >
                              {loading ? <Spinner animation="border" size="sm" /> : <FaEnvelope className="me-2" />}
                              Send Test
                            </Button>
                          </Col>
                        </Row>
                        <Form.Text className="text-muted d-block mt-2">
                          Send a test email to verify your configuration works correctly
                        </Form.Text>
                      </Card.Body>
                    </Card>
                  </Col>
                </Row>

                <Button variant="primary" onClick={handleSaveEmail} disabled={loading} className="mt-3">
                  {loading ? <Spinner animation="border" size="sm" className="me-2" /> : <FaSave className="me-2" />}
                  Save Email Settings
                </Button>
              </Tab.Pane>

              {/* Default Values */}
              <Tab.Pane eventKey="defaults">
                <Row>
                  <Col lg={6}>
                    <Card className="mb-4">
                      <Card.Header>
                        <h6 className="mb-0">Transaction Defaults</h6>
                      </Card.Header>
                      <Card.Body>
                        <Row>
                          <Col md={6}>
                            <Form.Group className="mb-3">
                              <Form.Label>Default Tax Rate (%)</Form.Label>
                              <Form.Control
                                type="number"
                                step="0.01"
                                min="0"
                                max="100"
                                value={defaultValues.defaultTaxRate}
                                onChange={(e) => setDefaultValues({...defaultValues, defaultTaxRate: parseFloat(e.target.value)})}
                              />
                              <Form.Text className="text-muted">
                                Applied to all new products
                              </Form.Text>
                            </Form.Group>
                          </Col>
                          <Col md={6}>
                            <Form.Group className="mb-3">
                              <Form.Label>Default Payment Method</Form.Label>
                              <Form.Select
                                value={defaultValues.defaultPaymentMethod}
                                onChange={(e) => setDefaultValues({...defaultValues, defaultPaymentMethod: e.target.value})}
                              >
                                <option value="Cash">💵 Cash</option>
                                <option value="Card">💳 Credit/Debit Card</option>
                                <option value="Mobile">📱 Mobile Payment</option>
                                <option value="Bank Transfer">🏦 Bank Transfer</option>
                                <option value="Split">🔀 Split Payment</option>
                              </Form.Select>
                            </Form.Group>
                          </Col>
                        </Row>

                        <Form.Check
                          type="checkbox"
                          label="Require customer information for all orders"
                          checked={defaultValues.requireCustomerForOrder}
                          onChange={(e) => setDefaultValues({...defaultValues, requireCustomerForOrder: e.target.checked})}
                        />
                      </Card.Body>
                    </Card>

                    <Card className="mb-4">
                      <Card.Header>
                        <h6 className="mb-0">Inventory Defaults</h6>
                      </Card.Header>
                      <Card.Body>
                        <Form.Group className="mb-3">
                          <Form.Label>Low Stock Threshold</Form.Label>
                          <Form.Control
                            type="number"
                            min="0"
                            value={defaultValues.defaultLowStockThreshold}
                            onChange={(e) => setDefaultValues({...defaultValues, defaultLowStockThreshold: parseInt(e.target.value)})}
                          />
                          <Form.Text className="text-muted">
                            Alert when stock falls below this number
                          </Form.Text>
                        </Form.Group>
                      </Card.Body>
                    </Card>

                    <Card className="mb-4">
                      <Card.Header>
                        <h6 className="mb-0">Receipt & Printing</h6>
                      </Card.Header>
                      <Card.Body>
                        <Form.Group className="mb-3">
                          <Form.Label>Receipt Print Copies</Form.Label>
                          <Form.Control
                            type="number"
                            min="1"
                            max="5"
                            value={defaultValues.receiptPrintCopies}
                            onChange={(e) => setDefaultValues({...defaultValues, receiptPrintCopies: parseInt(e.target.value)})}
                          />
                        </Form.Group>

                        <Form.Check
                          type="checkbox"
                          label="🖨️ Auto print receipt after payment"
                          checked={defaultValues.autoPrintReceipt}
                          onChange={(e) => setDefaultValues({...defaultValues, autoPrintReceipt: e.target.checked})}
                          className="mb-2"
                        />
                        <Form.Check
                          type="checkbox"
                          label="💰 Auto open cash drawer"
                          checked={defaultValues.autoOpenCashDrawer}
                          onChange={(e) => setDefaultValues({...defaultValues, autoOpenCashDrawer: e.target.checked})}
                        />
                      </Card.Body>
                    </Card>
                  </Col>

                  <Col lg={6}>
                    <Card className="mb-4">
                      <Card.Header>
                        <h6 className="mb-0">Security & Session</h6>
                      </Card.Header>
                      <Card.Body>
                        <Form.Group className="mb-3">
                          <Form.Label>Session Timeout (minutes)</Form.Label>
                          <Form.Control
                            type="number"
                            min="5"
                            max="1440"
                            value={defaultValues.sessionTimeoutMinutes}
                            onChange={(e) => setDefaultValues({...defaultValues, sessionTimeoutMinutes: parseInt(e.target.value)})}
                          />
                          <Form.Text className="text-muted">
                            Auto logout after inactivity (5-1440 minutes)
                          </Form.Text>
                        </Form.Group>

                        <Form.Group className="mb-3">
                          <Form.Label>Password Minimum Length</Form.Label>
                          <Form.Control
                            type="number"
                            min="4"
                            max="20"
                            value={defaultValues.passwordMinLength}
                            onChange={(e) => setDefaultValues({...defaultValues, passwordMinLength: parseInt(e.target.value)})}
                          />
                        </Form.Group>

                        <Form.Check
                          type="checkbox"
                          label="🔐 Require strong passwords (uppercase, lowercase, numbers)"
                          checked={defaultValues.requireStrongPassword}
                          onChange={(e) => setDefaultValues({...defaultValues, requireStrongPassword: e.target.checked})}
                        />
                      </Card.Body>
                    </Card>

                    <Card className="mb-4">
                      <Card.Header>
                        <h6 className="mb-0">POS Features</h6>
                      </Card.Header>
                      <Card.Body>
                        <Form.Check
                          type="checkbox"
                          label="📷 Enable barcode scanner lookup"
                          checked={defaultValues.enableBarcodeLookup}
                          onChange={(e) => setDefaultValues({...defaultValues, enableBarcodeLookup: e.target.checked})}
                          className="mb-2"
                        />
                        <Form.Check
                          type="checkbox"
                          label="⚡ Enable quick sale mode"
                          checked={defaultValues.enableQuickSale}
                          onChange={(e) => setDefaultValues({...defaultValues, enableQuickSale: e.target.checked})}
                        />
                        <Form.Text className="text-muted d-block mt-2">
                          Quick sale allows faster checkout for common items
                        </Form.Text>
                      </Card.Body>
                    </Card>

                    <Card className="bg-info bg-opacity-10">
                      <Card.Body>
                        <h6 className="text-info">💡 Pro Tip</h6>
                        <p className="mb-0 small">
                          These default values can be overridden for individual stores in Store Settings. 
                          System-wide defaults apply to all new stores and products.
                        </p>
                      </Card.Body>
                    </Card>
                  </Col>
                </Row>

                <Button variant="primary" onClick={handleSaveDefaults} disabled={loading} className="mt-3">
                  {loading ? <Spinner animation="border" size="sm" className="me-2" /> : <FaSave className="me-2" />}
                  Save Default Values
                </Button>
              </Tab.Pane>
            </Tab.Content>
          </Tab.Container>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default SystemSettingsPage;
