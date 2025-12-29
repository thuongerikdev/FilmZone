import { useState } from "react";
import { useNavigate } from "react-router-dom";
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
import { createPlan } from "../services/api";

const PlanCreate = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    code: "",
    name: "",
    description: "",
    isActive: true,
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
    setError("");
    setSuccess("");
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    setSuccess("");

    try {
      const response = await createPlan(formData);

      if (response.data.errorCode === 200) {
        setSuccess("Tạo gói dịch vụ thành công!");
        setTimeout(() => navigate("/plans"), 2000);
      } else {
        setError(response.data.errorMessage || "Tạo gói dịch vụ thất bại");
      }
    } catch (err) {
      console.error("Error creating plan:", err);
      setError(err.response?.data?.errorMessage || "Có lỗi xảy ra khi tạo gói dịch vụ");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Header title="THÊM GÓI DỊCH VỤ MỚI" subtitle="Tạo gói dịch vụ mới trong hệ thống" />
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
                  label="Mã gói *"
                  name="code"
                  value={formData.code}
                  onChange={handleChange}
                  required
                  helperText="VD: 001, 002, PREMIUM"
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Tên gói *"
                  name="name"
                  value={formData.name}
                  onChange={handleChange}
                  required
                  helperText="VD: Basic, Premium, Cinematic"
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Mô tả"
                  name="description"
                  value={formData.description}
                  onChange={handleChange}
                  multiline
                  rows={6}
                  placeholder="Nhập mô tả chi tiết về gói dịch vụ..."
                />
              </Grid>

              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={formData.isActive}
                      onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
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
                        Trạng thái: {formData.isActive ? "Đang hoạt động" : "Không hoạt động"}
                      </Box>
                      <Box sx={{ fontSize: "12px", color: colors.grey[400] }}>
                        {formData.isActive 
                          ? "Gói này sẽ hiển thị cho người dùng" 
                          : "Gói này sẽ bị ẩn khỏi danh sách"}
                      </Box>
                    </Box>
                  }
                />
              </Grid>

              <Grid item xs={12}>
                <Box display="flex" justifyContent="flex-end" gap={2}>
                  <Button
                    variant="outlined"
                    onClick={() => navigate("/plans")}
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
                    {submitting ? "Đang tạo..." : "Tạo gói dịch vụ"}
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

export default PlanCreate;