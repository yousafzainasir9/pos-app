import React, { useState, useEffect } from 'react';
import { 
  Container, 
  Row, 
  Col, 
  Card, 
  Table, 
  Button, 
  Form, 
  Modal, 
  Badge, 
  Alert,
  Tabs,
  Tab,
  InputGroup,
  Pagination,
  Spinner
} from 'react-bootstrap';
import { 
  FaPlus, 
  FaEdit, 
  FaTrash, 
  FaSearch, 
  FaSync,
  FaDownload,
  FaBox,
  FaTag,
  FaExclamationTriangle,
  FaBarcode,
  FaWarehouse
} from 'react-icons/fa';
import { toast } from 'react-toastify';
import productService from '@/services/product.service';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';

interface Category {
  id: number;
  name: string;
  slug: string;
  isActive: boolean;
}

interface Subcategory {
  id: number;
  name: string;
  slug: string;
  categoryId: number;
  isActive: boolean;
}

interface Supplier {
  id: number;
  name: string;
  isActive: boolean;
}

interface Product {
  id: number;
  name: string;
  slug: string;
  sku?: string;
  barcode?: string;
  description?: string;
  priceExGst: number;
  gstAmount: number;
  priceIncGst: number;
  cost?: number;
  packNotes?: string;
  packSize?: number;
  imageUrl?: string;
  isActive: boolean;
  trackInventory: boolean;
  stockQuantity: number;
  lowStockThreshold: number;
  displayOrder: number;
  subcategoryId: number;
  subcategory?: Subcategory;
  category?: Category;
  supplierId?: number;
  supplier?: Supplier;
}

interface FormData {
  name: string;
  sku?: string;
  barcode?: string;
  description?: string;
  priceExGst: number;
  cost?: number;
  packSize?: number;
  imageUrl?: string;
  isActive: boolean;
  trackInventory: boolean;
  stockQuantity: number;
  lowStockThreshold: number;
  displayOrder: number;
  subcategoryId: number;
  supplierId?: number;
}

const ProductsPage: React.FC = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [subcategories, setSubcategories] = useState<Subcategory[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);
  const [showModal, setShowModal] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<number | ''>('');
  const [activeTab, setActiveTab] = useState('all');
  const [validated, setValidated] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  const [formData, setFormData] = useState<FormData>({
    name: '',
    sku: undefined,
    barcode: undefined,
    description: undefined,
    priceExGst: 0,
    cost: undefined,
    packSize: 1,
    imageUrl: undefined,
    isActive: true,
    trackInventory: true,
    stockQuantity: 0,
    lowStockThreshold: 10,
    displayOrder: 0,
    subcategoryId: 0,
    supplierId: undefined,
  });

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    fetchProducts();
    fetchCategories();
    fetchSuppliers();
  }, [isAuthenticated, navigate]);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const data = await productService.getProducts();
      setProducts(data || []);
    } catch (error: any) {
      console.error('Error fetching products:', error);
      if (error.response?.status === 401) {
        navigate('/login');
      } else {
        toast.error('Error loading products');
      }
    } finally {
      setLoading(false);
    }
  };

  const fetchCategories = async () => {
    try {
      const data = await productService.getCategories();
      setCategories(data || []);
    } catch (error: any) {
      console.error('Error fetching categories:', error);
      if (error.response?.status === 401) {
        navigate('/login');
      }
    }
  };

  const fetchSubcategories = async (categoryId: number) => {
    try {
      const data = await productService.getSubcategoriesByCategory(categoryId);
      setSubcategories(data || []);
    } catch (error: any) {
      console.error('Error fetching subcategories:', error);
      if (error.response?.status === 401) {
        navigate('/login');
      }
    }
  };

  const fetchSuppliers = async () => {
    try {
      const data = await productService.getSuppliers();
      setSuppliers(data || []);
    } catch (error: any) {
      console.error('Error fetching suppliers:', error);
      if (error.response?.status === 401) {
        navigate('/login');
      }
    }
  };

  const handleOpenModal = (product: Product | null = null) => {
    setFormErrors({});
    setValidated(false);
    
    if (product) {
      setEditingProduct(product);
      setFormData({
        name: product.name,
        sku: product.sku,
        barcode: product.barcode,
        description: product.description,
        priceExGst: product.priceExGst,
        cost: product.cost,
        packSize: product.packSize || 1,
        imageUrl: product.imageUrl,
        isActive: product.isActive,
        trackInventory: product.trackInventory,
        stockQuantity: product.stockQuantity,
        lowStockThreshold: product.lowStockThreshold,
        displayOrder: product.displayOrder || 0,
        subcategoryId: product.subcategoryId,
        supplierId: product.supplierId
      });
      if (product.subcategory) {
        setSelectedCategory(product.subcategory.categoryId);
        fetchSubcategories(product.subcategory.categoryId);
      }
    } else {
      setEditingProduct(null);
      setFormData({
        name: '',
        sku: undefined,
        barcode: undefined,
        description: undefined,
        priceExGst: 0,
        cost: undefined,
        packSize: 1,
        imageUrl: undefined,
        isActive: true,
        trackInventory: true,
        stockQuantity: 0,
        lowStockThreshold: 10,
        displayOrder: 0,
        subcategoryId: 0,
        supplierId: undefined,
      });
      setSelectedCategory('');
      setSubcategories([]);
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingProduct(null);
    setSelectedCategory('');
    setSubcategories([]);
    setFormErrors({});
    setValidated(false);
  };

  const calculateGST = (priceExGst: number) => {
    const gstRate = 0.10; // 10% GST
    const gstAmount = Math.round(priceExGst * gstRate * 100) / 100;
    return {
      gstAmount,
      priceIncGst: Math.round((priceExGst + gstAmount) * 100) / 100
    };
  };

  const validateForm = () => {
    const errors: Record<string, string> = {};

    if (!formData.name || formData.name.trim() === '') {
      errors.name = 'Product name is required';
    }

    if (formData.priceExGst < 0) {
      errors.priceExGst = 'Price must be greater than or equal to 0';
    }

    if (!formData.subcategoryId || formData.subcategoryId === 0) {
      errors.subcategoryId = 'Subcategory is required';
    }

    if (formData.trackInventory) {
      if (formData.stockQuantity < 0) {
        errors.stockQuantity = 'Stock quantity cannot be negative';
      }
      if (formData.lowStockThreshold < 0) {
        errors.lowStockThreshold = 'Low stock threshold cannot be negative';
      }
    }

    if (formData.packSize && formData.packSize < 1) {
      errors.packSize = 'Pack size must be at least 1';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSaveProduct = async (event?: React.FormEvent) => {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }

    setValidated(true);

    if (!validateForm()) {
      toast.error('Please correct the form errors');
      return;
    }

    setSaving(true);

    try {
      const { gstAmount, priceIncGst } = calculateGST(formData.priceExGst || 0);
      
      const productData = {
        name: formData.name.trim(),
        slug: formData.name?.toLowerCase().replace(/\s+/g, '-') || '',
        sku: formData.sku?.trim() || null,
        barcode: formData.barcode?.trim() || null,
        description: formData.description?.trim() || null,
        priceExGst: formData.priceExGst,
        gstAmount,
        priceIncGst,
        cost: formData.cost || null,
        packSize: formData.packSize || 1,
        packNotes: null,
        imageUrl: formData.imageUrl?.trim() || null,
        isActive: formData.isActive,
        trackInventory: formData.trackInventory,
        stockQuantity: formData.stockQuantity,
        lowStockThreshold: formData.lowStockThreshold,
        displayOrder: formData.displayOrder || 0,
        subcategoryId: formData.subcategoryId,
        supplierId: formData.supplierId || null,
      };

      if (editingProduct) {
        await productService.updateProduct(editingProduct.id, productData);
        toast.success('Product updated successfully');
      } else {
        await productService.createProduct(productData);
        toast.success('Product added successfully');
      }
      
      handleCloseModal();
      fetchProducts();
    } catch (error: any) {
      console.error('Error saving product:', error);
      if (error.response?.status === 401) {
        navigate('/login');
      } else if (error.response?.data?.message) {
        toast.error(error.response.data.message);
      } else if (error.response?.data?.errors && error.response.data.errors.length > 0) {
        error.response.data.errors.forEach((err: string) => toast.error(err));
      } else {
        toast.error('Error saving product. Please check all fields and try again.');
      }
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteProduct = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this product?')) {
      try {
        await productService.deleteProduct(id);
        toast.success('Product deleted successfully');
        fetchProducts();
      } catch (error: any) {
        console.error('Error deleting product:', error);
        if (error.response?.status === 401) {
          navigate('/login');
        } else {
          toast.error('Error deleting product');
        }
      }
    }
  };

  const handleCategoryChange = (categoryId: string) => {
    const id = categoryId ? parseInt(categoryId) : '';
    setSelectedCategory(id);
    setFormData({ ...formData, subcategoryId: 0 });
    if (id) {
      fetchSubcategories(id);
    } else {
      setSubcategories([]);
    }
  };

  const filteredProducts = products.filter(product => 
    product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.sku?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.barcode?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const lowStockProducts = products.filter(p => p.trackInventory && p.stockQuantity <= p.lowStockThreshold);

  // Pagination
  const indexOfLastItem = currentPage * itemsPerPage;
  const indexOfFirstItem = indexOfLastItem - itemsPerPage;
  const currentProducts = filteredProducts.slice(indexOfFirstItem, indexOfLastItem);
  const totalPages = Math.ceil(filteredProducts.length / itemsPerPage);

  return (
    <Container fluid>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Products Management</h2>
        <div>
          <Button variant="outline-primary" className="me-2" title="Export Products">
            <FaDownload />
          </Button>
          <Button variant="outline-primary" className="me-2" onClick={fetchProducts} title="Refresh">
            <FaSync />
          </Button>
          <Button variant="primary" onClick={() => handleOpenModal()}>
            <FaPlus className="me-2" />
            Add Product
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      <Row className="mb-4">
        <Col md={3} className="mb-3">
          <Card className="text-center">
            <Card.Body>
              <FaBox size={30} className="text-primary mb-2" />
              <h3>{products.length}</h3>
              <p className="text-muted mb-0">Total Products</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3} className="mb-3">
          <Card className="text-center">
            <Card.Body>
              <FaTag size={30} className="text-success mb-2" />
              <h3>{products.filter(p => p.isActive).length}</h3>
              <p className="text-muted mb-0">Active Products</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3} className="mb-3">
          <Card className="text-center">
            <Card.Body>
              <FaExclamationTriangle size={30} className="text-warning mb-2" />
              <h3 className="text-warning">{lowStockProducts.length}</h3>
              <p className="text-muted mb-0">Low Stock Items</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3} className="mb-3">
          <Card className="text-center">
            <Card.Body>
              <FaWarehouse size={30} className="text-info mb-2" />
              <h3>{categories.length}</h3>
              <p className="text-muted mb-0">Categories</p>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <Card>
        <Card.Body>
          <Tabs activeKey={activeTab} onSelect={(k) => setActiveTab(k || 'all')} className="mb-3">
            <Tab eventKey="all" title={`All Products (${products.length})`}>
              <InputGroup className="mb-3" style={{ maxWidth: '300px' }}>
                <InputGroup.Text>
                  <FaSearch />
                </InputGroup.Text>
                <Form.Control
                  placeholder="Search products..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </InputGroup>

              {loading ? (
                <div className="text-center py-4">
                  <Spinner animation="border" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </Spinner>
                </div>
              ) : (
                <>
                  <Table responsive hover>
                    <thead>
                      <tr>
                        <th>SKU</th>
                        <th>Name</th>
                        <th>Category</th>
                        <th className="text-end">Price (ex GST)</th>
                        <th className="text-end">Price (inc GST)</th>
                        <th className="text-end">Stock</th>
                        <th className="text-center">Status</th>
                        <th className="text-center">Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {currentProducts.length === 0 ? (
                        <tr>
                          <td colSpan={8} className="text-center">No products found</td>
                        </tr>
                      ) : (
                        currentProducts.map(product => (
                          <tr key={product.id}>
                            <td>
                              <div>
                                <div>{product.sku || '-'}</div>
                                {product.barcode && (
                                  <small className="text-muted">
                                    <FaBarcode className="me-1" size={10} />
                                    {product.barcode}
                                  </small>
                                )}
                              </div>
                            </td>
                            <td>
                              <div>
                                <strong>{product.name}</strong>
                                {product.packSize && product.packSize > 1 && (
                                  <div><small className="text-muted">Pack of {product.packSize}</small></div>
                                )}
                              </div>
                            </td>
                            <td>
                              <small>{product.category?.name} / {product.subcategory?.name}</small>
                            </td>
                            <td className="text-end">${product.priceExGst.toFixed(2)}</td>
                            <td className="text-end"><strong>${product.priceIncGst.toFixed(2)}</strong></td>
                            <td className="text-end">
                              {product.trackInventory ? (
                                <div>
                                  <span className={product.stockQuantity <= product.lowStockThreshold ? 'text-danger fw-bold' : ''}>
                                    {product.stockQuantity}
                                  </span>
                                  {product.stockQuantity <= product.lowStockThreshold && (
                                    <Badge bg="warning" className="ms-2">Low Stock</Badge>
                                  )}
                                </div>
                              ) : (
                                <span className="text-muted">-</span>
                              )}
                            </td>
                            <td className="text-center">
                              <Badge bg={product.isActive ? 'success' : 'secondary'}>
                                {product.isActive ? 'Active' : 'Inactive'}
                              </Badge>
                            </td>
                            <td className="text-center">
                              <Button
                                size="sm"
                                variant="outline-primary"
                                className="me-1"
                                onClick={() => handleOpenModal(product)}
                              >
                                <FaEdit />
                              </Button>
                              <Button
                                size="sm"
                                variant="outline-danger"
                                onClick={() => handleDeleteProduct(product.id)}
                              >
                                <FaTrash />
                              </Button>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </Table>

                  {totalPages > 1 && (
                    <Pagination>
                      <Pagination.First onClick={() => setCurrentPage(1)} disabled={currentPage === 1} />
                      <Pagination.Prev onClick={() => setCurrentPage(currentPage - 1)} disabled={currentPage === 1} />
                      {[...Array(totalPages)].map((_, index) => (
                        <Pagination.Item
                          key={index + 1}
                          active={index + 1 === currentPage}
                          onClick={() => setCurrentPage(index + 1)}
                        >
                          {index + 1}
                        </Pagination.Item>
                      ))}
                      <Pagination.Next onClick={() => setCurrentPage(currentPage + 1)} disabled={currentPage === totalPages} />
                      <Pagination.Last onClick={() => setCurrentPage(totalPages)} disabled={currentPage === totalPages} />
                    </Pagination>
                  )}
                </>
              )}
            </Tab>

            <Tab eventKey="lowStock" title={`Low Stock (${lowStockProducts.length})`}>
              <Alert variant="warning">
                The following products are running low on stock and need to be reordered.
              </Alert>
              <Table responsive hover>
                <thead>
                  <tr>
                    <th>SKU</th>
                    <th>Name</th>
                    <th>Supplier</th>
                    <th className="text-end">Current Stock</th>
                    <th className="text-end">Low Stock Threshold</th>
                    <th className="text-center">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {lowStockProducts.map(product => (
                    <tr key={product.id}>
                      <td>{product.sku || '-'}</td>
                      <td>{product.name}</td>
                      <td>{product.supplier?.name || '-'}</td>
                      <td className="text-end text-danger fw-bold">{product.stockQuantity}</td>
                      <td className="text-end">{product.lowStockThreshold}</td>
                      <td className="text-center">
                        <Button size="sm" variant="outline-primary">Reorder</Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </Tab>

            <Tab eventKey="categories" title="Categories">
              <Row>
                {categories.map(category => (
                  <Col md={4} key={category.id} className="mb-3">
                    <Card>
                      <Card.Body>
                        <h5>{category.name}</h5>
                        <p className="text-muted">
                          {products.filter(p => p.category?.id === category.id).length} products
                        </p>
                        <Badge bg={category.isActive ? 'success' : 'secondary'}>
                          {category.isActive ? 'Active' : 'Inactive'}
                        </Badge>
                      </Card.Body>
                    </Card>
                  </Col>
                ))}
              </Row>
            </Tab>
          </Tabs>
        </Card.Body>
      </Card>

      {/* Add/Edit Product Modal */}
      <Modal show={showModal} onHide={handleCloseModal} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>{editingProduct ? 'Edit Product' : 'Add New Product'}</Modal.Title>
        </Modal.Header>
        <Form noValidate validated={validated} onSubmit={handleSaveProduct}>
          <Modal.Body>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Product Name <span className="text-danger">*</span></Form.Label>
                  <Form.Control
                    type="text"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    required
                    isInvalid={!!formErrors.name}
                  />
                  <Form.Control.Feedback type="invalid">
                    {formErrors.name}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>SKU</Form.Label>
                  <Form.Control
                    type="text"
                    value={formData.sku || ''}
                    onChange={(e) => setFormData({ ...formData, sku: e.target.value || undefined })}
                  />
                </Form.Group>
              </Col>
            </Row>

            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Barcode</Form.Label>
                  <Form.Control
                    type="text"
                    value={formData.barcode || ''}
                    onChange={(e) => setFormData({ ...formData, barcode: e.target.value || undefined })}
                  />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Pack Size</Form.Label>
                  <Form.Control
                    type="number"
                    value={formData.packSize || 1}
                    onChange={(e) => setFormData({ ...formData, packSize: parseInt(e.target.value) || 1 })}
                    min={1}
                    isInvalid={!!formErrors.packSize}
                  />
                  <Form.Control.Feedback type="invalid">
                    {formErrors.packSize}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
            </Row>

            <Form.Group className="mb-3">
              <Form.Label>Description</Form.Label>
              <Form.Control
                as="textarea"
                rows={3}
                value={formData.description || ''}
                onChange={(e) => setFormData({ ...formData, description: e.target.value || undefined })}
              />
            </Form.Group>

            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Category <span className="text-danger">*</span></Form.Label>
                  <Form.Select
                    value={selectedCategory}
                    onChange={(e) => handleCategoryChange(e.target.value)}
                    isInvalid={!!formErrors.subcategoryId && !selectedCategory}
                  >
                    <option value="">Select Category</option>
                    {categories.map(cat => (
                      <option key={cat.id} value={cat.id}>{cat.name}</option>
                    ))}
                  </Form.Select>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Subcategory <span className="text-danger">*</span></Form.Label>
                  <Form.Select
                    value={formData.subcategoryId}
                    onChange={(e) => setFormData({ ...formData, subcategoryId: parseInt(e.target.value) || 0 })}
                    disabled={!selectedCategory}
                    required
                    isInvalid={!!formErrors.subcategoryId}
                  >
                    <option value={0}>Select Subcategory</option>
                    {subcategories.map(sub => (
                      <option key={sub.id} value={sub.id}>{sub.name}</option>
                    ))}
                  </Form.Select>
                  <Form.Control.Feedback type="invalid">
                    {formErrors.subcategoryId}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
            </Row>

            <Row>
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Cost ($)</Form.Label>
                  <Form.Control
                    type="number"
                    step="0.01"
                    value={formData.cost || ''}
                    onChange={(e) => setFormData({ ...formData, cost: parseFloat(e.target.value) || undefined })}
                    min={0}
                  />
                </Form.Group>
              </Col>
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Price (ex GST) <span className="text-danger">*</span> ($)</Form.Label>
                  <Form.Control
                    type="number"
                    step="0.01"
                    value={formData.priceExGst}
                    onChange={(e) => setFormData({ ...formData, priceExGst: parseFloat(e.target.value) || 0 })}
                    required
                    min={0}
                    isInvalid={!!formErrors.priceExGst}
                  />
                  <Form.Control.Feedback type="invalid">
                    {formErrors.priceExGst}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Price (inc GST) ($)</Form.Label>
                  <Form.Control
                    type="number"
                    step="0.01"
                    value={calculateGST(formData.priceExGst || 0).priceIncGst}
                    disabled
                  />
                </Form.Group>
              </Col>
            </Row>

            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Supplier</Form.Label>
                  <Form.Select
                    value={formData.supplierId || 0}
                    onChange={(e) => setFormData({ ...formData, supplierId: parseInt(e.target.value) || undefined })}
                  >
                    <option value={0}>No Supplier</option>
                    {suppliers.map(sup => (
                      <option key={sup.id} value={sup.id}>{sup.name}</option>
                    ))}
                  </Form.Select>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Image URL</Form.Label>
                  <Form.Control
                    type="text"
                    value={formData.imageUrl || ''}
                    onChange={(e) => setFormData({ ...formData, imageUrl: e.target.value || undefined })}
                    placeholder="https://example.com/image.jpg"
                  />
                </Form.Group>
              </Col>
            </Row>

            <Row>
              <Col md={12}>
                <Form.Group className="mb-3">
                  <Form.Check
                    type="switch"
                    label="Track Inventory"
                    checked={formData.trackInventory}
                    onChange={(e) => setFormData({ ...formData, trackInventory: e.target.checked })}
                  />
                </Form.Group>
              </Col>
            </Row>

            {formData.trackInventory && (
              <Row>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Stock Quantity</Form.Label>
                    <Form.Control
                      type="number"
                      value={formData.stockQuantity}
                      onChange={(e) => setFormData({ ...formData, stockQuantity: parseInt(e.target.value) || 0 })}
                      min={0}
                      isInvalid={!!formErrors.stockQuantity}
                    />
                    <Form.Control.Feedback type="invalid">
                      {formErrors.stockQuantity}
                    </Form.Control.Feedback>
                  </Form.Group>
                </Col>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Low Stock Threshold</Form.Label>
                    <Form.Control
                      type="number"
                      value={formData.lowStockThreshold}
                      onChange={(e) => setFormData({ ...formData, lowStockThreshold: parseInt(e.target.value) || 0 })}
                      min={0}
                      isInvalid={!!formErrors.lowStockThreshold}
                    />
                    <Form.Control.Feedback type="invalid">
                      {formErrors.lowStockThreshold}
                    </Form.Control.Feedback>
                  </Form.Group>
                </Col>
              </Row>
            )}

            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Display Order</Form.Label>
                  <Form.Control
                    type="number"
                    value={formData.displayOrder}
                    onChange={(e) => setFormData({ ...formData, displayOrder: parseInt(e.target.value) || 0 })}
                    min={0}
                  />
                  <Form.Text className="text-muted">
                    Lower numbers appear first
                  </Form.Text>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3 d-flex align-items-center h-100">
                  <Form.Check
                    type="switch"
                    label="Active"
                    checked={formData.isActive}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  />
                </Form.Group>
              </Col>
            </Row>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={handleCloseModal} disabled={saving}>
              Cancel
            </Button>
            <Button 
              variant="primary" 
              type="submit"
              disabled={saving}
              style={{ minWidth: '80px' }}
            >
              {saving ? 'Saving...' : (editingProduct ? 'Update' : 'Save')}
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </Container>
  );
};

export default ProductsPage;
