import React, { useState, useEffect } from "react";
import {
  Box, Typography, Grid, Card, CardContent, Chip, Button, Divider,
  Accordion, AccordionSummary, AccordionDetails, IconButton, Tooltip, Dialog, AppBar, Toolbar, Slide
} from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { useTheme } from "@mui/material/styles";
import CloseIcon from "@mui/icons-material/Close";
import { tokens } from "../theme";

// Import API
import { deleteEpisode, deleteEpisodeSource } from "../services/api";

// Import các Modal xử lý
import UploadSourceModal from "./UploadSourceModal";
import AddSubtitleModal from "./AddSubtitleModal";
import SourceDetailModal from "./SourceDetailModal";
import TranslateSubtitleModal from "./TranslateSubtitleModal";
import UpdateEpisodeModal from "./UpdateEpisodeModal"; // <--- [NEW] Modal Update
import UpdateSourceModal from "./UpdateSourceModal";

// Icons
import VideoLibraryIcon from "@mui/icons-material/VideoLibrary";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import InfoIcon from "@mui/icons-material/Info";
import ClosedCaptionIcon from "@mui/icons-material/ClosedCaption";
import GTranslateIcon from "@mui/icons-material/GTranslate";
import LinkIcon from "@mui/icons-material/Link";
import DiamondIcon from "@mui/icons-material/Diamond";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import CalendarTodayIcon from "@mui/icons-material/CalendarToday";
import AccessTimeIcon from "@mui/icons-material/AccessTime";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditIcon from "@mui/icons-material/Edit"; // <--- [NEW] Icon Edit

// Hiệu ứng trượt lên khi mở Modal
const Transition = React.forwardRef(function Transition(props, ref) {
  return <Slide direction="up" ref={ref} {...props} />;
});



// Thêm prop onUpdateSuccess
const EpisodeDetailModal = ({ open, onClose, episode, movieTitle, onDeleteSuccess, onUpdateSuccess }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  const [sources, setSources] = useState([]);
  const [loading, setLoading] = useState(false);

  const [openUpdateSource, setOpenUpdateSource] = useState(false);
  const [selectedSourceForUpdate, setSelectedSourceForUpdate] = useState(null);
  // --- STATE MODALS (Riêng cho Episode) ---
  const [openUpload, setOpenUpload] = useState(false);
  
  const [openSubModal, setOpenSubModal] = useState(false);
  const [selectedSourceForSub, setSelectedSourceForSub] = useState(null);

  const [openSourceDetail, setOpenSourceDetail] = useState(false);
  const [selectedSourceDetail, setSelectedSourceDetail] = useState(null);

  const [openTranslateModal, setOpenTranslateModal] = useState(false);
  const [selectedSourceForTranslate, setSelectedSourceForTranslate] = useState(null);

  const [openUpdateModal, setOpenUpdateModal] = useState(false); // <--- [NEW] State Update

  const handleOpenUpdateSource = (source) => {
    setSelectedSourceForUpdate(source);
    setOpenUpdateSource(true);
  };
  const handleDeleteSource = async (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa Source tập này không?")) {
        try {
            const response = await deleteEpisodeSource(id);
            if (response.data.errorCode === 200) {
                alert("Xóa source thành công!");
                fetchEpisodeSources(); // Refresh list source của tập
            } else {
                alert(response.data.errorMessage || "Xóa thất bại.");
            }
        } catch (error) {
            console.error("Error deleting source:", error);
            alert("Lỗi khi xóa source.");
        }
    }
  };

  // --- HÀM XÓA EPISODE ---
  const handleDeleteCurrentEpisode = async () => {
    if (window.confirm(`CẢNH BÁO: Bạn đang muốn xóa "${episode.title}". Hành động này không thể hoàn tác?`)) {
        try {
            const response = await deleteEpisode(episode.episodeID);
            // Kiểm tra kết quả trả về từ BE (thường là errorCode 200)
            if (response.data.errorCode === 200 || response.data.status === 200) {
                alert("Đã xóa tập phim.");
                if (onDeleteSuccess) {
                    onDeleteSuccess(); // Gọi hàm refresh của cha (MovieDetail)
                } else {
                    onClose();
                }
            } else {
                alert(response.errorMessage || "Xóa thất bại");
            }
        } catch (error) {
            console.error(error);
            alert("Lỗi kết nối khi xóa");
        }
    }
  }

  useEffect(() => {
    if (open && episode) {
      fetchEpisodeSources();
    }
  }, [open, episode]);

  // API lấy Source của Episode
  const fetchEpisodeSources = async () => {
    setLoading(true);
    try {
      const response = await fetch(`https://filmzone-api.koyeb.app/movie/EpisodeSource/GetEpisodeSourcesByEpisodeId/${episode.episodeID}`);
      const data = await response.json();
      if (data.errorCode === 200) {
        setSources(data.data);
      } else {
        setSources([]);
      }
    } catch (error) {
      console.error("Error fetching episode sources:", error);
    } finally {
      setLoading(false);
    }
  };

  // --- HANDLERS ---
  const handleOpenSourceDetail = (source) => {
    setSelectedSourceDetail(source);
    setOpenSourceDetail(true);
  };
  const handleOpenSubModal = (source) => {
    setSelectedSourceForSub(source);
    setOpenSubModal(true);
  };
  const handleOpenTranslateModal = (source) => {
    setSelectedSourceForTranslate(source);
    setOpenTranslateModal(true);
  };

  // --- COLUMNS ---
  const sourceColumns = [
    { field: "episodeSourceID", headerName: "ID", width: 60 },
    { field: "sourceName", headerName: "Tên Source", flex: 1, minWidth: 150 },
    { 
      field: "quality", headerName: "Chất lượng", width: 100,
      renderCell: ({ value }) => (
        <Chip label={value} size="small" sx={{ backgroundColor: value?.includes("4K") ? colors.redAccent[600] : colors.blueAccent[600], fontWeight: "bold", color: "#fff" }} />
      )
    },
    { field: "language", headerName: "Ngôn ngữ", width: 80 },
    { 
        field: "isVipOnly", headerName: "VIP", width: 60, align: "center",
        renderCell: ({ value }) => (value ? <DiamondIcon sx={{ color: "#FFD700" }} /> : "-"),
    },
    {
        field: "sourceUrl", headerName: "Link", width: 60, sortable: false, align: "center",
        renderCell: ({ value }) => (
          <Tooltip title={value}>
            <IconButton onClick={() => window.open(value, "_blank")} sx={{ color: colors.blueAccent[400] }}>
              <LinkIcon />
            </IconButton>
          </Tooltip>
        ),
    },
    {
        field: "actions", headerName: "Actions", width: 150, sortable: false, align: "center",
        renderCell: ({ row }) => (
            <Box 
                display="flex" 
                justifyContent="center" 
                alignItems="center" 
                height="100%" 
                gap={0.5} // Khoảng cách nhỏ giữa các nút
            >
                <Tooltip title="Chỉnh sửa Source">
                    <IconButton onClick={() => handleOpenUpdateSource(row)} sx={{ color: colors.blueAccent[300] }}>
                        <EditIcon fontSize="small"/>
                    </IconButton>
                </Tooltip>


                
                <Tooltip title="Xóa Source">
                    {/* Lưu ý: ID ở đây là episodeSourceID */}
                    <IconButton onClick={() => handleDeleteSource(row.episodeSourceID)} sx={{ color: colors.redAccent[500] }}>
                        <DeleteOutlineIcon fontSize="small"/>
                    </IconButton>
                </Tooltip>

                <Tooltip title="Chi tiết & Subtitle">
                    <IconButton onClick={() => handleOpenSourceDetail(row)} sx={{ color: colors.blueAccent[300] }}><InfoIcon fontSize="small"/></IconButton>
                </Tooltip>


                <Tooltip title="Tạo Subtitle (AI)">
                    <IconButton onClick={() => handleOpenSubModal(row)} sx={{ color: colors.greenAccent[400] }}><ClosedCaptionIcon fontSize="small"/></IconButton>
                </Tooltip>
                {row.rawSubTitle && (
                    <Tooltip title="Dịch Subtitle">
                        <IconButton onClick={() => handleOpenTranslateModal(row)} sx={{ color: colors.redAccent[400] }}><GTranslateIcon fontSize="small"/></IconButton>
                    </Tooltip>
                )}
            </Box>
        ),
    }
  ];

  if (!episode) return null;

  return (
    <Dialog fullScreen open={open} onClose={onClose} TransitionComponent={Transition}>
      {/* HEADER */}
      <AppBar sx={{ position: 'relative', backgroundColor: colors.blueAccent[700] }}>
        <Toolbar>
          <IconButton edge="start" color="inherit" onClick={onClose} aria-label="close">
            <CloseIcon />
          </IconButton>
          <Typography sx={{ ml: 2, flex: 1 }} variant="h6" component="div">
            {movieTitle} - Mùa {episode.seasonNumber} Tập {episode.episodeNumber}
          </Typography>

          {/* [NEW] Nút Chỉnh sửa */}
          <Button 
            color="inherit" 
            variant="outlined" 
            onClick={() => setOpenUpdateModal(true)}
            startIcon={<EditIcon />}
            sx={{ mr: 2, borderColor: colors.grey[100], color: colors.grey[100] }}
          >
            Chỉnh sửa
          </Button>

          {/* Nút Xóa */}
          <Button 
            autoFocus 
            color="error" 
            variant="contained" 
            onClick={handleDeleteCurrentEpisode}
            startIcon={<DeleteOutlineIcon />}
            sx={{ mr: 2, backgroundColor: colors.redAccent[600] }}
          >
            Xóa Tập
          </Button>

          <Button autoFocus color="inherit" onClick={onClose}>
            Đóng
          </Button>
        </Toolbar>
      </AppBar>

      {/* BODY */}
      <Box p={3} sx={{ backgroundColor: colors.primary[500], minHeight: "100vh", overflowY: "auto" }}>
        <Grid container spacing={3}>
            {/* 1. INFO CARD */}
            <Grid item xs={12}>
                <Card sx={{ backgroundColor: colors.primary[400] }}>
                    <CardContent>
                        <Grid container spacing={2}>
                            <Grid item xs={12}>
                                <Typography variant="h3" fontWeight="bold" color={colors.greenAccent[500]} mb={1}>
                                    {episode.title}
                                </Typography>
                                <Typography variant="body1" color={colors.grey[100]} mb={2} sx={{ fontStyle: 'italic' }}>
                                    {episode.description || "Chưa có mô tả cho tập này."}
                                </Typography>
                                <Divider sx={{ backgroundColor: colors.grey[600], mb: 2 }} />
                                <Box display="flex" gap={3}>
                                    <Box display="flex" alignItems="center" gap={1}>
                                        <AccessTimeIcon color="secondary" />
                                        <Typography>{Math.floor(episode.durationSeconds / 60)} phút</Typography>
                                    </Box>
                                    <Box display="flex" alignItems="center" gap={1}>
                                        <CalendarTodayIcon color="secondary" />
                                        <Typography>{new Date(episode.releaseDate).toLocaleDateString('vi-VN')}</Typography>
                                    </Box>
                                </Box>
                            </Grid>
                        </Grid>
                    </CardContent>
                </Card>
            </Grid>

            {/* 2. SOURCES CARD */}
            <Grid item xs={12}>
                <Card sx={{ backgroundColor: colors.primary[400] }}>
                    <CardContent>
                        <Accordion defaultExpanded sx={{ backgroundColor: colors.primary[400], color: colors.grey[100], boxShadow: "none" }}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Box display="flex" alignItems="center" justifyContent="space-between" width="100%" pr={2}>
                                    <Box display="flex" alignItems="center">
                                        <VideoLibraryIcon sx={{ color: colors.blueAccent[500], mr: 1 }} />
                                        <Typography variant="h5" fontWeight="600">
                                            Danh sách Source Tập ({sources.length})
                                        </Typography>
                                    </Box>
                                    <Button 
                                        variant="contained" 
                                        startIcon={<CloudUploadIcon />} 
                                        onClick={(e) => { e.stopPropagation(); setOpenUpload(true); }}
                                        sx={{ backgroundColor: colors.greenAccent[600], "&:hover": { backgroundColor: colors.greenAccent[700] } }}
                                    >
                                        Thêm Source Tập
                                    </Button>
                                </Box>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Box height="400px">
                                    <DataGrid
                                        loading={loading}
                                        rows={sources}
                                        columns={sourceColumns}
                                        getRowId={(row) => row.episodeSourceID}
                                        pageSize={10}
                                        rowsPerPageOptions={[10, 20]}
                                        sx={{ 
                                            "& .MuiDataGrid-cell": { borderBottom: `1px solid ${colors.grey[700]}` },
                                            "& .MuiDataGrid-columnHeaders": { backgroundColor: colors.blueAccent[700] },
                                            "& .MuiDataGrid-footerContainer": { backgroundColor: colors.blueAccent[700] }
                                        }}
                                    />
                                </Box>
                            </AccordionDetails>
                        </Accordion>
                    </CardContent>
                </Card>
            </Grid>
        </Grid>
      </Box>

      {/* --- MODALS LOGIC --- */}
      
      {/* 1. Upload Source (Scope = episode) */}
      <UploadSourceModal 
        open={openUpload} 
        onClose={() => setOpenUpload(false)} 
        initialScope="episode"
        movieId={episode.episodeID} // targetId = EpisodeID
        movieTitle={`Tập ${episode.episodeNumber} - ${episode.title}`}
        onUploadSuccess={() => fetchEpisodeSources()}
      />

      {/* 2. Add Subtitle */}
      {selectedSourceForSub && (
        <AddSubtitleModal
            open={openSubModal}
            onClose={() => setOpenSubModal(false)}
            sourceId={selectedSourceForSub.episodeSourceID}
            sourceName={selectedSourceForSub.sourceName}
            onSuccess={() => console.log("Sub requested for episode")}
        />
      )}

      {/* 3. Translate Subtitle */}
      {selectedSourceForTranslate && (
        <TranslateSubtitleModal
            open={openTranslateModal}
            onClose={() => setOpenTranslateModal(false)}
            sourceId={selectedSourceForTranslate.episodeSourceID}
            sourceName={selectedSourceForTranslate.sourceName}
            onSuccess={() => console.log("Translate requested for episode")}
        />
      )}

      {/* 4. Detail Source + Sub List */}
      {selectedSourceDetail && (
        <SourceDetailModal
            open={openSourceDetail}
            onClose={() => setOpenSourceDetail(false)}
            source={selectedSourceDetail}
            scope="episode"
        />
      )}

      {/* 5. Update Episode Modal [NEW] */}
      <UpdateEpisodeModal 
        open={openUpdateModal}
        onClose={() => setOpenUpdateModal(false)}
        episode={episode}
        onSuccess={(updatedData) => {
            // Callback để cha (MovieDetail) biết và reload danh sách tập
            if (onUpdateSuccess) onUpdateSuccess();
            // Đóng modal chi tiết luôn để user chọn lại từ danh sách mới (tránh dữ liệu cũ)
            onClose(); 
        }}
      />

      <UpdateSourceModal 
        open={openUpdateSource}
        onClose={() => setOpenUpdateSource(false)}
        source={selectedSourceForUpdate}
        scope="episode" // Quan trọng
        onSuccess={() => fetchEpisodeSources()} // Refresh list
      />

    </Dialog>
  );
};

export default EpisodeDetailModal;