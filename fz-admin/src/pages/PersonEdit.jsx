import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
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
import { getPersonById, updatePerson } from "../services/api";

const PersonEdit = () => {
  const { personId } = useParams();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [fullName, setFullName] = useState("");
  const [knownFor, setKnownFor] = useState("");
  const [biography, setBiography] = useState("");
  const [regionID, setRegionID] = useState("");
  const [avatar, setAvatar] = useState(null);
  const [currentAvatar, setCurrentAvatar] = useState("");
  const [birthDate, setBirthDate] = useState("");

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    fetchPersonDetail();
  }, [personId]);

  const fetchPersonDetail = async () => {
    try {
      const response = await getPersonById(personId);
      if (response.data.errorCode === 200) {
        const data = response.data.data;
        setFullName(data.fullName);
        setKnownFor(data.knownFor);
        setBiography(data.biography || "");
        setRegionID(data.regionID);
        setCurrentAvatar(data.avatar);
        if (data.birthDate) {
          const date = new Date(data.birthDate);
          setBirthDate(date.toISOString().split('T')[0]);
        }
      }
    } catch (error) {
      console.error("Error fetching person:", error);
      setError("Không thể tải thông tin diễn viên");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    setSuccess("");

    try {
      const fd = new FormData();
      fd.append("personID", personId);
      fd.append("fullName", fullName);
      fd.append("knownFor", knownFor);
      if (biography) fd.append("biography", biography);
      fd.append("regionID", regionID);
      if (avatar) {
        fd.append("avatar", avatar);
      }
      if (birthDate) fd.append("birthDate", new Date(birthDate).toISOString());

      const response = await updatePerson(fd);

      if (response.data.errorCode === 200) {
        setSuccess("Cập nhật diễn viên thành công!");
        setTimeout(() => {
          navigate("/persons");
        }, 2000);
      } else {
        setError(response.data.errorMessage || "Cập nhật diễn viên thất bại");
      }
    } catch (err) {
      console.error("Error updating person:", err);
      setError(err.response?.data?.errorMessage || "Có lỗi xảy ra khi cập nhật diễn viên");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <Box m="20px">
        <Header title="CHỈNH SỬA DIỄN VIÊN" subtitle="Đang tải dữ liệu..." />
      </Box>
    );
  }

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Header title="CHỈNH SỬA DIỄN VIÊN" subtitle={`ID: ${personId} - ${fullName}`} />
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

      <Card sx={{ backgroundColor: colors.primary[400], maxWidth: "600px" }}>
        <CardContent>
          <Box component="form" onSubmit={handleSubmit} sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
            
            {/* Họ và tên */}
            <TextField
              fullWidth
              variant="filled"
              label="Họ và tên *"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              required
            />

            {/* Vai trò */}
            <TextField
              fullWidth
              variant="filled"
              label="Vai trò (Known For)"
              value={knownFor}
              disabled
              helperText="Không thể chỉnh sửa"
              sx={{
                "& .MuiInputBase-input.Mui-disabled": {
                  WebkitTextFillColor: colors.grey[300],
                }
              }}
            />

            {/* Ngày sinh */}
            <TextField
              fullWidth
              variant="filled"
              type="date"
              label="Ngày sinh"
              value={birthDate}
              onChange={(e) => setBirthDate(e.target.value)}
              InputLabelProps={{ shrink: true }}
            />

            {/* Region ID */}
            <TextField
              fullWidth
              variant="filled"
              type="number"
              label="Region ID"
              value={regionID}
              disabled
              helperText="Không thể chỉnh sửa"
              sx={{
                "& .MuiInputBase-input.Mui-disabled": {
                  WebkitTextFillColor: colors.grey[300],
                }
              }}
            />

            {/* Avatar */}
            <Box>
              <Typography variant="body2" color={colors.grey[300]} mb={1} fontWeight="600">
                Avatar hiện tại:
              </Typography>
              {currentAvatar && (
                <Box
                  component="img"
                  src={currentAvatar}
                  alt="Current avatar"
                  sx={{
                    width: "120px",
                    height: "120px",
                    objectFit: "cover",
                    borderRadius: "50%",
                    mb: 2,
                    border: `2px solid ${colors.grey[600]}`,
                  }}
                />
              )}
              <Typography variant="body2" color={colors.grey[300]} mb={1} fontWeight="600">
                Avatar mới (tùy chọn):
              </Typography>
              <input
                type="file"
                accept="image/*"
                onChange={(e) => setAvatar(e.target.files?.[0] || null)}
                style={{
                  width: "100%",
                  padding: "10px",
                  backgroundColor: colors.primary[500],
                  border: `1px solid ${colors.grey[700]}`,
                  borderRadius: "4px",
                  color: colors.grey[100],
                  boxSizing: "border-box",
                }}
              />
              <Typography variant="caption" color={colors.grey[400]} mt={1} display="block">
                Để trống nếu không muốn thay đổi avatar
              </Typography>
            </Box>

            {/* Tiểu sử */}
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

            {/* Nút action */}
            <Box display="flex" justifyContent="flex-end" gap={2}>
              <Button
                variant="outlined"
                onClick={() => navigate("/persons")}
                sx={{
                  borderColor: colors.grey[400],
                  color: colors.grey[100],
                  minWidth: "120px",
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
                  minWidth: "120px",
                  "&:hover": {
                    backgroundColor: colors.greenAccent[700],
                  },
                  "&:disabled": {
                    backgroundColor: colors.greenAccent[800],
                  },
                }}
              >
                {submitting ? "Đang cập nhật..." : "Cập nhật"}
              </Button>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default PersonEdit;