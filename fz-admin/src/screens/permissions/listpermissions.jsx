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
  MenuItem,
} from "@mui/material";
// 1. Import GridToolbar
import { DataGrid, GridToolbar } from "@mui/x-data-grid";
import { tokens } from "../../theme";
import Header from "../../components/Header";
import {
  getAllPermissions,
  addPermission,
  updatePermission,
  deletePermission,
} from "../../services/api";

import AddIcon from "@mui/icons-material/Add";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import SearchIcon from "@mui/icons-material/Search";
import VpnKeyIcon from "@mui/icons-material/VpnKey";

const Permissions = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const isAdmin = localStorage.getItem("isAdmin") === "true";
  const [permissions, setPermissions] = useState([]);
  const [filteredPermissions, setFilteredPermissions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState("");
  const [pageSize, setPageSize] = useState(10);

  const [openDialog, setOpenDialog] = useState(false);
  const [dialogMode, setDialogMode] = useState("add");
  const [currentPermission, setCurrentPermission] = useState({
    permissionID: 0,
    permissionName: "",
    permissionDescription: "",
    code: "",
    scope: "",
  });

  const fetchPermissions = useCallback(async () => {
    try {
      setLoading(true);
      const response = await getAllPermissions();
      if (response.data && response.data.errorCode === 200) {
        const mappedData = response.data.data.map((item) => ({
          ...item,
          id: item.permissionID,
        }));
        setPermissions(mappedData);
        setFilteredPermissions(mappedData);
      }
    } catch (error) {
      console.error("Error fetching permissions:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchPermissions();
  }, [fetchPermissions]);

  useEffect(() => {
    const lowerText = searchText.toLowerCase();
    const filtered = permissions.filter(
      (item) =>
        item.permissionName.toLowerCase().includes(lowerText) ||
        item.code.toLowerCase().includes(lowerText) ||
        (item.scope && item.scope.toLowerCase().includes(lowerText))
    );
    setFilteredPermissions(filtered);
  }, [searchText, permissions]);

  const handleOpenDialog = (mode, permission = null) => {
    setDialogMode(mode);
    if (mode === "edit" && permission) {
      setCurrentPermission(permission);
    } else {
      setCurrentPermission({
        permissionID: 0,
        permissionName: "",
        permissionDescription: "",
        code: "",
        scope: "user", 
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
  };

  const handleSubmit = async () => {
    if (!currentPermission.permissionName || !currentPermission.code) {
      alert("Vui lòng nhập Tên và Code!");
      return;
    }
    try {
      let response;
      if (dialogMode === "add") {
        const { permissionID, ...payload } = currentPermission;
        response = await addPermission(payload);
      } else {
        response = await updatePermission(currentPermission);
      }

      if (response.data && (response.data.errorCode === 200 || response.data.errorCode === 201)) {
        alert(dialogMode === "add" ? "Thêm mới thành công!" : "Cập nhật thành công!");
        handleCloseDialog();
        fetchPermissions();
      } else {
        alert(response.data.errorMessage || "Có lỗi xảy ra!");
      }
    } catch (error) {
      console.error("Error submitting form:", error);
      alert("Lỗi kết nối đến server.");
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa Permission này?")) {
      try {
        const response = await deletePermission(id);
        if (response.data.errorCode === 200) {
          alert("Xóa thành công!");
          fetchPermissions();
        } else {
          alert(response.data.errorMessage || "Xóa thất bại!");
        }
      } catch (error) {
        console.error("Error deleting:", error);
        alert("Lỗi khi xóa permission.");
      }
    }
  };


  const columns = [
    {
      field: "permissionID",
      headerName: "ID",
      width: 60,
      headerAlign: "center", 
      align: "center",   
      hideable: false,   
    },
    {
      field: "permissionName",
      headerName: "Tên Quyền",
      flex: 1,
      minWidth: 150,
      headerAlign: "left",
      align: "left",
      hideable: false,
      renderCell: ({ value }) => (
        <Box display="flex" alignItems="center" height="100%">
          <Typography variant="body2" fontWeight="bold" color={colors.greenAccent[300]}>
            {value}
          </Typography>
        </Box>
      ),
    },
    {
      field: "code",
      headerName: "Code (Key)",
      flex: 1,
      minWidth: 180,
      headerAlign: "left",
      align: "left",
      renderCell: ({ value }) => (
        <Box display="flex" alignItems="center" gap={1} height="100%">
          <VpnKeyIcon fontSize="small" sx={{ color: colors.grey[400] }} />
          <Typography variant="body2" fontFamily="monospace">
            {value}
          </Typography>
        </Box>
      ),
    },
    {
      field: "scope",
      headerName: "Scope",
      width: 120,
      headerAlign: "center",
      align: "center",
      renderCell: ({ value }) => (
        <Box display="flex" justifyContent="center" alignItems="center" height="100%">
          <Chip
            label={value || "Global"}
            size="small"
            sx={{
              backgroundColor: colors.blueAccent[700],
              color: colors.grey[100],
              height: "24px", 
            }}
          />
        </Box>
      ),
    },
    {
      field: "permissionDescription",
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
      )
    },
    {
      field: "actions",
      headerName: "Hành động",
      width: 120,
      sortable: false,
      headerAlign: "center",
      align: "center",
      renderCell: ({ row }) => (
        <Box display="flex" justifyContent="center" alignItems="center" height="100%" gap={1}>
          <Tooltip title="Chỉnh sửa">
            <IconButton
              onClick={() => handleOpenDialog("edit", row)}
              size="small"
              sx={{ color: colors.greenAccent[400] }}
            >
              <EditOutlinedIcon />
            </IconButton>
          </Tooltip>

          <Tooltip title="Xóa">
            <IconButton
              onClick={() => handleDelete(row.permissionID)}
              size="small"
              sx={{ color: colors.redAccent[500] }}
            >
              <DeleteOutlineIcon />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Header title="QUẢN LÝ QUYỀN (PERMISSIONS)" subtitle="Danh sách các quyền hạn trong hệ thống" />
        
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
          Thêm mới
        </Button>
      </Box>

      {/* SEARCH BAR */}
      <Box
        backgroundColor={colors.primary[400]}
        borderRadius="3px"
        p={2}
        mb={2}
      >
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Tìm kiếm nhanh..."
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

      <Box
        height="70vh"
        sx={{
          "& .MuiDataGrid-root": {
            border: "none",
          },
          "& .MuiDataGrid-cell": {
            borderBottom: `1px solid ${colors.primary[400]}`,
          },
          "& .name-column--cell": {
            color: colors.greenAccent[300],
          },
          "& .MuiDataGrid-columnHeaders": {
            backgroundColor: colors.blueAccent[700],
            borderBottom: "none",
            fontSize: "14px",
          },
          "& .MuiDataGrid-virtualScroller": {
            backgroundColor: colors.primary[400],
          },
          "& .MuiDataGrid-footerContainer": {
            borderTop: "none",
            backgroundColor: colors.blueAccent[700],
          },
          "& .MuiCheckbox-root": {
            color: `${colors.greenAccent[200]} !important`,
          },
          "& .MuiDataGrid-toolbarContainer .MuiButton-text": {
            color: `${colors.grey[100]} !important`,
          },
        }}
      >
        <DataGrid
          rows={filteredPermissions}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          onPageSizeChange={(newPageSize) => setPageSize(newPageSize)}
          rowsPerPageOptions={[10, 20, 50, 100]}
          disableSelectionOnClick
          slots={{
            toolbar: GridToolbar,
          }}
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
          sx: {
            backgroundColor: colors.primary[400],
            backgroundImage: "none",
          }
        }}
      >
        <DialogTitle sx={{ borderBottom: `1px solid ${colors.primary[500]}` }}>
          <Typography variant="h4" color={colors.grey[100]} fontWeight="bold">
            {dialogMode === "add" ? "THÊM QUYỀN MỚI" : "CHỈNH SỬA QUYỀN"}
          </Typography>
        </DialogTitle>
        <DialogContent sx={{ mt: 2 }}>
          <Box display="flex" flexDirection="column" gap={3} mt={1}>
            <TextField
              label="Tên Quyền (Permission Name)"
              fullWidth
              variant="filled"
              value={currentPermission.permissionName}
              onChange={(e) => setCurrentPermission({...currentPermission, permissionName: e.target.value})}
              required
            />
            <TextField
              label="Code (VD: account.read)"
              fullWidth
              variant="filled"
              value={currentPermission.code}
              onChange={(e) => setCurrentPermission({...currentPermission, code: e.target.value})}
              required
            />

            {isAdmin ? (
              <TextField
                select 
                label="Scope (Phạm vi)"
                fullWidth
                variant="filled"
                value={currentPermission.scope || "user"}
                onChange={(e) => setCurrentPermission({...currentPermission, scope: e.target.value})}
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

              />
            )}

            <TextField
              label="Mô tả"
              fullWidth
              multiline
              rows={3}
              variant="filled"
              value={currentPermission.permissionDescription}
              onChange={(e) => setCurrentPermission({...currentPermission, permissionDescription: e.target.value})}
            />
          </Box>
        </DialogContent>
         <DialogActions sx={{ p: 2, borderTop: `1px solid ${colors.primary[500]}` }}>
          <Button onClick={handleCloseDialog} sx={{ color: colors.grey[200] }}>
            Hủy bỏ
          </Button>
          <Button 
            onClick={handleSubmit} 
            variant="contained"
            sx={{ 
              backgroundColor: colors.greenAccent[600],
              "&:hover": { backgroundColor: colors.greenAccent[700] }
            }}
          >
            {dialogMode === "add" ? "Thêm mới" : "Lưu thay đổi"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Permissions;