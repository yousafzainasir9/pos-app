import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  TextInput,
  ActivityIndicator,
  ScrollView,
  RefreshControl,
} from 'react-native';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import Icon from 'react-native-vector-icons/Ionicons';
import { colors, spacing } from '../constants/theme';
import { AppDispatch, RootState } from '../store/store';
import {
  fetchProducts,
  fetchCategories,
  setSelectedCategory,
  searchProducts,
  clearFilters,
} from '../store/slices/productsSlice';
import { addToCart } from '../store/slices/cartSlice';
import ProductCard from '../components/products/ProductCard';
import { Product } from '../types/product.types';
import { RootStackParamList } from '../navigation/AppNavigator';

type NavigationProp = NativeStackNavigationProp<RootStackParamList>;

const HomeScreen = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigation = useNavigation<NavigationProp>();
  const {
    filteredProducts,
    categories,
    selectedCategoryId,
    isLoading,
    error,
  } = useSelector((state: RootState) => state.products);
  const { selectedStoreId } = useSelector((state: RootState) => state.store);

  const [searchQuery, setSearchQuery] = useState('');
  const [refreshing, setRefreshing] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    await Promise.all([
      dispatch(fetchProducts()),
      dispatch(fetchCategories()),
    ]);
  };

  const onRefresh = async () => {
    setRefreshing(true);
    await loadData();
    setRefreshing(false);
  };

  const handleSearch = (query: string) => {
    setSearchQuery(query);
    if (query.trim().length > 0) {
      dispatch(searchProducts(query));
    } else {
      dispatch(fetchProducts());
    }
  };

  const handleCategorySelect = (categoryId: number | null) => {
    setSearchQuery('');
    if (categoryId === null) {
      // Show all products
      dispatch(setSelectedCategory(null));
    } else {
      // Filter by category - backend uses categoryId query param
      dispatch(setSelectedCategory(categoryId));
    }
  };

  const handleAddToCart = (product: Product) => {
    dispatch(addToCart(product));
  };

  const handleProductPress = (product: Product) => {
    // Navigate to product detail screen
    navigation.navigate('ProductDetail', { productId: product.id });
  };

  const renderCategoryChip = ({ item }: { item: any }) => {
    const isSelected = selectedCategoryId === item.id;
    const isAll = item.id === null;

    return (
      <TouchableOpacity
        style={[
          styles.categoryChip,
          (isSelected || (isAll && selectedCategoryId === null)) && styles.categoryChipSelected,
        ]}
        onPress={() => handleCategorySelect(item.id)}
        activeOpacity={0.7}
      >
        <Text
          style={[
            styles.categoryChipText,
            (isSelected || (isAll && selectedCategoryId === null)) && styles.categoryChipTextSelected,
          ]}
        >
          {item.name}
        </Text>
      </TouchableOpacity>
    );
  };

  const renderProduct = ({ item }: { item: Product }) => (
    <View style={styles.productContainer}>
      <ProductCard
        product={item}
        onPress={() => handleProductPress(item)}
        onAddToCart={() => handleAddToCart(item)}
      />
    </View>
  );

  if (isLoading && filteredProducts.length === 0) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={colors.primary} />
        <Text style={styles.loadingText}>Loading products...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      {/* Search Bar */}
      <View style={styles.searchContainer}>
        <Icon name="search-outline" size={20} color={colors.textLight} style={styles.searchIcon} />
        <TextInput
          style={styles.searchInput}
          placeholder="Search cookies, cakes, pastries..."
          placeholderTextColor={colors.textLight}
          value={searchQuery}
          onChangeText={handleSearch}
          returnKeyType="search"
        />
        {searchQuery.length > 0 && (
          <TouchableOpacity
            onPress={() => {
              setSearchQuery('');
              dispatch(fetchProducts());
            }}
            style={styles.clearButton}
          >
            <Icon name="close-circle" size={20} color={colors.textLight} />
          </TouchableOpacity>
        )}
      </View>

      {/* Categories */}
      <View style={styles.categoriesContainer}>
        <View style={styles.categoriesWrap}>
          {/* All Categories */}
          <View key="category-all">
            {renderCategoryChip({ item: { id: null, name: 'All' } })}
          </View>
          
          {/* Category List */}
          {categories
            .filter((cat) => cat.isActive)
            .sort((a, b) => a.displayOrder - b.displayOrder)
            .map((category) => (
              <View key={`category-${category.id}`}>
                {renderCategoryChip({ item: category })}
              </View>
            ))}
        </View>
      </View>

      {/* Products Grid */}
      {error ? (
        <View style={styles.errorContainer}>
          <Icon name="alert-circle-outline" size={60} color={colors.error} />
          <Text style={styles.errorText}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={loadData}>
            <Text style={styles.retryButtonText}>Retry</Text>
          </TouchableOpacity>
        </View>
      ) : filteredProducts.length === 0 ? (
        <View style={styles.emptyContainer}>
          <Icon name="basket-outline" size={60} color={colors.textLight} />
          <Text style={styles.emptyText}>No products found</Text>
          <Text style={styles.emptySubtext}>
            {searchQuery
              ? 'Try a different search term'
              : 'Check back later for new items'}
          </Text>
        </View>
      ) : (
        <FlatList
          data={filteredProducts}
          renderItem={renderProduct}
          keyExtractor={(item) => item.id.toString()}
          numColumns={2}
          contentContainerStyle={styles.productsList}
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={onRefresh}
              colors={[colors.primary]}
            />
          }
        />
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
  searchContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#fff',
    marginHorizontal: spacing.md,
    marginTop: spacing.md,
    marginBottom: spacing.sm,
    paddingHorizontal: spacing.md,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  searchIcon: {
    marginRight: spacing.sm,
  },
  searchInput: {
    flex: 1,
    height: 44,
    fontSize: 15,
    color: colors.text,
  },
  clearButton: {
    padding: spacing.xs,
  },
  categoriesContainer: {
    backgroundColor: '#fff',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  categoriesWrap: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.sm,
  },
  categoryChip: {
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.sm,
    borderRadius: 20,
    backgroundColor: '#f5f5f5',
    borderWidth: 1,
    borderColor: colors.border,
  },
  categoryChipSelected: {
    backgroundColor: colors.primary,
    borderColor: colors.primary,
  },
  categoryChipText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.text,
  },
  categoryChipTextSelected: {
    color: '#fff',
  },
  productsList: {
    padding: spacing.md,
  },
  productContainer: {
    flex: 1,
    maxWidth: '50%',
    padding: spacing.xs,
  },
  errorContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
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
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: spacing.xl,
  },
  emptyText: {
    marginTop: spacing.md,
    fontSize: 18,
    fontWeight: '600',
    color: colors.textLight,
  },
  emptySubtext: {
    marginTop: spacing.xs,
    fontSize: 14,
    color: colors.textLight,
    textAlign: 'center',
  },
});

export default HomeScreen;
