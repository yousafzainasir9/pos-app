import React, { useState, useEffect } from 'react';
import {
  Container,
  Card,
  Table,
  Button,
  Modal,
  Form,
  Badge,
  Pagination,
  InputGroup,
  Alert,
  Spinner,
  Row,
  Col
} from 'react-bootstrap';
import {
  FaUser,
  FaPlus,
  FaEdit,
  FaKey,
  FaLock,
  FaSearch,
  FaFilter,
  FaCheckCircle,
  FaTimesCircle
} from 'react-icons/fa';
import { toast } from 'react-toastify';
import userService, { User, UserDetail, CreateUserDto, UpdateUserDto } from '@/services/user.service';
import storeService, { Store } from '@/services/store.service';
import { useAuth } from '@/contexts/AuthContext';

const UserManagementPage: React.FC = () => {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [stores, setStores] = useState<Store[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [showPinModal, setShowPinModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  // Filters
  const [searchTerm, setSearchTerm] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState<boolean | undefined>(undefined);

  // Form data
  const [formData, setFormData] = useState<CreateUserDto>({
    username: '',
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    phone: '',
    pin: '',
    role: 'Cashier',
    storeId: undefined
  });

  const [newPassword, setNewPassword] = useState('');
  const [newPin, setNewPin] = useState('');

  useEffect(() => {
    loadUsers();
    loadStores();
  }, [currentPage, searchTerm, roleFilter, statusFilter]);

  const loadUsers = async () => {
    setIsLoading(true);
    try {
      const response = await userService.getUsers({
        page: currentPage,
        pageSize,
        search: searchTerm || undefined,
        role: roleFilter || undefined,
        isActive: statusFilter
      });

      if (response.success && response.data) {
        setUsers(response.data.items);
        setTotalPages(response.data.totalPages);
        setTotalCount(response.data.totalCount);
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to load users');
    } finally {
      setIsLoading(false);
    }
  };

  const loadStores = async () => {
    try {
      const response = await storeService.getStores();
      if (response.success && response.data) {
        setStores(response.data);
      }
    } catch (error) {
      console.error('Failed to load stores:', error);
    }
  };

  const handleOpenCreateModal = () => {
    setModalMode('create');
    setFormData({
      username: '',
      email: '',
      password: '',
      firstName: '',
      lastName: '',
      phone: '',
      pin: '',
      role: 'Cashier',
      storeId: undefined
    });
    setSelectedUser(null);
    setShowModal(true);
  };

  const handleOpenEditModal = (user: User) => {
    setModalMode('edit');
    setFormData({
      username: user.username,
      email: user.email,
      password: '', // Don't populate password
      firstName: user.firstName,
      lastName: user.lastName,
      phone: user.phone || '',
      pin: '',
      role: user.role,
      storeId: user.storeId
    });
    setSelectedUser(user);
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedUser(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (modalMode === 'create') {
        // Validate PIN format if provided
        if (formData.pin && !/^\d{4,6}$/.test(formData.pin)) {
          toast.error('PIN must be 4-6 digits');
          return;
        }

        const response = await userService.createUser(formData);
        if (response.success) {
          toast.success('User created successfully');
          handleCloseModal();
          loadUsers();
        }
      } else if (selectedUser) {
        const updateData: UpdateUserDto = {
          email: formData.email,
          firstName: formData.firstName,
          lastName: formData.lastName,
          phone: formData.phone || undefined,
          role: formData.role,
          storeId: formData.storeId
        };
        const response = await userService.updateUser(selectedUser.id, updateData);
        if (response.success) {
          toast.success('User updated successfully');
          handleCloseModal();
          loadUsers();
        }
      }
    } catch (error: any) {
      if (error.response?.data?.message) {
        toast.error(error.response.data.message);
      } else {
        toast.error(`Failed to ${modalMode} user`);
      }
    }
  };

  const handleToggleStatus = async (user: User) => {
    try {
      if (user.isActive) {
        const response = await userService.deactivateUser(user.id);
        if (response.success) {
          toast.success('User deactivated successfully');
          loadUsers();
        }
      } else {
        const response = await userService.updateUser(user.id, { isActive: true });
        if (response.success) {
          toast.success('User activated successfully');
          loadUsers();
        }
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to update user status');
    }
  };

  const handleOpenPasswordModal = (user: User) => {
    setSelectedUser(user);
    setNewPassword('');
    setShowPasswordModal(true);
  };

  const handleResetPassword = async () => {
    if (!selectedUser || !newPassword) {
      toast.error('Please enter a new password');
      return;
    }

    try {
      const response = await userService.resetPassword(selectedUser.id, newPassword);
      if (response.success) {
        toast.success('Password reset successfully');
        setShowPasswordModal(false);
        setNewPassword('');
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to reset password');
    }
  };

  const handleOpenPinModal = (user: User) => {
    setSelectedUser(user);
    setNewPin('');
    setShowPinModal(true);
  };

  const handleResetPin = async () => {
    if (!selectedUser || !newPin) {
      toast.error('Please enter a new PIN');
      return;
    }

    // Validate PIN format
    if (!/^\d{4,6}$/.test(newPin)) {
      toast.error('PIN must be 4-6 digits');
      return;
    }

    try {
      // First check if PIN is already in use
      const allUsersResponse = await userService.getUsers({ pageSize: 1000 });
      if (allUsersResponse.success && allUsersResponse.data) {
        const usersWithPin = allUsersResponse.data.items.filter(
          (u: User) => u.id !== selectedUser.id
        );
        
        // We can't directly check PINs, but the backend should validate
        // For now, we'll let the backend handle the uniqueness check
      }

      const response = await userService.resetPin(selectedUser.id, newPin);
      if (response.success) {
        toast.success('PIN reset successfully');
        setShowPinModal(false);
        setNewPin('');
      }
    } catch (error: any) {
      if (error.response?.data?.message) {
        toast.error(error.response.data.message);
      } else {
        toast.error('Failed to reset PIN');
      }
    }
  };

  const handleSearch = () => {
    setCurrentPage(1);
    loadUsers();
  };

  const handleClearFilters = () => {
    setSearchTerm('');
    setRoleFilter('');
    setStatusFilter(undefined);
    setCurrentPage(1);
  };

  const getRoleBadgeColor = (role: string) => {
    switch (role) {
      case 'Admin': return 'danger';
      case 'Manager': return 'warning';
      case 'Cashier': return 'info';
      default: return 'secondary';
    }
  };

  return (
    <Container fluid>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>User Management</h2>
        {currentUser?.role === 'Admin' && (
          <Button variant="primary" onClick={handleOpenCreateModal}>
            <FaPlus className="me-2" />
            Add New User
          </Button>
        )}
      </div>

      {/* Filters */}
      <Card className="mb-4">
        <Card.Body>
          <Row>
            <Col md={4}>
              <InputGroup>
                <InputGroup.Text>
                  <FaSearch />
                </InputGroup.Text>
                <Form.Control
                  type="text"
                  placeholder="Search by name, username, or email..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
                />
              </InputGroup>
            </Col>
            <Col md={2}>
              <Form.Select value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)}>
                <option value="">All Roles</option>
                <option value="Admin">Admin</option>
                <option value="Manager">Manager</option>
                <option value="Cashier">Cashier</option>
              </Form.Select>
            </Col>
            <Col md={2}>
              <Form.Select 
                value={statusFilter === undefined ? '' : statusFilter ? 'true' : 'false'} 
                onChange={(e) => setStatusFilter(e.target.value === '' ? undefined : e.target.value === 'true')}
              >
                <option value="">All Status</option>
                <option value="true">Active</option>
                <option value="false">Inactive</option>
              </Form.Select>
            </Col>
            <Col md={4}>
              <Button variant="primary" onClick={handleSearch} className="me-2">
                <FaFilter className="me-2" />
                Apply Filters
              </Button>
              <Button variant="outline-secondary" onClick={handleClearFilters}>
                Clear
              </Button>
            </Col>
          </Row>
        </Card.Body>
      </Card>

      {/* Users Table */}
      <Card>
        <Card.Body>
          {isLoading ? (
            <div className="text-center py-5">
              <Spinner animation="border" />
            </div>
          ) : (
            <>
              <Table hover responsive>
                <thead>
                  <tr>
                    <th>Username</th>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Role</th>
                    <th>Store</th>
                    <th>Status</th>
                    <th>Last Login</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {users.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="text-center py-4">
                        No users found
                      </td>
                    </tr>
                  ) : (
                    users.map((user) => (
                      <tr key={user.id}>
                        <td>
                          <strong>{user.username}</strong>
                          {user.id === currentUser?.id && (
                            <Badge bg="primary" className="ms-2">You</Badge>
                          )}
                        </td>
                        <td>{user.fullName}</td>
                        <td>{user.email}</td>
                        <td>
                          <Badge bg={getRoleBadgeColor(user.role)}>
                            {user.role}
                          </Badge>
                        </td>
                        <td>{user.storeName || '-'}</td>
                        <td>
                          {user.isActive ? (
                            <Badge bg="success">
                              <FaCheckCircle className="me-1" />
                              Active
                            </Badge>
                          ) : (
                            <Badge bg="secondary">
                              <FaTimesCircle className="me-1" />
                              Inactive
                            </Badge>
                          )}
                        </td>
                        <td>
                          {user.lastLoginAt 
                            ? new Date(user.lastLoginAt).toLocaleString()
                            : 'Never'}
                        </td>
                        <td>
                          <div className="d-flex gap-1">
                            <Button
                              variant="outline-primary"
                              size="sm"
                              onClick={() => handleOpenEditModal(user)}
                              title="Edit User"
                            >
                              <FaEdit />
                            </Button>
                            {currentUser?.role === 'Admin' && (
                              <>
                                <Button
                                  variant="outline-warning"
                                  size="sm"
                                  onClick={() => handleOpenPasswordModal(user)}
                                  title="Reset Password"
                                >
                                  <FaLock />
                                </Button>
                                <Button
                                  variant="outline-info"
                                  size="sm"
                                  onClick={() => handleOpenPinModal(user)}
                                  title="Reset PIN"
                                >
                                  <FaKey />
                                </Button>
                                {user.id !== currentUser?.id && (
                                  <Button
                                    variant={user.isActive ? 'outline-danger' : 'outline-success'}
                                    size="sm"
                                    onClick={() => handleToggleStatus(user)}
                                    title={user.isActive ? 'Deactivate' : 'Activate'}
                                  >
                                    {user.isActive ? <FaTimesCircle /> : <FaCheckCircle />}
                                  </Button>
                                )}
                              </>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </Table>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="d-flex justify-content-between align-items-center mt-3">
                  <div className="text-muted">
                    Showing {((currentPage - 1) * pageSize) + 1} to {Math.min(currentPage * pageSize, totalCount)} of {totalCount} users
                  </div>
                  <Pagination className="mb-0">
                    <Pagination.First onClick={() => setCurrentPage(1)} disabled={currentPage === 1} />
                    <Pagination.Prev onClick={() => setCurrentPage(currentPage - 1)} disabled={currentPage === 1} />
                    
                    {/* Show page numbers dynamically */}
                    {(() => {
                      const pages = [];
                      const maxPagesToShow = 5;
                      let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
                      let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);
                      
                      // Adjust start if we're near the end
                      if (endPage - startPage < maxPagesToShow - 1) {
                        startPage = Math.max(1, endPage - maxPagesToShow + 1);
                      }
                      
                      // Always show first page if not in range
                      if (startPage > 1) {
                        pages.push(
                          <Pagination.Item key={1} onClick={() => setCurrentPage(1)}>
                            1
                          </Pagination.Item>
                        );
                        if (startPage > 2) {
                          pages.push(<Pagination.Ellipsis key="ellipsis-start" disabled />);
                        }
                      }
                      
                      // Show page numbers in range
                      for (let page = startPage; page <= endPage; page++) {
                        pages.push(
                          <Pagination.Item
                            key={page}
                            active={page === currentPage}
                            onClick={() => setCurrentPage(page)}
                          >
                            {page}
                          </Pagination.Item>
                        );
                      }
                      
                      // Always show last page if not in range
                      if (endPage < totalPages) {
                        if (endPage < totalPages - 1) {
                          pages.push(<Pagination.Ellipsis key="ellipsis-end" disabled />);
                        }
                        pages.push(
                          <Pagination.Item key={totalPages} onClick={() => setCurrentPage(totalPages)}>
                            {totalPages}
                          </Pagination.Item>
                        );
                      }
                      
                      return pages;
                    })()}
                    
                    <Pagination.Next onClick={() => setCurrentPage(currentPage + 1)} disabled={currentPage === totalPages} />
                    <Pagination.Last onClick={() => setCurrentPage(totalPages)} disabled={currentPage === totalPages} />
                  </Pagination>
                </div>
              )}
            </>
          )}
        </Card.Body>
      </Card>

      {/* Create/Edit User Modal */}
      <Modal show={showModal} onHide={handleCloseModal} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>
            <FaUser className="me-2" />
            {modalMode === 'create' ? 'Create New User' : 'Edit User'}
          </Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSubmit}>
          <Modal.Body>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Username *</Form.Label>
                  <Form.Control
                    type="text"
                    value={formData.username}
                    onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                    required
                    disabled={modalMode === 'edit'}
                  />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Email *</Form.Label>
                  <Form.Control
                    type="email"
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    required
                  />
                </Form.Group>
              </Col>
            </Row>

            {modalMode === 'create' && (
              <Form.Group className="mb-3">
                <Form.Label>Password *</Form.Label>
                <Form.Control
                  type="password"
                  value={formData.password}
                  onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                  required
                />
                <Form.Text className="text-muted">
                  Minimum 6 characters
                </Form.Text>
              </Form.Group>
            )}

            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>First Name *</Form.Label>
                  <Form.Control
                    type="text"
                    value={formData.firstName}
                    onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                    required
                  />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Last Name *</Form.Label>
                  <Form.Control
                    type="text"
                    value={formData.lastName}
                    onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                    required
                  />
                </Form.Group>
              </Col>
            </Row>

            <Form.Group className="mb-3">
              <Form.Label>Phone</Form.Label>
              <Form.Control
                type="tel"
                value={formData.phone}
                onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
              />
            </Form.Group>

            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Role *</Form.Label>
                  <Form.Select
                    value={formData.role}
                    onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                    required
                  >
                    <option value="Cashier">Cashier</option>
                    <option value="Manager">Manager</option>
                    <option value="Admin">Admin</option>
                  </Form.Select>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Store</Form.Label>
                  <Form.Select
                    value={formData.storeId || ''}
                    onChange={(e) => setFormData({ ...formData, storeId: e.target.value ? parseInt(e.target.value) : undefined })}
                  >
                    <option value="">No Store</option>
                    {stores.map((store) => (
                      <option key={store.id} value={store.id}>
                        {store.name}
                      </option>
                    ))}
                  </Form.Select>
                </Form.Group>
              </Col>
            </Row>

            {modalMode === 'create' && (
              <Form.Group className="mb-3">
                <Form.Label>PIN (Optional)</Form.Label>
                <Form.Control
                  type="password"
                  value={formData.pin}
                  onChange={(e) => setFormData({ ...formData, pin: e.target.value })}
                  maxLength={6}
                />
                <Form.Text className="text-muted">
                  4-6 digit PIN for quick POS login
                </Form.Text>
              </Form.Group>
            )}
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={handleCloseModal}>
              Cancel
            </Button>
            <Button variant="primary" type="submit">
              {modalMode === 'create' ? 'Create User' : 'Save Changes'}
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>

      {/* Reset Password Modal */}
      <Modal show={showPasswordModal} onHide={() => setShowPasswordModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>
            <FaLock className="me-2" />
            Reset Password
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Alert variant="warning">
            You are about to reset the password for <strong>{selectedUser?.fullName}</strong>
          </Alert>
          <Form.Group>
            <Form.Label>New Password</Form.Label>
            <Form.Control
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              placeholder="Enter new password"
            />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowPasswordModal(false)}>
            Cancel
          </Button>
          <Button variant="warning" onClick={handleResetPassword}>
            Reset Password
          </Button>
        </Modal.Footer>
      </Modal>

      {/* Reset PIN Modal */}
      <Modal show={showPinModal} onHide={() => setShowPinModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>
            <FaKey className="me-2" />
            Reset PIN
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Alert variant="info">
            You are about to reset the PIN for <strong>{selectedUser?.fullName}</strong>
          </Alert>
          <Alert variant="warning" className="small">
            <strong>Important:</strong> Each PIN must be unique across all users. The PIN will be used for quick POS login.
          </Alert>
          <Form.Group>
            <Form.Label>New PIN</Form.Label>
            <Form.Control
              type="password"
              value={newPin}
              onChange={(e) => setNewPin(e.target.value)}
              placeholder="Enter 4-6 digit PIN"
              maxLength={6}
              pattern="\d{4,6}"
            />
            <Form.Text className="text-muted">
              Must be 4-6 digits and unique across all users
            </Form.Text>
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowPinModal(false)}>
            Cancel
          </Button>
          <Button variant="info" onClick={handleResetPin}>
            Reset PIN
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default UserManagementPage;
