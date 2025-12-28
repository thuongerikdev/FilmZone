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
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import { getWatchNowMovieById, deleteMovie, getCommentsByMovieId, deleteComment } from "../services/api";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import MovieIcon from "@mui/icons-material/Movie";
import TvIcon from "@mui/icons-material/Tv";
import CalendarTodayIcon from "@mui/icons-material/CalendarToday";
import StarIcon from "@mui/icons-material/Star";
import PublicIcon from "@mui/icons-material/Public";
import LabelIcon from "@mui/icons-material/Label";
import PeopleIcon from "@mui/icons-material/People";
import ImageIcon from "@mui/icons-material/Image";
import CommentIcon from "@mui/icons-material/Comment";
import { DataGrid } from "@mui/x-data-grid";
import { Accordion, AccordionSummary, AccordionDetails, IconButton } from "@mui/material";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";

const MovieDetail = () => {
  const { movieId } = useParams();
  const navigate = useNavigate();
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  
  const [movieData, setMovieData] = useState(null);
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchMovieDetail();
    fetchMovieComments();
  }, [movieId]);

  const fetchMovieDetail = async () => {
    try {
      const response = await getWatchNowMovieById(movieId);
      if (response.data.errorCode === 200) {
        setMovieData(response.data.data);
      }
    } catch (error) {
      console.error("Error fetching movie detail:", error);
    } finally {
      setLoading(false);
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
          fetchMovieComments(); // Refresh comments
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
      case 'ongoing':
        return colors.blueAccent[600];
      case 'completed':
        return colors.greenAccent[600];
      case 'upcoming':
        return colors.grey[600];
      default:
        return colors.grey[600];
    }
  };

  const getStatusText = (status) => {
    switch (status) {
      case 'ongoing':
        return 'Đang chiếu';
      case 'completed':
        return 'Hoàn thành';
      case 'upcoming':
        return 'Sắp ra mắt';
      default:
        return status;
    }
  };

  if (loading) {
    return (
      <Box m="20px">
        <Header title="CHI TIẾT PHIM" subtitle="Đang tải dữ liệu..." />
      </Box>
    );
  }

  if (!movieData) {
    return (
      <Box m="20px">
        <Header title="CHI TIẾT PHIM" subtitle="Không tìm thấy phim" />
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate("/movies")}
          sx={{ color: colors.grey[100] }}
        >
          Quay lại
        </Button>
      </Box>
    );
  }

  // Columns cho Comments
  const commentsColumns = [
    { field: "commentID", headerName: "Comment ID", width: 100 },
    { field: "userID", headerName: "User ID", width: 100 },
    { 
      field: "content", 
      headerName: "Content", 
      flex: 1, 
      minWidth: 300,
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
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Header
          title={movieData.title}
          subtitle={movieData.originalTitle}
        />
        <Box display="flex" gap={2}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate("/movies")}
            sx={{
              color: colors.grey[100],
              borderColor: colors.grey[400],
            }}
            variant="outlined"
          >
            Quay lại
          </Button>
          <Button
            startIcon={<EditOutlinedIcon />}
            onClick={() => navigate(`/movies/edit/${movieId}`)}
            sx={{
              color: colors.greenAccent[500],
              borderColor: colors.greenAccent[500],
              "&:hover": {
                borderColor: colors.greenAccent[300],
                backgroundColor: colors.greenAccent[900],
              },
            }}
            variant="outlined"
          >
            Chỉnh sửa
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
            Xóa phim
          </Button>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Poster và thông tin cơ bản */}
        <Grid item xs={12} md={4}>
          <Card sx={{ backgroundColor: colors.primary[400] }}>
            <Box
              component="img"
              src={movieData.image}
              alt={movieData.title}
              sx={{
                width: "100%",
                height: "500px",
                objectFit: "cover",
              }}
            />
            <CardContent>
              <Box display="flex" flexDirection="column" gap={2}>
                <Box>
                  <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                    Loại phim
                  </Typography>
                  <Chip
                    icon={movieData.movieType === "movie" ? <MovieIcon /> : <TvIcon />}
                    label={movieData.movieType === "movie" ? "Phim lẻ" : "Phim bộ"}
                    sx={{
                      backgroundColor: movieData.movieType === "movie" 
                        ? colors.blueAccent[700] 
                        : colors.greenAccent[700],
                      color: colors.grey[100],
                    }}
                  />
                </Box>

                <Box>
                  <Typography variant="body2" color={colors.grey[300]} mb={0.5}>
                    Trạng thái
                  </Typography>
                  <Chip
                    label={getStatusText(movieData.status)}
                    sx={{
                      backgroundColor: getStatusColor(movieData.status),
                      color: colors.grey[100],
                    }}
                  />
                </Box>

                <Box>
                  <Typography variant="body2" color={colors.grey[300]}>
                    Năm phát hành
                  </Typography>
                  <Typography variant="h6" color={colors.grey[100]} fontWeight="600">
                    {movieData.year}
                  </Typography>
                </Box>

                <Box>
                  <Typography variant="body2" color={colors.grey[300]}>
                    Đánh giá
                  </Typography>
                  <Box display="flex" alignItems="center" gap={1}>
                    <StarIcon sx={{ color: colors.greenAccent[500] }} />
                    <Typography variant="h6" color={colors.greenAccent[500]} fontWeight="600">
                      {movieData.popularity}/5
                    </Typography>
                  </Box>
                </Box>

                {movieData.movieType === "series" && (
                  <>
                    <Box>
                      <Typography variant="body2" color={colors.grey[300]}>
                        Số mùa
                      </Typography>
                      <Typography variant="h6" color={colors.grey[100]} fontWeight="600">
                        {movieData.totalSeasons}
                      </Typography>
                    </Box>

                    <Box>
                      <Typography variant="body2" color={colors.grey[300]}>
                        Số tập
                      </Typography>
                      <Typography variant="h6" color={colors.grey[100]} fontWeight="600">
                        {movieData.totalEpisodes}
                      </Typography>
                    </Box>
                  </>
                )}
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Thông tin chi tiết */}
        <Grid item xs={12} md={8}>
          <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
            <CardContent>
              <Typography variant="h4" color={colors.grey[100]} fontWeight="600" mb={2}>
                Mô tả
              </Typography>
              <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
              <Typography variant="body1" color={colors.grey[200]} lineHeight={1.8}>
                {movieData.description}
              </Typography>
            </CardContent>
          </Card>

          {/* Khu vực */}
          {movieData.region && (
            <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
              <CardContent>
                <Box display="flex" alignItems="center" mb={2}>
                  <PublicIcon sx={{ color: colors.blueAccent[500], mr: 1 }} />
                  <Typography variant="h5" fontWeight="600" color={colors.grey[100]}>
                    Khu vực
                  </Typography>
                </Box>
                <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
                <Chip
                  label={movieData.region.regionName}
                  sx={{
                    backgroundColor: colors.blueAccent[700],
                    color: colors.grey[100],
                  }}
                />
              </CardContent>
            </Card>
          )}

          {/* Thể loại */}
          {movieData.tags && movieData.tags.length > 0 && (
            <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
              <CardContent>
                <Box display="flex" alignItems="center" mb={2}>
                  <LabelIcon sx={{ color: colors.greenAccent[500], mr: 1 }} />
                  <Typography variant="h5" fontWeight="600" color={colors.grey[100]}>
                    Thể loại
                  </Typography>
                </Box>
                <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
                <Box display="flex" gap={1} flexWrap="wrap">
                  {movieData.tags.map((tag) => (
                    <Chip
                      key={tag.tagID}
                      label={tag.tagName}
                      sx={{
                        backgroundColor: colors.greenAccent[700],
                        color: colors.grey[100],
                      }}
                    />
                  ))}
                </Box>
              </CardContent>
            </Card>
          )}

          {/* Diễn viên */}
          {movieData.actors && movieData.actors.length > 0 && (
            <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
              <CardContent>
                <Box display="flex" alignItems="center" mb={2}>
                  <PeopleIcon sx={{ color: colors.blueAccent[500], mr: 1 }} />
                  <Typography variant="h5" fontWeight="600" color={colors.grey[100]}>
                    Diễn viên ({movieData.actors.length})
                  </Typography>
                </Box>
                <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
                <Grid container spacing={2}>
                  {movieData.actors.map((actor, index) => (
                    <Grid item xs={6} sm={4} md={3} key={index}>
                      <Box textAlign="center">
                        <Box
                          component="img"
                          src={actor.avatar}
                          alt={actor.fullName}
                          sx={{
                            width: "100px",
                            height: "100px",
                            objectFit: "cover",
                            borderRadius: "50%",
                            mb: 1,
                          }}
                        />
                        <Typography variant="body2" color={colors.grey[100]} fontWeight="600">
                          {actor.fullName}
                        </Typography>
                        <Typography variant="caption" color={colors.grey[400]}>
                          {actor.characterName}
                        </Typography>
                      </Box>
                    </Grid>
                  ))}
                </Grid>
              </CardContent>
            </Card>
          )}

          {/* Hình ảnh */}
          {movieData.images && movieData.images.length > 0 && (
            <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
              <CardContent>
                <Box display="flex" alignItems="center" mb={2}>
                  <ImageIcon sx={{ color: colors.greenAccent[500], mr: 1 }} />
                  <Typography variant="h5" fontWeight="600" color={colors.grey[100]}>
                    Hình ảnh ({movieData.images.length})
                  </Typography>
                </Box>
                <Divider sx={{ backgroundColor: colors.grey[700], mb: 2 }} />
                <Grid container spacing={2}>
                  {movieData.images.map((image) => (
                    <Grid item xs={6} sm={4} key={image.movieImageID}>
                      <Box
                        component="img"
                        src={image.imageUrl}
                        alt={`Movie image ${image.movieImageID}`}
                        sx={{
                          width: "100%",
                          height: "150px",
                          objectFit: "cover",
                          borderRadius: "8px",
                        }}
                      />
                    </Grid>
                  ))}
                </Grid>
              </CardContent>
            </Card>
          )}

          {/* Comments */}
          <Card sx={{ backgroundColor: colors.primary[400] }}>
            <CardContent>
              <Accordion 
                defaultExpanded
                sx={{ 
                  backgroundColor: colors.primary[400],
                  color: colors.grey[100],
                  boxShadow: "none",
                }}
              >
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Box display="flex" alignItems="center">
                    <CommentIcon sx={{ color: colors.blueAccent[500], mr: 1 }} />
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
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default MovieDetail;