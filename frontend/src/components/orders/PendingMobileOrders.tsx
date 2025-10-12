import React, { useEffect, useState } from 'react';
import { Card, Badge, Button, Spinner, Dropdown } from 'react-bootstrap';
import { FaBell, FaMobileAlt, FaClock, FaCheck, FaEye, FaEllipsisV } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { format, formatDistanceToNow } from 'date-fns';
import orderService from '@/services/order.service';
import { Order, OrderStatus } from '@/types';
import { toast } from 'react-toastify';

const PendingMobileOrders: React.FC = () => {
  const navigate = useNavigate();
  const [pendingCount, setPendingCount] = useState(0);
  const [recentOrders, setRecentOrders] = useState<Order[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [lastCheck, setLastCheck] = useState<Date>(new Date());

  useEffect(() => {
    loadPendingOrders();
    
    // Auto-refresh every 30 seconds
    const interval = setInterval(() => {
      loadPendingOrders();
    }, 30000);
    
    return () => clearInterval(interval);
  }, []);

  const loadPendingOrders = async () => {
    setIsLoading(true);
    try {
      const response = await orderService.getOrders({
        status: OrderStatus.Pending,
        page: 1,
        pageSize: 10
      });
      
      // Filter mobile orders (those without shift)
      const mobileOrders = response.data.filter(
        (order: Order) => !order.shiftId
      );
      
      setPendingCount(mobileOrders.length);
      setRecentOrders(mobileOrders.slice(0, 5)); // Show max 5
      setLastCheck(new Date());
      
      // Play sound if there are new orders (optional)
      if (mobileOrders.length > 0 && pendingCount === 0) {
        playNotificationSound();
      }
    } catch (error) {
      console.error('Failed to load pending orders:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const playNotificationSound = () => {
    try {
      // Create a simple beep sound using Web Audio API
      const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
      const oscillator = audioContext.createOscillator();
      const gainNode = audioContext.createGain();
      
      oscillator.connect(gainNode);
      gainNode.connect(audioContext.destination);
      
      oscillator.frequency.value = 800;
      oscillator.type = 'sine';
      
      gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
      gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.5);
      
      oscillator.start(audioContext.currentTime);
      oscillator.stop(audioContext.currentTime + 0.5);
    } catch (error) {
      // Silently fail if sound doesn't work
      console.log('Sound notification not available');
    }
  };

  const getOrderAge = (orderDate: Date) => {
    return formatDistanceToNow(new Date(orderDate), { addSuffix: true });
  };

  const handleCompleteOrder = async (orderId: number, orderNumber: string, e: React.MouseEvent) => {
    e.stopPropagation(); // Prevent card click
    
    if (!window.confirm(`Mark order ${orderNumber} as completed?`)) {
      return;
    }

    try {
      // Process payment first (full amount)
      const order = await orderService.getOrder(orderId);
      
      await orderService.processPayment({
        orderId: order.id,
        amount: order.totalAmount,
        paymentMethod: 1, // Cash
        notes: 'Completed from mobile orders widget'
      });
      
      toast.success(`Order ${orderNumber} marked as completed!`);
      
      // Reload orders
      loadPendingOrders();
    } catch (error: any) {
      console.error('Failed to complete order:', error);
      toast.error(error.response?.data?.message || 'Failed to complete order');
    }
  };

  const handleViewOrder = (orderId: number, e: React.MouseEvent) => {
    e.stopPropagation();
    navigate(`/orders`);
  };

  if (pendingCount === 0) {
    return null; // Don't show widget if no pending orders
  }

  return (
    <Card className="mb-3 border-warning shadow-sm">
      <Card.Header className="bg-warning text-dark d-flex justify-content-between align-items-center">
        <div>
          <FaBell className="me-2" />
          <strong>New Mobile Orders</strong>
          <Badge bg="danger" className="ms-2">{pendingCount}</Badge>
        </div>
        {isLoading && <Spinner animation="border" size="sm" />}
      </Card.Header>
      <Card.Body className="p-2">
        {recentOrders.map(order => (
          <div 
            key={order.id} 
            className="d-flex justify-content-between align-items-start p-2 border-bottom"
          >
            <div className="flex-grow-1">
              <div className="d-flex align-items-center mb-1">
                <FaMobileAlt className="text-info me-2" size={14} />
                <strong style={{ fontSize: '0.9em' }}>{order.orderNumber}</strong>
              </div>
              <div className="text-muted" style={{ fontSize: '0.85em' }}>
                {order.customerName || 'Walk-in Customer'}
              </div>
              <div className="text-muted d-flex align-items-center mt-1" style={{ fontSize: '0.75em' }}>
                <FaClock className="me-1" />
                {getOrderAge(order.orderDate)}
              </div>
            </div>
            <div className="text-end d-flex align-items-start gap-2">
              <div>
                <strong className="text-success" style={{ fontSize: '0.95em' }}>
                  ${order.totalAmount.toFixed(2)}
                </strong>
                <div className="text-muted" style={{ fontSize: '0.75em' }}>
                  {order.items?.length || 0} items
                </div>
              </div>
              <Dropdown align="end">
                <Dropdown.Toggle 
                  variant="link" 
                  size="sm" 
                  className="text-secondary p-0 border-0"
                  style={{ boxShadow: 'none' }}
                >
                  <FaEllipsisV size={12} />
                </Dropdown.Toggle>
                <Dropdown.Menu>
                  <Dropdown.Item onClick={(e) => handleViewOrder(order.id, e)}>
                    <FaEye className="me-2" />
                    View Details
                  </Dropdown.Item>
                  <Dropdown.Divider />
                  <Dropdown.Item 
                    onClick={(e) => handleCompleteOrder(order.id, order.orderNumber, e)}
                    className="text-success"
                  >
                    <FaCheck className="me-2" />
                    Mark Complete
                  </Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>
            </div>
          </div>
        ))}
        
        {pendingCount > 5 && (
          <div className="text-center text-muted py-1" style={{ fontSize: '0.85em' }}>
            +{pendingCount - 5} more orders
          </div>
        )}
        
        <Button 
          variant="primary" 
          size="sm" 
          className="w-100 mt-2"
          onClick={() => navigate('/orders')}
        >
          View All Orders →
        </Button>
        
        <div className="text-center text-muted mt-2" style={{ fontSize: '0.7em' }}>
          Last checked: {format(lastCheck, 'HH:mm:ss')}
        </div>
      </Card.Body>
    </Card>
  );
};

export default PendingMobileOrders;
