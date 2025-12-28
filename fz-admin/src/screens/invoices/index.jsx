import { Box, Typography, useTheme, Chip, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, Button, Grid, Divider } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { tokens } from "../../theme";
import { useState, useEffect } from "react";
import Header from "../../components/Header";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import PendingIcon from "@mui/icons-material/Pending";
import CancelIcon from "@mui/icons-material/Cancel";
import CloseIcon from "@mui/icons-material/Close";
import axios from "axios";

const Invoices = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [openModal, setOpenModal] = useState(false);
  const [pageSize, setPageSize] = useState(10);

  useEffect(() => {
    fetchOrders();
  }, []);

  const fetchOrders = async () => {
    try {
      const response = await axios.get('https://filmzone-api.koyeb.app/api/payment/order/all', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });
      
      if (response.data.errorCode === 200) {
        // Transform data
        const transformedData = response.data.data.map((order) => ({
          id: order.orderID,
          ...order,
        }));
        setOrders(transformedData);
      }
    } catch (error) {
      console.error("Error fetching orders:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleViewDetail = (order) => {
    setSelectedOrder(order);
    setOpenModal(true);
  };

  const handleCloseModal = () => {
    setOpenModal(false);
    setSelectedOrder(null);
  };

  const formatDateTime = (dateString) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatCurrency = (amount, currency) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: currency || 'VND',
    }).format(amount);
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'paid':
        return colors.greenAccent[600];
      case 'pending':
        return colors.blueAccent[600];
      case 'failed':
      case 'cancelled':
        return colors.redAccent[600];
      default:
        return colors.grey[600];
    }
  };

  const getStatusIcon = (status) => {
    switch (status) {
      case 'paid':
        return <CheckCircleIcon />;
      case 'pending':
        return <PendingIcon />;
      case 'failed':
      case 'cancelled':
        return <CancelIcon />;
      default:
        return null;
    }
  };

  const getStatusText = (status) => {
    switch (status) {
      case 'paid':
        return 'Đã thanh toán';
      case 'pending':
        return 'Chờ xử lý';
      case 'failed':
        return 'Thất bại';
      case 'cancelled':
        return 'Đã hủy';
      default:
        return status;
    }
  };

  const columns = [
    { 
      field: "orderID", 
      headerName: "Order ID",
      width: 100,
    },
    {
      field: "userID",
      headerName: "User ID",
      width: 100,
    },
    {
      field: "amount",
      headerName: "Số tiền",
      flex: 1,
      minWidth: 150,
      renderCell: (params) => (
        <Typography color={colors.greenAccent[500]} fontWeight="600">
          {formatCurrency(params.row.amount, params.row.currency)}
        </Typography>
      ),
    },
    {
      field: "status",
      headerName: "Trạng thái",
      width: 150,
      renderCell: ({ row }) => {
        return (
          <Chip
            icon={getStatusIcon(row.status)}
            label={getStatusText(row.status)}
            size="small"
            sx={{
              backgroundColor: getStatusColor(row.status),
              color: colors.grey[100],
              fontWeight: "600",
            }}
          />
        );
      },
    },
    {
      field: "provider",
      headerName: "Phương thức",
      width: 120,
      renderCell: ({ row }) => (
        <Chip
          label={row.provider.toUpperCase()}
          size="small"
          variant="outlined"
          sx={{
            borderColor: colors.blueAccent[500],
            color: colors.blueAccent[500],
          }}
        />
      ),
    },
    {
      field: "createdAt",
      headerName: "Ngày tạo",
      flex: 1,
      minWidth: 180,
      renderCell: ({ row }) => formatDateTime(row.createdAt),
    },
    {
      field: "expiresAt",
      headerName: "Hết hạn",
      flex: 1,
      minWidth: 180,
      renderCell: ({ row }) => (
        <Typography color={colors.redAccent[400]}>
          {formatDateTime(row.expiresAt)}
        </Typography>
      ),
    },
    {
      field: "actions",
      headerName: "Hành động",
      width: 100,
      sortable: false,
      renderCell: ({ row }) => {
        return (
          <IconButton
            onClick={() => handleViewDetail(row)}
            sx={{
              color: colors.blueAccent[500],
              "&:hover": {
                backgroundColor: colors.blueAccent[800],
              },
            }}
          >
            <VisibilityOutlinedIcon />
          </IconButton>
        );
      },
    },
  ];

  return (
    <Box m="20px">
      <Header 
        title="QUẢN LÝ ĐƠN HÀNG" 
        subtitle="Danh sách tất cả đơn hàng và thanh toán" 
      />
      <Box
        m="40px 0 0 0"
        height="75vh"
        sx={{
          "& .MuiDataGrid-root": {
            border: "none",
          },
          "& .MuiDataGrid-cell": {
            borderBottom: "none",
          },
          "& .MuiDataGrid-columnHeaders": {
            backgroundColor: colors.blueAccent[700],
            borderBottom: "none",
          },
          "& .MuiDataGrid-virtualScroller": {
            backgroundColor: colors.primary[400],
          },
          "& .MuiDataGrid-footerContainer": {
            borderTop: "none",
            backgroundColor: colors.blueAccent[700],
          },
        }}
      >
        <DataGrid
          rows={orders}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          onPageSizeChange={(newPageSize) => setPageSize(newPageSize)}
          rowsPerPageOptions={[5, 10, 20, 50]}
          disableSelectionOnClick
        />
      </Box>

      {/* Modal chi tiết đơn hàng */}
      <Dialog
        open={openModal}
        onClose={handleCloseModal}
        maxWidth="md"
        fullWidth
        PaperProps={{
          sx: {
            backgroundColor: colors.primary[400],
            backgroundImage: 'none',
          }
        }}
      >
        <DialogTitle
          sx={{
            backgroundColor: colors.blueAccent[700],
            color: colors.grey[100],
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Typography variant="h4" fontWeight="600">
            Chi tiết đơn hàng #{selectedOrder?.orderID}
          </Typography>
          <IconButton onClick={handleCloseModal} sx={{ color: colors.grey[100] }}>
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        
        <DialogContent sx={{ mt: 2 }}>
          {selectedOrder && (
            <Grid container spacing={3}>
              {/* Thông tin đơn hàng */}
              <Grid item xs={12}>
                <Typography variant="h5" color={colors.grey[100]} fontWeight="600" mb={2}>
                  Thông tin đơn hàng
                </Typography>
                <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
                
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Typography variant="body2" color={colors.grey[300]}>
                      Order ID
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="500">
                      {selectedOrder.orderID}
                    </Typography>
                  </Grid>
                  
                  <Grid item xs={6}>
                    <Typography variant="body2" color={colors.grey[300]}>
                      User ID
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="500">
                      {selectedOrder.userID}
                    </Typography>
                  </Grid>

                  <Grid item xs={6}>
                    <Typography variant="body2" color={colors.grey[300]}>
                      Plan ID
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="500">
                      {selectedOrder.planID}
                    </Typography>
                  </Grid>

                  <Grid item xs={6}>
                    <Typography variant="body2" color={colors.grey[300]}>
                      Price ID
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="500">
                      {selectedOrder.priceID}
                    </Typography>
                  </Grid>

                  <Grid item xs={12}>
                    <Typography variant="body2" color={colors.grey[300]}>
                      Số tiền
                    </Typography>
                    <Typography variant="h4" color={colors.greenAccent[500]} fontWeight="700">
                      {formatCurrency(selectedOrder.amount, selectedOrder.currency)}
                    </Typography>
                  </Grid>

                  <Grid item xs={6}>
                    <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                      Trạng thái
                    </Typography>
                    <Chip
                      icon={getStatusIcon(selectedOrder.status)}
                      label={getStatusText(selectedOrder.status)}
                      sx={{
                        backgroundColor: getStatusColor(selectedOrder.status),
                        color: colors.grey[100],
                        fontWeight: "600",
                      }}
                    />
                  </Grid>

                  <Grid item xs={6}>
                    <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                      Phương thức thanh toán
                    </Typography>
                    <Chip
                      label={selectedOrder.provider.toUpperCase()}
                      variant="outlined"
                      sx={{
                        borderColor: colors.blueAccent[500],
                        color: colors.blueAccent[500],
                        fontWeight: "600",
                      }}
                    />
                  </Grid>
                </Grid>
              </Grid>

              {/* Provider Info */}
              <Grid item xs={12}>
                <Typography variant="h5" color={colors.grey[100]} fontWeight="600" mb={2}>
                  Thông tin thanh toán
                </Typography>
                <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
                
                <Box
                  sx={{
                    backgroundColor: colors.primary[500],
                    padding: "15px",
                    borderRadius: "8px",
                  }}
                >
                  <Typography variant="body2" color={colors.grey[300]}>
                    Provider Session ID
                  </Typography>
                  <Typography 
                    variant="body1" 
                    color={colors.grey[100]} 
                    fontWeight="500"
                    sx={{ wordBreak: 'break-all' }}
                  >
                    {selectedOrder.providerSessionId}
                  </Typography>
                </Box>
              </Grid>

              {/* Thời gian */}
              <Grid item xs={12}>
                <Typography variant="h5" color={colors.grey[100]} fontWeight="600" mb={2}>
                  Thời gian
                </Typography>
                <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
                
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Typography variant="body2" color={colors.grey[300]}>
                      Ngày tạo
                    </Typography>
                    <Typography variant="body1" color={colors.greenAccent[400]} fontWeight="500">
                      {formatDateTime(selectedOrder.createdAt)}
                    </Typography>
                  </Grid>

                  <Grid item xs={6}>
                    <Typography variant="body2" color={colors.grey[300]}>
                      Hết hạn
                    </Typography>
                    <Typography variant="body1" color={colors.redAccent[400]} fontWeight="500">
                      {formatDateTime(selectedOrder.expiresAt)}
                    </Typography>
                  </Grid>
                </Grid>
              </Grid>

              {/* Invoices */}
              {selectedOrder.invoices && selectedOrder.invoices.length > 0 && (
                <Grid item xs={12}>
                  <Typography variant="h5" color={colors.grey[100]} fontWeight="600" mb={2}>
                    Hóa đơn ({selectedOrder.invoices.length})
                  </Typography>
                  <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
                  
                  {selectedOrder.invoices.map((invoice, index) => (
                    <Box
                      key={index}
                      sx={{
                        backgroundColor: colors.primary[500],
                        padding: "10px",
                        borderRadius: "8px",
                        mb: 1,
                      }}
                    >
                      <Typography variant="body2" color={colors.grey[100]}>
                        Invoice #{index + 1}
                      </Typography>
                    </Box>
                  ))}
                </Grid>
              )}
            </Grid>
          )}
        </DialogContent>
        
        <DialogActions sx={{ p: 2 }}>
          <Button
            onClick={handleCloseModal}
            variant="contained"
            sx={{
              backgroundColor: colors.blueAccent[700],
              color: colors.grey[100],
              "&:hover": {
                backgroundColor: colors.blueAccent[800],
              },
            }}
          >
            Đóng
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Invoices;