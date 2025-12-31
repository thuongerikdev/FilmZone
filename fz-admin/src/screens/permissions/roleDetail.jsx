import { useState, useEffect, useMemo } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import {
  Box,
  Typography,
  useTheme,
  Button,
  Card,
  CardContent,
  Grid,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  CircularProgress,
  IconButton,
  TextField,
  InputAdornment,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Checkbox,
  Divider
} from "@mui/material";
import { DataGrid, GridToolbar } from "@mui/x-data-grid";
import { tokens } from "../../theme";
import Header from "../../components/Header";

// API Imports
import { 
  getPermissionbyRoleId, 
  getAllPermissions, 
  assignPermissionToRole 
} from "../../services/api";

// Icons
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import VpnKeyIcon from "@mui/icons-material/VpnKey";
import SecurityIcon from "@mui/icons-material/Security";
import VerifiedUserIcon from "@mui/icons-material/VerifiedUser";
import SaveIcon from "@mui/icons-material/Save";
import ManageAccountsIcon from "@mui/icons-material/ManageAccounts";
import CancelIcon from "@mui/icons-material/Cancel";
import SearchIcon from "@mui/icons-material/Search";
import SelectAllIcon from "@mui/icons-material/SelectAll";
import DeselectIcon from "@mui/icons-material/Deselect";

const RoleDetail = () => {
  const { roleId } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  const roleInfo = location.state?.roleData;

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  
  // Data
  const [rolePermissions, setRolePermissions] = useState([]); // Dữ liệu hiển thị bảng chính
  const [allPermissions, setAllPermissions] = useState([]);   // Dữ liệu gốc cho Dialog

  // Dialog State
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedIds, setSelectedIds] = useState([]); // Mảng ID đang chọn
  const [searchTerm, setSearchTerm] = useState("");   // Từ khóa tìm kiếm trong dialog


  useEffect(() => {
    const fetchInitialData = async () => {
      try {
        setLoading(true);
        const [rolePermRes, allPermRes] = await Promise.all([
          getPermissionbyRoleId(roleId),
          getAllPermissions()
        ]);

        if (rolePermRes.data?.errorCode === 200) {
          const mappedRolePerms = rolePermRes.data.data.map((item) => ({
            ...item,
            id: item.permissionID,
          }));
          setRolePermissions(mappedRolePerms);
        }

        if (allPermRes.data?.errorCode === 200) {
          const mappedAllPerms = allPermRes.data.data.map((item) => ({
            ...item,
            id: item.permissionID,
          }));
          setAllPermissions(mappedAllPerms);
        }

      } catch (error) {
        console.error("Error fetching data:", error);
        alert("Có lỗi xảy ra khi tải dữ liệu.");
      } finally {
        setLoading(false);
      }
    };

    fetchInitialData();
  }, [roleId]);

  // Lọc permission theo search term
  const filteredList = useMemo(() => {
    if (!searchTerm) return allPermissions;
    const lowerTerm = searchTerm.toLowerCase();
    return allPermissions.filter(p => 
      p.permissionName.toLowerCase().includes(lowerTerm) || 
      p.code.toLowerCase().includes(lowerTerm)
    );
  }, [allPermissions, searchTerm]);

  // Mở Dialog
  const handleOpenDialog = () => {
    const currentIds = rolePermissions.map((p) => p.permissionID);
    setSelectedIds(currentIds);
    setSearchTerm(""); 
    setOpenDialog(true);
  };

  // Toggle 1 item
  const handleToggle = (id) => () => {
    const currentIndex = selectedIds.indexOf(id);
    const newChecked = [...selectedIds];

    if (currentIndex === -1) {
      newChecked.push(id);
    } else {
      newChecked.splice(currentIndex, 1);
    }
    setSelectedIds(newChecked);
  };

  const handleSelectAll = () => {
    const visibleIds = filteredList.map(p => p.permissionID);
    const newSelected = [...new Set([...selectedIds, ...visibleIds])];
    setSelectedIds(newSelected);
  };

  const handleDeselectAll = () => {
    const visibleIds = filteredList.map(p => p.permissionID);
    const newSelected = selectedIds.filter(id => !visibleIds.includes(id));
    setSelectedIds(newSelected);
  };

  const handleSave = async () => {
    const confirmMsg = `Bạn đang gán ${selectedIds.length} quyền cho Role này. Tiếp tục?`;
    if (!window.confirm(confirmMsg)) return;

    try {
      setSaving(true);
      const payload = {
        roleID: parseInt(roleId),
        permissionIDs: selectedIds,
      };

      const response = await assignPermissionToRole(payload);

      if (response.data && response.data.errorCode === 200) {
        alert("Cập nhật quyền thành công!");
        setOpenDialog(false);
        const newRolePermissions = allPermissions.filter((p) => 
          selectedIds.includes(p.permissionID)
        );
        setRolePermissions(newRolePermissions);
      } else {
        alert(response.data.errorMessage || "Cập nhật thất bại.");
      }
    } catch (error) {
      console.error("Error saving permissions:", error);
      alert("Lỗi kết nối đến server.");
    } finally {
      setSaving(false);
    }
  };

  // --- COLUMNS CHO BẢNG NGOÀI (READ ONLY) ---
  const viewColumns = [
    { field: "permissionID", headerName: "ID", width: 70 },
    {
      field: "permissionName",
      headerName: "Tên Quyền",
      flex: 1,
      minWidth: 200,
      renderCell: ({ value }) => (
        <Typography variant="body2" fontWeight="bold" color={colors.greenAccent[300]}>
          {value}
        </Typography>
      ),
    },
    {
      field: "code",
      headerName: "Mã Code",
      flex: 1,
      minWidth: 150,
      renderCell: ({ value }) => (
        <Box display="flex" alignItems="center" gap={1}>
          <VpnKeyIcon fontSize="small" sx={{ color: colors.grey[400] }} />
          <Typography variant="body2" fontFamily="monospace">{value}</Typography>
        </Box>
      ),
    },
    {
      field: "scope",
      headerName: "Phạm vi",
      width: 120,
      renderCell: ({ value }) => (
        <Chip label={value || "Global"} size="small" sx={{ backgroundColor: colors.blueAccent[700] }} />
      ),
    },
    { field: "permissionDescription", headerName: "Mô tả", flex: 1.5 },
  ];

  if (loading) {
    return (
      <Box m="20px" display="flex" justifyContent="center" alignItems="center" height="80vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box m="20px">
      {/* HEADER */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Header
          title="CHI TIẾT ROLE"
          subtitle={`Phân quyền cho: ${roleInfo ? roleInfo.roleName : `Role #${roleId}`}`}
        />
        <Box display="flex" gap={2}>
          <Button
            startIcon={<ManageAccountsIcon />}
            variant="contained"
            onClick={handleOpenDialog}
            sx={{
              backgroundColor: colors.blueAccent[700],
              fontWeight: "bold",
              "&:hover": { backgroundColor: colors.blueAccent[800] },
            }}
          >
            Quản lý quyền
          </Button>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate("/roles")}
            variant="outlined"
            sx={{ color: colors.grey[100], borderColor: colors.grey[400] }}
          >
            Quay lại
          </Button>
        </Box>
      </Box>

      {/* INFO CARD */}
      {roleInfo && (
        <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
          <CardContent>
            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
                <Typography variant="body2" color={colors.grey[300]}>Tên Role:</Typography>
                <Typography variant="h5" color={colors.greenAccent[500]} fontWeight="bold">
                  {roleInfo.roleName}
                </Typography>
              </Grid>
              <Grid item xs={12} md={4}>
                <Typography variant="body2" color={colors.grey[300]}>Phạm vi:</Typography>
                <Box display="flex" alignItems="center" gap={1}>
                  {roleInfo.scope === "staff" ? <SecurityIcon fontSize="small" /> : <VerifiedUserIcon fontSize="small" />}
                  <Typography textTransform="capitalize">{roleInfo.scope}</Typography>
                </Box>
              </Grid>
              <Grid item xs={12} md={4}>
                <Typography variant="body2" color={colors.grey[300]}>Mô tả:</Typography>
                <Typography>{roleInfo.roleDescription || "Không có mô tả"}</Typography>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      )}

      {/* VIEW TABLE (READ ONLY) */}
      <Box mb={1}>
        <Typography variant="h5" color={colors.grey[100]} fontWeight="bold">
          Danh sách quyền hiện có ({rolePermissions.length})
        </Typography>
      </Box>
      <Box height="60vh" sx={{
          "& .MuiDataGrid-root": { border: "none" },
          "& .MuiDataGrid-cell": { borderBottom: `1px solid ${colors.primary[400]}` },
          "& .name-column--cell": { color: colors.greenAccent[300] },
          "& .MuiDataGrid-columnHeaders": { backgroundColor: colors.blueAccent[700] },
          "& .MuiDataGrid-virtualScroller": { backgroundColor: colors.primary[400] },
          "& .MuiDataGrid-footerContainer": { borderTop: "none", backgroundColor: colors.blueAccent[700] },
      }}>
        <DataGrid
          rows={rolePermissions}
          columns={viewColumns}
          disableSelectionOnClick
          slots={{ toolbar: GridToolbar }}
          slotProps={{ toolbar: { showQuickFilter: true, printOptions: { disableToolbarButton: true } } }}
        />
      </Box>

      <Dialog
        open={openDialog}
        onClose={() => setOpenDialog(false)}
        fullWidth
        maxWidth="md"
        PaperProps={{
          sx: { backgroundColor: colors.primary[400], backgroundImage: "none", height: '80vh' },
        }}
      >
        <DialogTitle sx={{ borderBottom: `1px solid ${colors.primary[500]}`, pb: 2 }}>
            <Box display="flex" justifyContent="space-between" alignItems="center">
                <Typography variant="h4" fontWeight="bold" color={colors.grey[100]}>
                    QUẢN LÝ QUYỀN HẠN
                </Typography>
                <IconButton onClick={() => setOpenDialog(false)}>
                    <CancelIcon sx={{ color: colors.grey[100] }} />
                </IconButton>
            </Box>
            
            {/* SEARCH BAR & TOOLS */}
            <Box mt={2} display="flex" gap={1}>
                <TextField 
                    fullWidth 
                    variant="outlined" 
                    size="small"
                    placeholder="Tìm kiếm quyền theo Tên hoặc Code..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    InputProps={{
                        startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>
                    }}
                />
                <Button 
                    variant="outlined" 
                    color="inherit" 
                    startIcon={<SelectAllIcon />}
                    onClick={handleSelectAll}
                    sx={{ minWidth: '120px', borderColor: colors.grey[500], color: colors.grey[100] }}
                >
                    Chọn hết
                </Button>
                <Button 
                    variant="outlined" 
                    color="inherit" 
                    startIcon={<DeselectIcon />}
                    onClick={handleDeselectAll}
                    sx={{ minWidth: '120px', borderColor: colors.grey[500], color: colors.grey[100] }}
                >
                    Bỏ chọn
                </Button>
            </Box>
        </DialogTitle>

        <DialogContent sx={{ p: 0 }}>
            {/* LIST PERMISSIONS */}
            <List sx={{ width: '100%', bgcolor: 'transparent' }}>
                {filteredList.length === 0 ? (
                    <Box p={4} textAlign="center">
                        <Typography color={colors.grey[300]}>Không tìm thấy quyền nào phù hợp.</Typography>
                    </Box>
                ) : (
                    filteredList.map((permission) => {
                        const labelId = `checkbox-list-label-${permission.permissionID}`;
                        const isChecked = selectedIds.indexOf(permission.permissionID) !== -1;

                        return (
                            <div key={permission.permissionID}>
                                <ListItem
                                    disablePadding
                                    secondaryAction={
                                        <Chip 
                                            label={permission.scope || "Global"} 
                                            size="small" 
                                            sx={{ 
                                                backgroundColor: permission.scope === 'user' ? colors.blueAccent[800] : colors.redAccent[800],
                                                borderRadius: '4px'
                                            }}
                                        />
                                    }
                                >
                                    <ListItemButton role={undefined} onClick={handleToggle(permission.permissionID)} dense>
                                        <ListItemIcon>
                                            <Checkbox
                                                edge="start"
                                                checked={isChecked}
                                                tabIndex={-1}
                                                disableRipple
                                                inputProps={{ 'aria-labelledby': labelId }}
                                                sx={{
                                                    color: colors.greenAccent[200],
                                                    '&.Mui-checked': {
                                                        color: colors.greenAccent[500],
                                                    },
                                                }}
                                            />
                                        </ListItemIcon>
                                        <ListItemText 
                                            id={labelId} 
                                            primary={
                                                <Typography variant="body1" fontWeight="bold" color={colors.grey[100]}>
                                                    {permission.permissionName}
                                                </Typography>
                                            }
                                            secondary={
                                                <Box component="span" display="flex" flexDirection="column">
                                                    <Typography variant="caption" fontFamily="monospace" color={colors.greenAccent[400]}>
                                                        {permission.code}
                                                    </Typography>
                                                    <Typography variant="caption" color={colors.grey[400]} noWrap>
                                                        {permission.permissionDescription}
                                                    </Typography>
                                                </Box>
                                            }
                                        />
                                    </ListItemButton>
                                </ListItem>
                                <Divider component="li" sx={{ borderColor: colors.primary[500] }} />
                            </div>
                        );
                    })
                )}
            </List>
        </DialogContent>

        <DialogActions sx={{ p: 2, borderTop: `1px solid ${colors.primary[500]}`, justifyContent: "space-between", bgcolor: colors.primary[400] }}>
            <Typography variant="body1" sx={{ ml: 1, color: colors.greenAccent[400] }}>
                Đang chọn: <b>{selectedIds.length}</b> quyền
            </Typography>
            <Box display="flex" gap={1}>
                <Button onClick={() => setOpenDialog(false)} sx={{ color: colors.grey[200] }}>
                    Hủy bỏ
                </Button>
                <Button
                    onClick={handleSave}
                    variant="contained"
                    disabled={saving}
                    startIcon={saving ? <CircularProgress size={20} /> : <SaveIcon />}
                    sx={{
                        backgroundColor: colors.greenAccent[600],
                        "&:hover": { backgroundColor: colors.greenAccent[700] },
                    }}
                >
                    {saving ? "Đang lưu..." : "Lưu thay đổi"}
                </Button>
            </Box>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default RoleDetail;