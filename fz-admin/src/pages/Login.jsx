import { useState } from "react";
import { Box, Button, TextField, Typography, useTheme, IconButton, InputAdornment, Alert } from "@mui/material";
import { Visibility, VisibilityOff, Google } from "@mui/icons-material";
import { useNavigate } from "react-router-dom";
import { tokens } from "../theme";
import { userLogin, googleLogin } from "../services/api";

const Login = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();
  
  const [formData, setFormData] = useState({
    userName: "",
    password: "",
  });
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
    setError("");
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      const response = await userLogin(formData);
      
      if (response.data.errorCode === 200) {
        const userData = response.data.data;
        
        // Lưu token, refreshToken và user info vào localStorage
        localStorage.setItem("token", userData.token);
        localStorage.setItem("tokenExpiration", userData.tokenExpiration);
        localStorage.setItem("refreshToken", userData.refreshToken);
        localStorage.setItem("refreshTokenExpiration", userData.refreshTokenExpiration);
        localStorage.setItem("sessionId", userData.sessionId);
        localStorage.setItem("deviceId", userData.deviceId);
        
        // Lưu permissions vào localStorage
        if (userData.permissions) {
          localStorage.setItem("permissions", JSON.stringify(userData.permissions));
        }
        
        localStorage.setItem("user", JSON.stringify({
          userID: userData.userID,
          userName: userData.userName,
          email: userData.email,
          isEmailVerified: userData.isEmailVerified,
          tokenExpiration: userData.tokenExpiration,
          refreshTokenExpiration: userData.refreshTokenExpiration
        }));

        const isAdmin = userData.roles?.some(role => role.roleName === 'admin');

        if (isAdmin) {
            localStorage.setItem("isAdmin", "true");
        } else {
            localStorage.removeItem("isAdmin"); 
        }
        
        // Kiểm tra nếu cần MFA
        if (userData.requiresMfa && userData.mfaTicket) {
          // TODO: Navigate to MFA verification page
          alert("Vui lòng xác thực MFA");
          return;
        }
        
        // Chuyển về dashboard
        navigate("/");
      } else {
        setError(response.data.errorMessage || "Đăng nhập thất bại");
      }
    } catch (err) {
      setError(err.response?.data?.errorMessage || "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.");
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleLogin = async () => {
    try {
      const response = await googleLogin();
      if (response.data?.redirectUrl) {
        window.location.href = response.data.redirectUrl;
      }
    } catch (err) {
      setError("Không thể đăng nhập bằng Google");
    }
  };

  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="100vh"
      sx={{
        background: `linear-gradient(135deg, ${colors.primary[400]} 0%, ${colors.primary[500]} 100%)`,
      }}
    >
      <Box
        sx={{
          backgroundColor: colors.primary[400],
          borderRadius: "12px",
          padding: "40px",
          width: "100%",
          maxWidth: "450px",
          boxShadow: "0 8px 32px rgba(0, 0, 0, 0.3)",
        }}
      >
        <Typography
          variant="h2"
          color={colors.grey[100]}
          fontWeight="bold"
          textAlign="center"
          mb={1}
        >
          Đăng Nhập
        </Typography>
        <Typography
          variant="h5"
          color={colors.grey[300]}
          textAlign="center"
          mb={4}
        >
          Chào mừng trở lại FilmZone
        </Typography>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <Box component="form" onSubmit={handleSubmit}>
          <TextField
            fullWidth
            variant="filled"
            type="text"
            label="Tên đăng nhập"
            name="userName"
            value={formData.userName}
            onChange={handleChange}
            required
            sx={{ mb: 2 }}
          />

          <TextField
            fullWidth
            variant="filled"
            type={showPassword ? "text" : "password"}
            label="Mật khẩu"
            name="password"
            value={formData.password}
            onChange={handleChange}
            required
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    onClick={() => setShowPassword(!showPassword)}
                    edge="end"
                  >
                    {showPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
            sx={{ mb: 3 }}
          />

          <Button
            type="submit"
            fullWidth
            variant="contained"
            disabled={loading}
            sx={{
              backgroundColor: colors.greenAccent[600],
              color: colors.grey[100],
              fontSize: "16px",
              fontWeight: "bold",
              padding: "12px",
              "&:hover": {
                backgroundColor: colors.greenAccent[700],
              },
              mb: 2,
            }}
          >
            {loading ? "Đang đăng nhập..." : "Đăng Nhập"}
          </Button>

          {/* <Button
            fullWidth
            variant="outlined"
            startIcon={<Google />}
            onClick={handleGoogleLogin}
            sx={{
              borderColor: colors.grey[300],
              color: colors.grey[100],
              padding: "12px",
              "&:hover": {
                borderColor: colors.grey[100],
                backgroundColor: "rgba(255, 255, 255, 0.05)",
              },
              mb: 2,
            }}
          >
            Đăng nhập bằng Google
          </Button> */}

          {/* <Box display="flex" justifyContent="center" mt={2}>
            <Typography variant="body1" color={colors.grey[300]}>
              Chưa có tài khoản?{" "}
              <Typography
                component="span"
                sx={{
                  color: colors.greenAccent[500],
                  cursor: "pointer",
                  fontWeight: "bold",
                  "&:hover": {
                    textDecoration: "underline",
                  },
                }}
                onClick={() => navigate("/register")}
              >
                Đăng ký ngay
              </Typography>
            </Typography>
          </Box> */}
        </Box>
      </Box>
    </Box>
  );
};

export default Login;