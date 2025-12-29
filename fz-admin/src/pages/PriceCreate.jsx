import { useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import {
  Box,
  useTheme,
  Button,
  TextField,
  Grid,
  Card,
  CardContent,
  Alert,
  FormControlLabel,
  Switch,
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { createPrice, getAllPlans } from "../services/api";
import { useEffect } from "react";

const PriceCreate = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const planIdFromParams = searchParams.get("planId");

  const [formData, setFormData] = useState({
    planID: planIdFromParams ? parseInt(planIdFromParams) : 0,
    currency: "VND",
    amount: 0,
    intervalUnit: "month",
    intervalCount: 1,
    trialDays: 0,
    isActive: true,
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const intervalUnits = [
    { value: "day", label: "Ngày" },
    { value: "week", label: "Tuần" },
    { value: "month", label: "Tháng" },
    { value: "year", label: "Năm" },
  ];

  const currencies = ["VND", "USD", "EUR"];

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]:
        name === "isActive"
          ? e.target.checked
          : ["amount", "intervalCount", "trialDays", "planID"].includes(name)
          ? Number(value)
          : value,
    });
    setError("");
    setSuccess("");
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    setSuccess("");

    if (!formData.planID) {
      setError("Vui lòng chọn gói dịch vụ");
      setSubmitting(false);
      return;
    }

    if (!formData.amount || formData.amount <= 0) {
      setError("Giá tiền phải lớn hơn 0");
      setSubmitting(false);
      return;
    }

    if (formData.intervalCount <= 0) {
      setError("Số kỳ phải lớn hơn 0");
      setSubmitting(false);
      return;
    }

    try {
      const response = await createPrice(formData);

      if (response.data.errorCode === 200) {
        setSuccess("Tạo giá tiền thành công!");
        setTimeout(
          () => navigate(planIdFromParams ? `/plans/${planIdFromParams}` : "/prices"),
          2000
        );
      } else {
        setError(response.data.errorMessage || "Tạo giá tiền thất bại");
      }
    } catch (err) {
      console.error("Error creating price:", err);
      setError(err.response?.data?.errorMessage || "Có lỗi xảy ra khi tạo giá tiền");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Header
          title="THÊM GIÁ MỚI"
          subtitle="Tạo giá tiền mới cho gói dịch vụ"
        />
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() =>
            navigate(planIdFromParams ? `/plans/${planIdFromParams}` : "/prices")
          }
          sx={{
            color: colors.grey[100],
            borderColor: colors.grey[400],
          }}
          variant="outlined"
        >
          Quay lại
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 3 }}>
          {success}
        </Alert>
      )}

      <Card sx={{ backgroundColor: colors.primary[400] }}>
        <CardContent>
          <Box component="form" onSubmit={handleSubmit}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Gói dịch vụ *"
                  name="planID"
                  type="number"
                  value={formData.planID}
                  onChange={handleChange}
                  required
                  helperText={
                    planIdFromParams
                      ? `Gói ID: ${planIdFromParams}`
                      : "Nhập ID của gói dịch vụ"
                  }
                  disabled={!!planIdFromParams}
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <TextField
                  select
                  fullWidth
                  variant="filled"
                  label="Tiền tệ *"
                  name="currency"
                  value={formData.currency}
                  onChange={handleChange}
                  required
                  SelectProps={{
                    native: true,
                  }}
                >
                  {currencies.map((curr) => (
                    <option key={curr} value={curr}>
                      {curr}
                    </option>
                  ))}
                </TextField>
              </Grid>

              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Giá tiền *"
                  name="amount"
                  type="number"
                  value={formData.amount}
                  onChange={handleChange}
                  required
                  inputProps={{ min: 0, step: "0.01" }}
                  helperText={`VD: 99000 cho ${formData.currency}`}
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <TextField
                  select
                  fullWidth
                  variant="filled"
                  label="Chu kỳ *"
                  name="intervalUnit"
                  value={formData.intervalUnit}
                  onChange={handleChange}
                  required
                  SelectProps={{
                    native: true,
                  }}
                >
                  {intervalUnits.map((unit) => (
                    <option key={unit.value} value={unit.value}>
                      {unit.label}
                    </option>
                  ))}
                </TextField>
              </Grid>

              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Số kỳ *"
                  name="intervalCount"
                  type="number"
                  value={formData.intervalCount}
                  onChange={handleChange}
                  required
                  inputProps={{ min: 1 }}
                  helperText="VD: 1 tháng, 3 tháng, 12 tháng"
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Ngày dùng thử"
                  name="trialDays"
                  type="number"
                  value={formData.trialDays}
                  onChange={handleChange}
                  inputProps={{ min: 0 }}
                  helperText="Nhập 0 nếu không có dùng thử"
                />
              </Grid>

              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={formData.isActive}
                      onChange={handleChange}
                      name="isActive"
                      sx={{
                        "& .MuiSwitch-switchBase.Mui-checked": {
                          color: colors.greenAccent[500],
                        },
                        "& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track": {
                          backgroundColor: colors.greenAccent[600],
                        },
                      }}
                    />
                  }
                  label={
                    <Box>
                      <Box sx={{ fontWeight: "600", color: colors.grey[100] }}>
                        Trạng thái: {formData.isActive ? "Hoạt động" : "Không hoạt động"}
                      </Box>
                      <Box sx={{ fontSize: "12px", color: colors.grey[400] }}>
                        {formData.isActive
                          ? "Giá này sẽ hiển thị cho người dùng"
                          : "Giá này sẽ bị ẩn khỏi danh sách"}
                      </Box>
                    </Box>
                  }
                />
              </Grid>

              <Grid item xs={12}>
                <Box display="flex" justifyContent="flex-end" gap={2}>
                  <Button
                    variant="outlined"
                    onClick={() =>
                      navigate(
                        planIdFromParams ? `/plans/${planIdFromParams}` : "/prices"
                      )
                    }
                    sx={{
                      borderColor: colors.grey[400],
                      color: colors.grey[100],
                    }}
                  >
                    Hủy
                  </Button>
                  <Button
                    type="submit"
                    variant="contained"
                    disabled={submitting}
                    sx={{
                      backgroundColor: colors.greenAccent[600],
                      color: colors.grey[100],
                      fontSize: "14px",
                      fontWeight: "bold",
                      "&:hover": {
                        backgroundColor: colors.greenAccent[700],
                      },
                    }}
                  >
                    {submitting ? "Đang tạo..." : "Tạo giá tiền"}
                  </Button>
                </Box>
              </Grid>
            </Grid>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default PriceCreate;