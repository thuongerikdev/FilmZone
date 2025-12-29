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
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import { getPlanById } from "../services/api";
import PriceManagement from "../components/PriceManagement";

const PlanDetail = () => {
  const { planId } = useParams();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [plan, setPlan] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchPlanDetail();
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

  const handlePricesUpdate = () => {
    fetchPlanDetail();
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
              <Typography 
                variant="h4" 
                sx={{ mb: 3, color: colors.greenAccent[400], fontWeight: "bold" }}
              >
                Quản lý giá tiền ({plan.prices?.length || 0})
              </Typography>
              
              <PriceManagement 
                planId={plan.planID} 
                prices={plan.prices || []}
                onPricesUpdate={handlePricesUpdate}
              />
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
    </Box>
  );
};

export default PlanDetail;