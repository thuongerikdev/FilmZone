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
  getAllTags,
  createTag,
  updateTag,
  deleteTag,
} from "../../services/api";

// Icons
import AddIcon from "@mui/icons-material/Add";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import SearchIcon from "@mui/icons-material/Search";
import LocalOfferIcon from "@mui/icons-material/LocalOffer"; 
import LabelIcon from "@mui/icons-material/Label";

const Tags = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  // --- States ---
  const [tags, setTags] = useState([]);
  const [filteredTags, setFilteredTags] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState("");
  const [pageSize, setPageSize] = useState(10);

  // Dialog States
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogMode, setDialogMode] = useState("add"); // 'add' | 'edit'
  const [currentTag, setCurrentTag] = useState({
    tagID: 0,
    tagName: "",
    tagDescription: "",
  });

  const fetchTags = useCallback(async () => {
    try {
      setLoading(true);
      const response = await getAllTags();
      if (response.data && response.data.errorCode === 200) {
        const mappedData = response.data.data.map((item) => ({
          ...item,
          id: item.tagID,
        }));
        setTags(mappedData);
        setFilteredTags(mappedData);
      }
    } catch (error) {
      console.error("Error fetching tags:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchTags();
  }, [fetchTags]);

  useEffect(() => {
    const lowerText = searchText.toLowerCase();
    const filtered = tags.filter(
      (item) =>
        item.tagName.toLowerCase().includes(lowerText) ||
        (item.tagDescription && item.tagDescription.toLowerCase().includes(lowerText))
    );
    setFilteredTags(filtered);
  }, [searchText, tags]);

  const handleOpenDialog = (mode, tag = null) => {
    setDialogMode(mode);
    if (mode === "edit" && tag) {
      setCurrentTag({
        tagID: tag.tagID,
        tagName: tag.tagName,
        tagDescription: tag.tagDescription || "",
      });
    } else {
      setCurrentTag({
        tagID: 0,
        tagName: "",
        tagDescription: "",
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
  };

  const handleSubmit = async () => {
    if (!currentTag.tagName) {
      alert("Vui lòng nhập Tên Tag!");
      return;
    }

    try {
      let response;
      if (dialogMode === "add") {
        const { tagID, ...payload } = currentTag;
        response = await createTag(payload);
      } else {
        response = await updateTag(currentTag);
      }

      if (response.data && (response.data.errorCode === 200 || response.data.errorCode === 201)) {
        alert(dialogMode === "add" ? "Thêm Tag thành công!" : "Cập nhật Tag thành công!");
        handleCloseDialog();
        fetchTags(); 
      } else {
        alert(response.data.errorMessage || "Có lỗi xảy ra!");
      }
    } catch (error) {
      console.error("Error submitting form:", error);
      alert("Lỗi kết nối đến server.");
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa Tag này? Các phim gắn thẻ này có thể bị ảnh hưởng.")) {
      try {
        const response = await deleteTag(id);
        if (response.data.errorCode === 200) {
          alert("Xóa thành công!");
          fetchTags();
        } else {
          alert(response.data.errorMessage || "Xóa thất bại!");
        }
      } catch (error) {
        console.error("Error deleting tag:", error);
        alert("Có lỗi xảy ra khi xóa.");
      }
    }
  };

  const columns = [
    {
      field: "tagID",
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
      field: "tagName",
      headerName: "Tên Thể loại (Tag)",
      flex: 1,
      minWidth: 200,
      headerAlign: "left",
      align: "left",
      renderCell: ({ value }) => (
        <Box display="flex" alignItems="center" gap={1} height="100%">
          <LocalOfferIcon fontSize="small" sx={{ color: colors.greenAccent[500] }} />
          <Typography variant="body2" fontWeight="bold" color={colors.grey[100]}>
            {value}
          </Typography>
        </Box>
      ),
    },
    {
      field: "tagDescription",
      headerName: "Mô tả",
      flex: 2,
      minWidth: 300,
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
              onClick={() => handleDelete(row.tagID)}
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
        <Header title="QUẢN LÝ TAG (THỂ LOẠI)" subtitle="Danh sách các nhãn/thể loại phim" />
        
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
          Thêm Tag
        </Button>
      </Box>

      {/* SEARCH BAR */}
      <Box backgroundColor={colors.primary[400]} borderRadius="3px" p={2} mb={2}>
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Tìm kiếm Tag theo tên hoặc mô tả..."
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
          rows={filteredTags}
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
            {dialogMode === "add" ? "THÊM TAG MỚI" : "CẬP NHẬT TAG"}
          </Typography>
        </DialogTitle>
        <DialogContent sx={{ mt: 2 }}>
          <Box display="flex" flexDirection="column" gap={3} mt={1}>
            <TextField
              label="Tên Tag (Tag Name)"
              fullWidth
              variant="filled"
              value={currentTag.tagName}
              onChange={(e) => setCurrentTag({ ...currentTag, tagName: e.target.value })}
              required
              helperText="Ví dụ: Action, Horror, Comedy, 18+"
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <LabelIcon />
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
              value={currentTag.tagDescription}
              onChange={(e) => setCurrentTag({ ...currentTag, tagDescription: e.target.value })}
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

export default Tags;