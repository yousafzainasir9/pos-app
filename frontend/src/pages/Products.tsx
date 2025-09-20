import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  TextField,
  IconButton,
  Typography,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Grid,
  MenuItem,
  InputAdornment,
  Alert,
  Snackbar,
  Tooltip,
  FormControlLabel,
  Switch,
  Select,
  FormControl,
  InputLabel,
  Tabs,
  Tab,
  Card,
  CardContent,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Search as SearchIcon,
  Refresh as RefreshIcon,
  FileDownload as ExportIcon,
  Inventory as InventoryIcon,
  Category as CategoryIcon,
  LocalOffer as PriceIcon,
  QrCode as BarcodeIcon,
  Warning as LowStockIcon,
} from '@mui/icons-material';
import axios from 'axios';

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
  sku: string;
  barcode: string;
  description: string;
  priceExGst: number;
  gstAmount: number;
  priceIncGst: number;
  cost: number;
  packNotes: string;
  packSize: number;
  imageUrl: string;
  isActive: boolean;
  trackInventory: boolean;
  stockQuantity: number;
  lowStockThreshold: number;
  displayOrder: number;
  subcategoryId: number;
  subcategory?: Subcategory;
  category?: Category;
  supplierId: number;
  supplier?: Supplier;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div hidden={value !== index} {...other}>
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const Products: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [subcategories, setSubcategories] = useState<Subcategory[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<number | ''>('');
  const [selectedSubcategory, setSelectedSubcategory] = useState<number | ''>('');
  const [tabValue, setTabValue] = useState(0);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const [formData, setFormData] = useState<Partial<Product>>({
    name: '',
    sku: '',
    barcode: '',
    description: '',
    priceExGst: 0,
    cost: 0,
    packSize: 1,
    isActive: true,
    trackInventory: true,
    stockQuantity: 0,
    lowStockThreshold: 10,
    subcategoryId: 0,
    supplierId: 0,
  });

  const API_BASE_URL = 'http://localhost:5124/api';

  useEffect(() => {
    fetchProducts();
    fetchCategories();
    fetchSuppliers();
  }, []);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem('token');
      const response = await axios.get(`${API_BASE_URL}/products`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setProducts(response.data.data || []);
    } catch (error) {
      console.error('Error fetching products:', error);
      showSnackbar('Error loading products', 'error');
    } finally {
      setLoading(false);
    }
  };

  const fetchCategories = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`${API_BASE_URL}/categories`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setCategories(response.data.data || []);
    } catch (error) {
      console.error('Error fetching categories:', error);
    }
  };

  const fetchSubcategories = async (categoryId: number) => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`${API_BASE_URL}/subcategories/category/${categoryId}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setSubcategories(response.data.data || []);
    } catch (error) {
      console.error('Error fetching subcategories:', error);
    }
  };

  const fetchSuppliers = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`${API_BASE_URL}/suppliers`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setSuppliers(response.data.data || []);
    } catch (error) {
      console.error('Error fetching suppliers:', error);
    }
  };

  const handleOpenDialog = (product: Product | null = null) => {
    if (product) {
      setEditingProduct(product);
      setFormData(product);
      // Set category and fetch subcategories if editing
      if (product.subcategory) {
        setSelectedCategory(product.subcategory.categoryId);
        fetchSubcategories(product.subcategory.categoryId);
      }
    } else {
      setEditingProduct(null);
      setFormData({
        name: '',
        sku: '',
        barcode: '',
        description: '',
        priceExGst: 0,
        cost: 0,
        packSize: 1,
        isActive: true,
        trackInventory: true,
        stockQuantity: 0,
        lowStockThreshold: 10,
        subcategoryId: 0,
        supplierId: 0,
      });
      setSelectedCategory('');
      setSelectedSubcategory('');
      setSubcategories([]);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingProduct(null);
    setSelectedCategory('');
    setSelectedSubcategory('');
    setSubcategories([]);
  };

  const calculateGST = (priceExGst: number) => {
    const gstRate = 0.10; // 10% GST
    const gstAmount = Math.round(priceExGst * gstRate * 100) / 100;
    return {
      gstAmount,
      priceIncGst: Math.round((priceExGst + gstAmount) * 100) / 100
    };
  };

  const handleSaveProduct = async () => {
    try {
      const token = localStorage.getItem('token');
      const { gstAmount, priceIncGst } = calculateGST(formData.priceExGst || 0);
      
      const productData = {
        ...formData,
        gstAmount,
        priceIncGst,
        slug: formData.name?.toLowerCase().replace(/\s+/g, '-') || '',
      };

      if (editingProduct) {
        await axios.put(`${API_BASE_URL}/products/${editingProduct.id}`, productData, {
          headers: { Authorization: `Bearer ${token}` }
        });
        showSnackbar('Product updated successfully', 'success');
      } else {
        await axios.post(`${API_BASE_URL}/products`, productData, {
          headers: { Authorization: `Bearer ${token}` }
        });
        showSnackbar('Product added successfully', 'success');
      }
      
      handleCloseDialog();
      fetchProducts();
    } catch (error) {
      console.error('Error saving product:', error);
      showSnackbar('Error saving product', 'error');
    }
  };

  const handleDeleteProduct = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this product?')) {
      try {
        const token = localStorage.getItem('token');
        await axios.delete(`${API_BASE_URL}/products/${id}`, {
          headers: { Authorization: `Bearer ${token}` }
        });
        showSnackbar('Product deleted successfully', 'success');
        fetchProducts();
      } catch (error) {
        console.error('Error deleting product:', error);
        showSnackbar('Error deleting product', 'error');
      }
    }
  };

  const handleCategoryChange = (categoryId: number) => {
    setSelectedCategory(categoryId);
    setFormData({ ...formData, subcategoryId: 0 });
    setSelectedSubcategory('');
    if (categoryId) {
      fetchSubcategories(categoryId);
    } else {
      setSubcategories([]);
    }
  };

  const showSnackbar = (message: string, severity: 'success' | 'error') => {
    setSnackbar({ open: true, message, severity });
  };

  const filteredProducts = products.filter(product => 
    product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.sku.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.barcode?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const lowStockProducts = products.filter(p => p.trackInventory && p.stockQuantity <= p.lowStockThreshold);

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">
          Products Management
        </Typography>
        <Box>
          <Tooltip title="Export Products">
            <IconButton color="primary">
              <ExportIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Refresh">
            <IconButton onClick={fetchProducts} color="primary">
              <RefreshIcon />
            </IconButton>
          </Tooltip>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
            sx={{ ml: 1 }}
          >
            Add Product
          </Button>
        </Box>
      </Box>

      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Total Products
                  </Typography>
                  <Typography variant="h4">
                    {products.length}
                  </Typography>
                </Box>
                <InventoryIcon sx={{ fontSize: 40, color: 'primary.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Active Products
                  </Typography>
                  <Typography variant="h4">
                    {products.filter(p => p.isActive).length}
                  </Typography>
                </Box>
                <CategoryIcon sx={{ fontSize: 40, color: 'success.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Low Stock Items
                  </Typography>
                  <Typography variant="h4" color="warning.main">
                    {lowStockProducts.length}
                  </Typography>
                </Box>
                <LowStockIcon sx={{ fontSize: 40, color: 'warning.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Categories
                  </Typography>
                  <Typography variant="h4">
                    {categories.length}
                  </Typography>
                </Box>
                <CategoryIcon sx={{ fontSize: 40, color: 'secondary.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Paper sx={{ width: '100%', mb: 2 }}>
        <Tabs value={tabValue} onChange={(e, v) => setTabValue(v)} sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tab label={`All Products (${products.length})`} />
          <Tab label={`Low Stock (${lowStockProducts.length})`} />
          <Tab label="Categories" />
        </Tabs>

        <TabPanel value={tabValue} index={0}>
          <TextField
            placeholder="Search products..."
            variant="outlined"
            size="small"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            sx={{ mb: 2, width: 300 }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            }}
          />

          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>SKU</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell align="right">Price (ex GST)</TableCell>
                  <TableCell align="right">Price (inc GST)</TableCell>
                  <TableCell align="right">Stock</TableCell>
                  <TableCell align="center">Status</TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {loading ? (
                  <TableRow>
                    <TableCell colSpan={8} align="center">Loading...</TableCell>
                  </TableRow>
                ) : filteredProducts.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} align="center">No products found</TableCell>
                  </TableRow>
                ) : (
                  filteredProducts
                    .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
                    .map((product) => (
                      <TableRow key={product.id}>
                        <TableCell>
                          <Box>
                            <Typography variant="body2">{product.sku}</Typography>
                            {product.barcode && (
                              <Typography variant="caption" color="textSecondary">
                                <BarcodeIcon sx={{ fontSize: 12, mr: 0.5 }} />
                                {product.barcode}
                              </Typography>
                            )}
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Box>
                            <Typography variant="body2" fontWeight="medium">
                              {product.name}
                            </Typography>
                            {product.packSize && product.packSize > 1 && (
                              <Typography variant="caption" color="textSecondary">
                                Pack of {product.packSize}
                              </Typography>
                            )}
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption">
                            {product.category?.name} / {product.subcategory?.name}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">${product.priceExGst.toFixed(2)}</TableCell>
                        <TableCell align="right">
                          <Typography fontWeight="medium">
                            ${product.priceIncGst.toFixed(2)}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">
                          {product.trackInventory ? (
                            <Box>
                              <Typography 
                                variant="body2"
                                color={product.stockQuantity <= product.lowStockThreshold ? 'error' : 'inherit'}
                              >
                                {product.stockQuantity}
                              </Typography>
                              {product.stockQuantity <= product.lowStockThreshold && (
                                <Chip 
                                  label="Low Stock" 
                                  size="small" 
                                  color="warning" 
                                  sx={{ mt: 0.5 }}
                                />
                              )}
                            </Box>
                          ) : (
                            <Typography variant="body2" color="textSecondary">-</Typography>
                          )}
                        </TableCell>
                        <TableCell align="center">
                          <Chip 
                            label={product.isActive ? 'Active' : 'Inactive'} 
                            color={product.isActive ? 'success' : 'default'}
                            size="small"
                          />
                        </TableCell>
                        <TableCell align="center">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleOpenDialog(product)}
                          >
                            <EditIcon />
                          </IconButton>
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => handleDeleteProduct(product.id)}
                          >
                            <DeleteIcon />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
          <TablePagination
            component="div"
            count={filteredProducts.length}
            page={page}
            onPageChange={(e, newPage) => setPage(newPage)}
            rowsPerPage={rowsPerPage}
            onRowsPerPageChange={(e) => {
              setRowsPerPage(parseInt(e.target.value, 10));
              setPage(0);
            }}
          />
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <Alert severity="warning" sx={{ mb: 2 }}>
            The following products are running low on stock and need to be reordered.
          </Alert>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>SKU</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Supplier</TableCell>
                  <TableCell align="right">Current Stock</TableCell>
                  <TableCell align="right">Low Stock Threshold</TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {lowStockProducts.map((product) => (
                  <TableRow key={product.id}>
                    <TableCell>{product.sku}</TableCell>
                    <TableCell>{product.name}</TableCell>
                    <TableCell>{product.supplier?.name || '-'}</TableCell>
                    <TableCell align="right" sx={{ color: 'error.main', fontWeight: 'bold' }}>
                      {product.stockQuantity}
                    </TableCell>
                    <TableCell align="right">{product.lowStockThreshold}</TableCell>
                    <TableCell align="center">
                      <Button size="small" variant="outlined">
                        Reorder
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <Grid container spacing={2}>
            {categories.map((category) => (
              <Grid item xs={12} sm={6} md={4} key={category.id}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      {category.name}
                    </Typography>
                    <Typography variant="body2" color="textSecondary">
                      {products.filter(p => p.category?.id === category.id).length} products
                    </Typography>
                    <Chip 
                      label={category.isActive ? 'Active' : 'Inactive'} 
                      size="small"
                      color={category.isActive ? 'success' : 'default'}
                      sx={{ mt: 1 }}
                    />
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </TabPanel>
      </Paper>

      {/* Add/Edit Product Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingProduct ? 'Edit Product' : 'Add New Product'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Product Name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="SKU"
                value={formData.sku}
                onChange={(e) => setFormData({ ...formData, sku: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Barcode"
                value={formData.barcode}
                onChange={(e) => setFormData({ ...formData, barcode: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Pack Size"
                type="number"
                value={formData.packSize}
                onChange={(e) => setFormData({ ...formData, packSize: parseInt(e.target.value) })}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                multiline
                rows={3}
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Category</InputLabel>
                <Select
                  value={selectedCategory}
                  onChange={(e) => handleCategoryChange(e.target.value as number)}
                  label="Category"
                >
                  <MenuItem value="">Select Category</MenuItem>
                  {categories.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>
                      {cat.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth disabled={!selectedCategory}>
                <InputLabel>Subcategory</InputLabel>
                <Select
                  value={formData.subcategoryId}
                  onChange={(e) => setFormData({ ...formData, subcategoryId: e.target.value as number })}
                  label="Subcategory"
                >
                  <MenuItem value={0}>Select Subcategory</MenuItem>
                  {subcategories.map((sub) => (
                    <MenuItem key={sub.id} value={sub.id}>
                      {sub.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Cost"
                type="number"
                value={formData.cost}
                onChange={(e) => setFormData({ ...formData, cost: parseFloat(e.target.value) })}
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                }}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Price (ex GST)"
                type="number"
                value={formData.priceExGst}
                onChange={(e) => setFormData({ ...formData, priceExGst: parseFloat(e.target.value) })}
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                }}
                required
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Price (inc GST)"
                type="number"
                value={calculateGST(formData.priceExGst || 0).priceIncGst}
                disabled
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Supplier</InputLabel>
                <Select
                  value={formData.supplierId}
                  onChange={(e) => setFormData({ ...formData, supplierId: e.target.value as number })}
                  label="Supplier"
                >
                  <MenuItem value={0}>No Supplier</MenuItem>
                  {suppliers.map((sup) => (
                    <MenuItem key={sup.id} value={sup.id}>
                      {sup.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formData.trackInventory}
                    onChange={(e) => setFormData({ ...formData, trackInventory: e.target.checked })}
                  />
                }
                label="Track Inventory"
              />
            </Grid>
            {formData.trackInventory && (
              <>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Stock Quantity"
                    type="number"
                    value={formData.stockQuantity}
                    onChange={(e) => setFormData({ ...formData, stockQuantity: parseInt(e.target.value) })}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Low Stock Threshold"
                    type="number"
                    value={formData.lowStockThreshold}
                    onChange={(e) => setFormData({ ...formData, lowStockThreshold: parseInt(e.target.value) })}
                  />
                </Grid>
              </>
            )}
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formData.isActive}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  />
                }
                label="Active"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveProduct} variant="contained">
            {editingProduct ? 'Update' : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar({ ...snackbar, open: false })}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default Products;
