import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
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
  CircularProgress,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { getPlanById, updatePlan, getAllScopeUser } from "../services/api";

const PlanEdit = () => {
  const { planId } = useParams();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    planID: "",
    code: "",
    name: "",
    description: "",
    isActive: true,
    roleID: "", // Thêm roleID vào formData
  });

  const [roles, setRoles] = useState([]); // State lưu danh sách vai trò
  const [loadingRoles, setLoadingRoles] = useState(true); // State loading cho vai trò
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    const initData = async () => {
      await fetchRoles();
      await fetchPlanDetail();
    };
    initData();
  }, [planId]);

  // Hàm lấy danh sách vai trò (giống PlanCreate)
  const fetchRoles = async () => {
    try {
      setLoadingRoles(true);
      const response = await getAllScopeUser();
      if (response.data.errorCode === 200) {
        // Lọc các vai trò có scope là "user"
        const userRoles = response.data.data.filter(role => role.scope === "user");
        setRoles(userRoles);
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

  const fetchPlanDetail = async () => {
    try {
      const response = await getPlanById(planId);
      if (response.data.errorCode === 200) {
        const data = response.data.data;
        setFormData({
          planID: data.planID,
          code: data.code,
          name: data.name,
          description: data.description || "",
          isActive: data.isActive,
          roleID: data.roleID, // Lấy roleID từ API detail
        });
      }
    } catch (error) {
      console.error("Error fetching plan:", error);
      setError("Không thể tải thông tin gói dịch vụ");
    } finally {
      setLoading(false);
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
      const response = await updatePlan(formData);

      if (response.data.errorCode === 200) {
        setSuccess("Cập nhật gói dịch vụ thành công!");
        setTimeout(() => navigate("/plans"), 2000);
      } else {
        setError(response.data.errorMessage || "Cập nhật gói dịch vụ thất bại");
      }
    } catch (err) {
      console.error("Error updating plan:", err);
      setError(err.response?.data?.errorMessage || "Có lỗi xảy ra khi cập nhật gói dịch vụ");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <Box m="20px" display="flex" justifyContent="center" alignItems="center" height="60vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Header 
          title="CHỈNH SỬA GÓI DỊCH VỤ" 
          subtitle={`ID: ${planId} - ${formData.name}`} 
        />
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
              />

              <TextField
                fullWidth
                variant="filled"
                label="Tên gói *"
                name="name"
                value={formData.name}
                onChange={handleChange}
                required
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

              {/* Phần chọn vai trò được thêm vào */}
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
                    required
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
                          ? "Gói này sẽ hiển thị cho người dùng" 
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
                  {submitting ? "Đang cập nhật..." : "Cập nhật"}
                </Button>
              </Box>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default PlanEdit;