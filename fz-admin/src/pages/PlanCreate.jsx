import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box,
  useTheme,
  Button,
  TextField,
  Card,
  CardContent,
  Alert,
  FormControlLabel,
  Switch,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  CircularProgress,
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { createPlan, getAllRoles } from "../services/api";

const PlanCreate = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    code: "",
    name: "",
    description: "",
    isActive: true,
    roleID: 11,
  });

  const [roles, setRoles] = useState([]);
  const [loadingRoles, setLoadingRoles] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  // Fetch roles on component mount
  useEffect(() => {
    fetchRoles();
  }, []);

  const fetchRoles = async () => {
    try {
      setLoadingRoles(true);
      const response = await getAllRoles();
      
      if (response.data.errorCode === 200) {
        // Filter roles with scope "user"
        const userRoles = response.data.data.filter(role => role.scope === "user");
        setRoles(userRoles);
        
        // Set default roleID to first user role if it exists
        if (userRoles.length > 0) {
          setFormData(prev => ({
            ...prev,
            roleID: userRoles[0].roleID
          }));
        }
      } else {
        setError("Không thể lấy danh sách vai trò");
      }
    } catch (err) {
      console.error("Error fetching roles:", err);
      setError("Có lỗi xảy ra khi lấy danh sách vai trò");
    } finally {
      setLoadingRoles(false);
    }
  };

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

      <Card sx={{ backgroundColor: colors.primary[400], maxWidth: 600, margin: "0 auto" }}>
        <CardContent>
          <Box component="form" onSubmit={handleSubmit}>
            <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
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

              <FormControl variant="filled" fullWidth>
                <InputLabel>Vai trò *</InputLabel>
                {loadingRoles ? (
                  <Box sx={{ display: "flex", alignItems: "center", justifyContent: "center", py: 2 }}>
                    <CircularProgress size={24} />
                  </Box>
                ) : (
                  <Select
                    name="roleID"
                    value={formData.roleID}
                    onChange={handleChange}
                  >
                    {roles.map((role) => (
                      <MenuItem key={role.roleID} value={role.roleID}>
                        {role.roleDescription} ({role.roleName})
                      </MenuItem>
                    ))}
                  </Select>
                )}
              </FormControl>

              <Box sx={{ py: 2, px: 2, backgroundColor: colors.primary[300], borderRadius: 1 }}>
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
                      <Box sx={{ fontSize: "12px", color: colors.grey[400], mt: 0.5 }}>
                        {formData.isActive 
                          ? "Gói này sẽ hiện thị cho người dùng" 
                          : "Gói này sẽ bị ẩn khỏi danh sách"}
                      </Box>
                    </Box>
                  }
                />
              </Box>

              <Box display="flex" gap={2} justifyContent="flex-end" sx={{ mt: 2 }}>
                <Button
                  variant="outlined"
                  onClick={() => navigate("/plans")}
                  sx={{
                    borderColor: colors.grey[400],
                    color: colors.grey[100],
                    flex: 1,
                  }}
                >
                  Hủy
                </Button>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={submitting || loadingRoles}
                  sx={{
                    backgroundColor: colors.greenAccent[600],
                    color: colors.grey[100],
                    fontSize: "14px",
                    fontWeight: "bold",
                    flex: 1,
                    "&:hover": {
                      backgroundColor: colors.greenAccent[700],
                    },
                  }}
                >
                  {submitting ? "Đang tạo..." : "Tạo gói dịch vụ"}
                </Button>
              </Box>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default PlanCreate;