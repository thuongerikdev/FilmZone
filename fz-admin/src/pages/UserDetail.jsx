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
  IconButton,
  Divider,
  Button,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { tokens } from "../theme";
import Header from "../components/Header";
import { getAllUsers, deleteUser, deleteComment, getCommentsByUserId } from "../services/api";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import PersonIcon from "@mui/icons-material/Person";
import EmailIcon from "@mui/icons-material/Email";
import VerifiedIcon from "@mui/icons-material/Verified";
import DevicesIcon from "@mui/icons-material/Devices";
import HistoryIcon from "@mui/icons-material/History";
import SecurityIcon from "@mui/icons-material/Security";
import VpnKeyIcon from "@mui/icons-material/VpnKey";
import CommentIcon from "@mui/icons-material/Comment";

const UserDetail = () => {
  const { userId } = useParams();
  const navigate = useNavigate();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  
  const [userData, setUserData] = useState(null);
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchUserDetail();
    fetchUserComments();
  }, [userId]);

  const fetchUserDetail = async () => {
    try {
      const response = await getAllUsers();
      if (response.data.errorCode === 200) {
        const user = response.data.data.find(u => u.userID === parseInt(userId));
        setUserData(user);
      }
    } catch (error) {
      console.error("Error fetching user detail:", error);
    } finally {
      setLoading(false);
    }
  };

  const fetchUserComments = async () => {
    try {
      const response = await getCommentsByUserId(userId);
      if (response.data.errorCode === 200) {
        setComments(response.data.data);
      }
    } catch (error) {
      console.error("Error fetching comments:", error);
    }
  };

  const handleDelete = async () => {
    if (window.confirm("Bạn có chắc chắn muốn xóa user này?")) {
      try {
        const response = await deleteUser({ userId: parseInt(userId) });
        if (response.data.errorCode === 200) {
          alert("Xóa user thành công!");
          navigate("/users");
        } else {
          alert(response.data.errorMessage || "Xóa user thất bại");
        }
      } catch (error) {
        console.error("Error deleting user:", error);
        alert("Có lỗi xảy ra khi xóa user");
      }
    }
  };

  const handleDeleteComment = async (commentId) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa comment này?")) {
      try {
        const response = await deleteComment(commentId);
        if (response.data.errorCode === 200) {
          alert("Xóa comment thành công!");
          fetchUserComments(); // Refresh comments
        } else {
          alert(response.data.errorMessage || "Xóa comment thất bại");
        }
      } catch (error) {
        console.error("Error deleting comment:", error);
        alert("Có lỗi xảy ra khi xóa comment");
      }
    }
  };

  const formatDateTime = (dateString) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleString('vi-VN');
  };

  if (loading) {
    return (
      <Box m="20px">
        <Header title="CHI TIẾT USER" subtitle="Đang tải dữ liệu..." />
      </Box>
    );
  }

  if (!userData) {
    return (
      <Box m="20px">
        <Header title="CHI TIẾT USER" subtitle="Không tìm thấy user" />
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate("/users")}
          sx={{ color: colors.grey[100] }}
        >
          Quay lại
        </Button>
      </Box>
    );
  }

  // Columns cho Sessions table
  const sessionsColumns = [
    { field: "sessionID", headerName: "Session ID", width: 100 },
    { field: "deviceId", headerName: "Device ID", flex: 1, minWidth: 200 },
    { field: "ip", headerName: "IP Address", width: 150 },
    { 
      field: "userAgent", 
      headerName: "User Agent", 
      flex: 1, 
      minWidth: 300,
      renderCell: ({ value }) => (
        <Typography variant="body2" noWrap title={value}>
          {value}
        </Typography>
      ),
    },
    { 
      field: "createdAt", 
      headerName: "Created At", 
      width: 180,
      renderCell: ({ value }) => formatDateTime(value),
    },
    { 
      field: "lastSeenAt", 
      headerName: "Last Seen", 
      width: 180,
      renderCell: ({ value }) => formatDateTime(value),
    },
    {
      field: "isRevoked",
      headerName: "Status",
      width: 100,
      renderCell: ({ value }) => (
        <Chip
          label={value ? "Revoked" : "Active"}
          size="small"
          color={value ? "error" : "success"}
        />
      ),
    },
  ];

  // Columns cho Audit Logs
  const auditLogsColumns = [
    { field: "auditID", headerName: "Audit ID", width: 100 },
    { field: "action", headerName: "Action", width: 120 },
    { 
      field: "result", 
      headerName: "Result", 
      width: 100,
      renderCell: ({ value }) => (
        <Chip
          label={value}
          size="small"
          color={value === "OK" ? "success" : "error"}
        />
      ),
    },
    { field: "ip", headerName: "IP", width: 150 },
    { 
      field: "createdAt", 
      headerName: "Time", 
      width: 180,
      renderCell: ({ value }) => formatDateTime(value),
    },
    { field: "detail", headerName: "Detail", flex: 1, minWidth: 200 },
  ];

  // Columns cho Refresh Tokens
  const refreshTokensColumns = [
    { field: "refreshTokenID", headerName: "Token ID", width: 100 },
    { field: "sessionID", headerName: "Session ID", width: 100 },
    { 
      field: "created", 
      headerName: "Created", 
      width: 180,
      renderCell: ({ value }) => formatDateTime(value),
    },
    { 
      field: "expires", 
      headerName: "Expires", 
      width: 180,
      renderCell: ({ value }) => formatDateTime(value),
    },
    {
      field: "isActive",
      headerName: "Status",
      width: 100,
      renderCell: ({ value }) => (
        <Chip
          label={value ? "Active" : "Expired"}
          size="small"
          color={value ? "success" : "default"}
        />
      ),
    },
  ];

  // Columns cho Comments
  const commentsColumns = [
    { field: "commentID", headerName: "Comment ID", width: 100 },
    { field: "movieID", headerName: "Movie ID", width: 100 },
    { 
      field: "content", 
      headerName: "Content", 
      flex: 1, 
      minWidth: 300,
      renderCell: ({ value }) => (
        <Typography variant="body2" noWrap title={value}>
          {value}
        </Typography>
      ),
    },
    { 
      field: "likeCount", 
      headerName: "Likes", 
      width: 80,
      align: "center",
    },
    {
      field: "isEdited",
      headerName: "Edited",
      width: 80,
      renderCell: ({ value }) => (
        <Chip
          label={value ? "Yes" : "No"}
          size="small"
          color={value ? "warning" : "default"}
        />
      ),
    },
    { 
      field: "createdAt", 
      headerName: "Created", 
      width: 180,
      renderCell: ({ value }) => formatDateTime(value),
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 100,
      sortable: false,
      renderCell: ({ row }) => (
        <IconButton
          onClick={() => handleDeleteComment(row.commentID)}
          sx={{
            color: colors.redAccent[500],
            "&:hover": {
              backgroundColor: colors.redAccent[800],
            },
          }}
        >
          <DeleteOutlineIcon />
        </IconButton>
      ),
    },
  ];

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center">
        <Header
          title={`USER: ${userData.userName}`}
          subtitle={`ID: ${userData.userID} - ${userData.email}`}
        />
        <Box display="flex" gap={2}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate("/users")}
            sx={{
              color: colors.grey[100],
              borderColor: colors.grey[400],
            }}
            variant="outlined"
          >
            Quay lại
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
            Xóa User
          </Button>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Thông tin cơ bản */}
        <Grid item xs={12} md={6}>
          <Card sx={{ backgroundColor: colors.primary[400] }}>
            <CardContent>
              <Box display="flex" alignItems="center" mb={2}>
                <PersonIcon sx={{ color: colors.greenAccent[500], mr: 1 }} />
                <Typography variant="h5" fontWeight="600" color={colors.grey[100]}>
                  Thông tin cá nhân
                </Typography>
              </Box>
              <Divider sx={{ mb: 2, backgroundColor: colors.grey[700] }} />
              
              <Box display="flex" flexDirection="column" gap={1.5}>
                <Box>
                  <Typography variant="body2" color={colors.grey[300]}>
                    Họ và tên
                  </Typography>
                  <Typography variant="body1" color={colors.grey[100]} fontWeight="500">
                    {userData.profile?.firstName} {userData.profile?.lastName}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="body2" color={colors.grey[300]}>
                    Username
                  </Typography>
                  <Typography variant="body1" color={colors.grey[100]} fontWeight="500">
                    {userData.userName}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="body2" color={colors.grey[300]}>
                    Giới tính
                  </Typography>
                  <Typography variant="body1" color={colors.grey[100]} fontWeight="500">
                    {userData.profile?.gender || "N/A"}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="body2" color={colors.grey[300]}>
                    Ngày sinh
                  </Typography>
                  <Typography variant="body1" color={colors.grey[100]} fontWeight="500">
                    {userData.profile?.dateOfBirth || "N/A"}
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Thông tin tài khoản */}
        <Grid item xs={12} md={6}>
          <Card sx={{ backgroundColor: colors.primary[400] }}>
            <CardContent>
              <Box display="flex" alignItems="center" mb={2}>
                <EmailIcon sx={{ color: colors.blueAccent[500], mr: 1 }} />
                <Typography variant="h5" fontWeight="600" color={colors.grey[100]}>
                  Thông tin tài khoản
                </Typography>
              </Box>
              <Divider sx={{ mb: 2, backgroundColor: colors.grey[700] }} />
              
              <Box display="flex" flexDirection="column" gap={1.5}>
                <Box>
                  <Typography variant="body2" color={colors.grey[300]}>
                    Email
                  </Typography>
                  <Typography variant="body1" color={colors.grey[100]} fontWeight="500">
                    {userData.email}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                    Trạng thái Email
                  </Typography>
                  <Chip
                    icon={userData.isEmailVerified ? <VerifiedIcon /> : null}
                    label={userData.isEmailVerified ? "Đã xác thực" : "Chưa xác thực"}
                    size="small"
                    sx={{
                      backgroundColor: userData.isEmailVerified 
                        ? colors.greenAccent[600] 
                        : colors.redAccent[600],
                      color: colors.grey[100],
                    }}
                  />
                </Box>
                <Box>
                  <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                    Trạng thái
                  </Typography>
                  <Chip
                    label={userData.status}
                    size="small"
                    sx={{
                      backgroundColor: userData.status === "Active" 
                        ? colors.greenAccent[700] 
                        : colors.redAccent[700],
                      color: colors.grey[100],
                    }}
                  />
                </Box>
                <Box>
                  <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                    Vai trò
                  </Typography>
                  <Box display="flex" gap={1} flexWrap="wrap">
                    {userData.roles?.map((role, index) => (
                      <Chip
                        key={index}
                        label={role}
                        size="small"
                        sx={{
                          backgroundColor: colors.blueAccent[700],
                          color: colors.grey[100],
                        }}
                      />
                    ))}
                  </Box>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Sessions */}
        <Grid item xs={12}>
          <Accordion 
            defaultExpanded
            sx={{ 
              backgroundColor: colors.primary[400],
              color: colors.grey[100],
            }}
          >
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box display="flex" alignItems="center">
                <DevicesIcon sx={{ color: colors.greenAccent[500], mr: 1 }} />
                <Typography variant="h5" fontWeight="600">
                  Sessions ({userData.sessions?.length || 0})
                </Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Box height="400px">
                <DataGrid
                  rows={userData.sessions || []}
                  columns={sessionsColumns}
                  getRowId={(row) => row.sessionID}
                  pageSize={5}
                  rowsPerPageOptions={[5, 10, 20]}
                  sx={{
                    "& .MuiDataGrid-cell": {
                      borderBottom: `1px solid ${colors.grey[700]}`,
                    },
                  }}
                />
              </Box>
            </AccordionDetails>
          </Accordion>
        </Grid>

        {/* Audit Logs */}
        <Grid item xs={12}>
          <Accordion 
            sx={{ 
              backgroundColor: colors.primary[400],
              color: colors.grey[100],
            }}
          >
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box display="flex" alignItems="center">
                <HistoryIcon sx={{ color: colors.blueAccent[500], mr: 1 }} />
                <Typography variant="h5" fontWeight="600">
                  Audit Logs ({userData.auditLogs?.length || 0})
                </Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Box height="400px">
                <DataGrid
                  rows={userData.auditLogs || []}
                  columns={auditLogsColumns}
                  getRowId={(row) => row.auditID}
                  pageSize={5}
                  rowsPerPageOptions={[5, 10, 20]}
                  sx={{
                    "& .MuiDataGrid-cell": {
                      borderBottom: `1px solid ${colors.grey[700]}`,
                    },
                  }}
                />
              </Box>
            </AccordionDetails>
          </Accordion>
        </Grid>

        {/* Refresh Tokens */}
        <Grid item xs={12}>
          <Accordion 
            sx={{ 
              backgroundColor: colors.primary[400],
              color: colors.grey[100],
            }}
          >
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box display="flex" alignItems="center">
                <VpnKeyIcon sx={{ color: colors.greenAccent[500], mr: 1 }} />
                <Typography variant="h5" fontWeight="600">
                  Refresh Tokens ({userData.refreshTokens?.length || 0})
                </Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Box height="400px">
                <DataGrid
                  rows={userData.refreshTokens || []}
                  columns={refreshTokensColumns}
                  getRowId={(row) => row.refreshTokenID}
                  pageSize={5}
                  rowsPerPageOptions={[5, 10, 20]}
                  sx={{
                    "& .MuiDataGrid-cell": {
                      borderBottom: `1px solid ${colors.grey[700]}`,
                    },
                  }}
                />
              </Box>
            </AccordionDetails>
          </Accordion>
        </Grid>

        {/* Email Verifications */}
        {userData.emailVerifications && userData.emailVerifications.length > 0 && (
          <Grid item xs={12}>
            <Accordion 
              sx={{ 
                backgroundColor: colors.primary[400],
                color: colors.grey[100],
              }}
            >
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Box display="flex" alignItems="center">
                  <SecurityIcon sx={{ color: colors.blueAccent[500], mr: 1 }} />
                  <Typography variant="h5" fontWeight="600">
                    Email Verifications ({userData.emailVerifications.length})
                  </Typography>
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                <Box>
                  {userData.emailVerifications.map((verification) => (
                    <Box 
                      key={verification.emailVerificationID}
                      p={2}
                      mb={1}
                      sx={{ 
                        backgroundColor: colors.primary[500],
                        borderRadius: "8px",
                      }}
                    >
                      <Typography variant="body2" color={colors.grey[300]}>
                        Created: {formatDateTime(verification.createdAt)}
                      </Typography>
                      <Typography variant="body2" color={colors.grey[300]}>
                        Expires: {formatDateTime(verification.expiresAt)}
                      </Typography>
                      <Typography variant="body2" color={colors.grey[300]}>
                        Consumed: {formatDateTime(verification.consumedAt)}
                      </Typography>
                    </Box>
                  ))}
                </Box>
              </AccordionDetails>
            </Accordion>
          </Grid>
        )}

        {/* Comments */}
        <Grid item xs={12}>
          <Accordion 
            sx={{ 
              backgroundColor: colors.primary[400],
              color: colors.grey[100],
            }}
          >
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box display="flex" alignItems="center">
                <CommentIcon sx={{ color: colors.greenAccent[500], mr: 1 }} />
                <Typography variant="h5" fontWeight="600">
                  Comments ({comments.length})
                </Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Box height="400px">
                <DataGrid
                  rows={comments}
                  columns={commentsColumns}
                  getRowId={(row) => row.commentID}
                  pageSize={5}
                  rowsPerPageOptions={[5, 10, 20]}
                  sx={{
                    "& .MuiDataGrid-cell": {
                      borderBottom: `1px solid ${colors.grey[700]}`,
                    },
                  }}
                />
              </Box>
            </AccordionDetails>
          </Accordion>
        </Grid>
      </Grid>
    </Box>
  );
};

export default UserDetail;