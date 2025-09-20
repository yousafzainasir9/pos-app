import React from 'react';
import { Card, Row, Col } from 'react-bootstrap';
import { FaChartBar, FaChartLine, FaChartPie, FaFileExport } from 'react-icons/fa';

const ReportsPage: React.FC = () => {
  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Reports & Analytics</h2>
        <button className="btn btn-primary">
          <FaFileExport className="me-2" />
          Export Data
        </button>
      </div>

      <Row className="g-4">
        <Col md={6} lg={4}>
          <Card className="text-center h-100">
            <Card.Body>
              <FaChartBar size={48} className="text-primary mb-3" />
              <h5>Sales Report</h5>
              <p className="text-muted">View daily, weekly, and monthly sales</p>
              <button className="btn btn-outline-primary">View Report</button>
            </Card.Body>
          </Card>
        </Col>

        <Col md={6} lg={4}>
          <Card className="text-center h-100">
            <Card.Body>
              <FaChartLine size={48} className="text-success mb-3" />
              <h5>Product Performance</h5>
              <p className="text-muted">Top selling products and categories</p>
              <button className="btn btn-outline-success">View Report</button>
            </Card.Body>
          </Card>
        </Col>

        <Col md={6} lg={4}>
          <Card className="text-center h-100">
            <Card.Body>
              <FaChartPie size={48} className="text-info mb-3" />
              <h5>Shift Reports</h5>
              <p className="text-muted">Detailed shift summaries and reconciliation</p>
              <button className="btn btn-outline-info">View Report</button>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <Card className="mt-4">
        <Card.Body>
          <div className="text-center py-5 text-muted">
            <p>Reports and analytics features coming soon</p>
            <p className="small">This page will provide detailed insights into your business performance</p>
          </div>
        </Card.Body>
      </Card>
    </div>
  );
};

export default ReportsPage;
