import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Box,
  Typography,
  useTheme,
  Grid,
  Card,
  CardContent,
  Chip,
  Button,
  Divider,
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import { getPersonById, deletePerson } from "../services/api";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import PersonIcon from "@mui/icons-material/Person";
import CakeIcon from "@mui/icons-material/Cake";
import PublicIcon from "@mui/icons-material/Public";
import DescriptionIcon from "@mui/icons-material/Description";
import CalendarTodayIcon from "@mui/icons-material/CalendarToday";

const PersonDetail = () => {
  const { personId } = useParams();
  const navigate = useNavigate();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  
  const [personData, setPersonData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchPersonDetail();
  }, [personId]);

  const fetchPersonDetail = async () => {
    try {
      const response = await getPersonById(personId);
      if (response.data.errorCode === 200) {
        setPersonData(response.data.data);
      }
    } catch (error) {
      console.error("Error fetching person detail:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (window.confirm("Bạn có chắc chắn muốn xóa diễn viên này?")) {
      try {
        const response = await deletePerson(personId);
        if (response.data.errorCode === 200) {
          alert("Xóa diễn viên thành công!");
          navigate("/persons");
        } else {
          alert(response.data.errorMessage || "Xóa diễn viên thất bại");
        }
      } catch (error) {
        console.error("Error deleting person:", error);
        alert("Có lỗi xảy ra khi xóa diễn viên");
      }
    }
  };

  const formatDateTime = (dateString) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleString('vi-VN');
  };

  const formatDate = (dateString) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const getRoleColor = (role) => {
    const roleLower = role.toLowerCase();
    if (roleLower.includes('director')) return colors.blueAccent[600];
    if (roleLower.includes('cast')) return colors.greenAccent[600];
    if (roleLower.includes('writer')) return colors.grey[600];
    return colors.grey[700];
  };

  if (loading) {
    return (
      <Box m="20px">
        <Header title="CHI TIẾT DIỄN VIÊN" subtitle="Đang tải dữ liệu..." />
      </Box>
    );
  }

  if (!personData) {
    return (
      <Box m="20px">
        <Header title="CHI TIẾT DIỄN VIÊN" subtitle="Không tìm thấy diễn viên" />
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate("/persons")}
          sx={{ color: colors.grey[100] }}
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
          title={personData.fullName}
          subtitle={`ID: ${personData.personID}`}
        />
        <Box display="flex" gap={2}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate("/persons")}
            sx={{
              color: colors.grey[100],
              borderColor: colors.grey[400],
            }}
            variant="outlined"
          >
            Quay lại
          </Button>
          <Button
            startIcon={<EditOutlinedIcon />}
            onClick={() => navigate(`/persons/edit/${personId}`)}
            sx={{
              color: colors.greenAccent[500],
              borderColor: colors.greenAccent[500],
              "&:hover": {
                borderColor: colors.greenAccent[300],
                backgroundColor: colors.greenAccent[900],
              },
            }}
            variant="outlined"
          >
            Chỉnh sửa
          </Button>
          <Button
            startIcon={<DeleteOutlineIcon />}
            onClick={handleDelete}
            sx={{
              color: colors.redAccent[500],
              borderColor: colors.redAccent[500],
              "&:hover": {
                borderColor: colors.redAccent[300],
                backgroundColor: colors.redAccent[900],
              },
            }}
            variant="outlined"
          >
            Xóa diễn viên
          </Button>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Avatar và thông tin cơ bản */}
        <Grid item xs={12} md={4}>
          <Card sx={{ backgroundColor: colors.primary[400] }}>
            <Box
              component="img"
              src={personData.avatar}
              alt={personData.fullName}
              sx={{
                width: "100%",
                height: "500px",
                objectFit: "cover",
              }}
            />
            <CardContent>
              <Box display="flex" flexDirection="column" gap={2}>
                <Box>
                  <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                    Vai trò chính
                  </Typography>
                  <Chip
                    label={personData.knownFor}
                    sx={{
                      backgroundColor: getRoleColor(personData.knownFor),
                      color: colors.grey[100],
                      fontWeight: "600",
                    }}
                  />
                </Box>

                <Box>
                  <Box display="flex" alignItems="center" gap={1} mb={0.5}>
                    <CakeIcon sx={{ color: colors.blueAccent[500], fontSize: "20px" }} />
                    <Typography variant="body2" color={colors.grey[300]}>
                      Ngày sinh
                    </Typography>
                  </Box>
                  <Typography variant="h6" color={colors.grey[100]} fontWeight="600">
                    {formatDate(personData.birthDate)}
                  </Typography>
                </Box>

                <Box>
                  <Box display="flex" alignItems="center" gap={1} mb={0.5}>
                    <PublicIcon sx={{ color: colors.greenAccent[500], fontSize: "20px" }} />
                    <Typography variant="body2" color={colors.grey[300]}>
                      Region ID
                    </Typography>
                  </Box>
                  <Typography variant="h6" color={colors.grey[100]} fontWeight="600">
                    {personData.regionID}
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Thông tin chi tiết */}
        <Grid item xs={12} md={8}>
          {/* Tiểu sử */}
          <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
            <CardContent>
              <Box display="flex" alignItems="center" mb={2}>
                <DescriptionIcon sx={{ color: colors.greenAccent[500], mr: 1 }} />
                <Typography variant="h5" fontWeight="600" color={colors.grey[100]}>
                  Tiểu sử
                </Typography>
              </Box>
              <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
              <Typography 
                variant="body1" 
                color={colors.grey[200]} 
                lineHeight={1.8}
                sx={{ whiteSpace: "pre-wrap" }}
              >
                {personData.biography || "Chưa có thông tin tiểu sử"}
              </Typography>
            </CardContent>
          </Card>

          {/* Thông tin hệ thống */}
          <Card sx={{ backgroundColor: colors.primary[400] }}>
            <CardContent>
              <Box display="flex" alignItems="center" mb={2}>
                <CalendarTodayIcon sx={{ color: colors.blueAccent[500], mr: 1 }} />
                <Typography variant="h5" fontWeight="600" color={colors.grey[100]}>
                  Thông tin hệ thống
                </Typography>
              </Box>
              <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
              
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Box>
                    <Typography variant="body2" color={colors.grey[300]}>
                      Person ID
                    </Typography>
                    <Typography variant="h6" color={colors.grey[100]} fontWeight="500">
                      {personData.personID}
                    </Typography>
                  </Box>
                </Grid>

                <Grid item xs={12} md={6}>
                  <Box>
                    <Typography variant="body2" color={colors.grey[300]}>
                      Region ID
                    </Typography>
                    <Typography variant="h6" color={colors.grey[100]} fontWeight="500">
                      {personData.regionID}
                    </Typography>
                  </Box>
                </Grid>

                <Grid item xs={12} md={6}>
                  <Box>
                    <Typography variant="body2" color={colors.grey[300]}>
                      Ngày tạo
                    </Typography>
                    <Typography variant="body1" color={colors.greenAccent[400]} fontWeight="500">
                      {formatDateTime(personData.createdAt)}
                    </Typography>
                  </Box>
                </Grid>

                <Grid item xs={12} md={6}>
                  <Box>
                    <Typography variant="body2" color={colors.grey[300]}>
                      Cập nhật lần cuối
                    </Typography>
                    <Typography variant="body1" color={colors.blueAccent[400]} fontWeight="500">
                      {formatDateTime(personData.updatedAt)}
                    </Typography>
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>

          {/* Credits (nếu có) */}
          {personData.credits && personData.credits.length > 0 && (
            <Card sx={{ backgroundColor: colors.primary[400], mt: 3 }}>
              <CardContent>
                <Box display="flex" alignItems="center" mb={2}>
                  <PersonIcon sx={{ color: colors.blueAccent[500], mr: 1 }} />
                  <Typography variant="h5" fontWeight="600" color={colors.grey[100]}>
                    Tham gia phim ({personData.credits.length})
                  </Typography>
                </Box>
                <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
                <Grid container spacing={2}>
                  {personData.credits.map((credit, index) => (
                    <Grid item xs={12} sm={6} md={4} key={index}>
                      <Box
                        sx={{
                          backgroundColor: colors.primary[500],
                          padding: "15px",
                          borderRadius: "8px",
                          cursor: "pointer",
                          transition: "all 0.3s",
                          "&:hover": {
                            backgroundColor: colors.primary[600],
                            transform: "translateY(-2px)",
                          },
                        }}
                      >
                        <Typography variant="body1" color={colors.grey[100]} fontWeight="600">
                          {credit.movieTitle || "Untitled"}
                        </Typography>
                        <Typography variant="body2" color={colors.grey[300]}>
                          {credit.role || "N/A"}
                        </Typography>
                        {credit.characterName && (
                          <Typography variant="caption" color={colors.grey[400]}>
                            as {credit.characterName}
                          </Typography>
                        )}
                      </Box>
                    </Grid>
                  ))}
                </Grid>
              </CardContent>
            </Card>
          )}
        </Grid>
      </Grid>
    </Box>
  );
};

export default PersonDetail;