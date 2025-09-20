import React from 'react';
import { Card, Table, Badge } from 'react-bootstrap';
import { FaSearch, FaPlus, FaEdit, FaTrash } from 'react-icons/fa';

const ProductsPage: React.FC = () => {
  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Products Management</h2>
        <button className="btn btn-primary">
          <FaPlus className="me-2" />
          Add Product
        </button>
      </div>

      <Card>
        <Card.Body>
          <div className="text-center py-5 text-muted">
            <FaSearch size={48} className="mb-3" />
            <p>Product management coming soon</p>
            <p className="small">This page will allow you to manage your product catalog</p>
          </div>
        </Card.Body>
      </Card>
    </div>
  );
};

export default ProductsPage;
