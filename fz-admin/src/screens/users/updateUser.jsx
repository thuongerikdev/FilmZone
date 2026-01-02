import { useState, useEffect } from "react";
import { 
  Box, 
  Button, 
  TextField, 
  useTheme, 
  Typography, 
  Avatar, 
  Grid, 
  Paper,
  MenuItem,
  IconButton
} from "@mui/material";
import { Formik } from "formik";
import * as yup from "yup";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import useMediaQuery from "@mui/material/useMediaQuery";
import Header from "../../components/Header";
import { tokens } from "../../theme";
import { updateUserProfile, getAllUsers } from "../../services/api"; 

// Icons
import PhotoCamera from "@mui/icons-material/PhotoCamera";
import SaveOutlinedIcon from "@mui/icons-material/SaveOutlined";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";

const UpdateUserProfile = () => {
  const isNonMobile = useMediaQuery("(min-width:600px)");
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();
  const { userId } = useParams(); 
  const location = useLocation();

  const [initialValues, setInitialValues] = useState(null);
  const [avatarPreview, setAvatarPreview] = useState(null);


  const textFieldSx = {
    "& .MuiInputLabel-root.Mui-focused": { color: colors.greenAccent[500] },
    "& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline": {
      borderColor: colors.greenAccent[500],
    },
  };

  useEffect(() => {
    const mapUserToForm = (user) => ({
      userID: user.userID,
      newUserName: user.userName,
      firstName: user.profile?.firstName || "",
      lastName: user.profile?.lastName || "",
      gender: user.profile?.gender || "Other",
      dateOfBirth: user.profile?.dateOfBirth ? user.profile.dateOfBirth.split('T')[0] : "",
      avatar: null,
    });

    const initData = async () => {
      if (location.state?.currentUser) {
        const user = location.state.currentUser;
        setInitialValues(mapUserToForm(user));
        
        if (user.profile?.avatar) {
          setAvatarPreview(user.profile.avatar);
        }
      } 
      else {
        try {
          const response = await getAllUsers();
          if (response.data.errorCode === 200) {
            const user = response.data.data.find(u => u.userID === parseInt(userId));
            if (user) {
              setInitialValues(mapUserToForm(user));
              if (user.profile?.avatar) setAvatarPreview(user.profile.avatar);
            }
          }
        } catch (error) {
          console.error("Lỗi tải thông tin user:", error);
        }
      }
    };
    initData();
  }, [userId, location.state]);;

  // 2. Xử lý Submit Form
  const handleFormSubmit = async (values) => {
    try {
      const formData = new FormData();
      
      formData.append("userID", values.userID);
      formData.append("newUserName", values.newUserName);
      formData.append("firstName", values.firstName);
      formData.append("lastName", values.lastName);
      formData.append("gender", values.gender);
      formData.append("dateOfBirth", values.dateOfBirth);

      if (values.avatar instanceof File) {
        formData.append("avatar", values.avatar);
      }

      const response = await updateUserProfile(formData);

      if (response.data && response.data.errorCode === 200) {
        alert("Cập nhật hồ sơ thành công!");
        navigate(`/users/${userId}`); 
      } else {
        alert("Lỗi: " + (response.data.errorMessage || "Cập nhật thất bại"));
      }

    } catch (error) {
      console.error("Lỗi submit form:", error);
      alert("Đã xảy ra lỗi khi cập nhật.");
    }
  };

  const handleBack = () => {
    navigate(`/users/${userId}`);
  }

  if (!initialValues) return <Box m="20px">Đang tải dữ liệu...</Box>;

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center">
        <Header title="CẬP NHẬT HỒ SƠ" subtitle={`Chỉnh sửa thông tin cho User ID: ${userId}`} />
        <Button startIcon={<ArrowBackIcon />} onClick={handleBack} sx={{ color: colors.grey[100] }}>
          Quay lại
        </Button>
      </Box>

      <Paper elevation={3} sx={{ p: 4, backgroundColor: colors.primary[400], borderRadius: "8px" }}>
        <Formik
          onSubmit={handleFormSubmit}
          initialValues={initialValues}
          validationSchema={userSchema}
          enableReinitialize
        >
          {({
            values,
            errors,
            touched,
            handleBlur,
            handleChange,
            handleSubmit,
            setFieldValue,
          }) => (
            <form onSubmit={handleSubmit}>
              <Grid container spacing={4}>
                
                {/* --- CỘT TRÁI: AVATAR --- */}
                <Grid item xs={12} md={4} display="flex" flexDirection="column" alignItems="center">
                  <Box position="relative">
                    <Avatar
                      src={avatarPreview}
                      sx={{ width: 150, height: 150, mb: 2, border: `4px solid ${colors.greenAccent[500]}` }}
                    />
                    <IconButton
                      color="primary"
                      aria-label="upload picture"
                      component="label"
                      sx={{
                        position: "absolute",
                        bottom: 20,
                        right: 0,
                        backgroundColor: colors.blueAccent[500],
                        "&:hover": { backgroundColor: colors.blueAccent[700] },
                      }}
                    >
                      <input
                        hidden
                        accept="image/*"
                        type="file"
                        onChange={(event) => {
                          const file = event.currentTarget.files[0];
                          if (file) {
                            setFieldValue("avatar", file); 
                            setAvatarPreview(URL.createObjectURL(file)); 
                          }
                        }}
                      />
                      <PhotoCamera sx={{ color: "white" }} />
                    </IconButton>
                  </Box>
                  <Typography variant="caption" color={colors.grey[300]}>
                    Nhấn vào icon máy ảnh để đổi avatar
                  </Typography>
                </Grid>

                {/* --- CỘT PHẢI: FORM TEXT --- */}
                <Grid item xs={12} md={8}>
                  <Box
                    display="grid"
                    gap="30px"
                    gridTemplateColumns="repeat(4, minmax(0, 1fr))"
                    sx={{ "& > div": { gridColumn: isNonMobile ? undefined : "span 4" } }}
                  >
                    {/* Username */}
                    <TextField
                      fullWidth
                      variant="outlined"
                      type="text"
                      label="Username (newUserName)"
                      onBlur={handleBlur}
                      onChange={handleChange}
                      value={values.newUserName}
                      name="newUserName"
                      error={!!touched.newUserName && !!errors.newUserName}
                      helperText={touched.newUserName && errors.newUserName}
                      sx={{ gridColumn: "span 4", ...textFieldSx }}
                    />

                    {/* First Name */}
                    <TextField
                      fullWidth
                      variant="outlined"
                      label="Tên (First Name)"
                      onBlur={handleBlur}
                      onChange={handleChange}
                      value={values.firstName}
                      name="firstName"
                      error={!!touched.firstName && !!errors.firstName}
                      helperText={touched.firstName && errors.firstName}
                      sx={{ gridColumn: "span 2", ...textFieldSx }}
                    />

                        {/* Last Name */}
                        <TextField
                        fullWidth
                        variant="outlined"
                        label="Họ (Last Name)"
                        onBlur={handleBlur}
                        onChange={handleChange}
                        value={values.lastName}
                        name="lastName"
                        error={!!touched.lastName && !!errors.lastName}
                        helperText={touched.lastName && errors.lastName}
                        sx={{ gridColumn: "span 2", ...textFieldSx }}
                        />

                    {/* Gender */}
                    <TextField
                      fullWidth
                      select
                      variant="outlined"
                      label="Giới tính"
                      onBlur={handleBlur}
                      onChange={handleChange}
                      value={values.gender}
                      name="gender"
                      error={!!touched.gender && !!errors.gender}
                      helperText={touched.gender && errors.gender}
                      sx={{ gridColumn: "span 2", ...textFieldSx }}
                    >
                      <MenuItem value="Male">Nam</MenuItem>
                      <MenuItem value="Female">Nữ</MenuItem>
                      <MenuItem value="Other">Khác</MenuItem>
                    </TextField>

                    {/* Date of Birth */}
                    <TextField
                      fullWidth
                      variant="outlined"
                      type="date"
                      label="Ngày sinh"
                      InputLabelProps={{ shrink: true }}
                      onBlur={handleBlur}
                      onChange={handleChange}
                      value={values.dateOfBirth}
                      name="dateOfBirth"
                      error={!!touched.dateOfBirth && !!errors.dateOfBirth}
                      helperText={touched.dateOfBirth && errors.dateOfBirth}
                      sx={{ gridColumn: "span 2", ...textFieldSx }}
                    />
                  </Box>

                  {/* BUTTONS */}
                  <Box display="flex" justifyContent="end" mt="30px" gap={2}>
                    <Button variant="outlined" color="error" onClick={handleBack}>
                      Hủy bỏ
                    </Button>
                    <Button
                      type="submit"
                      color="secondary"
                      variant="contained"
                      startIcon={<SaveOutlinedIcon />}
                      sx={{ px: 4, fontWeight: "bold", backgroundColor: colors.greenAccent[600] }}
                    >
                      Cập nhật
                    </Button>
                  </Box>
                </Grid>
              </Grid>
            </form>
          )}
        </Formik>
      </Paper>
    </Box>
  );
};

// Validation Schema
const userSchema = yup.object().shape({
  newUserName: yup.string().required("Vui lòng nhập Username"),
  firstName: yup.string().required("Vui lòng nhập Tên"),
  lastName: yup.string().nullable(),
  gender: yup.string().nullable(),
  dateOfBirth: yup.date().nullable().typeError("Ngày sinh không hợp lệ"),
  // Avatar không required vì user có thể không muốn đổi ảnh
});

export default UpdateUserProfile;