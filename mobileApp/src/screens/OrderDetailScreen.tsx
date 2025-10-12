import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  ActivityIndicator,
  TouchableOpacity,
  Alert,
} from 'react-native';
import { useRoute, RouteProp, useNavigation } from '@react-navigation/native';
import Icon from 'react-native-vector-icons/Ionicons';
import { colors, spacing } from '../constants/theme';
import { ordersApi } from '../api/orders.api';
import { Order } from '../types/order.types';
import { RootStackParamList } from '../navigation/AppNavigator';

type OrderDetailRouteProp = RouteProp<RootStackParamList, 'OrderDetail'>;

const OrderDetailScreen = () => {
  const route = useRoute<OrderDetailRouteProp>();
  const navigation = useNavigation();
  const { orderId } = route.params;

  const [order, setOrder] = useState<Order | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadOrderDetails();
  }, [orderId]);

  const loadOrderDetails = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const orderData = await ordersApi.getById(orderId);
      setOrder(orderData);
    } catch (err: any) {
      console.error('Error loading order details:', err);
      setError(err.response?.data?.message || 'Failed to load order details');
    } finally {
      setIsLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed':
        return '#10b981';
      case 'Pending':
        return '#f59e0b';
      case 'Processing':
        return '#3b82f6';
      case 'Cancelled':
        return '#ef4444';
      default:
        return colors.textLight;
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Completed':
        return 'checkmark-circle';
      case 'Pending':
        return 'time';
      case 'Processing':
        return 'hourglass';
      case 'Cancelled':
        return 'close-circle';
      default:
        return 'information-circle';
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-AU', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatCurrency = (amount: number) => {
    return `$${amount.toFixed(2)}`;
  };

  if (isLoading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={colors.primary} />
        <Text style={styles.loadingText}>Loading order details...</Text>
      </View>
    );
  }

  if (error || !order) {
    return (
      <View style={styles.errorContainer}>
        <Icon name="alert-circle-outline" size={80} color={colors.error} />
        <Text style={styles.errorTitle}>Failed to Load Order</Text>
        <Text style={styles.errorText}>{error || 'Order not found'}</Text>
        <TouchableOpacity style={styles.retryButton} onPress={loadOrderDetails}>
          <Text style={styles.retryButtonText}>Retry</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <ScrollView style={styles.container} showsVerticalScrollIndicator={false}>
      {/* Order Header */}
      <View style={styles.header}>
        <View style={styles.orderNumberSection}>
          <Text style={styles.orderLabel}>Order Number</Text>
          <Text style={styles.orderNumber}>#{order.orderNumber}</Text>
        </View>
        
        <View style={[styles.statusBadge, { backgroundColor: getStatusColor(order.status) + '20' }]}>
          <Icon name={getStatusIcon(order.status)} size={20} color={getStatusColor(order.status)} />
          <Text style={[styles.statusText, { color: getStatusColor(order.status) }]}>
            {order.status}
          </Text>
        </View>
      </View>

      {/* Order Info Card */}
      <View style={styles.card}>
        <View style={styles.infoRow}>
          <Icon name="calendar-outline" size={20} color={colors.textLight} />
          <View style={styles.infoContent}>
            <Text style={styles.infoLabel}>Order Date</Text>
            <Text style={styles.infoValue}>{formatDate(order.orderDate)}</Text>
          </View>
        </View>

        <View style={styles.divider} />

        <View style={styles.infoRow}>
          <Icon name="location-outline" size={20} color={colors.textLight} />
          <View style={styles.infoContent}>
            <Text style={styles.infoLabel}>Store</Text>
            <Text style={styles.infoValue}>{order.storeName || 'N/A'}</Text>
          </View>
        </View>

        <View style={styles.divider} />

        <View style={styles.infoRow}>
          <Icon name="bag-outline" size={20} color={colors.textLight} />
          <View style={styles.infoContent}>
            <Text style={styles.infoLabel}>Order Type</Text>
            <Text style={styles.infoValue}>{order.orderType || 'N/A'}</Text>
          </View>
        </View>

        {order.notes && (
          <>
            <View style={styles.divider} />
            <View style={styles.infoRow}>
              <Icon name="document-text-outline" size={20} color={colors.textLight} />
              <View style={styles.infoContent}>
                <Text style={styles.infoLabel}>Notes</Text>
                <Text style={styles.infoValue}>{order.notes}</Text>
              </View>
            </View>
          </>
        )}
      </View>

      {/* Order Items */}
      <View style={styles.card}>
        <Text style={styles.sectionTitle}>Order Items</Text>
        {order.items && order.items.map((item, index) => (
          <View key={item.id || index}>
            {index > 0 && <View style={styles.divider} />}
            <View style={styles.itemRow}>
              <View style={styles.itemInfo}>
                <Text style={styles.itemName}>{item.productName || 'Product'}</Text>
                {item.productSKU && (
                  <Text style={styles.itemSKU}>SKU: {item.productSKU}</Text>
                )}
                <Text style={styles.itemQuantity}>Qty: {item.quantity}</Text>
              </View>
              <View style={styles.itemPricing}>
                <Text style={styles.itemPrice}>{formatCurrency(item.unitPriceIncGst)}</Text>
                <Text style={styles.itemTotal}>{formatCurrency(item.totalAmount)}</Text>
              </View>
            </View>
          </View>
        ))}
      </View>

      {/* Order Summary */}
      <View style={styles.card}>
        <Text style={styles.sectionTitle}>Order Summary</Text>
        
        <View style={styles.summaryRow}>
          <Text style={styles.summaryLabel}>Subtotal</Text>
          <Text style={styles.summaryValue}>{formatCurrency(order.subTotal)}</Text>
        </View>

        {order.discountAmount > 0 && (
          <View style={styles.summaryRow}>
            <Text style={styles.summaryLabel}>Discount</Text>
            <Text style={[styles.summaryValue, { color: colors.success }]}>
              -{formatCurrency(order.discountAmount)}
            </Text>
          </View>
        )}

        <View style={styles.summaryRow}>
          <Text style={styles.summaryLabel}>GST (10%)</Text>
          <Text style={styles.summaryValue}>{formatCurrency(order.taxAmount)}</Text>
        </View>

        <View style={styles.divider} />

        <View style={styles.summaryRow}>
          <Text style={styles.totalLabel}>Total</Text>
          <Text style={styles.totalValue}>{formatCurrency(order.totalAmount)}</Text>
        </View>

        {order.paidAmount > 0 && (
          <>
            <View style={styles.summaryRow}>
              <Text style={styles.summaryLabel}>Paid</Text>
              <Text style={[styles.summaryValue, { color: colors.success }]}>
                {formatCurrency(order.paidAmount)}
              </Text>
            </View>
            
            {order.changeAmount > 0 && (
              <View style={styles.summaryRow}>
                <Text style={styles.summaryLabel}>Change</Text>
                <Text style={styles.summaryValue}>{formatCurrency(order.changeAmount)}</Text>
              </View>
            )}
          </>
        )}
      </View>

      {/* Payments Section (if any) */}
      {order.payments && order.payments.length > 0 && (
        <View style={styles.card}>
          <Text style={styles.sectionTitle}>Payment Details</Text>
          {order.payments.map((payment, index) => (
            <View key={payment.id || index}>
              {index > 0 && <View style={styles.divider} />}
              <View style={styles.paymentRow}>
                <View>
                  <Text style={styles.paymentMethod}>{payment.paymentMethod}</Text>
                  <Text style={styles.paymentDate}>
                    {new Date(payment.paymentDate).toLocaleDateString('en-AU')}
                  </Text>
                </View>
                <Text style={styles.paymentAmount}>{formatCurrency(payment.amount)}</Text>
              </View>
            </View>
          ))}
        </View>
      )}

      {/* Action Buttons */}
      {order.status === 'Pending' && (
        <View style={styles.actionsCard}>
          <TouchableOpacity
            style={styles.cancelButton}
            onPress={() => {
              Alert.alert(
                'Cancel Order',
                'Are you sure you want to cancel this order?',
                [
                  { text: 'No', style: 'cancel' },
                  {
                    text: 'Yes, Cancel',
                    style: 'destructive',
                    onPress: async () => {
                      try {
                        await ordersApi.voidOrder(order.id, 'Cancelled by customer');
                        Alert.alert('Success', 'Order cancelled successfully');
                        navigation.goBack();
                      } catch (err) {
                        Alert.alert('Error', 'Failed to cancel order');
                      }
                    },
                  },
                ]
              );
            }}
          >
            <Icon name="close-circle-outline" size={20} color="#fff" />
            <Text style={styles.cancelButtonText}>Cancel Order</Text>
          </TouchableOpacity>
        </View>
      )}

      <View style={{ height: spacing.xl }} />
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: colors.background,
  },
  loadingText: {
    marginTop: spacing.md,
    fontSize: 16,
    color: colors.textLight,
  },
  errorContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: spacing.xl,
    backgroundColor: colors.background,
  },
  errorTitle: {
    marginTop: spacing.lg,
    fontSize: 22,
    fontWeight: 'bold',
    color: colors.text,
  },
  errorText: {
    marginTop: spacing.sm,
    fontSize: 15,
    color: colors.textLight,
    textAlign: 'center',
  },
  retryButton: {
    marginTop: spacing.xl,
    backgroundColor: colors.primary,
    paddingHorizontal: spacing.xl,
    paddingVertical: spacing.md,
    borderRadius: 8,
  },
  retryButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
  header: {
    backgroundColor: '#fff',
    padding: spacing.lg,
    marginBottom: spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  orderNumberSection: {
    marginBottom: spacing.md,
  },
  orderLabel: {
    fontSize: 12,
    color: colors.textLight,
    marginBottom: 4,
  },
  orderNumber: {
    fontSize: 24,
    fontWeight: 'bold',
    color: colors.text,
  },
  statusBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.sm,
    borderRadius: 20,
    alignSelf: 'flex-start',
    gap: spacing.sm,
  },
  statusText: {
    fontSize: 14,
    fontWeight: '600',
  },
  card: {
    backgroundColor: '#fff',
    padding: spacing.lg,
    marginHorizontal: spacing.md,
    marginBottom: spacing.md,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: colors.text,
    marginBottom: spacing.md,
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: spacing.md,
  },
  infoContent: {
    flex: 1,
  },
  infoLabel: {
    fontSize: 12,
    color: colors.textLight,
    marginBottom: 4,
  },
  infoValue: {
    fontSize: 15,
    color: colors.text,
  },
  divider: {
    height: 1,
    backgroundColor: colors.border,
    marginVertical: spacing.md,
  },
  itemRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: spacing.sm,
  },
  itemInfo: {
    flex: 1,
  },
  itemName: {
    fontSize: 15,
    fontWeight: '600',
    color: colors.text,
    marginBottom: 4,
  },
  itemSKU: {
    fontSize: 12,
    color: colors.textLight,
    marginBottom: 4,
  },
  itemQuantity: {
    fontSize: 13,
    color: colors.textLight,
  },
  itemPricing: {
    alignItems: 'flex-end',
  },
  itemPrice: {
    fontSize: 13,
    color: colors.textLight,
    marginBottom: 4,
  },
  itemTotal: {
    fontSize: 16,
    fontWeight: '600',
    color: colors.text,
  },
  summaryRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: spacing.sm,
  },
  summaryLabel: {
    fontSize: 15,
    color: colors.text,
  },
  summaryValue: {
    fontSize: 15,
    color: colors.text,
  },
  totalLabel: {
    fontSize: 18,
    fontWeight: 'bold',
    color: colors.text,
  },
  totalValue: {
    fontSize: 22,
    fontWeight: 'bold',
    color: colors.primary,
  },
  paymentRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: spacing.sm,
  },
  paymentMethod: {
    fontSize: 15,
    fontWeight: '600',
    color: colors.text,
    marginBottom: 4,
  },
  paymentDate: {
    fontSize: 13,
    color: colors.textLight,
  },
  paymentAmount: {
    fontSize: 16,
    fontWeight: '600',
    color: colors.success,
  },
  actionsCard: {
    paddingHorizontal: spacing.md,
    marginBottom: spacing.md,
  },
  cancelButton: {
    flexDirection: 'row',
    backgroundColor: colors.error,
    padding: spacing.md,
    borderRadius: 8,
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.sm,
  },
  cancelButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
});

export default OrderDetailScreen;
