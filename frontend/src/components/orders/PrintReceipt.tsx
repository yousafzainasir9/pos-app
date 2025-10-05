import React from 'react';
import { format } from 'date-fns';
import { Order, OrderStatus, PaymentMethod } from '@/types';

interface PrintReceiptProps {
  order: Order;
}

const PrintReceipt: React.FC<PrintReceiptProps> = ({ order }) => {
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

  const getOrderTypeLabel = (type: number) => {
    const typeMap: { [key: number]: string } = {
      1: 'Dine In',
      2: 'Take Away',
      3: 'Delivery',
      4: 'Pickup'
    };

    return typeMap[type] || 'Unknown';
  };

  return (
    <div style={{
      fontFamily: 'monospace',
      fontSize: '12px',
      lineHeight: '1.4',
      maxWidth: '300px',
      margin: '0 auto',
      padding: '20px'
    }}>
      {/* Header */}
      <div style={{ textAlign: 'center', marginBottom: '20px' }}>
        <div style={{ fontSize: '18px', fontWeight: 'bold', marginBottom: '5px' }}>
          {order.storeName || 'Cookie Barrel POS'}
        </div>
        <div style={{ fontSize: '10px' }}>
          Tax Invoice / Receipt
        </div>
      </div>

      {/* Order Details */}
      <div style={{ borderTop: '1px dashed #000', borderBottom: '1px dashed #000', padding: '10px 0', marginBottom: '10px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '3px' }}>
          <span>Order #:</span>
          <span style={{ fontWeight: 'bold' }}>{order.orderNumber}</span>
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '3px' }}>
          <span>Date:</span>
          <span>{format(new Date(order.orderDate), 'dd/MM/yyyy HH:mm')}</span>
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '3px' }}>
          <span>Type:</span>
          <span>{getOrderTypeLabel(order.orderType)}</span>
        </div>
        {order.tableNumber && (
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '3px' }}>
            <span>Table:</span>
            <span>{order.tableNumber}</span>
          </div>
        )}
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '3px' }}>
          <span>Customer:</span>
          <span>{order.customerName || 'Walk-in'}</span>
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '3px' }}>
          <span>Cashier:</span>
          <span>{order.cashierName || 'N/A'}</span>
        </div>
      </div>

      {/* Items */}
      <div style={{ marginBottom: '10px' }}>
        <div style={{ fontWeight: 'bold', marginBottom: '5px', borderBottom: '1px solid #000' }}>
          ITEMS
        </div>
        {order.items && order.items.length > 0 ? (
          order.items.filter(item => !item.isVoided).map((item, index) => (
            <div key={index} style={{ marginBottom: '8px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ flex: 1 }}>{item.productName}</span>
                <span>${item.totalAmount.toFixed(2)}</span>
              </div>
              <div style={{ fontSize: '10px', color: '#666', paddingLeft: '10px' }}>
                {item.quantity} x ${item.unitPriceIncGst.toFixed(2)}
                {item.discountAmount > 0 && ` (Disc: -$${item.discountAmount.toFixed(2)})`}
              </div>
            </div>
          ))
        ) : (
          <div style={{ textAlign: 'center', color: '#999', padding: '10px' }}>
            No items
          </div>
        )}
      </div>

      {/* Totals */}
      <div style={{ borderTop: '1px dashed #000', paddingTop: '10px', marginBottom: '10px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '3px' }}>
          <span>Subtotal:</span>
          <span>${order.subTotal.toFixed(2)}</span>
        </div>
        {order.discountAmount > 0 && (
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '3px', color: '#d9534f' }}>
            <span>Discount:</span>
            <span>-${order.discountAmount.toFixed(2)}</span>
          </div>
        )}
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '3px' }}>
          <span>GST:</span>
          <span>${order.taxAmount.toFixed(2)}</span>
        </div>
        <div style={{ 
          display: 'flex', 
          justifyContent: 'space-between', 
          marginTop: '5px', 
          paddingTop: '5px',
          borderTop: '1px solid #000',
          fontSize: '14px',
          fontWeight: 'bold'
        }}>
          <span>TOTAL:</span>
          <span>${order.totalAmount.toFixed(2)}</span>
        </div>
      </div>

      {/* Payments */}
      {order.payments && order.payments.length > 0 && (
        <div style={{ borderTop: '1px dashed #000', paddingTop: '10px', marginBottom: '10px' }}>
          <div style={{ fontWeight: 'bold', marginBottom: '5px' }}>
            PAYMENT
          </div>
          {order.payments.map((payment, index) => (
            <div key={index} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '3px' }}>
              <span>{getPaymentMethodLabel(payment.paymentMethod)}:</span>
              <span>${payment.amount.toFixed(2)}</span>
            </div>
          ))}
          <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            marginTop: '5px',
            paddingTop: '5px',
            borderTop: '1px solid #000',
            fontWeight: 'bold'
          }}>
            <span>Paid:</span>
            <span>${order.paidAmount.toFixed(2)}</span>
          </div>
          {order.changeAmount > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '3px' }}>
              <span>Change:</span>
              <span>${order.changeAmount.toFixed(2)}</span>
            </div>
          )}
        </div>
      )}

      {/* Footer */}
      <div style={{ 
        borderTop: '1px dashed #000', 
        paddingTop: '10px', 
        textAlign: 'center',
        fontSize: '10px'
      }}>
        <div style={{ marginBottom: '5px' }}>
          Thank you for your purchase!
        </div>
        <div style={{ marginBottom: '5px' }}>
          {order.storeName || 'Cookie Barrel'}
        </div>
        <div>
          GST Included | ABN: XX XXX XXX XXX
        </div>
      </div>

      {/* Barcode placeholder */}
      <div style={{ 
        textAlign: 'center', 
        marginTop: '15px',
        fontFamily: 'monospace',
        fontSize: '10px'
      }}>
        <div>* {order.orderNumber} *</div>
      </div>
    </div>
  );
};

// Function to print the receipt
export const printReceipt = (order: Order) => {
  const printWindow = window.open('', '', 'width=400,height=600');
  
  if (!printWindow) {
    alert('Please allow pop-ups to print receipts');
    return;
  }

  const printContent = document.createElement('div');
  printContent.innerHTML = `
    <!DOCTYPE html>
    <html>
      <head>
        <title>Receipt - ${order.orderNumber}</title>
        <style>
          @media print {
            body { margin: 0; }
            @page { margin: 0; }
          }
          body {
            font-family: 'Courier New', monospace;
            font-size: 12px;
            line-height: 1.4;
            margin: 0;
            padding: 20px;
          }
          .receipt-container {
            max-width: 300px;
            margin: 0 auto;
          }
          .header {
            text-align: center;
            margin-bottom: 20px;
          }
          .header-title {
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 5px;
          }
          .header-subtitle {
            font-size: 10px;
          }
          .section {
            border-top: 1px dashed #000;
            border-bottom: 1px dashed #000;
            padding: 10px 0;
            margin-bottom: 10px;
          }
          .row {
            display: flex;
            justify-content: space-between;
            margin-bottom: 3px;
          }
          .items-header {
            font-weight: bold;
            margin-bottom: 5px;
            border-bottom: 1px solid #000;
          }
          .item {
            margin-bottom: 8px;
          }
          .item-details {
            font-size: 10px;
            color: #666;
            padding-left: 10px;
          }
          .totals {
            border-top: 1px dashed #000;
            padding-top: 10px;
            margin-bottom: 10px;
          }
          .total-line {
            display: flex;
            justify-content: space-between;
            margin-top: 5px;
            padding-top: 5px;
            border-top: 1px solid #000;
            font-size: 14px;
            font-weight: bold;
          }
          .payments {
            border-top: 1px dashed #000;
            padding-top: 10px;
            margin-bottom: 10px;
          }
          .footer {
            border-top: 1px dashed #000;
            padding-top: 10px;
            text-align: center;
            font-size: 10px;
          }
          .barcode {
            text-align: center;
            margin-top: 15px;
            font-family: monospace;
            font-size: 10px;
          }
        </style>
      </head>
      <body>
        <div class="receipt-container">
          <div class="header">
            <div class="header-title">${order.storeName || 'Cookie Barrel POS'}</div>
            <div class="header-subtitle">Tax Invoice / Receipt</div>
          </div>

          <div class="section">
            <div class="row"><span>Order #:</span><strong>${order.orderNumber}</strong></div>
            <div class="row"><span>Date:</span><span>${format(new Date(order.orderDate), 'dd/MM/yyyy HH:mm')}</span></div>
            <div class="row"><span>Type:</span><span>${getOrderType(order.orderType)}</span></div>
            ${order.tableNumber ? `<div class="row"><span>Table:</span><span>${order.tableNumber}</span></div>` : ''}
            <div class="row"><span>Customer:</span><span>${order.customerName || 'Walk-in'}</span></div>
            <div class="row"><span>Cashier:</span><span>${order.cashierName || 'N/A'}</span></div>
          </div>

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

          <div class="totals">
            <div class="row"><span>Subtotal:</span><span>$${order.subTotal.toFixed(2)}</span></div>
            ${order.discountAmount > 0 ? 
              `<div class="row" style="color: #d9534f;"><span>Discount:</span><span>-$${order.discountAmount.toFixed(2)}</span></div>` : ''
            }
            <div class="row"><span>GST:</span><span>$${order.taxAmount.toFixed(2)}</span></div>
            <div class="total-line"><span>TOTAL:</span><span>$${order.totalAmount.toFixed(2)}</span></div>
          </div>

          ${order.payments && order.payments.length > 0 ? `
            <div class="payments">
              <div style="font-weight: bold; margin-bottom: 5px;">PAYMENT</div>
              ${order.payments.map(payment => `
                <div class="row">
                  <span>${getPaymentMethod(payment.paymentMethod)}:</span>
                  <span>$${payment.amount.toFixed(2)}</span>
                </div>
              `).join('')}
              <div class="total-line"><span>Paid:</span><span>$${order.paidAmount.toFixed(2)}</span></div>
              ${order.changeAmount > 0 ? 
                `<div class="row" style="margin-top: 3px;"><span>Change:</span><span>$${order.changeAmount.toFixed(2)}</span></div>` : ''
              }
            </div>
          ` : ''}

          <div class="footer">
            <div style="margin-bottom: 5px;">Thank you for your purchase!</div>
            <div style="margin-bottom: 5px;">${order.storeName || 'Cookie Barrel'}</div>
            <div>GST Included | ABN: XX XXX XXX XXX</div>
          </div>

          <div class="barcode">
            <div>* ${order.orderNumber} *</div>
          </div>
        </div>
      </body>
    </html>
  `;

  printWindow.document.write(printContent.innerHTML);
  printWindow.document.close();
  
  // Wait for content to load before printing
  printWindow.onload = () => {
    printWindow.focus();
    printWindow.print();
    printWindow.close();
  };
};

// Helper functions for the print template
function getOrderType(type: number): string {
  const typeMap: { [key: number]: string } = {
    1: 'Dine In',
    2: 'Take Away',
    3: 'Delivery',
    4: 'Pickup'
  };
  return typeMap[type] || 'Unknown';
}

function getPaymentMethod(method: number): string {
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
}

export default PrintReceipt;
