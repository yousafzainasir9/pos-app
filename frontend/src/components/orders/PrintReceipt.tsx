import React from 'react';
import { format } from 'date-fns';
import { Order, OrderStatus, PaymentMethod } from '@/types';
import { ReceiptSettingsDto } from '@/services/systemSettings.service';
import { getCachedReceiptSettings } from '@/hooks/useSystemSettings';

interface PrintReceiptProps {
  order: Order;
  settings?: ReceiptSettingsDto;
}

const PrintReceipt: React.FC<PrintReceiptProps> = ({ order, settings }) => {
  // This component is for preview only, actual printing uses printReceipt function
  return <div>Print Preview Component</div>;
};

// Enhanced print function that loads settings
export const printReceipt = async (order: Order, copies: number = 1) => {
  // Load receipt settings
  const settings = await getCachedReceiptSettings();
  
  console.log('🖨️ Printing receipt with template:', settings.receiptTemplate);
  
  // Print the specified number of copies
  for (let i = 0; i < copies; i++) {
    await printSingleReceipt(order, settings);
    // Small delay between prints
    if (i < copies - 1) {
      await new Promise(resolve => setTimeout(resolve, 500));
    }
  }
};

const printSingleReceipt = async (order: Order, settings: ReceiptSettingsDto) => {
  const printWindow = window.open('', '', 'width=400,height=600');
  
  if (!printWindow) {
    alert('Please allow pop-ups to print receipts');
    return;
  }

  const getOrderType = (type: number): string => {
    const typeMap: { [key: number]: string } = {
      1: 'Dine In',
      2: 'Take Away',
      3: 'Delivery',
      4: 'Pickup'
    };
    return typeMap[type] || 'Unknown';
  };

  const getPaymentMethod = (method: number): string => {
    const methodMap: { [key: number]: string } = {
      1: 'Cash',
      2: 'Credit Card',
      3: 'Debit Card',
      4: 'Mobile Payment',
      5: 'Gift Card',
      6: 'Loyalty Points',
      7: 'Other'
    };
    return methodMap[method] || 'Unknown';
  };

  const maxWidth = settings.paperSize === 'A4' ? '210mm' : settings.paperSize === '58mm' ? '200px' : '300px';

  // Generate HTML based on selected template
  let receiptHTML = '';
  
  switch (settings.receiptTemplate) {
    case 'modern':
      receiptHTML = generateModernTemplate(order, settings, getOrderType, getPaymentMethod);
      break;
    case 'compact':
      receiptHTML = generateCompactTemplate(order, settings, getOrderType, getPaymentMethod);
      break;
    case 'detailed':
      receiptHTML = generateDetailedTemplate(order, settings, getOrderType, getPaymentMethod);
      break;
    case 'elegant':
      receiptHTML = generateElegantTemplate(order, settings, getOrderType, getPaymentMethod);
      break;
    case 'minimalist':
      receiptHTML = generateMinimalistTemplate(order, settings, getOrderType, getPaymentMethod);
      break;
    case 'thermal':
      receiptHTML = generateThermalTemplate(order, settings, getOrderType, getPaymentMethod);
      break;
    case 'custom':
      receiptHTML = generateCustomTemplate(order, settings, getOrderType, getPaymentMethod);
      break;
    default: // standard
      receiptHTML = generateStandardTemplate(order, settings, getOrderType, getPaymentMethod);
  }

  const printContent = `
    <!DOCTYPE html>
    <html>
      <head>
        <title>Receipt - ${order.orderNumber}</title>
        <style>
          @media print {
            body { margin: 0; }
            @page { 
              margin: ${settings.printMarginTop}mm ${settings.printMarginRight}mm ${settings.printMarginBottom}mm ${settings.printMarginLeft}mm;
              size: ${settings.paperSize === 'A4' ? 'A4' : 'auto'};
            }
          }
          body {
            font-family: ${settings.fontFamily || 'monospace'};
            font-size: ${settings.fontSize}px;
            line-height: 1.4;
            margin: 0;
            padding: 20px;
          }
          .receipt-container {
            max-width: ${maxWidth};
            margin: 0 auto;
          }
        </style>
      </head>
      <body>
        <div class="receipt-container">
          ${receiptHTML}
        </div>
      </body>
    </html>
  `;

  printWindow.document.write(printContent);
  printWindow.document.close();
  
  // Wait for content to load before printing
  printWindow.onload = () => {
    printWindow.focus();
    printWindow.print();
    printWindow.close();
  };
};

// ==================== STANDARD TEMPLATE ====================
const generateStandardTemplate = (order: Order, settings: ReceiptSettingsDto, getOrderType: Function, getPaymentMethod: Function): string => {
  return `
    <style>
      .header { text-align: center; margin-bottom: 20px; }
      .header-title { font-size: ${settings.fontSize + 6}px; font-weight: bold; margin-bottom: 5px; }
      .header-subtitle { font-size: ${Math.max(8, settings.fontSize - 2)}px; }
      .header-text { font-size: ${Math.max(8, settings.fontSize - 2)}px; margin-top: 5px; font-style: italic; }
      .section { border-top: 1px dashed #000; border-bottom: 1px dashed #000; padding: 10px 0; margin-bottom: 10px; }
      .row { display: flex; justify-content: space-between; margin-bottom: 3px; }
      .items-header { font-weight: bold; margin-bottom: 5px; border-bottom: 1px solid #000; }
      .item { margin-bottom: 8px; }
      .item-details { font-size: ${Math.max(8, settings.fontSize - 2)}px; color: #666; padding-left: 10px; }
      .totals { border-top: 1px dashed #000; padding-top: 10px; margin-bottom: 10px; }
      .total-line { display: flex; justify-content: space-between; margin-top: 5px; padding-top: 5px; border-top: 1px solid #000; font-size: ${settings.fontSize + 2}px; font-weight: bold; }
      .footer { border-top: 1px dashed #000; padding-top: 10px; text-align: center; font-size: ${Math.max(8, settings.fontSize - 2)}px; }
    </style>

    <div class="header">
      ${settings.showLogo ? `<div class="header-title">${settings.storeName || order.storeName || 'My Store'}</div>` : ''}
      ${settings.storeAddress ? `<div class="header-subtitle">${settings.storeAddress}</div>` : ''}
      ${settings.storePhone ? `<div class="header-subtitle">${settings.storePhone}</div>` : ''}
      <div class="header-subtitle">Tax Invoice / Receipt</div>
      ${settings.headerText ? `<div class="header-text">${settings.headerText}</div>` : ''}
    </div>

    <div class="section">
      <div class="row"><span>Order #:</span><strong>${order.orderNumber}</strong></div>
      <div class="row"><span>Date:</span><span>${format(new Date(order.orderDate), 'dd/MM/yyyy HH:mm')}</span></div>
      <div class="row"><span>Type:</span><span>${getOrderType(order.orderType)}</span></div>
      ${order.tableNumber ? `<div class="row"><span>Table:</span><span>${order.tableNumber}</span></div>` : ''}
      ${settings.showCustomerInfo ? `<div class="row"><span>Customer:</span><span>${order.customerName || 'Walk-in'}</span></div>` : ''}
      ${settings.showCashier ? `<div class="row"><span>Cashier:</span><span>${order.cashierName || 'N/A'}</span></div>` : ''}
    </div>

    ${settings.showItemDetails ? `
    <div>
      <div class="items-header">ITEMS</div>
      ${order.items && order.items.length > 0 ? 
        order.items.filter(item => !item.isVoided).map(item => `
          <div class="item">
            <div class="row">
              <span>${item.productName}</span>
              <span>$${item.totalAmount.toFixed(2)}</span>
            </div>
            <div class="item-details">
              ${item.quantity} x $${item.unitPriceIncGst.toFixed(2)}
              ${item.discountAmount > 0 ? ` (Disc: -$${item.discountAmount.toFixed(2)})` : ''}
            </div>
          </div>
        `).join('') : 
        '<div style="text-align: center; color: #999; padding: 10px;">No items</div>'
      }
    </div>
    ` : ''}

    <div class="totals">
      <div class="row"><span>Subtotal:</span><span>$${order.subTotal.toFixed(2)}</span></div>
      ${order.discountAmount > 0 ? `<div class="row" style="color: #d9534f;"><span>Discount:</span><span>-$${order.discountAmount.toFixed(2)}</span></div>` : ''}
      ${settings.showTaxDetails ? `<div class="row"><span>GST:</span><span>$${order.taxAmount.toFixed(2)}</span></div>` : ''}
      <div class="total-line"><span>TOTAL:</span><span>$${order.totalAmount.toFixed(2)}</span></div>
    </div>

    ${order.payments && order.payments.length > 0 ? `
      <div class="section">
        <div style="font-weight: bold; margin-bottom: 5px;">PAYMENT</div>
        ${order.payments.map(payment => `
          <div class="row">
            <span>${getPaymentMethod(payment.paymentMethod)}:</span>
            <span>$${payment.amount.toFixed(2)}</span>
          </div>
        `).join('')}
        <div class="total-line"><span>Paid:</span><span>$${order.paidAmount.toFixed(2)}</span></div>
        ${order.changeAmount > 0 ? `<div class="row" style="margin-top: 3px;"><span>Change:</span><span>$${order.changeAmount.toFixed(2)}</span></div>` : ''}
      </div>
    ` : ''}

    ${settings.showBarcode ? `
      <div style="text-align: center; margin-top: 15px; font-family: monospace;">
        <div>|||| ${order.orderNumber} ||||</div>
      </div>
    ` : ''}

    <div class="footer">
      ${settings.footerText ? `<div style="margin-bottom: 5px;">${settings.footerText}</div>` : ''}
      <div style="margin-bottom: 5px;">${settings.storeName || order.storeName || 'My Store'}</div>
      ${settings.showTaxDetails ? '<div>GST Included | ABN: XX XXX XXX XXX</div>' : ''}
    </div>
  `;
};

// ==================== MODERN TEMPLATE ====================
const generateModernTemplate = (order: Order, settings: ReceiptSettingsDto, getOrderType: Function, getPaymentMethod: Function): string => {
  return `
    <style>
      .modern-header { text-align: center; margin-bottom: 25px; padding-bottom: 15px; border-bottom: 2px solid #000; }
      .modern-title { font-size: ${settings.fontSize + 8}px; font-weight: bold; letter-spacing: 2px; }
      .modern-divider { height: 2px; background: #000; margin: 15px 0; }
      .modern-row { display: flex; justify-content: space-between; margin-bottom: 8px; padding: 5px 0; }
      .modern-label { font-weight: 600; color: #333; }
      .modern-item { background: #f8f8f8; padding: 10px; margin-bottom: 8px; border-radius: 5px; }
      .modern-total { background: #000; color: #fff; padding: 12px; margin: 15px 0; font-size: ${settings.fontSize + 4}px; font-weight: bold; }
      .modern-footer { text-align: center; margin-top: 25px; padding-top: 15px; border-top: 2px solid #000; }
    </style>

    <div class="modern-header">
      ${settings.showLogo && settings.logoUrl ? `<img src="${settings.logoUrl}" style="max-width: 120px; margin-bottom: 10px;" />` : ''}
      <div class="modern-title">${settings.storeName || 'MY STORE'}</div>
      ${settings.storeAddress ? `<div style="font-size: ${settings.fontSize - 1}px; margin-top: 5px;">${settings.storeAddress}</div>` : ''}
      ${settings.headerText ? `<div style="font-size: ${settings.fontSize}px; margin-top: 10px; font-style: italic;">${settings.headerText}</div>` : ''}
    </div>

    <div class="modern-row">
      <span class="modern-label">Receipt #</span>
      <strong>${order.orderNumber}</strong>
    </div>
    <div class="modern-row">
      <span class="modern-label">Date</span>
      <span>${format(new Date(order.orderDate), 'dd/MM/yyyy HH:mm')}</span>
    </div>
    ${settings.showCustomerInfo ? `
      <div class="modern-row">
        <span class="modern-label">Customer</span>
        <span>${order.customerName || 'Walk-in'}</span>
      </div>
    ` : ''}

    <div class="modern-divider"></div>

    ${order.items && order.items.length > 0 ? 
      order.items.filter(item => !item.isVoided).map(item => `
        <div class="modern-item">
          <div style="display: flex; justify-content: space-between; font-weight: bold; margin-bottom: 5px;">
            <span>${item.productName}</span>
            <span>$${item.totalAmount.toFixed(2)}</span>
          </div>
          <div style="font-size: ${settings.fontSize - 2}px; color: #666;">
            ${item.quantity} × $${item.unitPriceIncGst.toFixed(2)}
          </div>
        </div>
      `).join('') : ''
    }

    <div class="modern-divider"></div>

    <div class="modern-row">
      <span>Subtotal</span>
      <span>$${order.subTotal.toFixed(2)}</span>
    </div>
    ${settings.showTaxDetails ? `
      <div class="modern-row">
        <span>Tax</span>
        <span>$${order.taxAmount.toFixed(2)}</span>
      </div>
    ` : ''}

    <div class="modern-total">
      <div style="display: flex; justify-content: space-between;">
        <span>TOTAL</span>
        <span>$${order.totalAmount.toFixed(2)}</span>
      </div>
    </div>

    ${order.payments && order.payments.length > 0 ? `
      <div class="modern-row">
        <span class="modern-label">Payment</span>
        <span>${getPaymentMethod(order.payments[0].paymentMethod)}</span>
      </div>
    ` : ''}

    ${settings.showQRCode ? `
      <div style="text-align: center; margin: 20px 0;">
        <div style="border: 2px solid #000; width: 100px; height: 100px; display: inline-flex; align-items: center; justify-content: center; font-weight: bold;">
          [QR]
        </div>
      </div>
    ` : ''}

    <div class="modern-footer">
      ${settings.footerText ? `<div style="margin-bottom: 10px; font-style: italic;">${settings.footerText}</div>` : ''}
      ${settings.storeWebsite ? `<div style="font-size: ${settings.fontSize - 2}px;">${settings.storeWebsite}</div>` : ''}
    </div>
  `;
};

// ==================== COMPACT TEMPLATE ====================
const generateCompactTemplate = (order: Order, settings: ReceiptSettingsDto, getOrderType: Function, getPaymentMethod: Function): string => {
  return `
    <style>
      .compact { font-size: ${settings.fontSize - 1}px; }
      .compact-header { text-align: center; margin-bottom: 10px; }
      .compact-row { display: flex; justify-content: space-between; margin-bottom: 2px; }
      .compact-divider { border-top: 1px dashed #000; margin: 8px 0; }
    </style>

    <div class="compact">
      <div class="compact-header">
        <div style="font-weight: bold; font-size: ${settings.fontSize + 2}px;">${settings.storeName || 'My Store'}</div>
      </div>

      <div class="compact-row">
        <span>Order:</span>
        <strong>${order.orderNumber}</strong>
      </div>
      <div class="compact-row">
        <span>${format(new Date(order.orderDate), 'dd/MM/yyyy HH:mm')}</span>
      </div>

      <div class="compact-divider"></div>

      ${order.items && order.items.length > 0 ? 
        order.items.filter(item => !item.isVoided).map(item => `
          <div class="compact-row">
            <span>${item.productName}</span>
            <span>$${item.totalAmount.toFixed(2)}</span>
          </div>
        `).join('') : ''
      }

      <div class="compact-divider"></div>

      <div class="compact-row">
        <span>Subtotal:</span>
        <span>$${order.subTotal.toFixed(2)}</span>
      </div>
      ${settings.showTaxDetails ? `
        <div class="compact-row">
          <span>Tax:</span>
          <span>$${order.taxAmount.toFixed(2)}</span>
        </div>
      ` : ''}
      <div class="compact-row" style="font-weight: bold; font-size: ${settings.fontSize + 1}px; padding-top: 5px; border-top: 1px solid #000;">
        <span>TOTAL:</span>
        <span>$${order.totalAmount.toFixed(2)}</span>
      </div>

      <div class="compact-divider"></div>

      <div style="text-align: center; font-size: ${settings.fontSize - 2}px;">
        ${settings.footerText || 'Thank you!'}
      </div>
    </div>
  `;
};

// ==================== DETAILED TEMPLATE ====================
const generateDetailedTemplate = (order: Order, settings: ReceiptSettingsDto, getOrderType: Function, getPaymentMethod: Function): string => {
  return `
    <style>
      .detailed-header { text-align: center; margin-bottom: 20px; padding: 15px; background: #f5f5f5; }
      .detailed-section { margin-bottom: 15px; padding: 10px; border: 1px solid #ddd; }
      .detailed-title { font-weight: bold; margin-bottom: 10px; font-size: ${settings.fontSize + 2}px; border-bottom: 2px solid #000; padding-bottom: 5px; }
      .detailed-row { display: flex; justify-content: space-between; margin-bottom: 5px; }
      .detailed-item { padding: 10px; margin-bottom: 10px; border: 1px solid #eee; }
    </style>

    <div class="detailed-header">
      ${settings.showLogo && settings.logoUrl ? `<img src="${settings.logoUrl}" style="max-width: 100px; margin-bottom: 10px;" />` : ''}
      <div style="font-size: ${settings.fontSize + 6}px; font-weight: bold;">${settings.storeName || 'My Store'}</div>
      ${settings.storeAddress ? `<div style="font-size: ${settings.fontSize - 1}px;">${settings.storeAddress}</div>` : ''}
      ${settings.storePhone ? `<div style="font-size: ${settings.fontSize - 1}px;">📞 ${settings.storePhone}</div>` : ''}
      ${settings.storeEmail ? `<div style="font-size: ${settings.fontSize - 1}px;">✉ ${settings.storeEmail}</div>` : ''}
      ${settings.storeWebsite ? `<div style="font-size: ${settings.fontSize - 1}px;">🌐 ${settings.storeWebsite}</div>` : ''}
    </div>

    <div class="detailed-section">
      <div class="detailed-title">ORDER INFORMATION</div>
      <div class="detailed-row"><span>Receipt Number:</span><strong>${order.orderNumber}</strong></div>
      <div class="detailed-row"><span>Date & Time:</span><span>${format(new Date(order.orderDate), 'dd/MM/yyyy HH:mm:ss')}</span></div>
      <div class="detailed-row"><span>Order Type:</span><span>${getOrderType(order.orderType)}</span></div>
      ${order.tableNumber ? `<div class="detailed-row"><span>Table Number:</span><span>${order.tableNumber}</span></div>` : ''}
      ${settings.showCashier ? `<div class="detailed-row"><span>Served by:</span><span>${order.cashierName || 'N/A'}</span></div>` : ''}
    </div>

    ${settings.showCustomerInfo && order.customerName ? `
      <div class="detailed-section">
        <div class="detailed-title">CUSTOMER DETAILS</div>
        <div class="detailed-row"><span>Name:</span><span>${order.customerName}</span></div>
      </div>
    ` : ''}

    <div class="detailed-section">
      <div class="detailed-title">ITEMS PURCHASED</div>
      ${order.items && order.items.length > 0 ? 
        order.items.filter(item => !item.isVoided).map((item, index) => `
          <div class="detailed-item">
            <div style="font-weight: bold; margin-bottom: 5px;">${index + 1}. ${item.productName}</div>
            <div class="detailed-row">
              <span>Unit Price:</span>
              <span>$${item.unitPriceIncGst.toFixed(2)}</span>
            </div>
            <div class="detailed-row">
              <span>Quantity:</span>
              <span>${item.quantity}</span>
            </div>
            ${item.discountAmount > 0 ? `
              <div class="detailed-row" style="color: #d9534f;">
                <span>Discount:</span>
                <span>-$${item.discountAmount.toFixed(2)}</span>
              </div>
            ` : ''}
            <div class="detailed-row" style="font-weight: bold; border-top: 1px solid #ddd; padding-top: 5px; margin-top: 5px;">
              <span>Subtotal:</span>
              <span>$${item.totalAmount.toFixed(2)}</span>
            </div>
          </div>
        `).join('') : '<div style="text-align: center; color: #999;">No items</div>'
      }
    </div>

    <div class="detailed-section">
      <div class="detailed-title">PAYMENT SUMMARY</div>
      <div class="detailed-row"><span>Items Subtotal:</span><span>$${order.subTotal.toFixed(2)}</span></div>
      ${order.discountAmount > 0 ? `<div class="detailed-row" style="color: #d9534f;"><span>Total Discount:</span><span>-$${order.discountAmount.toFixed(2)}</span></div>` : ''}
      ${settings.showTaxDetails ? `<div class="detailed-row"><span>GST (10%):</span><span>$${order.taxAmount.toFixed(2)}</span></div>` : ''}
      <div class="detailed-row" style="font-weight: bold; font-size: ${settings.fontSize + 3}px; padding-top: 10px; border-top: 2px solid #000; margin-top: 10px;">
        <span>TOTAL AMOUNT:</span>
        <span>$${order.totalAmount.toFixed(2)}</span>
      </div>
      ${order.payments && order.payments.length > 0 ? `
        <div style="margin-top: 10px; padding-top: 10px; border-top: 1px dashed #000;">
          ${order.payments.map(payment => `
            <div class="detailed-row">
              <span>${getPaymentMethod(payment.paymentMethod)}:</span>
              <span>$${payment.amount.toFixed(2)}</span>
            </div>
          `).join('')}
          ${order.changeAmount > 0 ? `
            <div class="detailed-row" style="font-weight: bold;">
              <span>Change Given:</span>
              <span>$${order.changeAmount.toFixed(2)}</span>
            </div>
          ` : ''}
        </div>
      ` : ''}
    </div>

    ${settings.showBarcode || settings.showQRCode ? `
      <div class="detailed-section" style="text-align: center;">
        ${settings.showBarcode ? `<div style="font-family: monospace; font-size: ${settings.fontSize + 2}px;">|||| ${order.orderNumber} ||||</div>` : ''}
        ${settings.showQRCode ? `<div style="margin-top: 10px; border: 2px solid #000; width: 120px; height: 120px; display: inline-flex; align-items: center; justify-content: center; font-weight: bold;">[QR CODE]</div>` : ''}
      </div>
    ` : ''}

    <div style="text-align: center; margin-top: 20px; padding-top: 15px; border-top: 2px solid #000;">
      ${settings.headerText ? `<div style="font-style: italic; margin-bottom: 10px;">${settings.headerText}</div>` : ''}
      ${settings.footerText ? `<div style="font-weight: bold; margin-bottom: 10px;">${settings.footerText}</div>` : ''}
      ${settings.showTaxDetails ? '<div style="font-size: ' + (settings.fontSize - 2) + 'px;">GST Included | ABN: XX XXX XXX XXX</div>' : ''}
      ${settings.showSocial ? `
        <div style="margin-top: 10px; font-size: ${settings.fontSize - 2}px;">
          <div>Follow Us on Social Media</div>
          <div>Facebook | Instagram | Twitter</div>
        </div>
      ` : ''}
    </div>
  `;
};

// ==================== ELEGANT TEMPLATE ====================
const generateElegantTemplate = (order: Order, settings: ReceiptSettingsDto, getOrderType: Function, getPaymentMethod: Function): string => {
  return `
    <style>
      .elegant { font-family: 'Georgia', serif; }
      .elegant-header { text-align: center; margin-bottom: 25px; padding-bottom: 15px; border-bottom: 3px double #000; }
      .elegant-title { font-size: ${settings.fontSize + 8}px; font-weight: normal; letter-spacing: 3px; font-variant: small-caps; }
      .elegant-row { display: flex; justify-content: space-between; margin-bottom: 8px; padding: 5px 10px; }
      .elegant-item { border-bottom: 1px solid #eee; padding: 10px 0; }
      .elegant-total { border-top: 3px double #000; border-bottom: 3px double #000; padding: 15px 10px; margin: 20px 0; }
      .elegant-footer { text-align: center; margin-top: 25px; padding-top: 15px; border-top: 3px double #000; font-style: italic; }
    </style>

    <div class="elegant">
      <div class="elegant-header">
        <div class="elegant-title">${settings.storeName || 'MY STORE'}</div>
        ${settings.storeAddress ? `<div style="font-size: ${settings.fontSize - 1}px; margin-top: 8px;">${settings.storeAddress}</div>` : ''}
        ${settings.headerText ? `<div style="font-size: ${settings.fontSize}px; margin-top: 12px; font-style: italic;">${settings.headerText}</div>` : ''}
      </div>

      <div style="text-align: center; margin-bottom: 20px;">
        <div style="font-size: ${settings.fontSize + 2}px; font-weight: bold;">Receipt</div>
        <div style="font-size: ${settings.fontSize - 1}px;">${order.orderNumber}</div>
        <div style="font-size: ${settings.fontSize - 2}px; margin-top: 5px;">${format(new Date(order.orderDate), 'MMMM dd, yyyy • HH:mm')}</div>
      </div>

      <div style="margin: 20px 0;">
        ${order.items && order.items.length > 0 ? 
          order.items.filter(item => !item.isVoided).map(item => `
            <div class="elegant-item">
              <div style="display: flex; justify-content: space-between; font-weight: bold; margin-bottom: 5px;">
                <span>${item.productName}</span>
                <span>$${item.totalAmount.toFixed(2)}</span>
              </div>
              <div style="font-size: ${settings.fontSize - 2}px; color: #666; padding-left: 10px;">
                ${item.quantity} at $${item.unitPriceIncGst.toFixed(2)} each
              </div>
            </div>
          `).join('') : ''
        }
      </div>

      <div class="elegant-total">
        <div class="elegant-row">
          <span>Subtotal</span>
          <span>$${order.subTotal.toFixed(2)}</span>
        </div>
        ${settings.showTaxDetails ? `
          <div class="elegant-row">
            <span>Tax</span>
            <span>$${order.taxAmount.toFixed(2)}</span>
          </div>
        ` : ''}
        <div class="elegant-row" style="font-size: ${settings.fontSize + 3}px; font-weight: bold; margin-top: 10px; padding-top: 10px; border-top: 1px solid #000;">
          <span>Total</span>
          <span>$${order.totalAmount.toFixed(2)}</span>
        </div>
      </div>

      <div class="elegant-footer">
        ${settings.footerText ? `<div style="margin-bottom: 10px;">${settings.footerText}</div>` : ''}
        ${settings.showSocial && settings.storeEmail ? `<div style="font-size: ${settings.fontSize - 2}px;">${settings.storeEmail}</div>` : ''}
      </div>
    </div>
  `;
};

// ==================== MINIMALIST TEMPLATE ====================
const generateMinimalistTemplate = (order: Order, settings: ReceiptSettingsDto, getOrderType: Function, getPaymentMethod: Function): string => {
  return `
    <style>
      .minimalist { font-family: 'Helvetica', 'Arial', sans-serif; line-height: 1.8; }
      .minimalist-row { display: flex; justify-content: space-between; margin-bottom: 5px; }
    </style>

    <div class="minimalist">
      <div style="text-align: center; margin-bottom: 20px;">
        <div style="font-size: ${settings.fontSize + 4}px; font-weight: 300;">${settings.storeName || 'Store'}</div>
      </div>

      <div style="margin-bottom: 15px;">
        <div>${order.orderNumber}</div>
        <div style="font-size: ${settings.fontSize - 1}px; color: #999;">${format(new Date(order.orderDate), 'dd/MM/yyyy HH:mm')}</div>
      </div>

      <div style="margin: 20px 0;">
        ${order.items && order.items.length > 0 ? 
          order.items.filter(item => !item.isVoided).map(item => `
            <div class="minimalist-row">
              <span>${item.productName}</span>
              <span>$${item.totalAmount.toFixed(2)}</span>
            </div>
          `).join('') : ''
        }
      </div>

      <div style="border-top: 1px solid #000; margin-top: 20px; padding-top: 10px;">
        <div class="minimalist-row" style="font-size: ${settings.fontSize + 2}px; font-weight: bold;">
          <span>Total</span>
          <span>$${order.totalAmount.toFixed(2)}</span>
        </div>
      </div>

      <div style="text-align: center; margin-top: 30px; font-size: ${settings.fontSize - 2}px; color: #999;">
        ${settings.footerText || ''}
      </div>
    </div>
  `;
};

// ==================== THERMAL TEMPLATE ====================
const generateThermalTemplate = (order: Order, settings: ReceiptSettingsDto, getOrderType: Function, getPaymentMethod: Function): string => {
  return `
    <style>
      .thermal { font-family: 'Courier New', monospace; font-size: ${settings.fontSize - 1}px; }
      .thermal-row { display: flex; justify-content: space-between; margin-bottom: 2px; }
      .thermal-divider { border-top: 1px dashed #000; margin: 8px 0; }
    </style>

    <div class="thermal">
      <div style="text-align: center; margin-bottom: 10px;">
        <div style="font-weight: bold; font-size: ${settings.fontSize + 2}px;">${settings.storeName || 'Store'}</div>
      </div>

      <div class="thermal-divider"></div>

      <div class="thermal-row">
        <span>No:</span>
        <strong>${order.orderNumber}</strong>
      </div>
      <div class="thermal-row">
        <span>Date:</span>
        <span>${format(new Date(order.orderDate), 'dd/MM/yy HH:mm')}</span>
      </div>

      <div class="thermal-divider"></div>

      ${order.items && order.items.length > 0 ? 
        order.items.filter(item => !item.isVoided).map(item => `
          <div class="thermal-row">
            <span>${item.productName.substring(0, 20)}</span>
            <span>$${item.totalAmount.toFixed(2)}</span>
          </div>
          <div style="font-size: ${settings.fontSize - 2}px; padding-left: 5px;">
            ${item.quantity}x${item.unitPriceIncGst.toFixed(2)}
          </div>
        `).join('') : ''
      }

      <div class="thermal-divider"></div>

      <div class="thermal-row">
        <span>SUB:</span>
        <span>$${order.subTotal.toFixed(2)}</span>
      </div>
      ${settings.showTaxDetails ? `
        <div class="thermal-row">
          <span>GST:</span>
          <span>$${order.taxAmount.toFixed(2)}</span>
        </div>
      ` : ''}
      <div class="thermal-row" style="font-weight: bold; font-size: ${settings.fontSize + 1}px; padding-top: 3px; border-top: 1px solid #000;">
        <span>TOT:</span>
        <span>$${order.totalAmount.toFixed(2)}</span>
      </div>

      <div class="thermal-divider"></div>

      ${settings.showBarcode ? `
        <div style="text-align: center; font-family: monospace; margin: 10px 0;">
          |||| ${order.orderNumber} ||||
        </div>
      ` : ''}

      <div style="text-align: center; font-size: ${settings.fontSize - 2}px; margin-top: 10px;">
        ${settings.footerText || 'Thank you'}
      </div>
    </div>
  `;
};

// ==================== CUSTOM TEMPLATE ====================
const generateCustomTemplate = (order: Order, settings: ReceiptSettingsDto, getOrderType: Function, getPaymentMethod: Function): string => {
  // For custom template, use standard as base (user can modify via customTemplate field)
  return generateStandardTemplate(order, settings, getOrderType, getPaymentMethod);
};

export default PrintReceipt;
