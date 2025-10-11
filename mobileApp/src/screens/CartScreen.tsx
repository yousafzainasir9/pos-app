import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  Image,
  Alert,
} from 'react-native';
import { useSelector, useDispatch } from 'react-redux';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import Icon from 'react-native-vector-icons/Ionicons';
import { colors, spacing } from '../constants/theme';
import { RootState, AppDispatch } from '../store/store';
import { removeFromCart, updateQuantity, clearCart } from '../store/slices/cartSlice';
import { CartItem } from '../types/product.types';
import { RootStackParamList } from '../navigation/AppNavigator';

type NavigationProp = NativeStackNavigationProp<RootStackParamList>;

const CartScreen = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigation = useNavigation<NavigationProp>();
  const { items, subtotal, gstAmount, totalAmount } = useSelector(
    (state: RootState) => state.cart
  );

  const handleQuantityChange = (productId: number, currentQuantity: number, change: number) => {
    const newQuantity = currentQuantity + change;
    if (newQuantity > 0) {
      dispatch(updateQuantity({ productId, quantity: newQuantity }));
    } else {
      handleRemoveItem(productId);
    }
  };

  const handleRemoveItem = (productId: number) => {
    Alert.alert(
      'Remove Item',
      'Are you sure you want to remove this item from your cart?',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Remove',
          style: 'destructive',
          onPress: () => dispatch(removeFromCart(productId)),
        },
      ]
    );
  };

  const handleClearCart = () => {
    Alert.alert(
      'Clear Cart',
      'Are you sure you want to remove all items from your cart?',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Clear',
          style: 'destructive',
          onPress: () => dispatch(clearCart()),
        },
      ]
    );
  };

  const handleCheckout = () => {
    if (items.length === 0) {
      Alert.alert('Empty Cart', 'Please add items to your cart before checking out.');
      return;
    }
    navigation.navigate('Checkout');
  };

  const formatPrice = (price: number) => {
    return `$${price.toFixed(2)}`;
  };

  const renderCartItem = ({ item }: { item: CartItem }) => {
    const itemTotal = (item.product.priceIncGst || 0) * item.quantity;

    return (
      <View style={styles.cartItem}>
        {/* Product Image */}
        <View style={styles.imageContainer}>
          {item.product.imageUrl ? (
            <Image
              source={{ uri: item.product.imageUrl }}
              style={styles.productImage}
              resizeMode="cover"
            />
          ) : (
            <View style={styles.placeholderImage}>
              <Icon name="image-outline" size={30} color={colors.textLight} />
            </View>
          )}
        </View>

        {/* Product Details */}
        <View style={styles.itemDetails}>
          <Text style={styles.productName} numberOfLines={2}>
            {item.product.name}
          </Text>
          <Text style={styles.productPrice}>
            {formatPrice(item.product.priceIncGst || 0)}
          </Text>

          {/* Quantity Controls */}
          <View style={styles.quantityContainer}>
            <TouchableOpacity
              style={styles.quantityButton}
              onPress={() => handleQuantityChange(item.product.id, item.quantity, -1)}
            >
              <Icon name="remove" size={20} color={colors.primary} />
            </TouchableOpacity>

            <Text style={styles.quantityText}>{item.quantity}</Text>

            <TouchableOpacity
              style={styles.quantityButton}
              onPress={() => handleQuantityChange(item.product.id, item.quantity, 1)}
            >
              <Icon name="add" size={20} color={colors.primary} />
            </TouchableOpacity>
          </View>
        </View>

        {/* Item Total & Remove */}
        <View style={styles.itemRight}>
          <TouchableOpacity
            style={styles.removeButton}
            onPress={() => handleRemoveItem(item.product.id)}
          >
            <Icon name="trash-outline" size={20} color={colors.error} />
          </TouchableOpacity>
          <Text style={styles.itemTotal}>{formatPrice(itemTotal)}</Text>
        </View>
      </View>
    );
  };

  const renderEmptyCart = () => (
    <View style={styles.emptyContainer}>
      <Icon name="cart-outline" size={80} color={colors.textLight} />
      <Text style={styles.emptyTitle}>Your cart is empty</Text>
      <Text style={styles.emptyText}>Add some delicious items from our menu!</Text>
      <TouchableOpacity
        style={styles.shopButton}
        onPress={() => navigation.navigate('MainTabs')}
      >
        <Text style={styles.shopButtonText}>Start Shopping</Text>
      </TouchableOpacity>
    </View>
  );

  if (items.length === 0) {
    return <View style={styles.container}>{renderEmptyCart()}</View>;
  }

  return (
    <View style={styles.container}>
      {/* Cart Items List */}
      <FlatList
        data={items}
        renderItem={renderCartItem}
        keyExtractor={(item) => item.product.id.toString()}
        contentContainerStyle={styles.listContent}
        ListHeaderComponent={
          <View style={styles.header}>
            <Text style={styles.itemCount}>
              {items.length} {items.length === 1 ? 'item' : 'items'}
            </Text>
            <TouchableOpacity onPress={handleClearCart}>
              <Text style={styles.clearButton}>Clear All</Text>
            </TouchableOpacity>
          </View>
        }
      />

      {/* Cart Summary */}
      <View style={styles.summaryContainer}>
        <View style={styles.summaryRow}>
          <Text style={styles.summaryLabel}>Subtotal</Text>
          <Text style={styles.summaryValue}>{formatPrice(subtotal)}</Text>
        </View>

        <View style={styles.summaryRow}>
          <Text style={styles.summaryLabel}>GST (10% included)</Text>
          <Text style={styles.summaryValue}>{formatPrice(gstAmount)}</Text>
        </View>

        <View style={styles.divider} />

        <View style={styles.summaryRow}>
          <Text style={styles.totalLabel}>Total</Text>
          <Text style={styles.totalValue}>{formatPrice(totalAmount)}</Text>
        </View>

        {/* Checkout Button */}
        <TouchableOpacity style={styles.checkoutButton} onPress={handleCheckout}>
          <Text style={styles.checkoutButtonText}>Proceed to Checkout</Text>
          <Icon name="arrow-forward" size={20} color="#fff" />
        </TouchableOpacity>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  listContent: {
    padding: spacing.md,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.md,
  },
  itemCount: {
    fontSize: 16,
    fontWeight: '600',
    color: colors.text,
  },
  clearButton: {
    fontSize: 14,
    color: colors.error,
    fontWeight: '600',
  },
  cartItem: {
    flexDirection: 'row',
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: spacing.md,
    marginBottom: spacing.md,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  imageContainer: {
    width: 80,
    height: 80,
    borderRadius: 8,
    overflow: 'hidden',
    marginRight: spacing.md,
  },
  productImage: {
    width: '100%',
    height: '100%',
  },
  placeholderImage: {
    width: '100%',
    height: '100%',
    backgroundColor: '#f0f0f0',
    justifyContent: 'center',
    alignItems: 'center',
  },
  itemDetails: {
    flex: 1,
    justifyContent: 'space-between',
  },
  productName: {
    fontSize: 15,
    fontWeight: '600',
    color: colors.text,
    marginBottom: spacing.xs,
  },
  productPrice: {
    fontSize: 14,
    color: colors.textLight,
    marginBottom: spacing.xs,
  },
  quantityContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: spacing.xs,
  },
  quantityButton: {
    width: 28,
    height: 28,
    borderRadius: 14,
    backgroundColor: '#f0f0f0',
    justifyContent: 'center',
    alignItems: 'center',
  },
  quantityText: {
    fontSize: 16,
    fontWeight: '600',
    color: colors.text,
    marginHorizontal: spacing.md,
    minWidth: 30,
    textAlign: 'center',
  },
  itemRight: {
    alignItems: 'flex-end',
    justifyContent: 'space-between',
  },
  removeButton: {
    padding: spacing.xs,
  },
  itemTotal: {
    fontSize: 16,
    fontWeight: 'bold',
    color: colors.primary,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: spacing.xl,
  },
  emptyTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: colors.text,
    marginTop: spacing.lg,
    marginBottom: spacing.xs,
  },
  emptyText: {
    fontSize: 15,
    color: colors.textLight,
    textAlign: 'center',
    marginBottom: spacing.xl,
  },
  shopButton: {
    backgroundColor: colors.primary,
    paddingHorizontal: spacing.xl,
    paddingVertical: spacing.md,
    borderRadius: 8,
  },
  shopButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
  summaryContainer: {
    backgroundColor: '#fff',
    padding: spacing.lg,
    borderTopWidth: 1,
    borderTopColor: colors.border,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: -2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 8,
  },
  summaryRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.sm,
  },
  summaryLabel: {
    fontSize: 14,
    color: colors.textLight,
  },
  summaryValue: {
    fontSize: 14,
    color: colors.text,
    fontWeight: '500',
  },
  divider: {
    height: 1,
    backgroundColor: colors.border,
    marginVertical: spacing.sm,
  },
  totalLabel: {
    fontSize: 18,
    fontWeight: 'bold',
    color: colors.text,
  },
  totalValue: {
    fontSize: 20,
    fontWeight: 'bold',
    color: colors.primary,
  },
  checkoutButton: {
    backgroundColor: colors.primary,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    paddingVertical: spacing.md,
    borderRadius: 8,
    marginTop: spacing.md,
  },
  checkoutButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
    marginRight: spacing.sm,
  },
});

export default CartScreen;
