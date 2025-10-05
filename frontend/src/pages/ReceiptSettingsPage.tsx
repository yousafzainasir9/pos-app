import React, { useState, useEffect } from 'react';
import { Container, Alert, Spinner } from 'react-bootstrap';
import { FaReceipt } from 'react-icons/fa';
import { toast } from 'react-toastify';
import EnhancedReceiptSettings from '../components/EnhancedReceiptSettings';
import systemSettingsService from '../services/systemSettings.service';
import { clearSettingsCache } from '@/hooks/useSystemSettings';

/**
 * Receipt Settings Page - Available to ALL user types (Admin, Professional, Basic)
 * 
 * Features:
 * - 8 professional receipt templates
 * - Live preview
 * - Custom template editor
 * - Test print functionality
 * - 25 customizable elements
 */
const ReceiptSettingsPage: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [initialSettings, setInitialSettings] = useState<any>(null);

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const settings = await systemSettingsService.getReceiptSettings();
      setInitialSettings(settings);
    } catch (error: any) {
      console.error('Failed to load receipt settings:', error);
      toast.error('Failed to load receipt settings. Using defaults.');
      // Use default settings if load fails
      setInitialSettings({
        headerText: 'Thank you for your purchase!',
        footerText: 'Please visit us again',
        showLogo: true,
        showTaxDetails: true,
        showItemDetails: true,
        paperSize: '80mm',
        fontSize: 12,
        receiptTemplate: 'standard'
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (settings: any) => {
    try {
      await systemSettingsService.updateReceiptSettings(settings);
      clearSettingsCache();
      toast.success('Receipt settings saved successfully!');
    } catch (error: any) {
      console.error('Failed to save receipt settings:', error);
      const errorMessage = error.response?.data?.message || 'Failed to save receipt settings';
      toast.error(errorMessage);
      throw new Error(errorMessage);
    }
  };

  if (loading) {
    return (
      <Container fluid className="py-5 text-center">
        <Spinner animation="border" variant="primary" />
        <p className="mt-3 text-muted">Loading receipt settings...</p>
      </Container>
    );
  }

  return (
    <Container fluid className="py-4">
      {/* Page Header */}
      <div className="mb-4">
        <h2 className="mb-2">
          <FaReceipt className="me-2 text-primary" />
          Receipt Settings
        </h2>
        <p className="text-muted mb-0">
          Configure receipt templates and printing options for your POS system
        </p>
      </div>

      {/* Info Alert */}
      <Alert variant="info" className="mb-4">
        <div className="d-flex align-items-start">
          <div className="me-3" style={{ fontSize: '1.5rem' }}>💡</div>
          <div>
            <strong>Available to All Users</strong>
            <p className="mb-0 mt-1">
              All user types (Admin, Professional, Basic) can customize receipt settings. 
              Choose from 8 professional templates or create your own custom layout!
            </p>
          </div>
        </div>
      </Alert>

      {/* Main Component */}
      <EnhancedReceiptSettings 
        initialSettings={initialSettings}
        onSave={handleSave}
      />

      {/* Help Section */}
      <Alert variant="light" className="mt-4 border">
        <h6 className="mb-3">🎯 Quick Guide</h6>
        <div className="row">
          <div className="col-md-6">
            <h6 className="text-primary">Templates Available:</h6>
            <ul className="small mb-3">
              <li><strong>Standard</strong> - Classic receipt layout</li>
              <li><strong>Compact</strong> - Saves paper (30% less)</li>
              <li><strong>Detailed</strong> - Comprehensive information</li>
              <li><strong>Modern</strong> - Contemporary design</li>
              <li><strong>Elegant</strong> - Sophisticated layout</li>
              <li><strong>Minimalist</strong> - Ultra-simple format</li>
              <li><strong>Thermal</strong> - Printer optimized</li>
              <li><strong>Custom</strong> - Your own design</li>
            </ul>
          </div>
          <div className="col-md-6">
            <h6 className="text-primary">How to Use:</h6>
            <ol className="small mb-0">
              <li>Select a template from the options above</li>
              <li>Configure settings (paper size, font, margins)</li>
              <li>Toggle display options (logo, barcode, QR code)</li>
              <li>Preview your changes in real-time</li>
              <li>Test print to verify layout</li>
              <li>Save when satisfied</li>
            </ol>
          </div>
        </div>
      </Alert>
    </Container>
  );
};

export default ReceiptSettingsPage;
