import { useState, useEffect } from "react";
import { Box, Button, TextField, useTheme, Typography, Alert, MenuItem, Grid } from "@mui/material";
import { tokens } from "../theme";
import { getMe } from "../services/api";
import Header from "../components/Header";

const Profile = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  
  const [userData, setUserData] = useState({
    userID: "",
    userName: "",
    email: "",
    firstName: "",
    lastName: "",
    gender: "male",
    isEmailVerified: false,
    tokenExpiration: "",
    refreshTokenExpiration: "",
    sessionId: "",
    deviceId: "",
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  // Fetch user data khi component mount
  useEffect(() => {
    fetchUserData();
  }, []);

  const fetchUserData = async () => {
    try {
      const response = await getMe();
      if (response.data.errorCode === 200) {
        // Lấy thêm thông tin từ localStorage
        const localUser = JSON.parse(localStorage.getItem('user') || '{}');
        const sessionId = localStorage.getItem('sessionId');
        const deviceId = localStorage.getItem('deviceId');
        
        setUserData({
          ...response.data.data,
          tokenExpiration: localUser.tokenExpiration || "",
          refreshTokenExpiration: localUser.refreshTokenExpiration || "",
          sessionId: sessionId || "",
          deviceId: deviceId || "",
        });
      } else {
        setError("Không thể tải thông tin người dùng");
        loadFromLocalStorage();
      }
    } catch (err) {
      setError("Có lỗi xảy ra khi tải thông tin");
      loadFromLocalStorage();
    } finally {
      setLoading(false);
    }
  };

  const loadFromLocalStorage = () => {
    const localUser = JSON.parse(localStorage.getItem('user') || '{}');
    const sessionId = localStorage.getItem('sessionId');
    const deviceId = localStorage.getItem('deviceId');
    
    setUserData({
      userID: localUser.userID || "",
      userName: localUser.userName || "",
      email: localUser.email || "",
      firstName: localUser.firstName || "",
      lastName: localUser.lastName || "",
      gender: localUser.gender || "male",
      isEmailVerified: localUser.isEmailVerified || false,
      tokenExpiration: localUser.tokenExpiration || "",
      refreshTokenExpiration: localUser.refreshTokenExpiration || "",
      sessionId: sessionId || "",
      deviceId: deviceId || "",
    });
  };

  const handleChange = (e) => {
    setUserData({
      ...userData,
      [e.target.name]: e.target.value,
    });
    setError("");
    setSuccess("");
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError("");
    setSuccess("");

    try {
      // TODO: Gọi API cập nhật thông tin user khi có endpoint
      // const response = await updateUserProfile(userData);
      
      // Tạm thời update localStorage
      const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
      const updatedUser = { 
        ...currentUser,
        firstName: userData.firstName,
        lastName: userData.lastName,
        gender: userData.gender,
      };
      localStorage.setItem('user', JSON.stringify(updatedUser));
      
      setSuccess("Cập nhật thông tin thành công!");
    } catch (err) {
      setError("Có lỗi xảy ra khi cập nhật thông tin");
    } finally {
      setLoading(false);
    }
  };

  const formatDateTime = (dateString) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (loading && !userData.email) {
    return (
      <Box m="20px">
        <Header title="THÔNG TIN CÁ NHÂN" subtitle="Đang tải dữ liệu..." />
      </Box>
    );
  }

  return (
    <Box m="20px">
      <Header title="THÔNG TIN CÁ NHÂN" subtitle="Quản lý thông tin tài khoản của bạn" />

      <Grid container spacing={3}>
        {/* Thông tin chính */}
        <Grid item xs={12} md={8}>
          <Box
            sx={{
              backgroundColor: colors.primary[400],
              borderRadius: "12px",
              padding: "30px",
            }}
          >
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

            {/* Thông tin xác thực email */}
            <Box mb={3}>
              <Typography variant="h5" color={colors.grey[100]} mb={1} fontWeight="600">
                Trạng thái tài khoản
              </Typography>
              <Box
                sx={{
                  backgroundColor: userData.isEmailVerified 
                    ? colors.greenAccent[700] 
                    : colors.redAccent[700],
                  padding: "10px 15px",
                  borderRadius: "8px",
                  display: "inline-block",
                }}
              >
                <Typography color={colors.grey[100]}>
                  {userData.isEmailVerified 
                    ? "✓ Email đã được xác thực" 
                    : "⚠ Email chưa được xác thực"}
                </Typography>
              </Box>
            </Box>

            <Typography variant="h5" color={colors.grey[100]} mb={2} fontWeight="600">
              Thông tin cá nhân
            </Typography>

            <Box component="form" onSubmit={handleSubmit}>
              <Box display="grid" gap="20px" gridTemplateColumns="repeat(2, 1fr)">
                <TextField
                  fullWidth
                  variant="filled"
                  type="text"
                  label="User ID"
                  name="userID"
                  value={userData.userID}
                  disabled
                  sx={{ 
                    "& .MuiInputBase-input.Mui-disabled": {
                      WebkitTextFillColor: colors.grey[300],
                    }
                  }}
                />

                <TextField
                  fullWidth
                  variant="filled"
                  type="text"
                  label="Tên đăng nhập"
                  name="userName"
                  value={userData.userName}
                  disabled
                  sx={{ 
                    "& .MuiInputBase-input.Mui-disabled": {
                      WebkitTextFillColor: colors.grey[300],
                    }
                  }}
                />

                <TextField
                  fullWidth
                  variant="filled"
                  type="text"
                  label="Họ"
                  name="firstName"
                  value={userData.firstName}
                  onChange={handleChange}
                  required
                />

                <TextField
                  fullWidth
                  variant="filled"
                  type="text"
                  label="Tên"
                  name="lastName"
                  value={userData.lastName}
                  onChange={handleChange}
                  required
                />

                <TextField
                  fullWidth
                  variant="filled"
                  type="email"
                  label="Email"
                  name="email"
                  value={userData.email}
                  disabled
                  sx={{ 
                    gridColumn: "span 2",
                    "& .MuiInputBase-input.Mui-disabled": {
                      WebkitTextFillColor: colors.grey[300],
                    }
                  }}
                />

                <TextField
                  fullWidth
                  variant="filled"
                  select
                  label="Giới tính"
                  name="gender"
                  value={userData.gender}
                  onChange={handleChange}
                  sx={{ gridColumn: "span 2" }}
                >
                  <MenuItem value="male">Nam</MenuItem>
                  <MenuItem value="female">Nữ</MenuItem>
                  <MenuItem value="other">Khác</MenuItem>
                </TextField>
              </Box>

              <Box display="flex" justifyContent="flex-end" mt="30px" gap={2}>
                <Button
                  type="button"
                  variant="outlined"
                  onClick={fetchUserData}
                  sx={{
                    borderColor: colors.grey[400],
                    color: colors.grey[100],
                    "&:hover": {
                      borderColor: colors.grey[100],
                    },
                  }}
                >
                  Hủy thay đổi
                </Button>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={loading}
                  sx={{
                    backgroundColor: colors.greenAccent[600],
                    color: colors.grey[100],
                    fontSize: "14px",
                    fontWeight: "bold",
                    padding: "10px 20px",
                    "&:hover": {
                      backgroundColor: colors.greenAccent[700],
                    },
                  }}
                >
                  {loading ? "Đang lưu..." : "Lưu thay đổi"}
                </Button>
              </Box>
            </Box>

            {/* Phần bảo mật */}
            <Box mt={5}>
              <Typography variant="h5" color={colors.grey[100]} mb={2} fontWeight="600">
                Bảo mật
              </Typography>
              <Button
                variant="outlined"
                sx={{
                  borderColor: colors.blueAccent[500],
                  color: colors.blueAccent[500],
                  "&:hover": {
                    borderColor: colors.blueAccent[300],
                    backgroundColor: "rgba(104, 112, 250, 0.05)",
                  },
                }}
                onClick={() => {
                  // TODO: Navigate to change password page
                  alert("Chức năng đổi mật khẩu sẽ được bổ sung");
                }}
              >
                Đổi mật khẩu
              </Button>
            </Box>
          </Box>
        </Grid>

        {/* Thông tin phiên làm việc
        <Grid item xs={12} md={4}>
          <Box
            sx={{
              backgroundColor: colors.primary[400],
              borderRadius: "12px",
              padding: "20px",
            }}
          >
            <Typography variant="h5" color={colors.grey[100]} mb={3} fontWeight="600">
              Thông tin phiên
            </Typography>

            <Box mb={3}>
              <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                Session ID
              </Typography>
              <Typography variant="body1" color={colors.grey[100]} fontWeight="500">
                {userData.sessionId || "N/A"}
              </Typography>
            </Box>

            <Box mb={3}>
              <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                Device ID
              </Typography>
              <Typography 
                variant="body2" 
                color={colors.grey[100]} 
                sx={{ wordBreak: 'break-all' }}
              >
                {userData.deviceId || "N/A"}
              </Typography>
            </Box>

            <Box mb={3}>
              <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                Token hết hạn
              </Typography>
              <Typography variant="body2" color={colors.redAccent[400]}>
                {formatDateTime(userData.tokenExpiration)}
              </Typography>
            </Box>

            <Box mb={3}>
              <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                Refresh Token hết hạn
              </Typography>
              <Typography variant="body2" color={colors.greenAccent[400]}>
                {formatDateTime(userData.refreshTokenExpiration)}
              </Typography>
            </Box>

            <Button
              fullWidth
              variant="outlined"
              sx={{
                borderColor: colors.redAccent[500],
                color: colors.redAccent[500],
                mt: 2,
                "&:hover": {
                  borderColor: colors.redAccent[300],
                  backgroundColor: "rgba(219, 79, 74, 0.05)",
                },
              }}
              onClick={() => {
                // TODO: Đăng xuất tất cả phiên
                alert("Chức năng đăng xuất tất cả thiết bị sẽ được bổ sung");
              }}
            >
              Đăng xuất tất cả thiết bị
            </Button>
          </Box>
        </Grid> */}
      </Grid>
    </Box>
  );
};

export default Profile;