import React, { useState, useRef, useEffect } from 'react';
import { Card, Button, Form, Row, Col, Modal, Badge, ListGroup, Alert } from 'react-bootstrap';
import { FaPrint, FaEye, FaEdit, FaSave, FaPlus, FaTrash, FaTimes, FaArrowUp, FaArrowDown } from 'react-icons/fa';
import { toast } from 'react-toastify';

// Extended Receipt Templates - 8 templates instead of 3
export const RECEIPT_TEMPLATES = [
  { 
    id: 'standard', 
    name: 'Standard', 
    description: 'Classic receipt layout with all details',
    icon: '📄'
  },
  { 
    id: 'compact', 
    name: 'Compact', 
    description: 'Minimal design, saves paper',
    icon: '📃'
  },
  { 
    id: 'detailed', 
    name: 'Detailed', 
    description: 'Comprehensive with product descriptions',
    icon: '📋'
  },
  { 
    id: 'modern', 
    name: 'Modern', 
    description: 'Clean, contemporary design with icons',
    icon: '✨'
  },
  { 
    id: 'elegant', 
    name: 'Elegant', 
    description: 'Sophisticated layout for premium stores',
    icon: '💎'
  },
  { 
    id: 'minimalist', 
    name: 'Minimalist', 
    description: 'Ultra-simple, text-only format',
    icon: '⚪'
  },
  { 
    id: 'thermal', 
    name: 'Thermal Printer', 
    description: 'Optimized for 58mm/80mm thermal printers',
    icon: '🖨️'
  },
  { 
    id: 'custom', 
    name: 'Custom Template', 
    description: 'Create your own design',
    icon: '🎨'
  }
];

// Template Elements that can be dragged/reordered
export const TEMPLATE_ELEMENTS = [
  { id: 'logo', label: 'Company Logo', icon: '🏢', required: false },
  { id: 'storeName', label: 'Store Name', icon: '🏪', required: true },
  { id: 'address', label: 'Store Address', icon: '📍', required: false },
  { id: 'phone', label: 'Phone Number', icon: '📞', required: false },
  { id: 'email', label: 'Email Address', icon: '✉️', required: false },
  { id: 'website', label: 'Website', icon: '🌐', required: false },
  { id: 'header', label: 'Header Text', icon: '📌', required: false },
  { id: 'divider', label: 'Divider Line', icon: '━━━', required: false },
  { id: 'receiptNumber', label: 'Receipt Number', icon: '#️⃣', required: true },
  { id: 'date', label: 'Date & Time', icon: '📅', required: true },
  { id: 'cashier', label: 'Cashier Name', icon: '👤', required: false },
  { id: 'customer', label: 'Customer Info', icon: '👥', required: false },
  { id: 'items', label: 'Item List', icon: '📦', required: true },
  { id: 'subtotal', label: 'Subtotal', icon: '💵', required: true },
  { id: 'tax', label: 'Tax Details', icon: '📊', required: false },
  { id: 'discount', label: 'Discounts', icon: '🏷️', required: false },
  { id: 'total', label: 'Total Amount', icon: '💰', required: true },
  { id: 'payment', label: 'Payment Method', icon: '💳', required: false },
  { id: 'change', label: 'Change Given', icon: '💴', required: false },
  { id: 'barcode', label: 'Barcode', icon: '|||', required: false },
  { id: 'qrcode', label: 'QR Code', icon: '▪️', required: false },
  { id: 'footer', label: 'Footer Text', icon: '📝', required: false },
  { id: 'social', label: 'Social Media Links', icon: '🌐', required: false },
  { id: 'warranty', label: 'Warranty/Return Info', icon: '🛡️', required: false },
  { id: 'promotions', label: 'Promotional Message', icon: '🎁', required: false }
];

// Predefined template layouts
const TEMPLATE_LAYOUTS: Record<string, string[]> = {
  standard: [
    'logo', 'storeName', 'address', 'phone', 'divider', 
    'receiptNumber', 'date', 'cashier', 'customer', 'divider', 
    'items', 'divider', 'subtotal', 'tax', 'total', 
    'payment', 'divider', 'barcode', 'footer'
  ],
  compact: [
    'storeName', 'receiptNumber', 'date', 'divider', 
    'items', 'divider', 'subtotal', 'tax', 'total', 'footer'
  ],
  detailed: [
    'logo', 'storeName', 'address', 'phone', 'email', 'website', 'header', 'divider',
    'receiptNumber', 'date', 'cashier', 'customer', 'divider',
    'items', 'divider', 'subtotal', 'tax', 'discount', 'total',
    'payment', 'change', 'divider', 'barcode', 'qrcode', 
    'footer', 'social', 'warranty'
  ],
  modern: [
    'logo', 'storeName', 'divider', 'receiptNumber', 'date', 
    'customer', 'divider', 'items', 'divider', 
    'subtotal', 'tax', 'total', 'payment', 
    'divider', 'qrcode', 'footer'
  ],
  elegant: [
    'logo', 'storeName', 'address', 'divider',
    'receiptNumber', 'date', 'divider', 'items', 'divider',
    'subtotal', 'tax', 'total', 'divider', 'footer', 'social'
  ],
  minimalist: [
    'storeName', 'receiptNumber', 'date', 'items', 'total'
  ],
  thermal: [
    'storeName', 'divider', 'receiptNumber', 'date', 'divider',
    'items', 'divider', 'subtotal', 'tax', 'total', 
    'payment', 'divider', 'barcode', 'footer'
  ]
};

interface ReceiptSettings {
  headerText: string;
  footerText: string;
  showLogo: boolean;
  showTaxDetails: boolean;
  showItemDetails: boolean;
  showBarcode: boolean;
  showQRCode: boolean;
  showCustomerInfo: boolean;
  showCashier: boolean;
  showSocial: boolean;
  showPromotion: boolean;
  paperSize: string;
  fontSize: number;
  fontFamily: string;
  receiptTemplate: string;
  customTemplate?: string;
  printMarginTop: number;
  printMarginBottom: number;
  printMarginLeft: number;
  printMarginRight: number;
  storeName: string;
  storeAddress: string;
  storePhone: string;
  storeEmail: string;
  storeWebsite: string;
  logoUrl: string;
  promotionText: string;
}

interface EnhancedReceiptSettingsProps {
  initialSettings?: Partial<ReceiptSettings>;
  onSave?: (settings: ReceiptSettings) => Promise<void>;
}

const EnhancedReceiptSettings: React.FC<EnhancedReceiptSettingsProps> = ({ 
  initialSettings,
  onSave 
}) => {
  const [showPreview, setShowPreview] = useState(true);
  const [showTemplateEditor, setShowTemplateEditor] = useState(false);
  const [showPrintDialog, setShowPrintDialog] = useState(false);
  const [saving, setSaving] = useState(false);
  const printRef = useRef<HTMLDivElement>(null);

  const defaultSettings: ReceiptSettings = {
    headerText: 'Thank you for your purchase!',
    footerText: 'Please visit us again',
    showLogo: true,
    showTaxDetails: true,
    showItemDetails: true,
    showBarcode: true,
    showQRCode: false,
    showCustomerInfo: true,
    showCashier: true,
    showSocial: false,
    showPromotion: false,
    paperSize: '80mm',
    fontSize: 12,
    fontFamily: 'monospace',
    receiptTemplate: 'standard',
    printMarginTop: 5,
    printMarginBottom: 10,
    printMarginLeft: 5,
    printMarginRight: 5,
    storeName: 'My Store',
    storeAddress: '123 Main Street, City, State 12345',
    storePhone: '(555) 123-4567',
    storeEmail: 'info@mystore.com',
    storeWebsite: 'www.mystore.com',
    logoUrl: '',
    promotionText: '🎁 Join our loyalty program and save 10% on your next purchase!'
  };

  const [settings, setSettings] = useState<ReceiptSettings>({
    ...defaultSettings,
    ...initialSettings
  });

  // Update settings when initialSettings change (e.g., after loading from backend)
  useEffect(() => {
    if (initialSettings && initialSettings.receiptTemplate) {
      console.log('📥 [EnhancedReceipt] Received initialSettings:', {
        receiptTemplate: initialSettings.receiptTemplate,
        paperSize: initialSettings.paperSize,
        fontSize: initialSettings.fontSize,
        timestamp: new Date().toISOString()
      });
      
      setSettings(prev => {
        const newSettings = {
          ...prev,
          ...initialSettings
        };
        
        console.log('✅ [EnhancedReceipt] Updated local state:', {
          receiptTemplate: newSettings.receiptTemplate,
          paperSize: newSettings.paperSize,
          fontSize: newSettings.fontSize
        });
        
        return newSettings;
      });
    } else {
      console.log('⚠️ [EnhancedReceipt] No initialSettings or receiptTemplate missing');
    }
  }, [initialSettings, initialSettings?.receiptTemplate]); // Watch both

  const [customElements, setCustomElements] = useState<string[]>(
    TEMPLATE_LAYOUTS.standard
  );

  // Sample receipt data for preview
  const sampleReceipt = {
    receiptNumber: 'RCP-2025-001234',
    date: new Date().toLocaleString('en-US', { 
      year: 'numeric', 
      month: '2-digit', 
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    }),
    cashier: 'John Doe',
    customer: {
      name: 'Jane Smith',
      phone: '(555) 987-6543',
      email: 'jane.smith@email.com'
    },
    items: [
      { name: 'Premium Coffee Beans (1kg)', sku: 'COF-001', qty: 2, price: 15.99, total: 31.98 },
      { name: 'Organic Milk (1L)', sku: 'MLK-005', qty: 1, price: 4.50, total: 4.50 },
      { name: 'Fresh Bread', sku: 'BRD-012', qty: 3, price: 2.99, total: 8.97 }
    ],
    subtotal: 45.45,
    tax: 4.55,
    discount: 5.00,
    total: 45.00,
    paid: 50.00,
    change: 5.00,
    paymentMethod: 'Cash'
  };

  const handleSave = async () => {
    if (onSave) {
      try {
        setSaving(true);
        
        // 🔍 DEBUG: Log what we're about to pass to parent
        console.log('📤 [EnhancedReceipt] Calling onSave with settings:', {
          receiptTemplate: settings.receiptTemplate,
          paperSize: settings.paperSize,
          fontSize: settings.fontSize,
          fullSettings: settings
        });
        
        await onSave(settings);
        toast.success('Receipt settings saved successfully!');
      } catch (error: any) {
        toast.error(error.message || 'Failed to save settings');
      } finally {
        setSaving(false);
      }
    }
  };

  const handlePrint = () => {
    const printContent = printRef.current?.innerHTML;
    if (!printContent) {
      toast.error('Nothing to print');
      return;
    }

    const printWindow = window.open('', '', 'width=800,height=600');
    if (!printWindow) {
      toast.error('Please allow popups to print');
      return;
    }

    printWindow.document.write(`
      <!DOCTYPE html>
      <html>
        <head>
          <title>Print Receipt - ${sampleReceipt.receiptNumber}</title>
          <style>
            body { 
              font-family: ${settings.fontFamily}; 
              font-size: ${settings.fontSize}px;
              margin: ${settings.printMarginTop}mm ${settings.printMarginRight}mm ${settings.printMarginBottom}mm ${settings.printMarginLeft}mm;
              line-height: 1.4;
            }
            .receipt { 
              width: ${settings.paperSize === '58mm' ? '58mm' : settings.paperSize === '80mm' ? '80mm' : '210mm'};
              margin: 0 auto;
            }
            @media print {
              body { margin: 0; }
              @page { margin: 0; }
            }
          </style>
        </head>
        <body>
          <div class="receipt">${printContent}</div>
        </body>
      </html>
    `);
    printWindow.document.close();
    printWindow.print();
    setShowPrintDialog(false);
  };

  const renderTemplateElement = (elementId: string): React.ReactNode => {
    const commonStyles = {
      marginBottom: '6px'
    };

    const centerStyle = {
      textAlign: 'center' as const,
      marginBottom: '6px'
    };

    const flexStyle = {
      display: 'flex',
      justifyContent: 'space-between',
      marginBottom: '4px'
    };

    switch (elementId) {
      case 'logo':
        return settings.showLogo && (
          <div style={centerStyle}>
            {settings.logoUrl ? (
              <img 
                src={settings.logoUrl} 
                alt="Logo" 
                style={{ maxWidth: '120px', maxHeight: '60px', objectFit: 'contain' }} 
              />
            ) : (
              <div style={{ 
                border: '2px solid #000', 
                padding: '8px 16px', 
                display: 'inline-block',
                fontWeight: 'bold'
              }}>
                [COMPANY LOGO]
              </div>
            )}
          </div>
        );
      
      case 'storeName':
        return (
          <div style={{ ...centerStyle, fontSize: `${settings.fontSize + 2}px`, fontWeight: 'bold' }}>
            {settings.storeName}
          </div>
        );
      
      case 'address':
        return (
          <div style={{ ...centerStyle, fontSize: `${settings.fontSize - 1}px` }}>
            {settings.storeAddress}
          </div>
        );
      
      case 'phone':
        return (
          <div style={{ ...centerStyle, fontSize: `${settings.fontSize - 1}px` }}>
            📞 {settings.storePhone}
          </div>
        );
      
      case 'email':
        return (
          <div style={{ ...centerStyle, fontSize: `${settings.fontSize - 1}px` }}>
            ✉️ {settings.storeEmail}
          </div>
        );
      
      case 'website':
        return (
          <div style={{ ...centerStyle, fontSize: `${settings.fontSize - 1}px` }}>
            🌐 {settings.storeWebsite}
          </div>
        );
      
      case 'header':
        return settings.headerText && (
          <div style={{ ...centerStyle, fontStyle: 'italic', marginTop: '8px' }}>
            {settings.headerText}
          </div>
        );
      
      case 'divider':
        return <hr style={{ borderTop: '1px dashed #000', margin: '8px 0' }} />;
      
      case 'receiptNumber':
        return (
          <div style={commonStyles}>
            Receipt #: <strong>{sampleReceipt.receiptNumber}</strong>
          </div>
        );
      
      case 'date':
        return (
          <div style={commonStyles}>
            Date: {sampleReceipt.date}
          </div>
        );
      
      case 'cashier':
        return settings.showCashier && (
          <div style={commonStyles}>
            Cashier: {sampleReceipt.cashier}
          </div>
        );
      
      case 'customer':
        return settings.showCustomerInfo && (
          <div style={{ marginBottom: '8px' }}>
            <div><strong>Customer:</strong> {sampleReceipt.customer.name}</div>
            <div style={{ fontSize: `${settings.fontSize - 1}px` }}>
              Phone: {sampleReceipt.customer.phone}
            </div>
          </div>
        );
      
      case 'items':
        return (
          <div style={{ marginBottom: '8px' }}>
            {sampleReceipt.items.map((item, idx) => (
              <div key={idx} style={{ marginBottom: '6px' }}>
                <div style={flexStyle}>
                  <div style={{ flex: 1, fontWeight: '500' }}>{item.name}</div>
                  <div style={{ minWidth: '70px', textAlign: 'right' }}>
                    ${item.total.toFixed(2)}
                  </div>
                </div>
                {settings.showItemDetails && (
                  <div style={{ 
                    fontSize: `${settings.fontSize - 2}px`, 
                    color: '#666',
                    paddingLeft: '8px'
                  }}>
                    {item.qty} x ${item.price.toFixed(2)} (SKU: {item.sku})
                  </div>
                )}
              </div>
            ))}
          </div>
        );
      
      case 'subtotal':
        return (
          <div style={flexStyle}>
            <div>Subtotal:</div>
            <div>${sampleReceipt.subtotal.toFixed(2)}</div>
          </div>
        );
      
      case 'tax':
        return settings.showTaxDetails && (
          <div style={flexStyle}>
            <div>Tax (10%):</div>
            <div>${sampleReceipt.tax.toFixed(2)}</div>
          </div>
        );
      
      case 'discount':
        return sampleReceipt.discount > 0 && (
          <div style={{ ...flexStyle, color: '#16a34a' }}>
            <div>Discount:</div>
            <div>-${sampleReceipt.discount.toFixed(2)}</div>
          </div>
        );
      
      case 'total':
        return (
          <div style={{ 
            ...flexStyle, 
            fontWeight: 'bold', 
            fontSize: `${settings.fontSize + 2}px`,
            paddingTop: '4px',
            borderTop: '2px solid #000',
            marginTop: '4px'
          }}>
            <div>TOTAL:</div>
            <div>${sampleReceipt.total.toFixed(2)}</div>
          </div>
        );
      
      case 'payment':
        return (
          <div style={{ marginTop: '8px' }}>
            <div style={flexStyle}>
              <div>Payment Method:</div>
              <div>{sampleReceipt.paymentMethod}</div>
            </div>
            <div style={flexStyle}>
              <div>Amount Paid:</div>
              <div>${sampleReceipt.paid.toFixed(2)}</div>
            </div>
          </div>
        );
      
      case 'change':
        return sampleReceipt.change > 0 && (
          <div style={{ ...flexStyle, fontWeight: 'bold' }}>
            <div>Change Given:</div>
            <div>${sampleReceipt.change.toFixed(2)}</div>
          </div>
        );
      
      case 'barcode':
        return settings.showBarcode && (
          <div style={{ textAlign: 'center', margin: '12px 0' }}>
            <div style={{ 
              fontFamily: 'monospace', 
              letterSpacing: '2px',
              fontSize: `${settings.fontSize + 2}px`
            }}>
              |||| |||| |||| |||| ||||
            </div>
            <div style={{ fontSize: `${settings.fontSize - 2}px`, marginTop: '4px' }}>
              {sampleReceipt.receiptNumber}
            </div>
          </div>
        );
      
      case 'qrcode':
        return settings.showQRCode && (
          <div style={{ textAlign: 'center', margin: '12px 0' }}>
            <div style={{ 
              border: '2px solid #000', 
              width: '100px', 
              height: '100px', 
              display: 'inline-flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontWeight: 'bold'
            }}>
              [QR CODE]
            </div>
            <div style={{ fontSize: `${settings.fontSize - 2}px`, marginTop: '4px' }}>
              Scan for receipt details
            </div>
          </div>
        );
      
      case 'footer':
        return settings.footerText && (
          <div style={{ 
            ...centerStyle, 
            fontStyle: 'italic', 
            fontSize: `${settings.fontSize - 1}px`,
            marginTop: '12px'
          }}>
            {settings.footerText}
          </div>
        );
      
      case 'social':
        return settings.showSocial && (
          <div style={{ 
            textAlign: 'center', 
            fontSize: `${settings.fontSize - 2}px`,
            marginTop: '12px',
            padding: '8px',
            background: '#f3f4f6',
            borderRadius: '4px'
          }}>
            <div><strong>Follow Us:</strong></div>
            <div>📘 Facebook | 📷 Instagram | 🐦 Twitter</div>
            <div>✉️ {settings.storeEmail}</div>
          </div>
        );
      
      case 'warranty':
        return (
          <div style={{ 
            textAlign: 'center', 
            fontSize: `${settings.fontSize - 2}px`,
            marginTop: '8px',
            padding: '6px',
            border: '1px solid #ddd'
          }}>
            <div><strong>🛡️ Return Policy</strong></div>
            <div>30-day money-back guarantee</div>
            <div>Keep this receipt for returns</div>
          </div>
        );
      
      case 'promotions':
        return settings.showPromotion && settings.promotionText && (
          <div style={{ 
            textAlign: 'center', 
            fontSize: `${settings.fontSize - 1}px`,
            marginTop: '12px',
            padding: '8px',
            background: '#fef3c7',
            border: '1px dashed #f59e0b',
            borderRadius: '4px'
          }}>
            {settings.promotionText}
          </div>
        );
      
      default:
        return null;
    }
  };

  const getTemplateElements = (): string[] => {
    if (settings.receiptTemplate === 'custom') {
      return customElements;
    }
    return TEMPLATE_LAYOUTS[settings.receiptTemplate] || TEMPLATE_LAYOUTS.standard;
  };

  const moveElement = (index: number, direction: 'up' | 'down') => {
    const newElements = [...customElements];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    
    if (targetIndex < 0 || targetIndex >= newElements.length) return;
    
    [newElements[index], newElements[targetIndex]] = [newElements[targetIndex], newElements[index]];
    setCustomElements(newElements);
  };

  const removeElement = (index: number) => {
    const element = TEMPLATE_ELEMENTS.find(e => e.id === customElements[index]);
    if (element?.required) {
      toast.error(`${element.label} is required and cannot be removed`);
      return;
    }
    setCustomElements(customElements.filter((_, i) => i !== index));
  };

  const addElement = (elementId: string) => {
    if (customElements.includes(elementId)) {
      toast.warning('This element is already in the template');
      return;
    }
    setCustomElements([...customElements, elementId]);
  };

  return (
    <div>
      <Row>
        <Col lg={8}>
          {/* Template Selection Card */}
          <Card className="mb-4 shadow-sm">
            <Card.Header className="bg-primary text-white">
              <h5 className="mb-0">📝 Receipt Template Selection</h5>
            </Card.Header>
            <Card.Body>
              <Row className="g-3">
                {RECEIPT_TEMPLATES.map(template => (
                  <Col md={6} lg={3} key={template.id}>
                    <Card 
                      className={`h-100 cursor-pointer hover-shadow transition-all ${
                        settings.receiptTemplate === template.id ? 'border-primary border-3' : 'border'
                      }`}
                      onClick={() => {
                        console.log('🖱️ [EnhancedReceipt] Template clicked:', {
                          clicked: template.id,
                          currentTemplate: settings.receiptTemplate,
                          willChangeTo: template.id
                        });
                        
                        setSettings({...settings, receiptTemplate: template.id});
                        
                        if (template.id !== 'custom') {
                          setCustomElements(TEMPLATE_LAYOUTS[template.id] || TEMPLATE_LAYOUTS.standard);
                        }
                        
                        console.log('✅ [EnhancedReceipt] Template state updated to:', template.id);
                      }}
                      style={{ cursor: 'pointer', transition: 'all 0.2s' }}
                    >
                      <Card.Body className="text-center p-3">
                        <div style={{ fontSize: '2.5rem' }}>{template.icon}</div>
                        <h6 className="mt-2 mb-1">{template.name}</h6>
                        <small className="text-muted">{template.description}</small>
                        {settings.receiptTemplate === template.id && (
                          <Badge bg="primary" className="mt-2 w-100">✓ Selected</Badge>
                        )}
                      </Card.Body>
                    </Card>
                  </Col>
                ))}
              </Row>
              
              {settings.receiptTemplate === 'custom' && (
                <Alert variant="info" className="mt-3 mb-0">
                  <strong>🎨 Custom Template Mode Active</strong><br />
                  Click "Edit Template Layout" button below to customize the order of elements.
                </Alert>
              )}
            </Card.Body>
          </Card>

          {/* Settings Configuration Card */}
          <Card className="mb-4 shadow-sm">
            <Card.Header>
              <h6 className="mb-0">⚙️ Receipt Configuration</h6>
            </Card.Header>
            <Card.Body>
              <Row>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Store Name</Form.Label>
                    <Form.Control
                      value={settings.storeName}
                      onChange={(e) => setSettings({...settings, storeName: e.target.value})}
                      placeholder="Enter store name"
                    />
                  </Form.Group>
                </Col>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Paper Size</Form.Label>
                    <Form.Select
                      value={settings.paperSize}
                      onChange={(e) => setSettings({...settings, paperSize: e.target.value})}
                    >
                      <option value="58mm">58mm (Small Thermal)</option>
                      <option value="80mm">80mm (Standard Thermal)</option>
                      <option value="A4">A4 (Letter Size)</option>
                    </Form.Select>
                  </Form.Group>
                </Col>
                <Col md={12}>
                  <Form.Group className="mb-3">
                    <Form.Label>Store Address</Form.Label>
                    <Form.Control
                      value={settings.storeAddress}
                      onChange={(e) => setSettings({...settings, storeAddress: e.target.value})}
                      placeholder="123 Main Street, City, State ZIP"
                    />
                  </Form.Group>
                </Col>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Header Text</Form.Label>
                    <Form.Control
                      value={settings.headerText}
                      onChange={(e) => setSettings({...settings, headerText: e.target.value})}
                      placeholder="Thank you message"
                    />
                  </Form.Group>
                </Col>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Footer Text</Form.Label>
                    <Form.Control
                      value={settings.footerText}
                      onChange={(e) => setSettings({...settings, footerText: e.target.value})}
                      placeholder="Visit again message"
                    />
                  </Form.Group>
                </Col>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Font Size</Form.Label>
                    <Form.Control
                      type="number"
                      min="8"
                      max="24"
                      value={settings.fontSize}
                      onChange={(e) => setSettings({...settings, fontSize: parseInt(e.target.value) || 12})}
                    />
                    <Form.Text className="text-muted">8-24 pixels (recommended: 12)</Form.Text>
                  </Form.Group>
                </Col>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Promotional Text</Form.Label>
                    <Form.Control
                      value={settings.promotionText}
                      onChange={(e) => setSettings({...settings, promotionText: e.target.value})}
                      placeholder="Special offer message"
                    />
                  </Form.Group>
                </Col>
              </Row>

              <h6 className="mt-3 mb-3">Display Options</h6>
              <Row>
                {[
                  { key: 'showLogo', label: 'Show Logo', icon: '🏢' },
                  { key: 'showTaxDetails', label: 'Show Tax Details', icon: '📊' },
                  { key: 'showItemDetails', label: 'Show Item Details', icon: '📦' },
                  { key: 'showBarcode', label: 'Show Barcode', icon: '|||' },
                  { key: 'showQRCode', label: 'Show QR Code', icon: '▪️' },
                  { key: 'showCustomerInfo', label: 'Show Customer Info', icon: '👥' },
                  { key: 'showCashier', label: 'Show Cashier Name', icon: '👤' },
                  { key: 'showSocial', label: 'Show Social Media', icon: '🌐' },
                  { key: 'showPromotion', label: 'Show Promotion', icon: '🎁' }
                ].map(option => (
                  <Col md={4} key={option.key}>
                    <Form.Check
                      type="checkbox"
                      label={`${option.icon} ${option.label}`}
                      checked={settings[option.key as keyof ReceiptSettings] as boolean}
                      onChange={(e) => setSettings({
                        ...settings, 
                        [option.key]: e.target.checked
                      })}
                      className="mb-2"
                    />
                  </Col>
                ))}
              </Row>

              <h6 className="mt-4 mb-3">Print Margins (mm)</h6>
              <Row>
                <Col md={3}>
                  <Form.Group className="mb-3">
                    <Form.Label>Top</Form.Label>
                    <Form.Control
                      type="number"
                      min="0"
                      max="50"
                      value={settings.printMarginTop}
                      onChange={(e) => setSettings({
                        ...settings, 
                        printMarginTop: parseInt(e.target.value) || 0
                      })}
                    />
                  </Form.Group>
                </Col>
                <Col md={3}>
                  <Form.Group className="mb-3">
                    <Form.Label>Bottom</Form.Label>
                    <Form.Control
                      type="number"
                      min="0"
                      max="50"
                      value={settings.printMarginBottom}
                      onChange={(e) => setSettings({
                        ...settings, 
                        printMarginBottom: parseInt(e.target.value) || 0
                      })}
                    />
                  </Form.Group>
                </Col>
                <Col md={3}>
                  <Form.Group className="mb-3">
                    <Form.Label>Left</Form.Label>
                    <Form.Control
                      type="number"
                      min="0"
                      max="50"
                      value={settings.printMarginLeft}
                      onChange={(e) => setSettings({
                        ...settings, 
                        printMarginLeft: parseInt(e.target.value) || 0
                      })}
                    />
                  </Form.Group>
                </Col>
                <Col md={3}>
                  <Form.Group className="mb-3">
                    <Form.Label>Right</Form.Label>
                    <Form.Control
                      type="number"
                      min="0"
                      max="50"
                      value={settings.printMarginRight}
                      onChange={(e) => setSettings({
                        ...settings, 
                        printMarginRight: parseInt(e.target.value) || 0
                      })}
                    />
                  </Form.Group>
                </Col>
              </Row>
            </Card.Body>
          </Card>

          {/* Action Buttons */}
          <div className="d-flex gap-2 flex-wrap">
            <Button 
              variant="primary" 
              onClick={handleSave} 
              disabled={saving}
              size="lg"
            >
              {saving ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" />
                  Saving...
                </>
              ) : (
                <>
                  <FaSave className="me-2" />
                  Save Receipt Settings
                </>
              )}
            </Button>
            <Button 
              variant="outline-secondary" 
              onClick={() => setShowPreview(!showPreview)}
              size="lg"
            >
              <FaEye className="me-2" />
              {showPreview ? 'Hide' : 'Show'} Preview
            </Button>
            {settings.receiptTemplate === 'custom' && (
              <Button 
                variant="outline-info" 
                onClick={() => setShowTemplateEditor(true)}
                size="lg"
              >
                <FaEdit className="me-2" />
                Edit Template Layout
              </Button>
            )}
            <Button 
              variant="success" 
              onClick={() => setShowPrintDialog(true)}
              size="lg"
            >
              <FaPrint className="me-2" />
              Test Print Receipt
            </Button>
          </div>
        </Col>

        {/* Live Preview Panel */}
        {showPreview && (
          <Col lg={4}>
            <div style={{ position: 'sticky', top: '20px' }}>
              <Card className="shadow-lg">
                <Card.Header className="bg-dark text-white">
                  <h6 className="mb-0">👁️ Live Receipt Preview</h6>
                </Card.Header>
                <Card.Body className="bg-light p-4">
                  <div 
                    ref={printRef}
                    className="receipt-preview bg-white p-3 border shadow-sm"
                    style={{
                      fontSize: `${settings.fontSize}px`,
                      fontFamily: settings.fontFamily,
                      maxWidth: settings.paperSize === '58mm' ? '220px' : 
                                settings.paperSize === '80mm' ? '300px' : '100%',
                      margin: '0 auto',
                      minHeight: '400px',
                      borderRadius: '8px'
                    }}
                  >
                    {getTemplateElements().map((elementId, idx) => (
                      <React.Fragment key={`${elementId}-${idx}`}>
                        {renderTemplateElement(elementId)}
                      </React.Fragment>
                    ))}
                  </div>
                  
                  <div className="mt-3 d-flex gap-2 justify-content-center flex-wrap">
                    <Badge bg="secondary" className="px-3 py-2">
                      {RECEIPT_TEMPLATES.find(t => t.id === settings.receiptTemplate)?.name}
                    </Badge>
                    <Badge bg="info" className="px-3 py-2">
                      {settings.paperSize}
                    </Badge>
                    <Badge bg="success" className="px-3 py-2">
                      {settings.fontSize}px
                    </Badge>
                  </div>
                </Card.Body>
              </Card>
            </div>
          </Col>
        )}
      </Row>

      {/* Custom Template Editor Modal */}
      <Modal 
        show={showTemplateEditor} 
        onHide={() => setShowTemplateEditor(false)} 
        size="lg"
        centered
      >
        <Modal.Header closeButton className="bg-primary text-white">
          <Modal.Title>🎨 Custom Template Editor</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Alert variant="info">
            <strong>💡 How to use:</strong><br />
            • Use ↑↓ buttons to reorder elements<br />
            • Elements marked with ⭐ are required and cannot be removed<br />
            • Click "+" to add new elements to your template
          </Alert>
          
          <h6 className="mb-3">Current Template Elements:</h6>
          <ListGroup className="mb-4">
            {customElements.map((elementId, index) => {
              const element = TEMPLATE_ELEMENTS.find(e => e.id === elementId);
              if (!element) return null;
              
              return (
                <ListGroup.Item 
                  key={`${elementId}-${index}`}
                  className="d-flex justify-content-between align-items-center py-3"
                >
                  <div className="d-flex align-items-center gap-2">
                    <span style={{ fontSize: '1.5rem' }}>{element.icon}</span>
                    <div>
                      <strong>{element.label}</strong>
                      {element.required && (
                        <Badge bg="warning" text="dark" className="ms-2">
                          ⭐ Required
                        </Badge>
                      )}
                    </div>
                  </div>
                  <div className="d-flex gap-2">
                    <Button 
                      size="sm" 
                      variant="outline-secondary" 
                      disabled={index === 0}
                      onClick={() => moveElement(index, 'up')}
                      title="Move up"
                    >
                      <FaArrowUp />
                    </Button>
                    <Button 
                      size="sm" 
                      variant="outline-secondary"
                      disabled={index === customElements.length - 1}
                      onClick={() => moveElement(index, 'down')}
                      title="Move down"
                    >
                      <FaArrowDown />
                    </Button>
                    {!element.required && (
                      <Button 
                        size="sm" 
                        variant="outline-danger"
                        onClick={() => removeElement(index)}
                        title="Remove element"
                      >
                        <FaTrash />
                      </Button>
                    )}
                  </div>
                </ListGroup.Item>
              );
            })}
          </ListGroup>
          
          <h6 className="mb-3">Add Elements:</h6>
          <div className="d-flex flex-wrap gap-2">
            {TEMPLATE_ELEMENTS
              .filter(e => !customElements.includes(e.id))
              .map(element => (
                <Button
                  key={element.id}
                  size="sm"
                  variant="outline-primary"
                  onClick={() => addElement(element.id)}
                  className="d-flex align-items-center gap-1"
                >
                  <FaPlus size={12} />
                  <span>{element.icon} {element.label}</span>
                </Button>
              ))}
          </div>
          
          {TEMPLATE_ELEMENTS.filter(e => !customElements.includes(e.id)).length === 0 && (
            <Alert variant="success" className="mb-0">
              ✅ All available elements have been added to your template!
            </Alert>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowTemplateEditor(false)}>
            <FaTimes className="me-2" />
            Close
          </Button>
          <Button 
            variant="primary" 
            onClick={() => {
              setShowTemplateEditor(false);
              toast.success('Custom template layout saved!');
            }}
          >
            <FaSave className="me-2" />
            Save Template Layout
          </Button>
        </Modal.Footer>
      </Modal>

      {/* Print Dialog Modal */}
      <Modal 
        show={showPrintDialog} 
        onHide={() => setShowPrintDialog(false)}
        centered
      >
        <Modal.Header closeButton className="bg-success text-white">
          <Modal.Title>🖨️ Test Print Receipt</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Alert variant="success" className="mb-3">
            <strong>✅ Ready to Print</strong><br />
            A new window will open with your receipt preview. Make sure your printer is connected and ready.
          </Alert>
          
          <div className="bg-light p-3 rounded">
            <h6 className="mb-2">Print Configuration:</h6>
            <table className="table table-sm table-borderless mb-0">
              <tbody>
                <tr>
                  <td><strong>Template:</strong></td>
                  <td>{RECEIPT_TEMPLATES.find(t => t.id === settings.receiptTemplate)?.name}</td>
                </tr>
                <tr>
                  <td><strong>Paper Size:</strong></td>
                  <td>{settings.paperSize}</td>
                </tr>
                <tr>
                  <td><strong>Font Size:</strong></td>
                  <td>{settings.fontSize}px</td>
                </tr>
                <tr>
                  <td><strong>Margins:</strong></td>
                  <td>
                    T:{settings.printMarginTop}mm, 
                    B:{settings.printMarginBottom}mm, 
                    L:{settings.printMarginLeft}mm, 
                    R:{settings.printMarginRight}mm
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <Alert variant="info" className="mt-3 mb-0">
            <small>
              <strong>💡 Tip:</strong> This is a test print with sample data. 
              Actual receipts will contain real transaction information.
            </small>
          </Alert>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowPrintDialog(false)}>
            <FaTimes className="me-2" />
            Cancel
          </Button>
          <Button variant="success" onClick={handlePrint}>
            <FaPrint className="me-2" />
            Print Test Receipt
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
};

export default EnhancedReceiptSettings;
