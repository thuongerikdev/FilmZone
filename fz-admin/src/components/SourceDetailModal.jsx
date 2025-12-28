import React, { useEffect, useState } from "react";
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Typography,
  Grid,
  Chip,
  IconButton,
  useTheme,
  Divider,
  Tooltip,
  LinearProgress
} from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import CloseIcon from "@mui/icons-material/Close";
import LinkIcon from "@mui/icons-material/Link";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import DownloadIcon from "@mui/icons-material/Download";
import EditIcon from "@mui/icons-material/Edit";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import { tokens } from "../theme";

// Import API xóa
import { deleteMovieSubtitle, deleteEpisodeSubtitle } from "../services/api";
// Import Modal Update
import UpdateSubtitleModal from "./UpdateSubtitleModal";

const SourceDetailModal = ({ open, onClose, source, scope = 'movie' }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  const [subtitles, setSubtitles] = useState([]);
  const [loading, setLoading] = useState(false);

  // State cho Update Subtitle
  const [openUpdateSub, setOpenUpdateSub] = useState(false);
  const [selectedSub, setSelectedSub] = useState(null);

  // ID của source
  const sourceId = source ? (source.movieSourceID || source.episodeSourceID) : null;

  useEffect(() => {
    if (open && sourceId) {
      fetchSubtitles(sourceId);
    } else {
        setSubtitles([]);
    }
  }, [open, sourceId, scope]);

  const fetchSubtitles = async (id) => {
    setLoading(true);
    try {
      const endpointType = scope === 'movie' ? 'GetAllSubTitlesByMovieId/movie' : 'GetAllSubTitlesByEpisodeId/episode';
      const response = await fetch(`https://filmzone-api.koyeb.app/api/MovieSubTitle/${endpointType}/GetAllSubTitlesBySourceID/${id}`);
      const data = await response.json();
      
      if (data.errorCode === 200) {
        setSubtitles(data.data);
      } else {
        setSubtitles([]);
      }
    } catch (error) {
      console.error("Error fetching subtitles:", error);
      setSubtitles([]);
    } finally {
      setLoading(false);
    }
  };

  // --- HÀM XÓA SUBTITLE ---
  const handleDeleteSub = async (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa Subtitle này không?")) {
        try {
            let res;
            if (scope === "movie") {
                res = await deleteMovieSubtitle(id);
            } else {
                res = await deleteEpisodeSubtitle(id);
            }

            const data = res.data || res;
            if (res.errorCode === 200) {
                alert("Xóa subtitle thành công!");
                fetchSubtitles(sourceId); // Refresh list
            } else {
                alert(data?.errorMessage || "Xóa thất bại.");
            }
        } catch (error) {
            console.error("Error deleting sub:", error);
            alert("Lỗi khi xóa subtitle.");
        }
    }
  };

  // --- HÀM MỞ UPDATE MODAL ---
  const handleEditSub = (sub) => {
    setSelectedSub(sub);
    setOpenUpdateSub(true);
  };

  const formatDateTime = (dateString) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleString('vi-VN');
  };

  // Columns
  const subtitleColumns = [
    { 
      field: "id", 
      headerName: "ID", 
      width: 60, 
      // [FIXED] Sửa valueGetter để tương thích MUI v6+
      valueGetter: (value, row) => {
         // Nếu row nằm ở tham số thứ 2 (MUI v6+)
         if (row) return row.movieSubTitleID || row.episodeSubTitleID;
         // Fallback nếu dùng MUI v5 (row nằm trong value)
         return value?.row?.movieSubTitleID || value?.row?.episodeSubTitleID;
      }
    },
    { field: "subTitleName", headerName: "Tên Sub", flex: 1, minWidth: 150 },
    { field: "language", headerName: "Ngôn ngữ", width: 100 },
    { 
        field: "isActive", headerName: "Active", width: 80, align: "center",
        renderCell: ({ value }) => (value ? <CheckCircleIcon sx={{ color: colors.greenAccent[500] }} fontSize="small"/> : <CancelIcon sx={{ color: colors.redAccent[500] }} fontSize="small"/>)
    },
    { 
        field: "createdAt", headerName: "Ngày tạo", width: 150, 
        renderCell: ({ value }) => formatDateTime(value)
    },
    {
      field: "actions", headerName: "Hành động", width: 150, sortable: false, align: "center",
      renderCell: ({ row }) => {
        // Xác định ID dựa trên scope (movieSubTitleID hoặc episodeSubTitleID)
        const subId = row.movieSubTitleID || row.episodeSubTitleID;
        return (
            <Box display="flex" justifyContent="center" alignItems="center" height="100%">
                {/* Download */}
                <Tooltip title="Tải xuống">
                    <IconButton onClick={() => window.open(row.linkSubTitle, "_blank")} sx={{ color: colors.blueAccent[400] }}>
                        <DownloadIcon fontSize="small" />
                    </IconButton>
                </Tooltip>
                
                {/* Edit */}
                <Tooltip title="Chỉnh sửa">
                    <IconButton onClick={() => handleEditSub(row)} sx={{ color: colors.greenAccent[400] }}>
                        <EditIcon fontSize="small" />
                    </IconButton>
                </Tooltip>

                {/* Delete */}
                <Tooltip title="Xóa">
                    <IconButton onClick={() => handleDeleteSub(subId)} sx={{ color: colors.redAccent[500] }}>
                        <DeleteOutlineIcon fontSize="small" />
                    </IconButton>
                </Tooltip>
            </Box>
        );
      },
    },
  ];

  if (!source) return null;

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ backgroundColor: colors.blueAccent[700], color: colors.grey[100] }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight="bold">Chi tiết Source #{sourceId}</Typography>
          <IconButton onClick={onClose}>
            <CloseIcon sx={{ color: colors.grey[100] }} />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent sx={{ backgroundColor: colors.primary[400], pt: 3 }}>
        
        {/* INFO SOURCE */}
        <Typography variant="h5" color={colors.greenAccent[500]} fontWeight="bold" mb={2}>Thông tin Source</Typography>
        <Grid container spacing={2} mb={3}>
            <Grid item xs={6} md={3}><Typography variant="subtitle2" color={colors.grey[400]}>Tên Source:</Typography><Typography>{source.sourceName}</Typography></Grid>
            <Grid item xs={6} md={3}><Typography variant="subtitle2" color={colors.grey[400]}>Chất lượng:</Typography><Typography>{source.quality}</Typography></Grid>
            <Grid item xs={6} md={3}><Typography variant="subtitle2" color={colors.grey[400]}>Ngôn ngữ:</Typography><Typography>{source.language}</Typography></Grid>
            <Grid item xs={6} md={3}><Typography variant="subtitle2" color={colors.grey[400]}>Loại:</Typography><Typography>{source.sourceType}</Typography></Grid>
            
            <Grid item xs={12}>
                <Typography variant="subtitle2" color={colors.grey[400]}>Link Gốc:</Typography>
                <Box display="flex" alignItems="center" gap={1} bgcolor={colors.primary[500]} p={1} borderRadius={1}>
                    <Typography noWrap sx={{flex: 1, fontFamily: 'monospace', fontSize: '0.85rem'}}>{source.sourceUrl}</Typography>
                    <IconButton size="small" onClick={() => window.open(source.sourceUrl, "_blank")}><LinkIcon /></IconButton>
                </Box>
            </Grid>
        </Grid>

        <Divider sx={{ my: 2, backgroundColor: colors.grey[600] }} />

        {/* SUBTITLE LIST */}
        <Typography variant="h5" color={colors.greenAccent[500]} fontWeight="bold" mb={2}>
            Danh sách Subtitle ({subtitles.length})
        </Typography>
        
        <Box height="300px" sx={{ width: '100%' }}>
            <DataGrid
                loading={loading}
                rows={subtitles}
                columns={subtitleColumns}
                // Sử dụng getRowId để tránh lỗi nếu ID không phải là 'id'
                getRowId={(row) => row.movieSubTitleID || row.episodeSubTitleID} 
                pageSize={5}
                rowsPerPageOptions={[5, 10]}
                sx={{
                    "& .MuiDataGrid-cell": { borderBottom: `1px solid ${colors.grey[700]}` },
                    "& .MuiDataGrid-columnHeaders": { backgroundColor: colors.blueAccent[800], borderBottom: "none" },
                    "& .MuiDataGrid-footerContainer": { borderTop: "none", backgroundColor: colors.blueAccent[800] },
                }}
            />
        </Box>

      </DialogContent>

      <DialogActions sx={{ backgroundColor: colors.blueAccent[700], p: 2 }}>
        <Button onClick={onClose} variant="contained" sx={{ backgroundColor: colors.grey[500] }}>
          Đóng
        </Button>
      </DialogActions>

      {/* RENDER MODAL UPDATE SUBTITLE */}
      <UpdateSubtitleModal 
        open={openUpdateSub}
        onClose={() => setOpenUpdateSub(false)}
        subtitle={selectedSub}
        scope={scope}
        onSuccess={() => fetchSubtitles(sourceId)} 
      />

    </Dialog>
  );
};

export default SourceDetailModal;