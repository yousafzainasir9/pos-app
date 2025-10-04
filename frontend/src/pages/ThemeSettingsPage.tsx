import React, { useState } from 'react';
import { Container, Row, Col, Card, Form, Button, Badge, Alert } from 'react-bootstrap';
import { FaPalette, FaSave, FaUndo, FaEye } from 'react-icons/fa';
import { useTheme } from '@/theme/ThemeContext';
import { ThemeName } from '@/theme/theme.config';
import { toast } from 'react-toastify';

const ThemeSettingsPage: React.FC = () => {
  const { theme, themeName, setTheme, updateColors, updateCompanyName, updateLogo, resetTheme, availableThemeNames } = useTheme();
  const [companyName, setCompanyNameLocal] = useState(theme.companyName);
  const [logoUrl, setLogoUrl] = useState(theme.logoUrl || '');
  const [customColors, setCustomColors] = useState(theme.colors);
  const [showPreview, setShowPreview] = useState(false);

  const handleThemeChange = (newThemeName: ThemeName) => {
    setTheme(newThemeName);
    toast.success(`Theme changed to ${newThemeName}`);
  };

  const handleColorChange = (colorKey: keyof typeof customColors, value: string) => {
    setCustomColors(prev => ({ ...prev, [colorKey]: value }));
  };

  const handleSaveColors = () => {
    updateColors(customColors);
    toast.success('Colors updated successfully!');
  };

  const handleSaveCompanyName = () => {
    updateCompanyName(companyName);
    toast.success('Company name updated!');
  };

  const handleSaveLogo = () => {
    updateLogo(logoUrl);
    toast.success('Logo updated!');
  };

  const handleResetTheme = () => {
    if (window.confirm('Are you sure you want to reset to default theme? This will clear all customizations.')) {
      resetTheme();
      setCompanyNameLocal('Cookie Barrel POS');
      setLogoUrl('');
      toast.info('Theme reset to default');
    }
  };

  const colorSections = [
    {
      title: 'Primary Colors',
      colors: [
        { key: 'primary', label: 'Primary' },
        { key: 'primaryHover', label: 'Primary Hover' },
        { key: 'primaryActive', label: 'Primary Active' },
        { key: 'primaryLight', label: 'Primary Light' },
        { key: 'primaryDark', label: 'Primary Dark' },
      ],
    },
    {
      title: 'Secondary Colors',
      colors: [
        { key: 'secondary', label: 'Secondary' },
        { key: 'secondaryHover', label: 'Secondary Hover' },
        { key: 'secondaryActive', label: 'Secondary Active' },
        { key: 'secondaryLight', label: 'Secondary Light' },
        { key: 'secondaryDark', label: 'Secondary Dark' },
      ],
    },
    {
      title: 'Accent Colors',
      colors: [
        { key: 'accent', label: 'Accent' },
        { key: 'accentHover', label: 'Accent Hover' },
        { key: 'accentLight', label: 'Accent Light' },
      ],
    },
    {
      title: 'Logo Colors',
      colors: [
        { key: 'logoBackground', label: 'Logo Background' },
        { key: 'logoText', label: 'Logo Text' },
        { key: 'logoAccent', label: 'Logo Accent' },
        { key: 'logoIcon', label: 'Logo Icon' },
      ],
    },
    {
      title: 'Status Colors',
      colors: [
        { key: 'success', label: 'Success' },
        { key: 'successLight', label: 'Success Light' },
        { key: 'warning', label: 'Warning' },
        { key: 'warningLight', label: 'Warning Light' },
        { key: 'danger', label: 'Danger' },
        { key: 'dangerLight', label: 'Danger Light' },
        { key: 'info', label: 'Info' },
        { key: 'infoLight', label: 'Info Light' },
      ],
    },
    {
      title: 'Neutral Colors',
      colors: [
        { key: 'background', label: 'Background' },
        { key: 'backgroundSecondary', label: 'Background Secondary' },
        { key: 'surface', label: 'Surface' },
        { key: 'surfaceHover', label: 'Surface Hover' },
        { key: 'border', label: 'Border' },
        { key: 'borderLight', label: 'Border Light' },
      ],
    },
    {
      title: 'Text Colors',
      colors: [
        { key: 'textPrimary', label: 'Text Primary' },
        { key: 'textSecondary', label: 'Text Secondary' },
        { key: 'textMuted', label: 'Text Muted' },
        { key: 'textLight', label: 'Text Light' },
        { key: 'textDark', label: 'Text Dark' },
      ],
    },
  ];

  return (
    <Container fluid className="py-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2>
            <FaPalette className="me-2" />
            Theme Settings
          </h2>
          <p className="text-muted">Customize the appearance of your POS system</p>
        </div>
        <div>
          <Button variant="outline-secondary" onClick={() => setShowPreview(!showPreview)} className="me-2">
            <FaEye className="me-2" />
            {showPreview ? 'Hide' : 'Show'} Preview
          </Button>
          <Button variant="outline-danger" onClick={handleResetTheme}>
            <FaUndo className="me-2" />
            Reset to Default
          </Button>
        </div>
      </div>

      <Row>
        {/* Preset Themes */}
        <Col lg={12} className="mb-4">
          <Card>
            <Card.Header>
              <h5 className="mb-0">Preset Themes</h5>
            </Card.Header>
            <Card.Body>
              <Row>
                {availableThemeNames.map((name) => (
                  <Col md={3} key={name} className="mb-3">
                    <Card
                      className={`cursor-pointer ${themeName === name ? 'border-primary' : ''}`}
                      onClick={() => handleThemeChange(name)}
                      style={{ cursor: 'pointer' }}
                    >
                      <Card.Body className="text-center">
                        <h6 className="text-capitalize">{name}</h6>
                        {themeName === name && (
                          <Badge bg="primary" className="mt-2">Current</Badge>
                        )}
                      </Card.Body>
                    </Card>
                  </Col>
                ))}
              </Row>
            </Card.Body>
          </Card>
        </Col>

        {/* Company Branding */}
        <Col lg={6} className="mb-4">
          <Card>
            <Card.Header>
              <h5 className="mb-0">Company Branding</h5>
            </Card.Header>
            <Card.Body>
              <Form.Group className="mb-3">
                <Form.Label>Company Name</Form.Label>
                <Form.Control
                  type="text"
                  value={companyName}
                  onChange={(e) => setCompanyNameLocal(e.target.value)}
                  placeholder="Enter company name"
                />
              </Form.Group>
              <Button variant="primary" onClick={handleSaveCompanyName}>
                <FaSave className="me-2" />
                Save Company Name
              </Button>
            </Card.Body>
          </Card>
        </Col>

        <Col lg={6} className="mb-4">
          <Card>
            <Card.Header>
              <h5 className="mb-0">Logo</h5>
            </Card.Header>
            <Card.Body>
              <Form.Group className="mb-3">
                <Form.Label>Logo URL</Form.Label>
                <Form.Control
                  type="text"
                  value={logoUrl}
                  onChange={(e) => setLogoUrl(e.target.value)}
                  placeholder="Enter logo URL (optional)"
                />
                <Form.Text className="text-muted">
                  Leave empty to use the default logo
                </Form.Text>
              </Form.Group>
              <Button variant="primary" onClick={handleSaveLogo}>
                <FaSave className="me-2" />
                Save Logo
              </Button>
            </Card.Body>
          </Card>
        </Col>

        {/* Color Customization */}
        {colorSections.map((section) => (
          <Col lg={6} key={section.title} className="mb-4">
            <Card>
              <Card.Header>
                <h5 className="mb-0">{section.title}</h5>
              </Card.Header>
              <Card.Body>
                {section.colors.map((color) => (
                  <Form.Group key={color.key} className="mb-3">
                    <Form.Label>{color.label}</Form.Label>
                    <div className="d-flex align-items-center gap-2">
                      <Form.Control
                        type="color"
                        value={customColors[color.key as keyof typeof customColors]}
                        onChange={(e) => handleColorChange(color.key as keyof typeof customColors, e.target.value)}
                        style={{ width: '60px', height: '40px' }}
                      />
                      <Form.Control
                        type="text"
                        value={customColors[color.key as keyof typeof customColors]}
                        onChange={(e) => handleColorChange(color.key as keyof typeof customColors, e.target.value)}
                        placeholder="#000000"
                      />
                    </div>
                  </Form.Group>
                ))}
              </Card.Body>
            </Card>
          </Col>
        ))}

        {/* Save All Colors */}
        <Col lg={12}>
          <Card className="bg-light">
            <Card.Body className="text-center">
              <Button variant="success" size="lg" onClick={handleSaveColors}>
                <FaSave className="me-2" />
                Save All Color Changes
              </Button>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Preview Section */}
      {showPreview && (
        <Row className="mt-4">
          <Col lg={12}>
            <Card>
              <Card.Header>
                <h5 className="mb-0">Theme Preview</h5>
              </Card.Header>
              <Card.Body>
                <Alert variant="info">
                  <strong>Note:</strong> Changes are applied live. Navigate through the application to see the theme in action.
                </Alert>
                
                <Row className="g-3">
                  <Col md={4}>
                    <Button variant="primary" className="w-100">Primary Button</Button>
                  </Col>
                  <Col md={4}>
                    <Button variant="secondary" className="w-100">Secondary Button</Button>
                  </Col>
                  <Col md={4}>
                    <Button className="btn-theme-accent w-100">Accent Button</Button>
                  </Col>
                </Row>

                <Row className="g-3 mt-3">
                  <Col md={3}>
                    <Badge bg="success" className="p-2 w-100">Success</Badge>
                  </Col>
                  <Col md={3}>
                    <Badge bg="warning" className="p-2 w-100">Warning</Badge>
                  </Col>
                  <Col md={3}>
                    <Badge bg="danger" className="p-2 w-100">Danger</Badge>
                  </Col>
                  <Col md={3}>
                    <Badge bg="info" className="p-2 w-100">Info</Badge>
                  </Col>
                </Row>

                <Card className="mt-3">
                  <Card.Header>Card Header</Card.Header>
                  <Card.Body>
                    <p className="text-theme-primary">Primary text color</p>
                    <p className="text-theme-secondary">Secondary text color</p>
                    <p className="text-theme-accent">Accent text color</p>
                  </Card.Body>
                </Card>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}
    </Container>
  );
};

export default ThemeSettingsPage;
