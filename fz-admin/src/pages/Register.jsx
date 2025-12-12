import { useState } from "react";
import { Box, Button, TextField, Typography, useTheme, IconButton, InputAdornment, Alert, MenuItem } from "@mui/material";
import { Visibility, VisibilityOff, Email as EmailIcon } from "@mui/icons-material";
import { useNavigate } from "react-router-dom";
import { tokens } from "../theme";
import { register, verifyRegisterEmail } from "../services/api";

const Register = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();
  
  const [step, setStep] = useState(1); // 1: Register, 2: Verify Email
  const [formData, setFormData] = useState({
    userName: "",
    email: "",
    password: "",
    confirmPassword: "",
    firstName: "",
    lastName: "",
    gender: "male",
  });
  const [userID, setUserID] = useState(null);
  const [verifyToken, setVerifyToken] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
    setError("");
  };

  const handleRegisterSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    // Validate password match
    if (formData.password !== formData.confirmPassword) {
      setError("Mật khẩu xác nhận không khớp");
      setLoading(false);
      return;
    }

    // Validate password length
    if (formData.password.length < 6) {
      setError("Mật khẩu phải có ít nhất 6 ký tự");
      setLoading(false);
      return;
    }

    try {
      const response = await register({
        userName: formData.userName,
        email: formData.email,
        password: formData.password,
        firstName: formData.firstName,
        lastName: formData.lastName,
        gender: formData.gender,
      });

      if (response.data.errorCode === 200) {
        setUserID(response.data.data.userID);
        setStep(2); // Chuyển sang bước verify email
      } else {
        setError(response.data.errorMessage || "Đăng ký thất bại");
      }
    } catch (err) {
      setError(err.response?.data?.errorMessage || "Đăng ký thất bại. Email hoặc username có thể đã được sử dụng.");
    } finally {
      setLoading(false);
    }
  };

  const handleVerifySubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      const response = await verifyRegisterEmail({
        userID: userID,
        token: verifyToken,
      });

      if (response.data.errorCode === 200 && response.data.data === true) {
        // Thành công, chuyển về login
        navigate("/login");
      } else {
        setError(response.data.errorMessage || "Mã xác thực không đúng");
      }
    } catch (err) {
      setError(err.response?.data?.errorMessage || "Mã xác thực không đúng");
    } finally {
      setLoading(false);
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
          maxWidth: "500px",
          boxShadow: "0 8px 32px rgba(0, 0, 0, 0.3)",
        }}
      >
        {step === 1 ? (
          <>
            <Typography
              variant="h2"
              color={colors.grey[100]}
              fontWeight="bold"
              textAlign="center"
              mb={1}
            >
              Đăng Ký
            </Typography>
            <Typography
              variant="h5"
              color={colors.grey[300]}
              textAlign="center"
              mb={4}
            >
              Tạo tài khoản FilmZone mới
            </Typography>

            {error && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleRegisterSubmit}>
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

              <Box display="grid" gridTemplateColumns="1fr 1fr" gap={2} mb={2}>
                <TextField
                  fullWidth
                  variant="filled"
                  type="text"
                  label="Họ"
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                  required
                />
                <TextField
                  fullWidth
                  variant="filled"
                  type="text"
                  label="Tên"
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                  required
                />
              </Box>

              <TextField
                fullWidth
                variant="filled"
                type="email"
                label="Email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                required
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                variant="filled"
                select
                label="Giới tính"
                name="gender"
                value={formData.gender}
                onChange={handleChange}
                sx={{ mb: 2 }}
              >
                <MenuItem value="male">Nam</MenuItem>
                <MenuItem value="female">Nữ</MenuItem>
                <MenuItem value="other">Khác</MenuItem>
              </TextField>

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
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                variant="filled"
                type={showConfirmPassword ? "text" : "password"}
                label="Xác nhận mật khẩu"
                name="confirmPassword"
                value={formData.confirmPassword}
                onChange={handleChange}
                required
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        edge="end"
                      >
                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
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
                {loading ? "Đang xử lý..." : "Đăng Ký"}
              </Button>

              <Box display="flex" justifyContent="center" mt={2}>
                <Typography variant="body1" color={colors.grey[300]}>
                  Đã có tài khoản?{" "}
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
                    onClick={() => navigate("/login")}
                  >
                    Đăng nhập ngay
                  </Typography>
                </Typography>
              </Box>
            </Box>
          </>
        ) : (
          <>
            <Box textAlign="center" mb={4}>
              <Box
                sx={{
                  width: "80px",
                  height: "80px",
                  backgroundColor: colors.greenAccent[600] + "20",
                  borderRadius: "50%",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  margin: "0 auto 20px",
                }}
              >
                <EmailIcon sx={{ fontSize: "40px", color: colors.greenAccent[500] }} />
              </Box>
              <Typography
                variant="h2"
                color={colors.grey[100]}
                fontWeight="bold"
                mb={1}
              >
                Xác Thực Email
              </Typography>
              <Typography variant="h5" color={colors.grey[300]}>
                Nhập mã xác thực đã gửi đến
              </Typography>
              <Typography
                variant="h5"
                color={colors.greenAccent[500]}
                fontWeight="bold"
              >
                {formData.email}
              </Typography>
            </Box>

            {error && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleVerifySubmit}>
              <TextField
                fullWidth
                variant="filled"
                type="text"
                label="Mã xác thực (6 số)"
                value={verifyToken}
                onChange={(e) => {
                  setVerifyToken(e.target.value);
                  setError("");
                }}
                required
                inputProps={{ maxLength: 6 }}
                sx={{ 
                  mb: 3,
                  "& input": {
                    textAlign: "center",
                    fontSize: "24px",
                    letterSpacing: "8px",
                  }
                }}
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
                {loading ? "Đang xác thực..." : "Xác Thực"}
              </Button>

              <Button
                fullWidth
                variant="text"
                onClick={() => {
                  setStep(1);
                  setError("");
                }}
                sx={{
                  color: colors.grey[300],
                  "&:hover": {
                    backgroundColor: "rgba(255, 255, 255, 0.05)",
                  },
                }}
              >
                Quay lại
              </Button>
            </Box>
          </>
        )}
      </Box>
    </Box>
  );
};

export default Register;