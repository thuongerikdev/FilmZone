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
  Button,
  Divider,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  IconButton,
  Tooltip,
} from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { tokens } from "../theme";
import Header from "../components/Header";
import { getWatchNowMovieById, deleteMovie, getCommentsByMovieId, deleteComment, deleteEpisode ,deleteMovieSource, getMovieSourcesByMovieId, getEpisodesByMovieId } from "../services/api";

// Import Modals
import UploadSourceModal from "../components/UploadSourceModal"; 
import AddSubtitleModal from "../components/AddSubtitleModal"; 
import SourceDetailModal from "../components/SourceDetailModal"; 
import TranslateSubtitleModal from "../components/TranslateSubtitleModal";
import EpisodeDetailModal from "../components/EpisodeDetailModal";
import CreateEpisodeModal from "../components/CreateEpisodeModal";
import UpdateSourceModal from "../components/UpdateSourceModal";

// Icons
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import MovieIcon from "@mui/icons-material/Movie";
import TvIcon from "@mui/icons-material/Tv";
import StarIcon from "@mui/icons-material/Star";
import PublicIcon from "@mui/icons-material/Public";
import LabelIcon from "@mui/icons-material/Label";
import PeopleIcon from "@mui/icons-material/People";
import ImageIcon from "@mui/icons-material/Image";
import CommentIcon from "@mui/icons-material/Comment";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import VideoLibraryIcon from "@mui/icons-material/VideoLibrary";
import LinkIcon from "@mui/icons-material/Link";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import DiamondIcon from "@mui/icons-material/Diamond";
import CloudUploadIcon from "@mui/icons-material/CloudUpload"; 
import ClosedCaptionIcon from "@mui/icons-material/ClosedCaption";
import InfoIcon from "@mui/icons-material/Info"; 
import GTranslateIcon from "@mui/icons-material/GTranslate";
import ListIcon from "@mui/icons-material/List";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit"; // <--- [FIX] Import EditIcon

const MovieDetail = () => {
  const { movieId } = useParams();
  const navigate = useNavigate();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  
  const [movieData, setMovieData] = useState(null);
  const [comments, setComments] = useState([]);
  const [sources, setSources] = useState([]);
  const [episodes, setEpisodes] = useState([]);
  const [loading, setLoading] = useState(true);
  
  // --- STATE MODALS (MOVIE) ---
  const [openUpload, setOpenUpload] = useState(false);
  const [openSubModal, setOpenSubModal] = useState(false);
  const [selectedSourceForSub, setSelectedSourceForSub] = useState(null);
  const [openTranslateModal, setOpenTranslateModal] = useState(false);
  const [selectedSourceForTranslate, setSelectedSourceForTranslate] = useState(null);
  const [openSourceDetail, setOpenSourceDetail] = useState(false);
  const [selectedSourceDetail, setSelectedSourceDetail] = useState(null);

  // --- STATE MODALS (UPDATE SOURCE MOVIE) ---
  const [openUpdateSource, setOpenUpdateSource] = useState(false);
  const [selectedSourceForUpdate, setSelectedSourceForUpdate] = useState(null);

  // --- STATE MODALS (EPISODE) ---
  const [selectedEpisode, setSelectedEpisode] = useState(null);
  const [openEpisodeDetail, setOpenEpisodeDetail] = useState(false);
  const [openCreateEpisode, setOpenCreateEpisode] = useState(false);

  const handleDeleteSource = async (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa Source này không?")) {
        try {
            const response = await deleteMovieSource(id);
            if (response.data.errorCode === 200) {
                alert("Xóa source thành công!");
                fetchMovieSources(); // Refresh list
            } else {
                alert(response.data.errorMessage || "Xóa thất bại.");
            }
        } catch (error) {
            console.error("Error deleting source:", error);
            alert("Lỗi khi xóa source.");
        }
    }
  };

  useEffect(() => {
    const loadAllData = async () => {
      setLoading(true);
      await Promise.all([
        fetchMovieDetail(),
        fetchMovieComments(),
        fetchMovieSources(),
        fetchEpisodes()
      ]);
      setLoading(false);
    };
    loadAllData();
  }, [movieId]);

  const fetchMovieDetail = async () => {
    try {
      const response = await getWatchNowMovieById(movieId);
      if (response.data.errorCode === 200) {
        setMovieData(response.data.data);
      }
    } catch (error) {
      console.error("Error fetching movie detail:", error);
    }
  };

  const fetchMovieComments = async () => {
    try {
      const response = await getCommentsByMovieId(movieId);
      if (response.data.errorCode === 200) {
        setComments(response.data.data);
      }
    } catch (error) {
      console.error("Error fetching comments:", error);
    }
  };

  const fetchMovieSources = async () => {
    try {
      const response = await getMovieSourcesByMovieId(movieId);
      const data = await response.data;
      if (data.errorCode === 200) {
        setSources(data.data);
      }
    } catch (error) {
      console.error("Error fetching movie sources:", error);
    }
  };

  const fetchEpisodes = async () => {
    try {
      const response = await getEpisodesByMovieId(movieId);
      const data = await response.data;
      if (data.errorCode === 200) {
        setEpisodes(data.data);
      }
    } catch (error) {
      // console.error("Error fetching episodes (ok if not series):", error);
    }
  };

  const handleDelete = async () => {
    if (window.confirm("Bạn có chắc chắn muốn xóa phim này?")) {
      try {
        const response = await deleteMovie(movieId);
        if (response.data.errorCode === 200) {
          alert("Xóa phim thành công!");
          navigate("/movies");
        } else {
          alert(response.data.errorMessage || "Xóa phim thất bại");
        }
      } catch (error) {
        console.error("Error deleting movie:", error);
        alert("Có lỗi xảy ra khi xóa phim");
      }
    }
  };

  const handleDeleteComment = async (commentId) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa comment này?")) {
      try {
        const response = await deleteComment(commentId);
        if (response.data.errorCode === 200) {
          alert("Xóa comment thành công!");
          fetchMovieComments(); 
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

  const getStatusColor = (status) => {
    switch (status) {
      case 'ongoing': return colors.blueAccent[600];
      case 'completed': return colors.greenAccent[600];
      case 'upcoming': return colors.grey[600];
      default: return colors.grey[600];
    }
  };

  const getStatusText = (status) => {
    switch (status) {
      case 'ongoing': return 'Đang chiếu';
      case 'completed': return 'Hoàn thành';
      case 'upcoming': return 'Sắp ra mắt';
      default: return status;
    }
  };

  // --- Handlers Modal (Movie) ---
  const handleOpenSubModal = (source) => {
    setSelectedSourceForSub(source);
    setOpenSubModal(true);
  };

  const handleOpenTranslateModal = (source) => {
    setSelectedSourceForTranslate(source);
    setOpenTranslateModal(true);
  };

  const handleOpenSourceDetail = (source) => {
    setSelectedSourceDetail(source);
    setOpenSourceDetail(true);
  };

  // --- [FIX] Handler Open Update Source ---
  const handleOpenUpdateSource = (source) => {
    setSelectedSourceForUpdate(source);
    setOpenUpdateSource(true);
  };

  // --- COLUMNS (SOURCE MOVIE) ---
  const sourceColumns = [
    { field: "movieSourceID", headerName: "ID", width: 60 },
    { field: "sourceName", headerName: "Tên Source", flex: 1, minWidth: 150 },
    { 
      field: "quality", headerName: "Chất lượng", width: 100,
      renderCell: ({ value }) => (
        <Chip label={value} size="small" sx={{ backgroundColor: value.includes("4K") ? colors.redAccent[600] : colors.blueAccent[600], fontWeight: "bold", color: "#fff" }} />
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
        field: "actions", headerName: "Actions", width: 180, sortable: false, align: "center",
        renderCell: ({ row }) => (
            <Box 
                display="flex" 
                justifyContent="center" 
                alignItems="center" 
                height="100%" 
                gap={0.5} // Khoảng cách nhỏ giữa các nút
            >
                {/* Nút Edit Source */}
                <Tooltip title="Chỉnh sửa Source">
                    <IconButton onClick={() => handleOpenUpdateSource(row)} sx={{ color: colors.blueAccent[300] }}>
                        <EditIcon fontSize="small"/>
                    </IconButton>
                </Tooltip>
                <Tooltip title="Xóa Source">
                    <IconButton onClick={() => handleDeleteSource(row.movieSourceID)} sx={{ color: colors.redAccent[500] }}>
                        <DeleteOutlineIcon fontSize="small"/>
                    </IconButton>
                </Tooltip>

                {/* Nút Xem chi tiết */}
                <Tooltip title="Chi tiết & Subtitle">
                    <IconButton onClick={() => handleOpenSourceDetail(row)} sx={{ color: colors.grey[100] }}>
                        <InfoIcon fontSize="small"/>
                    </IconButton>
                </Tooltip>



                {/* Nút Tạo Sub */}
                <Tooltip title="Tạo Subtitle (AI)">
                    <IconButton onClick={() => handleOpenSubModal(row)} sx={{ color: colors.greenAccent[400] }}>
                        <ClosedCaptionIcon fontSize="small"/>
                    </IconButton>
                </Tooltip>

                {/* Nút Dịch Sub */}
                {row.rawSubTitle && (
                    <Tooltip title="Dịch Subtitle tự động">
                        <IconButton onClick={() => handleOpenTranslateModal(row)} sx={{ color: colors.redAccent[400] }}>
                            <GTranslateIcon fontSize="small"/>
                        </IconButton>
                    </Tooltip>
                )}
            </Box>
        ),
    }
  ];

  // --- COLUMNS (EPISODE) ---
  const handleDeleteEpisode = async (id) => {
    if (window.confirm(`Bạn có chắc chắn muốn xóa Tập ID: ${id} không?`)) {
      try {
        const response = await deleteEpisode(id);
        if (response.errorCode === 200 || response.status === 200) {
          alert("Xóa tập phim thành công!");
          fetchEpisodes(); 
        } else {
          alert(response.errorMessage || "Xóa thất bại.");
        }
      } catch (error) {
        console.error("Error deleting episode:", error);
        alert("Có lỗi xảy ra khi xóa tập phim.");
      }
    }
  };

  const episodeColumns = [
    { field: "episodeID", headerName: "ID", width: 60 },
    { field: "seasonNumber", headerName: "Mùa", width: 60, align: "center" },
    { field: "episodeNumber", headerName: "Tập", width: 60, align: "center" },
    { field: "title", headerName: "Tên tập", flex: 1, minWidth: 200 },
    { 
      field: "durationSeconds", headerName: "Thời lượng", width: 100,
      renderCell: ({ value }) => `${Math.floor(value / 60)} phút`
    },
    { 
      field: "releaseDate", headerName: "Ngày chiếu", width: 120,
      renderCell: ({ value }) => new Date(value).toLocaleDateString()
    },
    {
      field: "actions", headerName: "Hành động", width: 150, sortable: false, align: "center",
      renderCell: ({ row }) => (
        <Box display="flex" gap={1}>
            <Button 
                variant="outlined" 
                size="small" 
                startIcon={<InfoIcon />}
                onClick={() => {
                    setSelectedEpisode(row);
                    setOpenEpisodeDetail(true);
                }}
                sx={{ color: colors.blueAccent[300], borderColor: colors.blueAccent[300] }}
            >
                Xem
            </Button>

            <IconButton 
                onClick={() => handleDeleteEpisode(row.episodeID)}
                sx={{ color: colors.redAccent[500] }}
                title="Xóa tập này"
            >
                <DeleteOutlineIcon />
            </IconButton>
        </Box>
      ),
    }
  ];

  const commentsColumns = [
    { field: "commentID", headerName: "ID", width: 70 },
    { field: "userID", headerName: "User ID", width: 80 },
    { field: "content", headerName: "Content", flex: 1, minWidth: 300 },
    { field: "likeCount", headerName: "Likes", width: 80, align: "center" },
    {
      field: "isEdited", headerName: "Edited", width: 80,
      renderCell: ({ value }) => (<Chip label={value ? "Yes" : "No"} size="small" color={value ? "warning" : "default"} />),
    },
    { field: "createdAt", headerName: "Created", width: 160, renderCell: ({ value }) => formatDateTime(value) },
    {
      field: "actions", headerName: "Actions", width: 80, sortable: false,
      renderCell: ({ row }) => (
        <IconButton onClick={() => handleDeleteComment(row.commentID)} sx={{ color: colors.redAccent[500] }}><DeleteOutlineIcon /></IconButton>
      ),
    },
  ];

  if (loading) return (<Box m="20px"><Header title="CHI TIẾT PHIM" subtitle="Đang tải dữ liệu..." /></Box>);
  if (!movieData) return (<Box m="20px"><Header title="CHI TIẾT PHIM" subtitle="Không tìm thấy phim" /><Button startIcon={<ArrowBackIcon />} onClick={() => navigate("/movies")} sx={{ color: colors.grey[100] }}>Quay lại</Button></Box>);

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Header title={movieData.title} subtitle={movieData.originalTitle} />
        <Box display="flex" gap={2}>
          <Button startIcon={<ArrowBackIcon />} onClick={() => navigate("/movies")} variant="outlined" sx={{ color: colors.grey[100], borderColor: colors.grey[400] }}>Quay lại</Button>
          <Button startIcon={<EditOutlinedIcon />} onClick={() => navigate(`/movies/edit/${movieId}`)} variant="outlined" sx={{ color: colors.greenAccent[500], borderColor: colors.greenAccent[500] }}>Chỉnh sửa</Button>
          <Button startIcon={<DeleteOutlineIcon />} onClick={handleDelete} variant="outlined" sx={{ color: colors.redAccent[500], borderColor: colors.redAccent[500] }}>Xóa phim</Button>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Left Column (Poster + Info) */}
        <Grid item xs={12} md={4}>
          <Card sx={{ backgroundColor: colors.primary[400] }}>
            <Box component="img" src={movieData.image} alt={movieData.title} sx={{ width: "100%", height: "500px", objectFit: "cover" }} />
            <CardContent>
              <Box display="flex" flexDirection="column" gap={2}>
                <Box><Typography variant="body2" color={colors.grey[300]} mb={0.5}>Loại phim</Typography><Chip icon={movieData.movieType === "movie" ? <MovieIcon /> : <TvIcon />} label={movieData.movieType === "movie" ? "Phim lẻ" : "Phim bộ"} sx={{ backgroundColor: movieData.movieType === "movie" ? colors.blueAccent[700] : colors.greenAccent[700], color: colors.grey[100] }} /></Box>
                <Box><Typography variant="body2" color={colors.grey[300]} mb={0.5}>Trạng thái</Typography><Chip label={getStatusText(movieData.status)} sx={{ backgroundColor: getStatusColor(movieData.status), color: colors.grey[100] }} /></Box>
                <Box><Typography variant="body2" color={colors.grey[300]}>Năm phát hành</Typography><Typography variant="h6" color={colors.grey[100]} fontWeight="600">{movieData.year}</Typography></Box>
                <Box><Typography variant="body2" color={colors.grey[300]}>Đánh giá</Typography><Box display="flex" alignItems="center" gap={1}><StarIcon sx={{ color: colors.greenAccent[500] }} /><Typography variant="h6" color={colors.greenAccent[500]} fontWeight="600">{movieData.popularity}/5</Typography></Box></Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Right Column */}
        <Grid item xs={12} md={8}>
          
          {/* Description */}
          <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}><CardContent><Typography variant="h4" color={colors.grey[100]} fontWeight="600" mb={2}>Mô tả</Typography><Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} /><Typography variant="body1" color={colors.grey[200]} lineHeight={1.8}>{movieData.description}</Typography></CardContent></Card>
          
          {/* --- DANH SÁCH TẬP (Nếu là Series) --- */}
          {movieData.movieType === "series" && (
            <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
                <CardContent>
                    <Accordion defaultExpanded sx={{ backgroundColor: colors.primary[400], color: colors.grey[100], boxShadow: "none" }}>
                        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                            <Box display="flex" alignItems="center" justifyContent="space-between" width="100%" pr={2}>
                                <Box display="flex" alignItems="center">
                                    <ListIcon sx={{ color: colors.greenAccent[500], mr: 1 }} />
                                    <Typography variant="h5" fontWeight="600">
                                        Danh sách tập ({episodes.length})
                                    </Typography>
                                </Box>
                                <Button 
                                    variant="contained" 
                                    startIcon={<AddIcon />} 
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        setOpenCreateEpisode(true);
                                    }}
                                    sx={{ 
                                        backgroundColor: colors.blueAccent[600], 
                                        "&:hover": { backgroundColor: colors.blueAccent[700] } 
                                    }}
                                >
                                    Thêm Tập
                                </Button>
                            </Box>
                        </AccordionSummary>
                        <AccordionDetails>
                            <Box height="400px">
                                <DataGrid
                                    rows={episodes}
                                    columns={episodeColumns}
                                    getRowId={(row) => row.episodeID}
                                    pageSize={10}
                                    rowsPerPageOptions={[10, 20]}
                                    initialState={{
                                        sorting: {
                                            sortModel: [{ field: 'seasonNumber', sort: 'asc' }, { field: 'episodeNumber', sort: 'asc' }],
                                        },
                                    }}
                                    sx={{ "& .MuiDataGrid-cell": { borderBottom: `1px solid ${colors.grey[700]}` } }}
                                />
                            </Box>
                        </AccordionDetails>
                    </Accordion>
                </CardContent>
            </Card>
          )}

          {/* --- DANH SÁCH SOURCE (Nếu là Movie) --- */}
          {movieData.movieType === "movie" && (
            <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
                <CardContent>
                <Accordion defaultExpanded sx={{ backgroundColor: colors.primary[400], color: colors.grey[100], boxShadow: "none" }}>
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                    <Box display="flex" alignItems="center" justifyContent="space-between" width="100%" pr={2}>
                        <Box display="flex" alignItems="center">
                        <VideoLibraryIcon sx={{ color: colors.blueAccent[500], mr: 1 }} />
                        <Typography variant="h5" fontWeight="600">
                            Danh sách Source Phim ({sources.length})
                        </Typography>
                        </Box>
                        <Button 
                        variant="contained" 
                        startIcon={<CloudUploadIcon />} 
                        onClick={(e) => {
                            e.stopPropagation();
                            setOpenUpload(true);
                        }}
                        sx={{ backgroundColor: colors.greenAccent[600], "&:hover": { backgroundColor: colors.greenAccent[700] } }}
                        >
                        Thêm Source
                        </Button>
                    </Box>
                    </AccordionSummary>
                    <AccordionDetails>
                    <Box height="350px">
                        <DataGrid
                        rows={sources}
                        columns={sourceColumns}
                        getRowId={(row) => row.movieSourceID}
                        pageSize={5}
                        rowsPerPageOptions={[5, 10]}
                        sx={{
                            "& .MuiDataGrid-cell": { borderBottom: `1px solid ${colors.grey[700]}` },
                            "& .MuiDataGrid-columnHeaders": { backgroundColor: colors.blueAccent[700], borderBottom: "none" },
                            "& .MuiDataGrid-footerContainer": { borderTop: "none", backgroundColor: colors.blueAccent[700] },
                        }}
                        />
                    </Box>
                    </AccordionDetails>
                </Accordion>
                </CardContent>
            </Card>
          )}

          {/* Comments */}
          <Card sx={{ backgroundColor: colors.primary[400] }}>
            <CardContent>
              <Accordion defaultExpanded sx={{ backgroundColor: colors.primary[400], color: colors.grey[100], boxShadow: "none" }}>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Box display="flex" alignItems="center"><CommentIcon sx={{ color: colors.blueAccent[500], mr: 1 }} /><Typography variant="h5" fontWeight="600">Comments ({comments.length})</Typography></Box>
                </AccordionSummary>
                <AccordionDetails>
                  <Box height="400px"><DataGrid rows={comments} columns={commentsColumns} getRowId={(row) => row.commentID} pageSize={5} rowsPerPageOptions={[5, 10, 20]} sx={{ "& .MuiDataGrid-cell": { borderBottom: `1px solid ${colors.grey[700]}` } }} /></Box>
                </AccordionDetails>
              </Accordion>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* --- RENDER MODALS (MOVIE) --- */}
      <UploadSourceModal 
        open={openUpload} 
        onClose={() => setOpenUpload(false)} 
        movieId={movieId} 
        movieTitle={movieData.title}
        onUploadSuccess={() => fetchMovieSources()}
        initialScope="movie"
      />

      {/* [NEW] Modal Update Source (Movie) */}
      <UpdateSourceModal 
        open={openUpdateSource}
        onClose={() => setOpenUpdateSource(false)}
        source={selectedSourceForUpdate}
        scope="movie"
        onSuccess={() => fetchMovieSources()}
      />

      {selectedSourceForSub && (
        <AddSubtitleModal
            open={openSubModal}
            onClose={() => setOpenSubModal(false)}
            sourceId={selectedSourceForSub.movieSourceID}
            sourceName={selectedSourceForSub.sourceName}
            onSuccess={() => console.log("Subtitle request sent")}
        />
      )}

      {selectedSourceForTranslate && (
        <TranslateSubtitleModal
            open={openTranslateModal}
            onClose={() => setOpenTranslateModal(false)}
            sourceId={selectedSourceForTranslate.movieSourceID}
            sourceName={selectedSourceForTranslate.sourceName}
            onSuccess={() => console.log("Translate request sent")}
        />
      )}

      {selectedSourceDetail && (
        <SourceDetailModal
            open={openSourceDetail}
            onClose={() => setOpenSourceDetail(false)}
            source={selectedSourceDetail}
            scope="movie"
        />
      )}

      {/* --- RENDER EPISODE DETAIL MODAL --- */}
      <EpisodeDetailModal 
        open={openEpisodeDetail}
        onClose={() => setOpenEpisodeDetail(false)}
        episode={selectedEpisode}
        movieTitle={movieData.title}
        onDeleteSuccess={() => {
            setOpenEpisodeDetail(false);
            fetchEpisodes();
        }}
        onUpdateSuccess={() => {
            setOpenEpisodeDetail(false);
            fetchEpisodes();
        }}
      />
      <CreateEpisodeModal
        open={openCreateEpisode}
        onClose={() => setOpenCreateEpisode(false)}
        movieId={movieId}
        onSuccess={() => fetchEpisodes()} 
      />

    </Box>
  );
};

export default MovieDetail;