import { useState, useEffect, useCallback } from "react";
import {
  Box,
  useTheme,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
  Typography,
  IconButton,
  Tooltip,
  Chip,
  InputAdornment,
  FormControlLabel,
  Checkbox,
  MenuItem,
} from "@mui/material";
import { DataGrid, GridToolbar } from "@mui/x-data-grid";
import { tokens } from "../../theme";
import Header from "../../components/Header";
import {
  getAllRoles,
  addRole,
  updateRole,
  deleteRole,
  cloneRole, 
} from "../../services/api";

// Icons
import AddIcon from "@mui/icons-material/Add";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import SearchIcon from "@mui/icons-material/Search";
import AdminPanelSettingsIcon from "@mui/icons-material/AdminPanelSettings";
import VerifiedUserIcon from "@mui/icons-material/VerifiedUser";
import SecurityIcon from "@mui/icons-material/Security";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import ContentCopyIcon from '@mui/icons-material/ContentCopy'; 
import { useNavigate } from "react-router-dom";

const Roles = () => {
  const theme = useTheme();
  const navigate = useNavigate();
  const colors = tokens(theme.palette.mode);

  const isAdmin = localStorage.getItem("isAdmin") === "true";

  const [roles, setRoles] = useState([]);
  const [filteredRoles, setFilteredRoles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState("");
  const [pageSize, setPageSize] = useState(10);

  // State cho Add/Edit Dialog
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogMode, setDialogMode] = useState("add");
  const [currentRole, setCurrentRole] = useState({
    roleID: 0,
    roleName: "",
    roleDescription: "",
    scope: "user", 
    isDefault: false,
  });

  // --- 3. STATE CHO CLONE DIALOG ---
  const [openCloneDialog, setOpenCloneDialog] = useState(false);
  const [cloneData, setCloneData] = useState({
    sourceRoleId: 0,
    newRoleName: "",
    newRoleDescription: "",
    newScope: "user",
    isDefault: false,
  });

  const fetchRoles = useCallback(async () => {
    try {
      setLoading(true);
      const response = await getAllRoles();
      if (response.data && response.data.errorCode === 200) {
        const mappedData = response.data.data.map((item, index) => ({
          ...item,
          id: item.roleID || index, 
        }));
        setRoles(mappedData);
        setFilteredRoles(mappedData);
      }
    } catch (error) {
      console.error("Error fetching roles:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchRoles();
  }, [fetchRoles]);

  useEffect(() => {
    const lowerText = searchText.toLowerCase();
    const filtered = roles.filter(
      (item) =>
        item.roleName.toLowerCase().includes(lowerText) ||
        (item.roleDescription && item.roleDescription.toLowerCase().includes(lowerText)) ||
        (item.scope && item.scope.toLowerCase().includes(lowerText))
    );
    setFilteredRoles(filtered);
  }, [searchText, roles]);

  // --- Handlers cho Add/Edit ---
  const handleOpenDialog = (mode, role = null) => {
    setDialogMode(mode);
    if (mode === "edit" && role) {
      setCurrentRole(role);
    } else {
      setCurrentRole({
        roleID: 0,
        roleName: "",
        roleDescription: "",
        scope: "user", 
        isDefault: false,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
  };

  const handleSubmit = async () => {
    if (!currentRole.roleName) {
      alert("Vui lòng nhập Tên Role!");
      return;
    }

    try {
      let response;
      if (dialogMode === "add") {
        const { roleID, ...payload } = currentRole;
        response = await addRole(payload);
      } else {
        response = await updateRole(currentRole);
      }

      if (response.data && (response.data.errorCode === 200 || response.data.errorCode === 201)) {
        alert(dialogMode === "add" ? "Thêm Role thành công!" : "Cập nhật Role thành công!");
        handleCloseDialog();
        fetchRoles();
      } else {
        alert(response.data.errorMessage || "Có lỗi xảy ra!");
      }
    } catch (error) {
      console.error("Error submitting form:", error);
      alert("Lỗi kết nối đến server.");
    }
  };

  const handleDelete = async (roleID) => {
    if (!roleID && roleID !== 0) {
      alert("Không tìm thấy Role ID để xóa.");
      return;
    }
    if (window.confirm("Bạn có chắc chắn muốn xóa Role này?")) {
      try {
        const response = await deleteRole(roleID);
        if (response.data.errorCode === 200) {
          alert("Xóa Role thành công!");
          fetchRoles();
        } else {
          alert(response.data.errorMessage || "Xóa thất bại!");
        }
      } catch (error) {
        console.error("Error deleting role:", error);
        alert("Có lỗi xảy ra khi xóa.");
      }
    }
  };

  // --- 4. HANDLERS CHO CLONE ---
  const handleOpenClone = (row) => {
    setCloneData({
        sourceRoleId: row.roleID,
        newRoleName: `${row.roleName}_copy`, 
        newRoleDescription: row.roleDescription,
        newScope: row.scope || "user", 
        isDefault: false,
    });
    setOpenCloneDialog(true);
  };

  const handleCloneSubmit = async () => {
    if (!cloneData.newRoleName) {
        alert("Vui lòng nhập tên Role mới!");
        return;
    }

    try {
        const payload = {
            sourceRoleId: cloneData.sourceRoleId,
            newRoleName: cloneData.newRoleName,
            newRoleDescription: cloneData.newRoleDescription,
            isDefault: cloneData.isDefault
        };

        if (isAdmin) {
            payload.newScope = cloneData.newScope;
        }

        const response = await cloneRole(payload);

        if (response.data && (response.data.errorCode === 200 || response.data.errorCode === 201)) {
            alert("Sao chép Role thành công!");
            setOpenCloneDialog(false);
            fetchRoles();
        } else {
            alert(response.data.errorMessage || "Sao chép thất bại!");
        }

    } catch (error) {
        console.error("Error cloning role:", error);
        alert("Lỗi kết nối đến server.");
    }
  };


  const columns = [
    {
      field: "roleID",
      headerName: "ID",
      width: 60,
      headerAlign: "center",
      align: "center",
      hideable: false, 
      renderCell: ({ value }) => (
         <Box display="flex" justifyContent="center" alignItems="center" height="100%">
             {value}
         </Box>
      )
    },
    {
      field: "roleName",
      headerName: "Tên Role",
      flex: 1,
      minWidth: 150,
      headerAlign: "left",
      align: "left",
      hideable: false, 
      renderCell: ({ value }) => (
        <Box display="flex" alignItems="center" gap={1} height="100%">
          <AdminPanelSettingsIcon 
            fontSize="small" 
            sx={{ color: value === 'admin' ? colors.redAccent[500] : colors.greenAccent[500] }} 
          />
          <Typography variant="body2" fontWeight="bold" color={colors.grey[100]}>
            {value}
          </Typography>
        </Box>
      ),
    },
    {
      field: "scope",
      headerName: "Phạm vi",
      width: 120,
      headerAlign: "center",
      align: "center",
      renderCell: ({ value }) => {
        const isStaff = value === 'staff';
        return (
          <Box display="flex" justifyContent="center" alignItems="center" height="100%">
            <Chip
              label={value}
              size="small"
              icon={isStaff ? <SecurityIcon /> : <VerifiedUserIcon />}
              sx={{
                backgroundColor: isStaff ? colors.redAccent[700] : colors.blueAccent[700],
                color: colors.grey[100],
                textTransform: "capitalize",
                height: "24px"
              }}
            />
          </Box>
        );
      },
    },
    {
      field: "isDefault",
      headerName: "Mặc định",
      width: 100,
      headerAlign: "center",
      align: "center",
      renderCell: ({ value }) => (
        <Box display="flex" justifyContent="center" alignItems="center" height="100%">
          <Checkbox 
            checked={value} 
            disabled 
            size="small"
            sx={{ 
                color: colors.greenAccent[500],
                '&.Mui-checked': { color: colors.greenAccent[500] },
            }} 
          />
        </Box>
      ),
    },
    {
      field: "roleDescription",
      headerName: "Mô tả",
      flex: 1.5,
      minWidth: 200,
      headerAlign: "left",
      align: "left",
      renderCell: ({ value }) => (
        <Box display="flex" alignItems="center" height="100%">
          <Typography variant="body2" noWrap title={value}>
            {value}
          </Typography>
        </Box>
      ),
    },
    {
      field: "actions",
      headerName: "Hành động",
      width: 160, 
      sortable: false,
      headerAlign: "center",
      align: "center",
      renderCell: ({ row }) => (
        <Box display="flex" justifyContent="center" alignItems="center" gap={0.5} height="100%">
          <Tooltip title="Xem Permissions">
            <IconButton
              onClick={() => navigate(`/roles/${row.roleID}`, { state: { roleData: row } })}
              size="small"
              sx={{ color: colors.blueAccent[400] }}
            >
              <VisibilityOutlinedIcon fontSize="small" />
            </IconButton>
          </Tooltip>

          {/* --- 5. BUTTON CLONE --- */}
          <Tooltip title="Sao chép Role">
            <IconButton
              onClick={() => handleOpenClone(row)}
              size="small"
              sx={{ color: colors.grey[100] }}
            >
              <ContentCopyIcon fontSize="small" />
            </IconButton>
          </Tooltip>

          <Tooltip title="Chỉnh sửa">
            <IconButton
              onClick={() => handleOpenDialog("edit", row)}
              size="small"
              sx={{ color: colors.greenAccent[400] }}
            >
              <EditOutlinedIcon fontSize="small" />
            </IconButton>
          </Tooltip>

          <Tooltip title="Xóa">
            <IconButton
              onClick={() => handleDelete(row.roleID)}
              size="small"
              sx={{ color: colors.redAccent[500] }}
            >
              <DeleteOutlineIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Header title="QUẢN LÝ ROLES" subtitle="Phân quyền vai trò người dùng" />
        
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog("add")}
          sx={{
            backgroundColor: colors.blueAccent[700],
            color: colors.grey[100],
            fontWeight: "bold",
            padding: "10px 20px",
            "&:hover": { backgroundColor: colors.blueAccent[800] },
          }}
        >
          Thêm Role
        </Button>
      </Box>

      {/* SEARCH BAR */}
      <Box backgroundColor={colors.primary[400]} borderRadius="3px" p={2} mb={2}>
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Tìm kiếm Role theo tên, mô tả..."
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          size="small"
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          }}
        />
      </Box>

      {/* DATA GRID */}
      <Box
        height="70vh"
        sx={{
          "& .MuiDataGrid-root": { border: "none" },
          "& .MuiDataGrid-cell": { borderBottom: `1px solid ${colors.primary[400]}` },
          "& .name-column--cell": { color: colors.greenAccent[300] },
          "& .MuiDataGrid-columnHeaders": {
            backgroundColor: colors.blueAccent[700],
            borderBottom: "none",
            fontSize: "14px",
          },
          "& .MuiDataGrid-virtualScroller": { backgroundColor: colors.primary[400] },
          "& .MuiDataGrid-footerContainer": {
            borderTop: "none",
            backgroundColor: colors.blueAccent[700],
          },
          "& .MuiCheckbox-root": { color: `${colors.greenAccent[200]} !important` },
          "& .MuiDataGrid-toolbarContainer .MuiButton-text": {
            color: `${colors.grey[100]} !important`,
          },
        }}
      >
        <DataGrid
          rows={filteredRoles}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          onPageSizeChange={(newPageSize) => setPageSize(newPageSize)}
          rowsPerPageOptions={[10, 20, 50]}
          disableSelectionOnClick
          slots={{ toolbar: GridToolbar }}
          slotProps={{
            toolbar: {
              showQuickFilter: false,
              printOptions: { disableToolbarButton: true },
            },
          }}
        />
      </Box>

      {/* DIALOG ADD/EDIT */}
      <Dialog 
        open={openDialog} 
        onClose={handleCloseDialog}
        fullWidth
        maxWidth="sm"
        PaperProps={{
            sx: { backgroundColor: colors.primary[400], backgroundImage: "none" }
          }}
      >
        <DialogTitle sx={{ borderBottom: `1px solid ${colors.primary[500]}` }}>
          <Typography variant="h4" color={colors.grey[100]} fontWeight="bold">
            {dialogMode === "add" ? "THÊM ROLE MỚI" : "CẬP NHẬT ROLE"}
          </Typography>
        </DialogTitle>
        <DialogContent sx={{ mt: 2 }}>
          <Box display="flex" flexDirection="column" gap={3} mt={1}>
            <TextField
              label="Tên Role (Role Name)"
              fullWidth
              variant="filled"
              value={currentRole.roleName}
              onChange={(e) => setCurrentRole({ ...currentRole, roleName: e.target.value })}
              required
            />
            <Box display="flex" gap={2}>
                {isAdmin ? (
                    <TextField
                        select
                        label="Scope (Phạm vi)"
                        fullWidth
                        variant="filled"
                        value={currentRole.scope}
                        onChange={(e) => setCurrentRole({ ...currentRole, scope: e.target.value })}
                    >
                        <MenuItem value="user">User (Người dùng)</MenuItem>
                        <MenuItem value="staff">Staff (Nhân viên)</MenuItem>
                    </TextField>
                ) : (
                    <TextField
                        label="Scope (Phạm vi)"
                        fullWidth
                        variant="filled"
                        value="user"
                        disabled
                        helperText="Mặc định là User"
                    />
                )}
                <FormControlLabel
                control={
                    <Checkbox
                    checked={currentRole.isDefault}
                    onChange={(e) => setCurrentRole({ ...currentRole, isDefault: e.target.checked })}
                    sx={{ color: colors.greenAccent[500], '&.Mui-checked': { color: colors.greenAccent[500] } }}
                    />
                }
                label="Đặt làm mặc định"
                sx={{ width: '100%' }}
                />
            </Box>
            <TextField
              label="Mô tả"
              fullWidth
              multiline
              rows={3}
              variant="filled"
              value={currentRole.roleDescription}
              onChange={(e) => setCurrentRole({ ...currentRole, roleDescription: e.target.value })}
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 2, borderTop: `1px solid ${colors.primary[500]}` }}>
          <Button onClick={handleCloseDialog} sx={{ color: colors.grey[200] }}>Hủy bỏ</Button>
          <Button onClick={handleSubmit} variant="contained" sx={{ backgroundColor: colors.greenAccent[600], "&:hover": { backgroundColor: colors.greenAccent[700] } }}>
            {dialogMode === "add" ? "Thêm mới" : "Lưu thay đổi"}
          </Button>
        </DialogActions>
      </Dialog>

      {/* --- 6. DIALOG CLONE ROLE --- */}
      <Dialog 
        open={openCloneDialog} 
        onClose={() => setOpenCloneDialog(false)}
        fullWidth
        maxWidth="sm"
        PaperProps={{
            sx: { backgroundColor: colors.primary[400], backgroundImage: "none" }
        }}
      >
        <DialogTitle sx={{ borderBottom: `1px solid ${colors.primary[500]}` }}>
          <Typography variant="h4" color={colors.grey[100]} fontWeight="bold">
            SAO CHÉP ROLE
          </Typography>
          <Typography variant="caption" color={colors.grey[300]}>
            Tạo role mới dựa trên Role ID: {cloneData.sourceRoleId}
          </Typography>
        </DialogTitle>
        <DialogContent sx={{ mt: 2 }}>
          <Box display="flex" flexDirection="column" gap={3} mt={1}>
            <TextField
              label="Tên Role Mới"
              fullWidth
              variant="filled"
              value={cloneData.newRoleName}
              onChange={(e) => setCloneData({ ...cloneData, newRoleName: e.target.value })}
              required
              helperText="Ví dụ: customer-pro, admin-level-2"
            />

            <Box display="flex" gap={2}>
                {isAdmin ? (
                    <TextField
                        select
                        label="Scope Mới"
                        fullWidth
                        variant="filled"
                        value={cloneData.newScope}
                        onChange={(e) => setCloneData({ ...cloneData, newScope: e.target.value })}
                    >
                        <MenuItem value="user">User (Người dùng)</MenuItem>
                        <MenuItem value="staff">Staff (Nhân viên)</MenuItem>
                    </TextField>
                ) : (
                    <TextField
                        label="Scope Mới"
                        fullWidth
                        variant="filled"
                        value={cloneData.newScope}
                        disabled
                        helperText="Mặc định là User"
                    />
                )}

                <FormControlLabel
                control={
                    <Checkbox
                    checked={cloneData.isDefault}
                    onChange={(e) => setCloneData({ ...cloneData, isDefault: e.target.checked })}
                    sx={{ color: colors.greenAccent[500], '&.Mui-checked': { color: colors.greenAccent[500] } }}
                    />
                }
                label="Đặt làm mặc định"
                sx={{ width: '100%' }}
                />
            </Box>

            <TextField
              label="Mô tả Mới"
              fullWidth
              multiline
              rows={3}
              variant="filled"
              value={cloneData.newRoleDescription}
              onChange={(e) => setCloneData({ ...cloneData, newRoleDescription: e.target.value })}
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 2, borderTop: `1px solid ${colors.primary[500]}` }}>
          <Button onClick={() => setOpenCloneDialog(false)} sx={{ color: colors.grey[200] }}>Hủy bỏ</Button>
          <Button 
            onClick={handleCloneSubmit} 
            variant="contained" 
            startIcon={<ContentCopyIcon />}
            sx={{ backgroundColor: colors.blueAccent[600], "&:hover": { backgroundColor: colors.blueAccent[700] } }}
          >
            Sao chép
          </Button>
        </DialogActions>
      </Dialog>

    </Box>
  );
};

export default Roles;