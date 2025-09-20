import React, { useState, useEffect } from 'react';
import { Row, Col, Card, Button, Form, InputGroup, Badge } from 'react-bootstrap';
import { FaSearch, FaBarcode, FaShoppingCart, FaTh, FaList, FaImage } from 'react-icons/fa';
import { useCart } from '@/contexts/CartContext';
import { useShift } from '@/contexts/ShiftContext';
import { Product, Category } from '@/types';
import productService from '@/services/product.service';
import { toast } from 'react-toastify';
import { useNavigate } from 'react-router-dom';
import './POSPage.css';

const POSPage: React.FC = () => {
  const navigate = useNavigate();
  const { addItem } = useCart();
  const { isShiftOpen } = useShift();
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<number | null>(null);
  const [selectedSubcategory, setSelectedSubcategory] = useState<number | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [barcode, setBarcode] = useState('');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    loadCategories();
    loadProducts();
  }, []);

  useEffect(() => {
    if (!isShiftOpen) {
      toast.warning('Please open a shift to start selling', {
        toastId: 'shift-warning',
        autoClose: false
      });
    }
  }, [isShiftOpen]);

  const loadCategories = async () => {
    try {
      const data = await productService.getCategories(true);
      setCategories(data);
    } catch (error) {
      console.error('Failed to load categories:', error);
    }
  };

  const loadProducts = async () => {
    setIsLoading(true);
    try {
      const params: any = {};
      if (searchTerm) params.search = searchTerm;
      if (selectedCategory) params.categoryId = selectedCategory;
      if (selectedSubcategory) params.subcategoryId = selectedSubcategory;

      const data = await productService.getProducts(params);
      setProducts(data as unknown as Product[]);
    } catch (error) {
      console.error('Failed to load products:', error);
      toast.error('Failed to load products');
    } finally {
      setIsLoading(false);
    }
  };

  const handleBarcodeSearch = async () => {
    if (!barcode) return;

    try {
      const product = await productService.getProductByBarcode(barcode);
      if (product) {
        handleAddToCart(product);
        setBarcode('');
      }
    } catch (error) {
      toast.error('Product not found');
    }
  };

  const handleAddToCart = (product: Product) => {
    if (!isShiftOpen) {
      toast.error('Please open a shift first');
      navigate('/shift');
      return;
    }

    if (product.trackInventory && product.stockQuantity <= 0) {
      toast.error('Product out of stock');
      return;
    }

    addItem(product);
    toast.success(`${product.name} added to cart`, {
      position: 'bottom-right',
      autoClose: 2000
    });
  };

  const handleCategorySelect = (categoryId: number | null) => {
    setSelectedCategory(categoryId);
    setSelectedSubcategory(null);
    loadProducts();
  };

  const handleSubcategorySelect = (subcategoryId: number | null) => {
    setSelectedSubcategory(subcategoryId);
    loadProducts();
  };

  const getSelectedCategory = () => {
    return categories.find(c => c.id === selectedCategory);
  };

  const getProductImage = (product: Product) => {
    // Return product image URL or a placeholder
    if (product.imageUrl) {
      return product.imageUrl;
    }
    // Use a placeholder image service or default image
    return `https://via.placeholder.com/300x200/f8f9fa/6c757d?text=${encodeURIComponent(product.name)}`;
  };

  return (
    <div className="pos-page">
      <Row className="mb-3">
        <Col>
          <div className="d-flex justify-content-between align-items-center">
            <h2>Point of Sale</h2>
            <div className="d-flex gap-2">
              <Button
                variant={viewMode === 'grid' ? 'primary' : 'outline-primary'}
                size="sm"
                onClick={() => setViewMode('grid')}
              >
                <FaTh />
              </Button>
              <Button
                variant={viewMode === 'list' ? 'primary' : 'outline-primary'}
                size="sm"
                onClick={() => setViewMode('list')}
              >
                <FaList />
              </Button>
              <Button
                variant="success"
                onClick={() => navigate('/cart')}
                disabled={!isShiftOpen}
              >
                <FaShoppingCart className="me-2" />
                Checkout
              </Button>
            </div>
          </div>
        </Col>
      </Row>

      {/* Search Bar */}
      <Row className="mb-3">
        <Col md={6}>
          <InputGroup>
            <InputGroup.Text>
              <FaSearch />
            </InputGroup.Text>
            <Form.Control
              type="text"
              placeholder="Search products..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && loadProducts()}
            />
            <Button variant="primary" onClick={loadProducts}>
              Search
            </Button>
          </InputGroup>
        </Col>
        <Col md={6}>
          <InputGroup>
            <InputGroup.Text>
              <FaBarcode />
            </InputGroup.Text>
            <Form.Control
              type="text"
              placeholder="Scan or enter barcode..."
              value={barcode}
              onChange={(e) => setBarcode(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleBarcodeSearch()}
            />
            <Button variant="primary" onClick={handleBarcodeSearch}>
              Add
            </Button>
          </InputGroup>
        </Col>
      </Row>

      {/* Categories */}
      <Row className="mb-3">
        <Col>
          <div className="d-flex gap-2 flex-wrap">
            <Button
              variant={!selectedCategory ? 'primary' : 'outline-primary'}
              size="sm"
              onClick={() => handleCategorySelect(null)}
            >
              All Products
            </Button>
            {categories.map(category => (
              <Button
                key={category.id}
                variant={selectedCategory === category.id ? 'primary' : 'outline-primary'}
                size="sm"
                onClick={() => handleCategorySelect(category.id)}
              >
                {category.name}
              </Button>
            ))}
          </div>
        </Col>
      </Row>

      {/* Subcategories */}
      {selectedCategory && getSelectedCategory()?.subcategories && (
        <Row className="mb-3">
          <Col>
            <div className="d-flex gap-2 flex-wrap">
              <Button
                variant={!selectedSubcategory ? 'secondary' : 'outline-secondary'}
                size="sm"
                onClick={() => handleSubcategorySelect(null)}
              >
                All {getSelectedCategory()?.name}
              </Button>
              {getSelectedCategory()?.subcategories?.map(subcategory => (
                <Button
                  key={subcategory.id}
                  variant={selectedSubcategory === subcategory.id ? 'secondary' : 'outline-secondary'}
                  size="sm"
                  onClick={() => handleSubcategorySelect(subcategory.id)}
                >
                  {subcategory.name}
                </Button>
              ))}
            </div>
          </Col>
        </Row>
      )}

      {/* Products */}
      {isLoading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      ) : viewMode === 'grid' ? (
        <Row className="g-3">
          {products.map(product => (
            <Col key={product.id} xs={6} md={4} lg={3}>
              <Card 
                className="h-100 product-card shadow-sm"
                role="button"
                onClick={() => handleAddToCart(product)}
                style={{ cursor: isShiftOpen ? 'pointer' : 'not-allowed' }}
              >
                <div className="product-image-container" style={{ 
                  height: '180px', 
                  overflow: 'hidden',
                  backgroundColor: '#f8f9fa',
                  position: 'relative'
                }}>
                  {product.imageUrl ? (
                    <Card.Img 
                      variant="top" 
                      src={product.imageUrl}
                      alt={product.name}
                      style={{
                        height: '100%',
                        width: '100%',
                        objectFit: 'cover'
                      }}
                      onError={(e: any) => {
                        e.target.onerror = null;
                        e.target.src = `https://via.placeholder.com/300x200/f8f9fa/6c757d?text=${encodeURIComponent(product.name)}`;
                      }}
                    />
                  ) : (
                    <div className="d-flex align-items-center justify-content-center h-100">
                      <div className="text-center text-muted">
                        <FaImage size={48} />
                        <div className="small mt-2">{product.name}</div>
                      </div>
                    </div>
                  )}
                  {product.trackInventory && product.stockQuantity <= 5 && product.stockQuantity > 0 && (
                    <Badge 
                      bg="warning" 
                      className="position-absolute top-0 end-0 m-2"
                    >
                      Low Stock
                    </Badge>
                  )}
                  {product.trackInventory && product.stockQuantity === 0 && (
                    <Badge 
                      bg="danger" 
                      className="position-absolute top-0 end-0 m-2"
                    >
                      Out of Stock
                    </Badge>
                  )}
                </div>
                <Card.Body className="d-flex flex-column">
                  <h6 className="card-title text-truncate mb-1">{product.name}</h6>
                  <div className="text-muted small mb-2">
                    {product.categoryName}
                    {product.subcategoryName && ` / ${product.subcategoryName}`}
                  </div>
                  <div className="mt-auto">
                    <div className="d-flex justify-content-between align-items-center">
                      <div className="fs-5 fw-bold text-primary">
                        ${product.priceIncGst.toFixed(2)}
                      </div>
                      {product.trackInventory && (
                        <Badge 
                          bg={product.stockQuantity > 10 ? 'success' : product.stockQuantity > 0 ? 'warning' : 'danger'}
                          className="ms-2"
                        >
                          {product.stockQuantity}
                        </Badge>
                      )}
                    </div>
                    {product.sku && (
                      <div className="text-muted small mt-1">
                        SKU: {product.sku}
                      </div>
                    )}
                    {product.barcode && (
                      <div className="text-muted small">
                        <FaBarcode className="me-1" size={10} />
                        {product.barcode}
                      </div>
                    )}
                  </div>
                </Card.Body>
              </Card>
            </Col>
          ))}
        </Row>
      ) : (
        <div className="list-view">
          {products.map(product => (
            <Card key={product.id} className="mb-2 shadow-sm">
              <Card.Body 
                className="d-flex align-items-center py-2"
                role="button"
                onClick={() => handleAddToCart(product)}
                style={{ cursor: isShiftOpen ? 'pointer' : 'not-allowed' }}
              >
                <div className="product-image-thumbnail me-3" style={{
                  width: '60px',
                  height: '60px',
                  flexShrink: 0
                }}>
                  {product.imageUrl ? (
                    <img 
                      src={product.imageUrl}
                      alt={product.name}
                      style={{
                        width: '100%',
                        height: '100%',
                        objectFit: 'cover',
                        borderRadius: '4px'
                      }}
                      onError={(e: any) => {
                        e.target.onerror = null;
                        e.target.src = `https://via.placeholder.com/60x60/f8f9fa/6c757d?text=${product.name.substring(0, 2).toUpperCase()}`;
                      }}
                    />
                  ) : (
                    <div 
                      className="d-flex align-items-center justify-content-center h-100 bg-light rounded"
                      style={{ borderRadius: '4px' }}
                    >
                      <FaImage className="text-muted" size={24} />
                    </div>
                  )}
                </div>
                <div className="flex-grow-1">
                  <h6 className="mb-0">{product.name}</h6>
                  <small className="text-muted">
                    {product.categoryName} / {product.subcategoryName}
                    {product.sku && ` • SKU: ${product.sku}`}
                    {product.barcode && (
                      <>
                        <br />
                        <FaBarcode className="me-1" size={10} />
                        {product.barcode}
                      </>
                    )}
                  </small>
                </div>
                <div className="text-end">
                  <div className="fs-5 fw-bold text-primary">
                    ${product.priceIncGst.toFixed(2)}
                  </div>
                  {product.trackInventory && (
                    <Badge 
                      bg={product.stockQuantity > 10 ? 'success' : product.stockQuantity > 0 ? 'warning' : 'danger'}
                    >
                      Stock: {product.stockQuantity}
                    </Badge>
                  )}
                </div>
              </Card.Body>
            </Card>
          ))}
        </div>
      )}

      {products.length === 0 && !isLoading && (
        <div className="text-center py-5 text-muted">
          <p>No products found</p>
        </div>
      )}
    </div>
  );
};

export default POSPage;
