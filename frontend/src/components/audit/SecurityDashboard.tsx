import React from 'react';
import { Row, Col, Card, Table, Button } from 'react-bootstrap';
import { FaSync, FaUserShield, FaChartBar } from 'react-icons/fa';
import { BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { format } from 'date-fns';
import { AuditStatistics } from '../../services/audit.service';

interface SecurityDashboardProps {
  statistics: AuditStatistics | null;
  onRefresh: () => void;
}

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8', '#82CA9D'];

const SecurityDashboard: React.FC<SecurityDashboardProps> = ({ statistics, onRefresh }) => {
  if (!statistics) {
    return (
      <div className="text-center py-5">
        <p className="text-muted">Loading statistics...</p>
      </div>
    );
  }

  const activityData = statistics.activityByDay.map(item => ({
    date: format(new Date(item.date), 'MMM dd'),
    count: item.count
  }));

  const eventTypeData = statistics.securityEventCounts.map(item => ({
    name: item.eventTypeName,
    count: item.count
  }));

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h5 className="mb-0">
          <FaChartBar className="me-2" />
          Security Dashboard
        </h5>
        <Button variant="outline-primary" size="sm" onClick={onRefresh}>
          <FaSync className="me-1" />
          Refresh
        </Button>
      </div>

      {/* Activity Chart */}
      <Row className="mb-4">
        <Col md={12}>
          <Card className="border-0 shadow-sm">
            <Card.Body>
              <h6 className="mb-3">Activity Over Time (Last 30 Days)</h6>
              <ResponsiveContainer width="100%" height={250}>
                <LineChart data={activityData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line type="monotone" dataKey="count" stroke="#8884d8" strokeWidth={2} name="Events" />
                </LineChart>
              </ResponsiveContainer>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Event Type Distribution */}
      <Row className="mb-4">
        <Col md={6}>
          <Card className="border-0 shadow-sm">
            <Card.Body>
              <h6 className="mb-3">Security Events by Type</h6>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={eventTypeData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" angle={-45} textAnchor="end" height={100} />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="count" fill="#8884d8" name="Count" />
                </BarChart>
              </ResponsiveContainer>
            </Card.Body>
          </Card>
        </Col>

        <Col md={6}>
          <Card className="border-0 shadow-sm">
            <Card.Body>
              <h6 className="mb-3">Event Distribution</h6>
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={eventTypeData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={(entry) => `${entry.name}: ${entry.count}`}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="count"
                  >
                    {eventTypeData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Top Users Table */}
      <Row>
        <Col md={12}>
          <Card className="border-0 shadow-sm">
            <Card.Body>
              <h6 className="mb-3">
                <FaUserShield className="me-2" />
                Top Active Users (Last 30 Days)
              </h6>
              {statistics.topUsers.length > 0 ? (
                <Table striped hover size="sm">
                  <thead>
                    <tr>
                      <th>Rank</th>
                      <th>User</th>
                      <th>Activity Count</th>
                      <th>Percentage</th>
                    </tr>
                  </thead>
                  <tbody>
                    {statistics.topUsers.map((user, index) => {
                      const totalActivity = statistics.topUsers.reduce((sum, u) => sum + u.activityCount, 0);
                      const percentage = ((user.activityCount / totalActivity) * 100).toFixed(1);
                      return (
                        <tr key={user.userId}>
                          <td>#{index + 1}</td>
                          <td>{user.userName}</td>
                          <td>{user.activityCount.toLocaleString()}</td>
                          <td>
                            <div className="progress" style={{ height: '20px' }}>
                              <div
                                className="progress-bar"
                                role="progressbar"
                                style={{ width: `${percentage}%` }}
                                aria-valuenow={parseFloat(percentage)}
                                aria-valuemin={0}
                                aria-valuemax={100}
                              >
                                {percentage}%
                              </div>
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </Table>
              ) : (
                <p className="text-muted text-center py-3">No activity data available</p>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default SecurityDashboard;
