import React, { useState, useEffect } from 'react';
import { Modal, Button, Form, Row, Col, Badge, Image, InputGroup } from 'react-bootstrap';
import { FaMinus, FaPlus, FaShoppingCart, FaImage, FaTimes, FaBarcode } from 'react-icons/fa';
import { Product } from '@/types';

interface ProductDetailModalProps {
  show: boolean;
  onHide: () => void;
  product: Product | null;
  onAddToCart: (product: Product, quantity: number, notes?: string) => void;
}

const ProductDetailModal: React.FC<ProductDetailModalProps> = ({
  show,
  onHide,
  product,
  onAddToCart
}) => {
  const [quantity, setQuantity] = useState(1);
  const [notes, setNotes] = useState('');

  useEffect(() => {
    if (show) {
      // Reset state when modal opens
      setQuantity(1);
      setNotes('');
    }
  }, [show]);

  if (!product) return null;

  const handleQuantityChange = (value: number) => {
    const newQuantity = Math.max(1, value);
    
    // Check stock if product tracks inventory
    if (product.trackInventory) {
      if (newQuantity > product.stockQuantity) {
        // Show warning but don't allow
        return;
      }
    }
    
    setQuantity(newQuantity);
  };

  const handleIncrement = () => {
    if (product.trackInventory && quantity >= product.stockQuantity) {
      // Show toast when trying to exceed stock
      return;
    }
    handleQuantityChange(quantity + 1);
  };

  const handleDecrement = () => {
    if (quantity <= 1) return;
    handleQuantityChange(quantity - 1);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(e.target.value);
    if (isNaN(value) || value < 1) {
      setQuantity(1);
      return;
    }
    
    if (product.trackInventory && value > product.stockQuantity) {
      // Don't allow more than stock
      setQuantity(product.stockQuantity);
      return;
    }
    
    setQuantity(value);
  };

  const handleAddToCart = () => {
    onAddToCart(product, quantity, notes || undefined);
    onHide();
  };

  const getStockBadge = () => {
    if (!product.trackInventory) {
      return null;
    }

    if (product.stockQuantity === 0) {
      return <Badge bg="danger">Out of Stock</Badge>;
    } else if (product.stockQuantity <= product.lowStockThreshold) {
      return <Badge bg="warning">Low Stock: {product.stockQuantity} left</Badge>;
    } else {
      return <Badge bg="success">In Stock: {product.stockQuantity}</Badge>;
    }
  };

  const getTotalPrice = () => {
    return product.priceIncGst * quantity;
  };

  const isOutOfStock = product.trackInventory && product.stockQuantity === 0;

  return (
    <Modal show={show} onHide={onHide} size="lg" centered>
      <Modal.Header closeButton>
        <Modal.Title>Product Details</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <Row>
          {/* Product Image */}
          <Col md={5}>
            <div 
              className="product-image-large rounded"
              style={{
                width: '100%',
                height: '300px',
                backgroundColor: '#f8f9fa',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                overflow: 'hidden',
                border: '1px solid #dee2e6'
              }}
            >
              {product.imageUrl ? (
                <Image
                  src={product.imageUrl}
                  alt={product.name}
                  style={{
                    width: '100%',
                    height: '100%',
                    objectFit: 'cover'
                  }}
                  onError={(e: any) => {
                    e.target.onerror = null;
                    e.target.src = `https://via.placeholder.com/400x300/f8f9fa/6c757d?text=${encodeURIComponent(product.name)}`;
                  }}
                />
              ) : (
                <div className="text-center text-muted">
                  <FaImage size={64} />
                  <div className="mt-2">No Image</div>
                </div>
              )}
            </div>
          </Col>

          {/* Product Information */}
          <Col md={7}>
            <h4 className="mb-3">{product.name}</h4>
            
            {/* Stock Status */}
            <div className="mb-3">
              {getStockBadge()}
            </div>

            {/* Category & SKU */}
            <div className="mb-3">
              <div className="text-muted small mb-1">
                <strong>Category:</strong> {product.categoryName}
                {product.subcategoryName && ` / ${product.subcategoryName}`}
              </div>
              {product.sku && (
                <div className="text-muted small mb-1">
                  <strong>SKU:</strong> {product.sku}
                </div>
              )}
              {product.barcode && (
                <div className="text-muted small mb-1">
                  <FaBarcode className="me-1" />
                  <strong>Barcode:</strong> {product.barcode}
                </div>
              )}
            </div>

            {/* Description */}
            {product.description && (
              <div className="mb-3">
                <strong>Description:</strong>
                <p className="text-muted mb-0">{product.description}</p>
              </div>
            )}

            {/* Pack Info */}
            {(product.packSize || product.packNotes) && (
              <div className="mb-3">
                {product.packSize && (
                  <div className="text-muted small">
                    <strong>Pack Size:</strong> {product.packSize}
                  </div>
                )}
                {product.packNotes && (
                  <div className="text-muted small">
                    <strong>Pack Notes:</strong> {product.packNotes}
                  </div>
                )}
              </div>
            )}

            {/* Price */}
            <div className="mb-4">
              <div className="d-flex justify-content-between align-items-center p-3 bg-light rounded">
                <div>
                  <div className="text-muted small">Unit Price (Inc GST)</div>
                  <div className="fs-4 fw-bold text-primary">
                    ${product.priceIncGst.toFixed(2)}
                  </div>
                  <div className="text-muted small">
                    Ex GST: ${product.priceExGst.toFixed(2)} + GST: ${product.gstAmount.toFixed(2)}
                  </div>
                </div>
              </div>
            </div>

            {/* Quantity Selector */}
            <Form.Group className="mb-3">
              <Form.Label className="fw-bold">Quantity</Form.Label>
              <InputGroup>
                <Button 
                  variant="outline-secondary" 
                  onClick={handleDecrement}
                  disabled={quantity <= 1 || isOutOfStock}
                >
                  <FaMinus />
                </Button>
                <Form.Control
                  type="number"
                  min="1"
                  max={product.trackInventory ? product.stockQuantity : undefined}
                  value={quantity}
                  onChange={handleInputChange}
                  className="text-center"
                  disabled={isOutOfStock}
                  style={{ maxWidth: '100px' }}
                />
                <Button 
                  variant="outline-secondary" 
                  onClick={handleIncrement}
                  disabled={isOutOfStock || (product.trackInventory && quantity >= product.stockQuantity)}
                >
                  <FaPlus />
                </Button>
              </InputGroup>
              {product.trackInventory && (
                <Form.Text className="text-muted">
                  {product.stockQuantity > 0 
                    ? `Available: ${product.stockQuantity - quantity} remaining (${product.stockQuantity} in stock)`
                    : 'Out of stock'
                  }
                </Form.Text>
              )}
            </Form.Group>

            {/* Notes */}
            <Form.Group className="mb-3">
              <Form.Label className="fw-bold">Special Instructions (Optional)</Form.Label>
              <Form.Control
                as="textarea"
                rows={2}
                placeholder="Add any special instructions for this item..."
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                disabled={isOutOfStock}
              />
            </Form.Group>

            {/* Total Price */}
            <div className="p-3 bg-primary bg-opacity-10 rounded mb-3">
              <div className="d-flex justify-content-between align-items-center">
                <span className="fw-bold">Total:</span>
                <span className="fs-4 fw-bold text-primary">
                  ${getTotalPrice().toFixed(2)}
                </span>
              </div>
              <small className="text-muted">
                {quantity} x ${product.priceIncGst.toFixed(2)}
              </small>
            </div>
          </Col>
        </Row>
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={onHide}>
          <FaTimes className="me-2" />
          Cancel
        </Button>
        <Button 
          variant="success" 
          onClick={handleAddToCart}
          disabled={isOutOfStock}
          size="lg"
        >
          <FaShoppingCart className="me-2" />
          {isOutOfStock ? 'Out of Stock' : `Add to Cart - $${getTotalPrice().toFixed(2)}`}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};

export default ProductDetailModal;
