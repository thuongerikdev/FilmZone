import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Box,
  useTheme,
  Button,
  Card,
  CardContent,
  Grid,
  Typography,
  Chip,
  Divider,
  CircularProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControlLabel,
  Switch,
  Alert,
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import AddIcon from "@mui/icons-material/Add";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import { getPlanById, getAllPrices, createPrice, updatePrice, deletePrice } from "../services/api";

const PlanDetail = () => {
  const { planId } = useParams();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [plan, setPlan] = useState(null);
  const [prices, setPrices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  
  // Dialog states
  const [openDialog, setOpenDialog] = useState(false);
  const [editingPrice, setEditingPrice] = useState(null);
  const [dialogLoading, setDialogLoading] = useState(false);
  const [dialogError, setDialogError] = useState("");
  const [dialogSuccess, setDialogSuccess] = useState("");

  const [formData, setFormData] = useState({
    planID: parseInt(planId),
    currency: "VND",
    amount: 0,
    intervalUnit: "month",
    intervalCount: 1,
    trialDays: 0,
    isActive: true,
  });

  const intervalUnits = [
    { value: "day", label: "Ngày" },
    { value: "week", label: "Tuần" },
    { value: "month", label: "Tháng" },
    { value: "year", label: "Năm" },
  ];

  const currencies = ["VND", "USD", "EUR"];

  useEffect(() => {
    fetchPlanDetail();
    fetchPrices();
  }, [planId]);

  const fetchPlanDetail = async () => {
    try {
      const response = await getPlanById(planId);
      if (response.data.errorCode === 200) {
        setPlan(response.data.data);
      } else {
        setError("Không thể tải thông tin gói dịch vụ");
      }
    } catch (error) {
      console.error("Error fetching plan:", error);
      setError("Có lỗi xảy ra khi tải thông tin gói dịch vụ");
    } finally {
      setLoading(false);
    }
  };

  const fetchPrices = async () => {
    try {
      const response = await getAllPrices();
      if (response.data.errorCode === 200) {
        const planPrices = response.data.data.filter(
          (price) => price.planID === parseInt(planId)
        );
        setPrices(planPrices);
      }
    } catch (error) {
      console.error("Error fetching prices:", error);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(amount);
  };

  const getIntervalLabel = (intervalUnit, intervalCount) => {
    const labels = {
      day: "ngày",
      week: "tuần",
      month: "tháng",
      year: "năm",
    };
    const unit = labels[intervalUnit] || intervalUnit;
    return `${intervalCount} ${unit}`;
  };

  const handleOpenDialog = (price = null) => {
    if (price) {
      setEditingPrice(price);
      setFormData({
        planID: price.planID,
        currency: price.currency,
        amount: price.amount,
        intervalUnit: price.intervalUnit,
        intervalCount: price.intervalCount,
        trialDays: price.trialDays || 0,
        isActive: price.isActive,
      });
    } else {
      setEditingPrice(null);
      setFormData({
        planID: parseInt(planId),
        currency: "VND",
        amount: 0,
        intervalUnit: "month",
        intervalCount: 1,
        trialDays: 0,
        isActive: true,
      });
    }
    setDialogError("");
    setDialogSuccess("");
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingPrice(null);
  };

  const handleChange = (e) => {
    const { name, value, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === "isActive" 
        ? checked 
        : ["amount", "intervalCount", "trialDays", "planID"].includes(name)
        ? Number(value)
        : value,
    }));
  };

  const handleSubmit = async () => {
    if (!formData.amount || formData.amount <= 0) {
      setDialogError("Giá tiền phải lớn hơn 0");
      return;
    }

    if (formData.intervalCount <= 0) {
      setDialogError("Số kỳ phải lớn hơn 0");
      return;
    }

    setDialogLoading(true);
    try {
      const dataToSubmit = {
        planID: parseInt(formData.planID),
        currency: formData.currency,
        amount: formData.amount,
        intervalUnit: formData.intervalUnit,
        intervalCount: formData.intervalCount,
        trialDays: formData.trialDays,
        isActive: formData.isActive,
      };

      let response;
      if (editingPrice) {
        response = await updatePrice({
          ...dataToSubmit,
          priceID: editingPrice.priceID,
        });
      } else {
        response = await createPrice(dataToSubmit);
      }

      if (response.data.errorCode === 200) {
        setDialogSuccess(
          editingPrice ? "Cập nhật giá tiền thành công!" : "Tạo giá tiền thành công!"
        );
        setTimeout(() => {
          handleCloseDialog();
          fetchPrices();
        }, 1000);
      } else {
        setDialogError(response.data.errorMessage || "Có lỗi xảy ra");
      }
    } catch (err) {
      console.error("Error:", err);
      setDialogError(err.response?.data?.errorMessage || "Có lỗi xảy ra khi lưu giá tiền");
    } finally {
      setDialogLoading(false);
    }
  };

  const handleDelete = async (priceId) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa giá tiền này?")) {
      try {
        const response = await deletePrice(priceId);
        if (response.data.errorCode === 200) {
          fetchPrices();
        } else {
          setError(response.data.errorMessage || "Xóa giá tiền thất bại");
        }
      } catch (err) {
        console.error("Error deleting price:", err);
        setError("Có lỗi xảy ra khi xóa giá tiền");
      }
    }
  };

  if (loading) {
    return (
      <Box m="20px" display="flex" justifyContent="center" alignItems="center" height="60vh">
        <CircularProgress />
      </Box>
    );
  }

  if (error || !plan) {
    return (
      <Box m="20px">
        <Header title="CHI TIẾT GÓI DỊCH VỤ" subtitle="Không tìm thấy thông tin" />
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate("/plans")}
          sx={{ mt: 2, color: colors.grey[100], borderColor: colors.grey[400] }}
          variant="outlined"
        >
          Quay lại
        </Button>
      </Box>
    );
  }

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Header 
          title="CHI TIẾT GÓI DỊCH VỤ" 
          subtitle={`ID: ${plan.planID} - ${plan.name}`} 
        />
        <Box display="flex" gap={2}>
          <Button
            startIcon={<EditOutlinedIcon />}
            onClick={() => navigate(`/plans/edit/${planId}`)}
            sx={{
              backgroundColor: colors.greenAccent[600],
              color: colors.grey[100],
              "&:hover": {
                backgroundColor: colors.greenAccent[700],
              },
            }}
            variant="contained"
          >
            Chỉnh sửa
          </Button>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate("/plans")}
            sx={{
              color: colors.grey[100],
              borderColor: colors.grey[400],
            }}
            variant="outlined"
          >
            Quay lại
          </Button>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Thông tin cơ bản */}
        <Grid item xs={12}>
          <Card sx={{ backgroundColor: colors.primary[400] }}>
            <CardContent>
              <Typography 
                variant="h4" 
                sx={{ mb: 3, color: colors.greenAccent[400], fontWeight: "bold" }}
              >
                Thông tin cơ bản
              </Typography>
              
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Box mb={2}>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={0.5}>
                      ID
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="600">
                      {plan.planID}
                    </Typography>
                  </Box>

                  <Box mb={2}>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={0.5}>
                      Mã gói
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="600">
                      {plan.code}
                    </Typography>
                  </Box>

                  <Box mb={2}>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={0.5}>
                      Tên gói
                    </Typography>
                    <Typography 
                      variant="h5" 
                      color={colors.greenAccent[300]} 
                      fontWeight="700"
                    >
                      {plan.name}
                    </Typography>
                  </Box>
                </Grid>

                <Grid item xs={12} md={6}>
                  <Box mb={2}>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={0.5}>
                      Trạng thái
                    </Typography>
                    <Chip
                      icon={plan.isActive ? <CheckCircleIcon /> : <CancelIcon />}
                      label={plan.isActive ? "Đang hoạt động" : "Không hoạt động"}
                      sx={{
                        backgroundColor: plan.isActive 
                          ? colors.greenAccent[600] 
                          : colors.redAccent[600],
                        color: colors.grey[100],
                        fontWeight: "600",
                        mt: 0.5,
                      }}
                    />
                  </Box>
                </Grid>

                <Grid item xs={12}>
                  <Divider sx={{ my: 2, borderColor: colors.grey[700] }} />
                  
                  <Box>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={1}>
                      Mô tả
                    </Typography>
                    <Typography 
                      variant="body1" 
                      color={colors.grey[100]}
                      sx={{ 
                        whiteSpace: "pre-wrap",
                        lineHeight: 1.8,
                      }}
                    >
                      {plan.description || "Chưa có mô tả"}
                    </Typography>
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>

        {/* Quản lý giá tiền */}
        <Grid item xs={12}>
          <Card sx={{ backgroundColor: colors.primary[400] }}>
            <CardContent>
              <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
                <Typography 
                  variant="h4" 
                  sx={{ color: colors.greenAccent[400], fontWeight: "bold" }}
                >
                  Quản lý giá tiền ({prices.length})
                </Typography>
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => handleOpenDialog()}
                  sx={{
                    backgroundColor: colors.blueAccent[600],
                    color: colors.grey[100],
                    "&:hover": {
                      backgroundColor: colors.blueAccent[700],
                    },
                  }}
                >
                  Thêm giá
                </Button>
              </Box>
              
              {prices.length > 0 ? (
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow sx={{ backgroundColor: colors.primary[300] }}>
                        <TableCell sx={{ color: colors.grey[100], fontWeight: "bold" }}>
                          ID
                        </TableCell>
                        <TableCell sx={{ color: colors.grey[100], fontWeight: "bold" }}>
                          Loại tiền
                        </TableCell>
                        <TableCell sx={{ color: colors.grey[100], fontWeight: "bold" }}>
                          Số tiền
                        </TableCell>
                        <TableCell sx={{ color: colors.grey[100], fontWeight: "bold" }}>
                          Khoảng thời gian
                        </TableCell>
                        <TableCell sx={{ color: colors.grey[100], fontWeight: "bold" }}>
                          Ngày dùng thử
                        </TableCell>
                        <TableCell sx={{ color: colors.grey[100], fontWeight: "bold" }}>
                          Trạng thái
                        </TableCell>
                        <TableCell sx={{ color: colors.grey[100], fontWeight: "bold" }}>
                          Hành động
                        </TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {prices.map((price) => (
                        <TableRow 
                          key={price.priceID}
                          sx={{
                            backgroundColor: colors.primary[400],
                            "&:hover": {
                              backgroundColor: colors.primary[300],
                            },
                          }}
                        >
                          <TableCell sx={{ color: colors.grey[100] }}>
                            {price.priceID}
                          </TableCell>
                          <TableCell sx={{ color: colors.grey[100] }}>
                            {price.currency}
                          </TableCell>
                          <TableCell sx={{ color: colors.greenAccent[300], fontWeight: "600" }}>
                            {formatCurrency(price.amount)}
                          </TableCell>
                          <TableCell sx={{ color: colors.grey[100] }}>
                            {getIntervalLabel(price.intervalUnit, price.intervalCount)}
                          </TableCell>
                          <TableCell sx={{ color: colors.grey[100] }}>
                            {price.trialDays ? `${price.trialDays} ngày` : "Không"}
                          </TableCell>
                          <TableCell>
                            <Chip
                              icon={price.isActive ? <CheckCircleIcon /> : <CancelIcon />}
                              label={price.isActive ? "Hoạt động" : "Vô hiệu"}
                              size="small"
                              sx={{
                                backgroundColor: price.isActive 
                                  ? colors.greenAccent[600] 
                                  : colors.redAccent[600],
                                color: colors.grey[100],
                              }}
                            />
                          </TableCell>
                          <TableCell>
                            <Box display="flex" gap={1}>
                              <Button
                                size="small"
                                onClick={() => handleOpenDialog(price)}
                                sx={{
                                  color: colors.greenAccent[500],
                                  minWidth: "auto",
                                  "&:hover": {
                                    backgroundColor: colors.greenAccent[800],
                                  },
                                }}
                              >
                                <EditOutlinedIcon fontSize="small" />
                              </Button>
                              <Button
                                size="small"
                                onClick={() => handleDelete(price.priceID)}
                                sx={{
                                  color: colors.redAccent[500],
                                  minWidth: "auto",
                                  "&:hover": {
                                    backgroundColor: colors.redAccent[800],
                                  },
                                }}
                              >
                                <DeleteOutlineIcon fontSize="small" />
                              </Button>
                            </Box>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              ) : (
                <Typography color={colors.grey[300]} sx={{ py: 3, textAlign: "center" }}>
                  Chưa có giá tiền nào cho gói dịch vụ này
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Thống kê đơn hàng */}
        {plan.orders && plan.orders.length > 0 && (
          <Grid item xs={12}>
            <Card sx={{ backgroundColor: colors.primary[400] }}>
              <CardContent>
                <Typography 
                  variant="h4" 
                  sx={{ mb: 3, color: colors.greenAccent[400], fontWeight: "bold" }}
                >
                  Thống kê đơn hàng
                </Typography>
                <Typography variant="body1" color={colors.grey[100]}>
                  Có {plan.orders.length} đơn hàng sử dụng gói này
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>

      {/* Dialog thêm/sửa giá tiền */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ backgroundColor: colors.primary[400], color: colors.grey[100], fontWeight: "bold" }}>
          {editingPrice ? "Chỉnh sửa giá tiền" : "Thêm giá tiền mới"}
        </DialogTitle>
        <DialogContent sx={{ backgroundColor: colors.primary[400], pt: 3 }}>
          {dialogError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {dialogError}
            </Alert>
          )}

          {dialogSuccess && (
            <Alert severity="success" sx={{ mb: 2 }}>
              {dialogSuccess}
            </Alert>
          )}

          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                select
                fullWidth
                label="Tiền tệ"
                name="currency"
                value={formData.currency}
                onChange={handleChange}
                SelectProps={{
                  native: true,
                }}
                variant="filled"
              >
                {currencies.map((curr) => (
                  <option key={curr} value={curr}>
                    {curr}
                  </option>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Giá tiền"
                name="amount"
                type="number"
                value={formData.amount}
                onChange={handleChange}
                variant="filled"
                inputProps={{ min: 0, step: "0.01" }}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                select
                fullWidth
                label="Chu kỳ"
                name="intervalUnit"
                value={formData.intervalUnit}
                onChange={handleChange}
                SelectProps={{
                  native: true,
                }}
                variant="filled"
              >
                {intervalUnits.map((unit) => (
                  <option key={unit.value} value={unit.value}>
                    {unit.label}
                  </option>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Số kỳ"
                name="intervalCount"
                type="number"
                value={formData.intervalCount}
                onChange={handleChange}
                variant="filled"
                inputProps={{ min: 1 }}
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Ngày dùng thử (0 nếu không có)"
                name="trialDays"
                type="number"
                value={formData.trialDays}
                onChange={handleChange}
                variant="filled"
                inputProps={{ min: 0 }}
              />
            </Grid>

            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formData.isActive}
                    onChange={handleChange}
                    name="isActive"
                  />
                }
                label={
                  <Box>
                    <Box sx={{ fontWeight: "600", color: colors.grey[100] }}>
                      Trạng thái: {formData.isActive ? "Hoạt động" : "Không hoạt động"}
                    </Box>
                  </Box>
                }
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions sx={{ backgroundColor: colors.primary[400], p: 2 }}>
          <Button onClick={handleCloseDialog} sx={{ color: colors.grey[100] }}>
            Hủy
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={dialogLoading}
            variant="contained"
            sx={{
              backgroundColor: colors.greenAccent[600],
              color: colors.grey[100],
              "&:hover": {
                backgroundColor: colors.greenAccent[700],
              },
            }}
          >
            {dialogLoading ? "Đang lưu..." : "Lưu"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PlanDetail;