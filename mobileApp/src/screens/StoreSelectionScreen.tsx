import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
} from 'react-native';
import { useDispatch, useSelector } from 'react-redux';
import Icon from 'react-native-vector-icons/Ionicons';
import { colors, spacing } from '../constants/theme';
import { AppDispatch, RootState } from '../store/store';
import { setSelectedStore, fetchStores } from '../store/slices/storeSlice';
import Logo from '../components/common/Logo';

interface Store {
  id: number;
  name: string;
  address: string;
  phone: string;
  isActive: boolean;
}

const StoreSelectionScreen = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { stores, isLoading, error, selectedStoreId } = useSelector(
    (state: RootState) => state.store
  );
  const { user } = useSelector((state: RootState) => state.auth);

  useEffect(() => {
    // Fetch stores when component mounts
    dispatch(fetchStores());
  }, [dispatch]);

  const handleStoreSelect = (storeId: number) => {
    dispatch(setSelectedStore(storeId));
  };

  const renderStoreItem = ({ item }: { item: Store }) => {
    const isSelected = selectedStoreId === item.id;

    return (
      <TouchableOpacity
        style={[styles.storeCard, isSelected && styles.storeCardSelected]}
        onPress={() => handleStoreSelect(item.id)}
        activeOpacity={0.7}
      >
        <View style={styles.storeIconContainer}>
          <Icon
            name={isSelected ? 'checkmark-circle' : 'storefront-outline'}
            size={40}
            color={isSelected ? colors.primary : colors.textLight}
          />
        </View>
        <View style={styles.storeInfo}>
          <Text style={styles.storeName}>{item.name}</Text>
          <Text style={styles.storeAddress}>{item.address}</Text>
          <Text style={styles.storePhone}>{item.phone}</Text>
        </View>
        {isSelected && (
          <View style={styles.selectedBadge}>
            <Icon name="checkmark" size={20} color="#fff" />
          </View>
        )}
      </TouchableOpacity>
    );
  };

  if (isLoading && stores.length === 0) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={colors.primary} />
        <Text style={styles.loadingText}>Loading stores...</Text>
      </View>
    );
  }

  if (error) {
    return (
      <View style={styles.errorContainer}>
        <Icon name="alert-circle-outline" size={60} color={colors.error} />
        <Text style={styles.errorText}>{error}</Text>
        <TouchableOpacity
          style={styles.retryButton}
          onPress={() => dispatch(fetchStores())}
        >
          <Text style={styles.retryButtonText}>Retry</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Logo size={80} />
        <Text style={styles.title}>Select Your Store</Text>
        <Text style={styles.subtitle}>
          Welcome, {user?.firstName}! Choose a store to continue shopping.
        </Text>
      </View>

      <FlatList
        data={stores.filter(store => store.isActive)}
        renderItem={renderStoreItem}
        keyExtractor={(item) => item.id.toString()}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Icon name="storefront-outline" size={60} color={colors.textLight} />
            <Text style={styles.emptyText}>No stores available</Text>
            <Text style={styles.emptySubtext}>
              Please contact support if you believe this is an error.
            </Text>
          </View>
        }
      />

      {selectedStoreId && (
        <View style={styles.footer}>
          <Text style={styles.footerText}>
            Store selected! You can now continue shopping.
          </Text>
        </View>
      )}
    </View>
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
    backgroundColor: colors.background,
    padding: spacing.xl,
  },
  errorText: {
    marginTop: spacing.md,
    fontSize: 16,
    color: colors.error,
    textAlign: 'center',
  },
  retryButton: {
    marginTop: spacing.lg,
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
    paddingTop: spacing.xl * 1.5,
    paddingBottom: spacing.xl,
    paddingHorizontal: spacing.lg,
    alignItems: 'center',
    backgroundColor: '#fff',
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    marginBottom: spacing.sm,
  },
  title: {
    fontSize: 18,
    fontWeight: '600',
    color: colors.text,
    marginTop: spacing.md,
  },
  subtitle: {
    fontSize: 13,
    color: colors.textLight,
    marginTop: spacing.xs,
    textAlign: 'center',
    paddingHorizontal: spacing.sm,
  },
  listContainer: {
    padding: spacing.lg,
  },
  storeCard: {
    flexDirection: 'row',
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: spacing.lg,
    marginBottom: spacing.md,
    borderWidth: 2,
    borderColor: colors.border,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  storeCardSelected: {
    borderColor: colors.primary,
    backgroundColor: '#f0f9ff',
  },
  storeIconContainer: {
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: spacing.md,
  },
  storeInfo: {
    flex: 1,
    justifyContent: 'center',
  },
  storeName: {
    fontSize: 18,
    fontWeight: 'bold',
    color: colors.text,
    marginBottom: spacing.xs,
  },
  storeAddress: {
    fontSize: 14,
    color: colors.textLight,
    marginBottom: 2,
  },
  storePhone: {
    fontSize: 14,
    color: colors.textLight,
  },
  selectedBadge: {
    position: 'absolute',
    top: 12,
    right: 12,
    backgroundColor: colors.primary,
    borderRadius: 20,
    width: 32,
    height: 32,
    justifyContent: 'center',
    alignItems: 'center',
  },
  footer: {
    padding: spacing.lg,
    backgroundColor: '#d1fae5',
    borderTopWidth: 1,
    borderTopColor: '#86efac',
  },
  footerText: {
    fontSize: 14,
    color: '#166534',
    textAlign: 'center',
    fontWeight: '600',
  },
  emptyContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: spacing.xl * 2,
  },
  emptyText: {
    fontSize: 18,
    fontWeight: '600',
    color: colors.textLight,
    marginTop: spacing.md,
  },
  emptySubtext: {
    fontSize: 14,
    color: colors.textLight,
    marginTop: spacing.xs,
    textAlign: 'center',
  },
});

export default StoreSelectionScreen;
