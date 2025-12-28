import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box,
  Typography,
  useTheme,
  Button,
  TextField,
  Card,
  CardContent,
  Alert,
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";

const PersonCreate = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [fullName, setFullName] = useState("");
  const [biography, setBiography] = useState("");
  const [avatar, setAvatar] = useState(null);
  const [avatarPreview, setAvatarPreview] = useState(null);
  const [birthDate, setBirthDate] = useState("");

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const handleAvatarChange = (e) => {
    const file = e.target.files?.[0] || null;
    setAvatar(file);
    
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatarPreview(reader.result);
      };
      reader.readAsDataURL(file);
    } else {
      setAvatarPreview(null);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    setSuccess("");

    if (!avatar) {
      setError("Avatar là bắt buộc");
      setSubmitting(false);
      return;
    }

    if (!fullName.trim()) {
      setError("Họ và tên là bắt buộc");
      setSubmitting(false);
      return;
    }

    try {
      const formData = new FormData();
      formData.append("fullName", fullName);
      formData.append("knownFor", "cast");
      formData.append("biography", biography);
      formData.append("regionID", "1");
      formData.append("avatar", avatar);
      
      if (birthDate) {
        const date = new Date(birthDate);
        formData.append("birthDate", date.toISOString());
      }

      console.log("Submitting person:", {
        fullName,
        biography,
        avatar: avatar.name,
        birthDate,
      });

      const response = await fetch(
        "https://filmzone-api.koyeb.app/movie/Person/CreatePerson",
        {
          method: "POST",
          body: formData,
        }
      );

      const data = await response.json();

      if (data.errorCode === 200) {
        setSuccess("Tạo diễn viên thành công!");
        setTimeout(() => navigate("/persons"), 2000);
      } else {
        setError(data.errorMessage || "Tạo diễn viên thất bại");
      }
    } catch (err) {
      console.error("Error creating person:", err);
      setError(err.message || "Có lỗi xảy ra khi tạo diễn viên");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Header title="THÊM DIỄN VIÊN MỚI" subtitle="Tạo diễn viên mới trong hệ thống" />
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

      <Card sx={{ backgroundColor: colors.primary[400], maxWidth: 600, mx: "auto" }}>
        <CardContent>
          <Box component="form" onSubmit={handleSubmit} display="flex" flexDirection="column" gap={3}>
            
            {/* Avatar Preview */}
            {avatarPreview && (
              <Box
                sx={{
                  width: "100%",
                  display: "flex",
                  justifyContent: "center",
                  mb: 2,
                }}
              >
                <Box
                  component="img"
                  src={avatarPreview}
                  alt="Avatar preview"
                  sx={{
                    width: 200,
                    height: 250,
                    objectFit: "cover",
                    borderRadius: "8px",
                    border: `2px solid ${colors.greenAccent[500]}`,
                  }}
                />
              </Box>
            )}

            {/* Full Name */}
            <TextField
              fullWidth
              variant="filled"
              label="Họ và tên *"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              required
              placeholder="Nhập họ và tên diễn viên"
            />

            {/* Birth Date */}
            <TextField
              fullWidth
              variant="filled"
              type="date"
              label="Ngày sinh"
              value={birthDate}
              onChange={(e) => setBirthDate(e.target.value)}
              InputLabelProps={{ shrink: true }}
            />

            {/* Avatar Upload */}
            <Box>
              <Typography variant="body2" color={colors.grey[300]} mb={1}>
                Avatar * {avatar && <span style={{ color: colors.greenAccent[500] }}>✓ {avatar.name}</span>}
              </Typography>
              <input
                type="file"
                accept="image/*"
                onChange={handleAvatarChange}
                style={{
                  width: "100%",
                  padding: "10px",
                  backgroundColor: colors.primary[500],
                  border: `1px solid ${colors.grey[700]}`,
                  borderRadius: "4px",
                  color: colors.grey[100],
                  cursor: "pointer",
                  boxSizing: "border-box",
                }}
                required
              />
            </Box>

            {/* Biography */}
            <TextField
              fullWidth
              variant="filled"
              label="Tiểu sử"
              value={biography}
              onChange={(e) => setBiography(e.target.value)}
              multiline
              rows={6}
              placeholder="Nhập tiểu sử của diễn viên..."
            />

            {/* Buttons */}
            <Box display="flex" justifyContent="flex-end" gap={2}>
              <Button
                variant="outlined"
                onClick={() => navigate("/persons")}
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
                  "&:disabled": {
                    backgroundColor: colors.greenAccent[300],
                  },
                }}
              >
                {submitting ? "Đang tạo..." : "Tạo diễn viên"}
              </Button>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default PersonCreate;