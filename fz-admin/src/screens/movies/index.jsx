import { Box, Typography, useTheme, IconButton, Chip, Button } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { tokens } from "../../theme";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../../components/Header";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import AddIcon from "@mui/icons-material/Add";
import MovieIcon from "@mui/icons-material/Movie";
import TvIcon from "@mui/icons-material/Tv";
import { getAllMovies, deleteMovie } from "../../services/api";

const Movies = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();
  
  const [movies, setMovies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [pageSize, setPageSize] = useState(10);

  useEffect(() => {
    fetchMovies();
  }, []);

  const fetchMovies = async () => {
    try {
      const response = await getAllMovies();
      if (response.data.errorCode === 200) {
        const transformedData = response.data.data.map((movie) => ({
          id: movie.movieID,
          ...movie,
        }));
        setMovies(transformedData);
      }
    } catch (error) {
      console.error("Error fetching movies:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (movieId) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa phim này?")) {
      try {
        const response = await deleteMovie(movieId);
        if (response.data.errorCode === 200) {
          fetchMovies();
          alert("Xóa phim thành công!");
        } else {
          alert(response.data.errorMessage || "Xóa phim thất bại");
        }
      } catch (error) {
        console.error("Error deleting movie:", error);
        alert("Có lỗi xảy ra khi xóa phim");
      }
    }
  };

  const handleViewDetail = (movieId) => {
    navigate(`/movies/${movieId}`);
  };

  const handleEdit = (movieId) => {
    navigate(`/movies/edit/${movieId}`);
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

  const columns = [
    { 
      field: "movieID", 
      headerName: "ID",
      width: 70,
    },
    {
      field: "image",
      headerName: "Poster",
      width: 100,
      renderCell: ({ row }) => (
        <Box
          component="img"
          src={row.image}
          alt={row.title}
          sx={{
            width: "60px",
            height: "80px",
            objectFit: "cover",
            borderRadius: "4px",
            my: 1,
          }}
        />
      ),
    },
    {
      field: "title",
      headerName: "Tên phim",
      flex: 1,
      minWidth: 200,
      cellClassName: "name-column--cell",
      renderCell: ({ row }) => (
        <Box>
          <Typography variant="body1" color={colors.greenAccent[300]} fontWeight="600">
            {row.title}
          </Typography>
          <Typography variant="body2" color={colors.grey[400]} fontSize="12px">
            {row.originalTitle}
          </Typography>
        </Box>
      ),
    },
    {
      field: "movieType",
      headerName: "Loại",
      width: 100,
      renderCell: ({ row }) => {
        return (
          <Chip
            icon={row.movieType === "movie" ? <MovieIcon /> : <TvIcon />}
            label={row.movieType === "movie" ? "Phim lẻ" : "Phim bộ"}
            size="small"
            sx={{
              backgroundColor: row.movieType === "movie" 
                ? colors.blueAccent[700] 
                : colors.greenAccent[700],
              color: colors.grey[100],
            }}
          />
        );
      },
    },
    {
      field: "year",
      headerName: "Năm",
      width: 80,
      align: "center",
      headerAlign: "center",
    },
    {
      field: "status",
      headerName: "Trạng thái",
      width: 130,
      renderCell: ({ row }) => (
        <Chip
          label={getStatusText(row.status)}
          size="small"
          sx={{
            backgroundColor: getStatusColor(row.status),
            color: colors.grey[100],
            fontWeight: "600",
          }}
        />
      ),
    },
    {
      field: "popularity",
      headerName: "Phổ biến",
      width: 100,
      align: "center",
      headerAlign: "center",
      renderCell: ({ row }) => (
        <Typography color={colors.greenAccent[500]} fontWeight="600">
          {row.popularity}/5
        </Typography>
      ),
    },
    {
      field: "totalSeasons",
      headerName: "Seasons",
      width: 90,
      align: "center",
      headerAlign: "center",
      renderCell: ({ row }) => (
        <Typography color={colors.grey[100]}>
          {row.totalSeasons || "-"}
        </Typography>
      ),
    },
    {
      field: "totalEpisodes",
      headerName: "Episodes",
      width: 90,
      align: "center",
      headerAlign: "center",
      renderCell: ({ row }) => (
        <Typography color={colors.grey[100]}>
          {row.totalEpisodes || "-"}
        </Typography>
      ),
    },
    {
      field: "releaseDate",
      headerName: "Ngày phát hành",
      width: 130,
      renderCell: ({ row }) => {
        if (!row.releaseDate) return "-";
        return new Date(row.releaseDate).toLocaleDateString('vi-VN');
      },
    },
    {
      field: "actions",
      headerName: "Hành động",
      width: 150,
      sortable: false,
      renderCell: ({ row }) => {
        return (
          <Box display="flex" gap="5px">
            <IconButton
              onClick={() => handleViewDetail(row.movieID)}
              sx={{
                color: colors.blueAccent[500],
                "&:hover": {
                  backgroundColor: colors.blueAccent[800],
                },
              }}
            >
              <VisibilityOutlinedIcon />
            </IconButton>
            <IconButton
              onClick={() => handleEdit(row.movieID)}
              sx={{
                color: colors.greenAccent[500],
                "&:hover": {
                  backgroundColor: colors.greenAccent[800],
                },
              }}
            >
              <EditOutlinedIcon />
            </IconButton>
            <IconButton
              onClick={() => handleDelete(row.movieID)}
              sx={{
                color: colors.redAccent[500],
                "&:hover": {
                  backgroundColor: colors.redAccent[800],
                },
              }}
            >
              <DeleteOutlineIcon />
            </IconButton>
          </Box>
        );
      },
    },
  ];

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center">
        <Header 
          title="QUẢN LÝ PHIM" 
          subtitle="Danh sách tất cả phim trong hệ thống" 
        />
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate("/movies/create")}
          sx={{
            backgroundColor: colors.greenAccent[600],
            color: colors.grey[100],
            fontSize: "14px",
            fontWeight: "bold",
            "&:hover": {
              backgroundColor: colors.greenAccent[700],
            },
          }}
        >
          Thêm phim mới
        </Button>
      </Box>

      <Box
        m="40px 0 0 0"
        height="75vh"
        sx={{
          "& .MuiDataGrid-root": {
            border: "none",
          },
          "& .MuiDataGrid-cell": {
            borderBottom: "none",
          },
          "& .name-column--cell": {
            color: colors.greenAccent[300],
            fontWeight: "bold",
          },
          "& .MuiDataGrid-columnHeaders": {
            backgroundColor: colors.blueAccent[700],
            borderBottom: "none",
          },
          "& .MuiDataGrid-virtualScroller": {
            backgroundColor: colors.primary[400],
          },
          "& .MuiDataGrid-footerContainer": {
            borderTop: "none",
            backgroundColor: colors.blueAccent[700],
          },
          "& .MuiDataGrid-row": {
            minHeight: "100px !important",
            maxHeight: "100px !important",
          },
          "& .MuiDataGrid-cell": {
            minHeight: "100px !important",
            maxHeight: "100px !important",
            display: "flex",
            alignItems: "center",
          },
        }}
      >
        <DataGrid
          rows={movies}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          onPageSizeChange={(newPageSize) => setPageSize(newPageSize)}
          rowsPerPageOptions={[5, 10, 20, 50]}
          disableSelectionOnClick
          getRowHeight={() => 100}
        />
      </Box>
    </Box>
  );
};

export default Movies;