import { useState, useEffect, useCallback } from "react";
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
  Button,
  Tabs,
  Tab,
  Avatar,
  Paper,
  Tooltip,
} from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { tokens } from "../theme";
import Header from "../components/Header";
import { getAllUsers, deleteUser, deleteComment, getCommentsByUserId, getUserSlimByID } from "../services/api";

// Icons
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import UpgradeIcon from '@mui/icons-material/Upgrade';
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import PersonIcon from "@mui/icons-material/Person";
import EmailIcon from "@mui/icons-material/Email";
import VerifiedIcon from "@mui/icons-material/Verified";
import DevicesIcon from "@mui/icons-material/Devices";
import HistoryIcon from "@mui/icons-material/History";
import SecurityIcon from "@mui/icons-material/Security";
import VpnKeyIcon from "@mui/icons-material/VpnKey";
import CommentIcon from "@mui/icons-material/Comment";
import AdminPanelSettingsOutlinedIcon from "@mui/icons-material/AdminPanelSettingsOutlined";

// Custom TabPanel Component
function CustomTabPanel(props) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const UserDetail = () => {
  const { userId } = useParams();
  const navigate = useNavigate();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  
  const [userData, setUserData] = useState(null);
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [tabValue, setTabValue] = useState(0); 

  // Fetch User Info
  const fetchUserDetail = useCallback(async () => {
    try {
      const response = await getUserSlimByID(userId);
      if (response.data.errorCode === 200) {
        const user = response.data.data
        setUserData(user);
      }
    } catch (error) {
      console.error("Error fetching user detail:", error);
    } finally {
      setLoading(false);
    }
  }, [userId]);

  // Fetch Comments
  const fetchUserComments = useCallback(async () => {
    try {
      const response = await getCommentsByUserId(userId);
      if (response.data.errorCode === 200) {
        setComments(response.data.data);
      }
    } catch (error) {
      console.error("Error fetching comments:", error);
    }
  }, [userId]);

  useEffect(() => {
    let isActive = true;
    fetchUserDetail().then(() => { if (isActive) fetchUserComments(); });
    return () => { isActive = false; };
  }, [fetchUserDetail, fetchUserComments]);

  const handleNavigateToUpdate = () => {
      navigate(`/users/update/${userId}`, { 
          state: { currentUser: userData } 
      });
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
        alert("Có lỗi xảy ra khi xóa user");
      }
    }
  };

  const handleDeleteComment = async (commentId) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa comment này?")) {
      try {
        const response = await deleteComment(commentId);
        if (response.data.errorCode === 200) {
          fetchUserComments();
        } else {
          alert("Xóa comment thất bại");
        }
      } catch (error) {
        alert("Có lỗi xảy ra");
      }
    }
  };

  const formatDateTime = (dateString) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleString('vi-VN');
  };

  const handleTabChange = (event, newValue) => {
    setTabValue(newValue);
  };

  if (loading) return <Box m="20px"><Header title="CHI TIẾT USER" subtitle="Đang tải..." /></Box>;
  if (!userData) return <Box m="20px"><Header title="CHI TIẾT USER" subtitle="Không tìm thấy user" /></Box>;

  // --- Cấu hình DataGrid Style chung ---
  const commonDataGridSx = {
    "& .MuiDataGrid-root": { border: "none" },
    "& .MuiDataGrid-cell": { borderBottom: `1px solid ${colors.primary[400]}` },
    "& .MuiDataGrid-columnHeaders": { backgroundColor: colors.blueAccent[700], borderBottom: "none" },
    "& .MuiDataGrid-virtualScroller": { backgroundColor: colors.primary[400] },
    "& .MuiDataGrid-footerContainer": { borderTop: "none", backgroundColor: colors.blueAccent[700] },
  };

  // --- Columns Definitions ---
  const sessionsColumns = [
    { field: "sessionID", headerName: "ID", width: 70 },
    { field: "deviceId", headerName: "Device ID", flex: 1, minWidth: 150 },
    { field: "ip", headerName: "IP Address", width: 130 },
    { 
      field: "userAgent", headerName: "User Agent", flex: 1, minWidth: 200,
      renderCell: ({ value }) => <Tooltip title={value}><Typography variant="body2" noWrap>{value}</Typography></Tooltip>
    },
    { field: "lastSeenAt", headerName: "Last Seen", width: 160, renderCell: ({ value }) => formatDateTime(value) },
    {
      field: "isRevoked", headerName: "Status", width: 100,
      renderCell: ({ value }) => (
        <Chip label={value ? "Revoked" : "Active"} size="small" color={value ? "error" : "success"} variant="outlined" />
      ),
    },
  ];

  const auditLogsColumns = [
    { field: "action", headerName: "Action", width: 150 },
    { 
      field: "result", headerName: "Result", width: 100,
      renderCell: ({ value }) => <Chip label={value} size="small" color={value === "OK" ? "success" : "error"} />
    },
    { field: "ip", headerName: "IP", width: 130 },
    { field: "createdAt", headerName: "Time", width: 160, renderCell: ({ value }) => formatDateTime(value) },
    { field: "detail", headerName: "Detail", flex: 1, minWidth: 200 },
  ];

  const commentsColumns = [
    { field: "movieID", headerName: "Movie ID", width: 90, align: "center" },
    { 
      field: "content", headerName: "Content", flex: 1, minWidth: 300,
      renderCell: ({ value }) => <Tooltip title={value}><Typography variant="body2" noWrap>{value}</Typography></Tooltip>
    },
    { field: "likeCount", headerName: "Likes", width: 80, align: "center" },
    { field: "createdAt", headerName: "Created", width: 160, renderCell: ({ value }) => formatDateTime(value) },
    {
      field: "actions", headerName: "Action", width: 80, sortable: false, align: "center",
      renderCell: ({ row }) => (
        <IconButton onClick={() => handleDeleteComment(row.commentID)} sx={{ color: colors.redAccent[500] }}>
          <DeleteOutlineIcon />
        </IconButton>
      ),
    },
  ];

  return (
    <Box m="20px">
      {/* Header Section */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Box display="flex" alignItems="center" gap={2}>
          <Avatar 
            src={userData.profile?.avatar} 
            
            alt={userData.userName}
            
            sx={{ width: 64, height: 64, bgcolor: colors.blueAccent[500] }}
          >
            {userData.userName?.charAt(0).toUpperCase()}
          </Avatar>
          <Box>
             <Typography variant="h2" color={colors.grey[100]} fontWeight="bold">
               {userData.userName}
             </Typography>
             <Typography variant="subtitle1" color={colors.greenAccent[500]}>
               {userData.email}
             </Typography>
          </Box>
        </Box>
        
        <Box display="flex" gap={2}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate("/users")}
            sx={{ color: colors.grey[100], borderColor: colors.grey[100] }}
            variant="outlined"
          >
            Quay lại
          </Button>
           <Button
            startIcon={<UpgradeIcon />} 
            onClick={handleNavigateToUpdate} 
            color="success"
            variant="outlined"
          >
            Cập nhật User
          </Button>
          <Button
            startIcon={<DeleteOutlineIcon />}
            onClick={handleDelete}
            color="error"
            variant="contained"
          >
            Xóa User
          </Button>
        </Box>
      </Box>

      {/* Main Content Tabs */}
      <Paper sx={{ backgroundColor: colors.primary[400], borderRadius: "8px" }}>
        <Tabs 
           value={tabValue} 
           onChange={handleTabChange} 
           textColor="secondary"
           indicatorColor="secondary"
           sx={{ borderBottom: 1, borderColor: "divider", p: 1 }}
        >
          <Tab icon={<PersonIcon />} iconPosition="start" label="Thông tin chung" />
          <Tab icon={<DevicesIcon />} iconPosition="start" label={`Sessions (${userData.sessions?.length || 0})`} />
          <Tab icon={<CommentIcon />} iconPosition="start" label={`Comments (${comments.length})`} />
          <Tab icon={<HistoryIcon />} iconPosition="start" label="Audit Logs" />
          <Tab icon={<SecurityIcon />} iconPosition="start" label="Security" />
        </Tabs>

        {/* Tab 1: General Info */}
        <CustomTabPanel value={tabValue} index={0}>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
               <Card sx={{ bgcolor: colors.primary[500], height: '100%' }}>
                 <CardContent>
                   <Typography variant="h5" mb={2} display="flex" alignItems="center" gap={1}>
                      <AdminPanelSettingsOutlinedIcon color="secondary"/> Thông tin cá nhân
                   </Typography>
                   <Box display="grid" gridTemplateColumns="1fr 2fr" rowGap={2}>
                      <Typography color={colors.grey[400]}>Full Name:</Typography>
                      <Typography fontWeight="500">{userData.profile?.firstName} {userData.profile?.lastName}</Typography>
                      
                      <Typography color={colors.grey[400]}>Gender:</Typography>
                      <Typography>{userData.profile?.gender || "N/A"}</Typography>
                      
                      <Typography color={colors.grey[400]}>Birthday:</Typography>
                      <Typography>{userData.profile?.dateOfBirth || "N/A"}</Typography>
                   </Box>
                 </CardContent>
               </Card>
            </Grid>
            <Grid item xs={12} md={6}>
              <Card sx={{ bgcolor: colors.primary[500], height: '100%' }}>
                 <CardContent>
                   <Typography variant="h5" mb={2} display="flex" alignItems="center" gap={1}>
                      <EmailIcon color="secondary"/> Tài khoản
                   </Typography>
                   <Box display="grid" gridTemplateColumns="1fr 2fr" rowGap={2}>
                      <Typography color={colors.grey[400]}>Status:</Typography>
                      <Box>
                        <Chip label={userData.status} color={userData.status === "Active" ? "success" : "error"} size="small" />
                      </Box>

                      <Typography color={colors.grey[400]}>Email Verified:</Typography>
                      <Box>
                         {userData.isEmailVerified ? <VerifiedIcon color="success"/> : "No"}
                      </Box>

                      <Typography color={colors.grey[400]}>Roles:</Typography>
                      <Box display="flex" gap={1}>
                        {userData.roles?.map((role, index) => (
                          <Chip 
                            key={role.roleID || index} 
                            
                            label={role.roleName} 
                            
                            title={role.roleDescription}
                            
                            size="small" 
                            color="info" 
                          />
                        ))}
                      </Box>
                   </Box>
                 </CardContent>
               </Card>
            </Grid>
          </Grid>
        </CustomTabPanel>

        {/* Tab 2: Sessions */}
        <CustomTabPanel value={tabValue} index={1}>
          <Box height="400px" sx={commonDataGridSx}>
            <DataGrid 
                rows={userData.sessions || []} 
                columns={sessionsColumns} 
                getRowId={(r) => r.sessionID} 
                pageSize={5} 
                rowsPerPageOptions={[5, 10]}
                rowHeight={50}
            />
          </Box>
        </CustomTabPanel>

        {/* Tab 3: Comments */}
        <CustomTabPanel value={tabValue} index={2}>
          <Box height="400px" sx={commonDataGridSx}>
             <DataGrid 
                rows={comments} 
                columns={commentsColumns} 
                getRowId={(r) => r.commentID} 
                pageSize={5}
                rowHeight={50}
             />
          </Box>
        </CustomTabPanel>

         {/* Tab 4: Audit Logs */}
         <CustomTabPanel value={tabValue} index={3}>
           <Box height="400px" sx={commonDataGridSx}>
             <DataGrid 
                rows={userData.auditLogs || []} 
                columns={auditLogsColumns} 
                getRowId={(r) => r.auditID} 
                pageSize={10}
                rowHeight={50}
             />
           </Box>
        </CustomTabPanel>

        {/* Tab 5: Security (Refresh Tokens & Verifications) */}
        <CustomTabPanel value={tabValue} index={4}>
           <Typography variant="h6" mb={2} display="flex" alignItems="center" gap={1}>
             <VpnKeyIcon /> Refresh Tokens
           </Typography>
           <Box height="300px" mb={4} sx={commonDataGridSx}>
             <DataGrid 
               rows={userData.refreshTokens || []} 
               columns={[
                  { field: "refreshTokenID", headerName: "Token ID", width: 90 },
                  { field: "created", headerName: "Created", width: 180, renderCell: (p) => formatDateTime(p.value) },
                  { field: "expires", headerName: "Expires", width: 180, renderCell: (p) => formatDateTime(p.value) },
                  { field: "isActive", headerName: "Active", width: 100, renderCell: (p) => <Chip label={p.value ? "Yes" : "No"} size="small" color={p.value ? "success" : "default"} /> }
               ]} 
               getRowId={(r) => r.refreshTokenID} 
               pageSize={5}
               rowHeight={50}
             />
           </Box>
           
           {userData.emailVerifications?.length > 0 && (
             <>
                <Typography variant="h6" mb={2} display="flex" alignItems="center" gap={1}>
                  <SecurityIcon /> Email Verifications
                </Typography>
                <Box display="flex" gap={2} flexWrap="wrap">
                  {userData.emailVerifications.map((v) => (
                    <Paper key={v.emailVerificationID} sx={{ p: 2, bgcolor: colors.primary[500], minWidth: 200 }}>
                       <Typography variant="caption" display="block" color={colors.grey[400]}>Expire: {formatDateTime(v.expiresAt)}</Typography>
                       <Chip label={v.consumedAt ? "Consumed" : "Pending"} size="small" color={v.consumedAt ? "default" : "warning"} sx={{ mt: 1 }} />
                    </Paper>
                  ))}
                </Box>
             </>
           )}
        </CustomTabPanel>

      </Paper>
    </Box>
  );
};

export default UserDetail;