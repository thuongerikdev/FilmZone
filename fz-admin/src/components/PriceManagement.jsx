import { useState } from "react";
import { useTheme } from "@mui/material";
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Chip,
  FormControlLabel,
  Switch,
  Grid,
  Alert,
} from "@mui/material";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import AddIcon from "@mui/icons-material/Add";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import { createPrice, updatePrice, deletePrice } from "../services/api";
import { tokens } from "../theme";

const PriceManagement = ({ planId, prices = [], onPricesUpdate }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  const [openDialog, setOpenDialog] = useState(false);
  const [editingPrice, setEditingPrice] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [formData, setFormData] = useState({
    planID: planId || 0,
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
        planID: planId || 0,
        currency: "VND",
        amount: 0,
        intervalUnit: "month",
        intervalCount: 1,
        trialDays: 0,
        isActive: true,
      });
    }
    setError("");
    setSuccess("");
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
      setError("Giá tiền phải lớn hơn 0");
      return;
    }

    if (formData.intervalCount <= 0) {
      setError("Số kỳ phải lớn hơn 0");
      return;
    }

    setLoading(true);
    try {
      const dataToSubmit = {
        ...formData,
        planID: parseInt(formData.planID),
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
        setSuccess(
          editingPrice ? "Cập nhật giá tiền thành công!" : "Tạo giá tiền thành công!"
        );
        handleCloseDialog();
        if (onPricesUpdate) {
          onPricesUpdate();
        }
        setTimeout(() => setSuccess(""), 2000);
      } else {
        setError(response.data.errorMessage || "Có lỗi xảy ra");
      }
    } catch (err) {
      console.error("Error:", err);
      setError(err.response?.data?.errorMessage || "Có lỗi xảy ra khi lưu giá tiền");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (priceId) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa giá tiền này?")) {
      try {
        const response = await deletePrice(priceId);
        if (response.data.errorCode === 200) {
          setSuccess("Xóa giá tiền thành công!");
          if (onPricesUpdate) {
            onPricesUpdate();
          }
          setTimeout(() => setSuccess(""), 2000);
        } else {
          setError(response.data.errorMessage || "Xóa giá tiền thất bại");
        }
      } catch (err) {
        console.error("Error deleting price:", err);
        setError("Có lỗi xảy ra khi xóa giá tiền");
      }
    }
  };

  const formatCurrency = (amount, currency) => {
    if (currency === "VND") {
      return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
      }).format(amount);
    }
    return `${amount} ${currency}`;
  };

  const formatInterval = (intervalCount, intervalUnit) => {
    const unitMap = {
      day: "ngày",
      week: "tuần",
      month: "tháng",
      year: "năm",
    };
    return `${intervalCount} ${unitMap[intervalUnit] || intervalUnit}`;
  };

  return (
    <Box>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      <Box display="flex" justifyContent="flex-end" mb={2}>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
          sx={{
            backgroundColor: colors.blueAccent,
            color: colors.grey[100],
            "&:hover": {
              backgroundColor: colors.blueAccent,
            },
          }}
        >
          Thêm giá mới
        </Button>
      </Box>

      {prices && prices.length > 0 ? (
        <TableContainer component={Paper} sx={{ backgroundColor: colors.primary[500] }}>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: colors.blueAccent[700] }}>
                <TableCell sx={{ color: colors.grey[100], fontWeight: "600" }}>
                  ID
                </TableCell>
                <TableCell sx={{ color: colors.grey[100], fontWeight: "600" }}>
                  Giá tiền
                </TableCell>
                <TableCell sx={{ color: colors.grey[100], fontWeight: "600" }}>
                  Chu kỳ
                </TableCell>
                <TableCell sx={{ color: colors.grey[100], fontWeight: "600" }}>
                  Dùng thử
                </TableCell>
                <TableCell sx={{ color: colors.grey[100], fontWeight: "600" }}>
                  Trạng thái
                </TableCell>
                <TableCell sx={{ color: colors.grey[100], fontWeight: "600" }}>
                  Hành động
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {prices.map((price) => (
                <TableRow
                  key={price.priceID}
                  sx={{
                    "&:hover": { backgroundColor: colors.primary[600] },
                    borderBottom: `1px solid ${colors.grey[700]}`,
                  }}
                >
                  <TableCell sx={{ color: colors.grey[100] }}>
                    {price.priceID}
                  </TableCell>
                  <TableCell sx={{ color: colors.greenAccent[300], fontWeight: "600" }}>
                    {formatCurrency(price.amount, price.currency)}
                  </TableCell>
                  <TableCell sx={{ color: colors.grey[100] }}>
                    {formatInterval(price.intervalCount, price.intervalUnit)}
                  </TableCell>
                  <TableCell sx={{ color: colors.grey[100] }}>
                    {price.trialDays ? `${price.trialDays} ngày` : "-"}
                  </TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      icon={price.isActive ? <CheckCircleIcon /> : <CancelIcon />}
                      label={price.isActive ? "Hoạt động" : "Không"}
                      sx={{
                        backgroundColor: price.isActive
                          ? colors.greenAccent[600]
                          : colors.redAccent[600],
                        color: colors.grey[100],
                        fontWeight: "600",
                      }}
                    />
                  </TableCell>
                  <TableCell>
                    <Box display="flex" gap={1}>
                      <IconButton
                        onClick={() => handleOpenDialog(price)}
                        sx={{
                          color: colors.greenAccent[500],
                          "&:hover": {
                            backgroundColor: colors.greenAccent[800],
                          },
                        }}
                        size="small"
                      >
                        <EditOutlinedIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        onClick={() => handleDelete(price.priceID)}
                        sx={{
                          color: colors.redAccent[500],
                          "&:hover": {
                            backgroundColor: colors.redAccent[800],
                          },
                        }}
                        size="small"
                      >
                        <DeleteOutlineIcon fontSize="small" />
                      </IconButton>
                    </Box>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        <Box
          sx={{
            textAlign: "center",
            py: 4,
            backgroundColor: colors.primary[500],
            borderRadius: "8px",
          }}
        >
          Chưa có giá nào được tạo cho gói này
        </Box>
      )}

      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ backgroundColor: colors.primary[400], color: colors.grey[100] }}>
          {editingPrice ? "Chỉnh sửa giá tiền" : "Thêm giá tiền mới"}
        </DialogTitle>
        <DialogContent sx={{ backgroundColor: colors.primary[400], pt: 2 }}>
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
                    <Box sx={{ fontWeight: "600" }}>
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
            disabled={loading}
            variant="contained"
            sx={{
              backgroundColor: colors.greenAccent[600],
              "&:hover": {
                backgroundColor: colors.greenAccent[700],
              },
            }}
          >
            {loading ? "Đang lưu..." : "Lưu"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PriceManagement;