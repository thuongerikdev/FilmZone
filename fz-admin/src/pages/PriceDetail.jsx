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
import { getPriceById } from "../services/api";

const PriceDetail = () => {
  const { priceId } = useParams();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [price, setPrice] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchPriceDetail();
  }, [priceId]);

  const fetchPriceDetail = async () => {
    try {
      const response = await getPriceById(priceId);
      if (response.data.errorCode === 200) {
        setPrice(response.data.data);
      } else {
        setError("Không thể tải thông tin giá");
      }
    } catch (error) {
      console.error("Error fetching price:", error);
      setError("Có lỗi xảy ra khi tải thông tin giá");
    } finally {
      setLoading(false);
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

  if (loading) {
    return (
      <Box m="20px" display="flex" justifyContent="center" alignItems="center" height="60vh">
        <CircularProgress />
      </Box>
    );
  }

  if (error || !price) {
    return (
      <Box m="20px">
        <Header title="CHI TIẾT GIÁ" subtitle="Không tìm thấy thông tin" />
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate("/prices")}
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
          title="CHI TIẾT GIÁ" 
          subtitle={`ID: ${price.priceID}`} 
        />
        <Box display="flex" gap={2}>
          <Button
            startIcon={<EditOutlinedIcon />}
            onClick={() => navigate(`/prices/edit/${priceId}`)}
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
            onClick={() => navigate("/prices")}
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
                      ID Giá
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="600">
                      {price.priceID}
                    </Typography>
                  </Box>

                  <Box mb={2}>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={0.5}>
                      ID Gói dịch vụ
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="600">
                      {price.planID}
                    </Typography>
                  </Box>

                  <Box mb={2}>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={0.5}>
                      Giá tiền
                    </Typography>
                    <Typography 
                      variant="h4" 
                      color={colors.greenAccent[300]} 
                      fontWeight="700"
                    >
                      {formatCurrency(price.amount, price.currency)}
                    </Typography>
                  </Box>

                  <Box mb={2}>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={0.5}>
                      Tiền tệ
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="600">
                      {price.currency}
                    </Typography>
                  </Box>
                </Grid>

                <Grid item xs={12} md={6}>
                  <Box mb={2}>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={0.5}>
                      Chu kỳ thanh toán
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="600">
                      {formatInterval(price.intervalCount, price.intervalUnit)}
                    </Typography>
                  </Box>

                  <Box mb={2}>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={0.5}>
                      Thời gian dùng thử
                    </Typography>
                    <Typography variant="body1" color={colors.grey[100]} fontWeight="600">
                      {price.trialDays ? `${price.trialDays} ngày` : "Không có"}
                    </Typography>
                  </Box>

                  <Box mb={2}>
                    <Typography variant="subtitle2" color={colors.grey[300]} mb={0.5}>
                      Trạng thái
                    </Typography>
                    <Chip
                      icon={price.isActive ? <CheckCircleIcon /> : <CancelIcon />}
                      label={price.isActive ? "Đang hoạt động" : "Không hoạt động"}
                      sx={{
                        backgroundColor: price.isActive 
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
                    <Typography variant="h5" color={colors.grey[100]} mb={2} fontWeight="600">
                      Thông tin chi tiết
                    </Typography>
                    <Grid container spacing={2}>
                      <Grid item xs={12} md={4}>
                        <Box 
                          sx={{ 
                            p: 2, 
                            backgroundColor: colors.primary[500],
                            borderRadius: "8px"
                          }}
                        >
                          <Typography variant="subtitle2" color={colors.grey[300]}>
                            Đơn vị chu kỳ
                          </Typography>
                          <Typography variant="h6" color={colors.grey[100]} fontWeight="600">
                            {price.intervalUnit === "month" ? "Tháng" :
                             price.intervalUnit === "year" ? "Năm" :
                             price.intervalUnit === "week" ? "Tuần" : "Ngày"}
                          </Typography>
                        </Box>
                      </Grid>
                      <Grid item xs={12} md={4}>
                        <Box 
                          sx={{ 
                            p: 2, 
                            backgroundColor: colors.primary[500],
                            borderRadius: "8px"
                          }}
                        >
                          <Typography variant="subtitle2" color={colors.grey[300]}>
                            Số lượng chu kỳ
                          </Typography>
                          <Typography variant="h6" color={colors.grey[100]} fontWeight="600">
                            {price.intervalCount}
                          </Typography>
                        </Box>
                      </Grid>
                      <Grid item xs={12} md={4}>
                        <Box 
                          sx={{ 
                            p: 2, 
                            backgroundColor: colors.primary[500],
                            borderRadius: "8px"
                          }}
                        >
                          <Typography variant="subtitle2" color={colors.grey[300]}>
                            Số đơn hàng
                          </Typography>
                          <Typography variant="h6" color={colors.grey[100]} fontWeight="600">
                            {price.orders?.length || 0}
                          </Typography>
                        </Box>
                      </Grid>
                    </Grid>
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>

        {price.orders && price.orders.length > 0 && (
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
                  Có {price.orders.length} đơn hàng sử dụng giá này
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>
    </Box>
  );
};

export default PriceDetail;