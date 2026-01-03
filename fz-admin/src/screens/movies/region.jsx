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
  InputAdornment,
} from "@mui/material";
import { DataGrid, GridToolbar } from "@mui/x-data-grid";
import { tokens } from "../../theme";
import Header from "../../components/Header";
import {
  getAllRegions,
  createRegion,
  updateRegion,
  deleteRegion,
} from "../../services/api";

// Icons
import AddIcon from "@mui/icons-material/Add";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import SearchIcon from "@mui/icons-material/Search";
import PublicIcon from "@mui/icons-material/Public"; 
import AbcIcon from "@mui/icons-material/Abc";

const Regions = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  // --- States ---
  const [regions, setRegions] = useState([]);
  const [filteredRegions, setFilteredRegions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState("");
  const [pageSize, setPageSize] = useState(10);

  // Dialog States
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogMode, setDialogMode] = useState("add"); // 'add' | 'edit'
  const [currentRegion, setCurrentRegion] = useState({
    regionID: 0,
    name: "",
    code: "",
    description: "",
  });

  const fetchRegions = useCallback(async () => {
    try {
      setLoading(true);
      const response = await getAllRegions();
      if (response.data && response.data.errorCode === 200) {
        const mappedData = response.data.data.map((item) => ({
          ...item,
          id: item.regionID,
        }));
        setRegions(mappedData);
        setFilteredRegions(mappedData);
      }
    } catch (error) {
      console.error("Error fetching regions:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchRegions();
  }, [fetchRegions]);

  useEffect(() => {
    const lowerText = searchText.toLowerCase();
    const filtered = regions.filter(
      (item) =>
        item.name.toLowerCase().includes(lowerText) ||
        item.code.toLowerCase().includes(lowerText) ||
        (item.description && item.description.toLowerCase().includes(lowerText))
    );
    setFilteredRegions(filtered);
  }, [searchText, regions]);

  const handleOpenDialog = (mode, region = null) => {
    setDialogMode(mode);
    if (mode === "edit" && region) {
      setCurrentRegion({
        regionID: region.regionID,
        name: region.name,
        code: region.code,
        description: region.description || "",
      });
    } else {
      setCurrentRegion({
        regionID: 0,
        name: "",
        code: "",
        description: "",
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
  };

  const handleSubmit = async () => {
    if (!currentRegion.name || !currentRegion.code) {
      alert("Vui lòng nhập Tên và Mã Region!");
      return;
    }

    try {
      let response;
      if (dialogMode === "add") {
        const { regionID, ...payload } = currentRegion;
        response = await createRegion(payload);
      } else {
        response = await updateRegion(currentRegion);
      }

      if (response.data && (response.data.errorCode === 200 || response.data.errorCode === 201)) {
        alert(dialogMode === "add" ? "Thêm Region thành công!" : "Cập nhật Region thành công!");
        handleCloseDialog();
        fetchRegions(); 
      } else {
        alert(response.data.errorMessage || "Có lỗi xảy ra!");
      }
    } catch (error) {
      console.error("Error submitting form:", error);
      alert("Lỗi kết nối đến server.");
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa Region này? Hành động này có thể ảnh hưởng đến phim thuộc vùng này.")) {
      try {
        const response = await deleteRegion(id);
        if (response.data.errorCode === 200) {
          alert("Xóa thành công!");
          fetchRegions();
        } else {
          alert(response.data.errorMessage || "Xóa thất bại!");
        }
      } catch (error) {
        console.error("Error deleting region:", error);
        alert("Có lỗi xảy ra khi xóa.");
      }
    }
  };

  const columns = [
    {
      field: "regionID",
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
      field: "name",
      headerName: "Tên Quốc gia / Khu vực",
      flex: 1,
      minWidth: 200,
      headerAlign: "left",
      align: "left",
      renderCell: ({ value }) => (
        <Box display="flex" alignItems="center" gap={1} height="100%">
          <PublicIcon fontSize="small" sx={{ color: colors.blueAccent[500] }} />
          <Typography variant="body2" fontWeight="bold" color={colors.grey[100]}>
            {value}
          </Typography>
        </Box>
      ),
    },
    {
      field: "code",
      headerName: "Mã (Code)",
      width: 120,
      headerAlign: "center",
      align: "center",
      renderCell: ({ value }) => (
        <Box display="flex" justifyContent="center" alignItems="center" height="100%">
          <Typography 
            variant="body2" 
            sx={{ 
                fontFamily: "monospace", 
                backgroundColor: colors.primary[500],
                padding: "2px 8px",
                borderRadius: "4px",
                color: colors.greenAccent[300]
            }}
          >
            {value.toUpperCase()}
          </Typography>
        </Box>
      ),
    },
    {
      field: "description",
      headerName: "Mô tả",
      flex: 1.5,
      minWidth: 250,
      headerAlign: "left",
      align: "left",
      renderCell: ({ value }) => (
        <Box display="flex" alignItems="center" height="100%">
          <Typography variant="body2" noWrap title={value} color={colors.grey[300]}>
            {value || "Không có mô tả"}
          </Typography>
        </Box>
      ),
    },
    {
      field: "actions",
      headerName: "Hành động",
      width: 120,
      sortable: false,
      headerAlign: "center",
      align: "center",
      renderCell: ({ row }) => (
        <Box display="flex" justifyContent="center" alignItems="center" gap={1} height="100%">
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
              onClick={() => handleDelete(row.regionID)}
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
      {/* HEADER */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Header title="QUẢN LÝ REGION" subtitle="Danh sách quốc gia và khu vực phát hành phim" />
        
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
          Thêm Region
        </Button>
      </Box>

      {/* SEARCH BAR */}
      <Box backgroundColor={colors.primary[400]} borderRadius="3px" p={2} mb={2}>
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Tìm kiếm Region theo tên, mã code..."
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
          rows={filteredRegions}
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
          sx: {
            backgroundColor: colors.primary[400],
            backgroundImage: "none",
          }
        }}
      >
        <DialogTitle sx={{ borderBottom: `1px solid ${colors.primary[500]}` }}>
          <Typography variant="h4" color={colors.grey[100]} fontWeight="bold">
            {dialogMode === "add" ? "THÊM REGION MỚI" : "CẬP NHẬT REGION"}
          </Typography>
        </DialogTitle>
        <DialogContent sx={{ mt: 2 }}>
          <Box display="flex" flexDirection="column" gap={3} mt={1}>
            <TextField
              label="Tên Region (Name)"
              fullWidth
              variant="filled"
              value={currentRegion.name}
              onChange={(e) => setCurrentRegion({ ...currentRegion, name: e.target.value })}
              required
              helperText="Ví dụ: United States, Vietnam, Global"
            />
            
            <TextField
              label="Mã Code"
              fullWidth
              variant="filled"
              value={currentRegion.code}
              onChange={(e) => setCurrentRegion({ ...currentRegion, code: e.target.value })}
              required
              helperText="Mã ISO 2 ký tự hoặc 3 ký tự (VD: US, VN, GLO)"
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <AbcIcon />
                  </InputAdornment>
                ),
              }}
            />

            <TextField
              label="Mô tả"
              fullWidth
              multiline
              rows={3}
              variant="filled"
              value={currentRegion.description}
              onChange={(e) => setCurrentRegion({ ...currentRegion, description: e.target.value })}
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

export default Regions;