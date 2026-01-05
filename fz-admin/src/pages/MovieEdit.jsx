import { useState, useEffect, useMemo } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  Box,
  Typography,
  useTheme,
  Button,
  TextField,
  MenuItem,
  Grid,
  Card,
  CardContent,
  IconButton,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  CircularProgress,
  Stack,
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import CloseIcon from "@mui/icons-material/Close";
import { updateMovie, getMovieById, getPersonsByMovie } from "../services/api";

const MovieEdit = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();
  const { movieID } = useParams();

  // Form states
  const [slug, setSlug] = useState("");
  const [title, setTitle] = useState("");
  const [image, setImage] = useState(null);
  const [imagePoster, setImagePoster] = useState("");
  const [imagePreview, setImagePreview] = useState("");
  const [originalTitle, setOriginalTitle] = useState("");
  const [description, setDescription] = useState("");
  const [movieType, setMovieType] = useState("movie");
  const [status, setStatus] = useState("completed");
  const [releaseDate, setReleaseDate] = useState("");
  const [durationSeconds, setDurationSeconds] = useState("");
  const [totalSeasons, setTotalSeasons] = useState("");
  const [totalEpisodes, setTotalEpisodes] = useState("");
  const [year, setYear] = useState("");
  const [rated, setRated] = useState("0");
  const [regionID, setRegionID] = useState("");
  const [popularity, setPopularity] = useState("");
  const [tagIDsInput, setTagIDsInput] = useState("");
  const [people, setPeople] = useState([
    { personID: "", role: "cast", characterName: "", creditOrder: "" },
  ]);
  const [movieImages, setMovieImages] = useState([]);
  const [movieImagesPreviews, setMovieImagesPreviews] = useState([]);

  const [submitting, setSubmitting] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  // Parse tag IDs
  const parsedTagIDs = useMemo(() => {
    return tagIDsInput
      .split(",")
      .map((s) => s.trim())
      .filter(Boolean)
      .map((n) => Number(n))
      .filter((n) => !Number.isNaN(n));
  }, [tagIDsInput]);

  // Load movie data
  useEffect(() => {
    const fetchMovie = async () => {
      try {
        setLoading(true);
        const response = await getMovieById(movieID);
        
        // AXIOS trả về data trực tiếp, KHÔNG dùng .json()
        const data = response.data; 

        if (data.errorCode === 200 && data.data) {
          const movie = data.data;
          setSlug(movie.slug || "");
          setTitle(movie.title || "");
          setImagePoster(movie.image || "");
          setOriginalTitle(movie.originalTitle || "");
          setDescription(movie.description || "");
          setMovieType(movie.movieType || "movie");
          setStatus(movie.status || "completed");
          setReleaseDate(movie.releaseDate ? movie.releaseDate.split("T")[0] : "");
          setDurationSeconds(movie.durationSeconds || "");
          setTotalSeasons(movie.totalSeasons || "");
          setTotalEpisodes(movie.totalEpisodes || "");
          setYear(movie.year || "");
          setRated(movie.rated || "0");
          setRegionID(movie.regionID || "");
          setPopularity(movie.popularity || "");

          if (movie.movieTags) {
            setTagIDsInput(movie.movieTags.map(t => t.tagID).join(","));
          }

          if (movie.credits && movie.credits.length > 0) {
            setPeople(movie.credits.map(c => ({
              personID: c.personID || "",
              role: c.role || "cast",
              characterName: c.characterName || "",
              creditOrder: c.creditOrder || "",
            })));
          }
          setError("");
        } else {
          setError(data.errorMessage || "Không thể tải dữ liệu phim");
        }
      } catch (err) {
        console.error("Error loading movie:", err);
        setError("Có lỗi xảy ra khi tải phim");
      } finally {
        setLoading(false);
      }
    };

    fetchMovie();
  }, [movieID]);

  useEffect(() => {
    const fetchMovieAndPeople = async () => {
      try {
        setLoading(true);
        setError("");

        // Gọi đồng thời 2 API
        const [movieRes, personsRes] = await Promise.all([
          getMovieById(movieID),
          getPersonsByMovie(movieID)
        ]);

        const movieData = movieRes.data;
        const personsData = personsRes.data;

        // 1. Xử lý thông tin phim cơ bản
        if (movieData.errorCode === 200 && movieData.data) {
          const movie = movieData.data;
          setSlug(movie.slug || "");
          setTitle(movie.title || "");
          setImagePoster(movie.image || "");
          setOriginalTitle(movie.originalTitle || "");
          setDescription(movie.description || "");
          setMovieType(movie.movieType || "movie");
          setStatus(movie.status || "completed");
          setReleaseDate(movie.releaseDate ? movie.releaseDate.split("T")[0] : "");
          setDurationSeconds(movie.durationSeconds || "");
          setTotalSeasons(movie.totalSeasons || "");
          setTotalEpisodes(movie.totalEpisodes || "");
          setYear(movie.year || "");
          setRated(movie.rated || "0");
          setRegionID(movie.regionID || "");
          setPopularity(movie.popularity || "");

          // Load Tags
          if (movie.movieTags && movie.movieTags.length > 0) {
            setTagIDsInput(movie.movieTags.map((tag) => tag.tagID).join(","));
          }

          // 2. Xử lý danh sách Người tham gia (Credits)
          // Ưu tiên lấy từ movie.credits vì nó chứa Role và CharacterName
          if (movie.credits && movie.credits.length > 0) {
            const mappedPeople = movie.credits.map((c) => ({
              personID: c.personID || "",
              role: c.role || "cast",
              characterName: c.characterName || "",
              creditOrder: c.creditOrder || "",
            }));
            setPeople(mappedPeople);
          } 
          // Nếu movie.credits trống, thử lấy từ API GetPersonsByMovie
          else if (personsData.errorCode === 200 && personsData.data.length > 0) {
            const mappedPeople = personsData.data.map((p) => ({
              personID: p.personID,
              role: "cast", // Mặc định nếu không có dữ liệu role
              characterName: "",
              creditOrder: "",
            }));
            setPeople(mappedPeople);
          }
        } else {
          setError(movieData.errorMessage || "Không thể tải dữ liệu phim");
        }
      } catch (err) {
        console.error("Error loading movie detail:", err);
        setError("Có lỗi xảy ra khi tải thông tin chi tiết phim");
      } finally {
        setLoading(false);
      }
    };

    fetchMovieAndPeople();
  }, [movieID]);

  const handlePosterChange = (e) => {
    const file = e.target.files?.[0];
    if (file) {
      setImage(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleMovieImagesChange = (e) => {
    const files = Array.from(e.target.files || []);
    setMovieImages(files);
    
    // Create previews
    const previews = [];
    files.forEach((file) => {
      const reader = new FileReader();
      reader.onloadend = () => {
        previews.push(reader.result);
        if (previews.length === files.length) {
          setMovieImagesPreviews([...previews]);
        }
      };
      reader.readAsDataURL(file);
    });
  };

  const removeMovieImage = (index) => {
    setMovieImages((prev) => prev.filter((_, i) => i !== index));
    setMovieImagesPreviews((prev) => prev.filter((_, i) => i !== index));
  };

  const updatePerson = (index, key, value) => {
    setPeople((prev) =>
      prev.map((p, i) => (i === index ? { ...p, [key]: value } : p))
    );
  };

  const addPerson = () => {
    setPeople((prev) => [
      ...prev,
      { personID: "", role: "cast", characterName: "", creditOrder: "" },
    ]);
  };

  const removePerson = (index) => {
    setPeople((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    setSuccess("");

    try {
      const fd = new FormData();
      fd.append("movieID", movieID); // Bắt buộc phải có ID để update
      fd.append("slug", slug);
      fd.append("title", title);
      if (image) fd.append("image", image);
      fd.append("originalTitle", originalTitle || "");
      fd.append("description", description || "");
      fd.append("movieType", movieType);
      fd.append("status", status);
      if (releaseDate) fd.append("releaseDate", releaseDate);
      if (year) fd.append("year", year);
      fd.append("rated", rated);
      fd.append("regionID", regionID);
      fd.append("popularity", popularity || 0);
      fd.append("durationSeconds", durationSeconds || 0);

      // Tags
      parsedTagIDs.forEach((id) => fd.append("tagIDs", id));

      // Persons (Theo cấu trúc Swagger: person[0].personID)
      people.filter(p => p.personID).forEach((p, idx) => {
        fd.append(`person[${idx}].personID`, p.personID);
        fd.append(`person[${idx}].role`, p.role);
        fd.append(`person[${idx}].characterName`, p.characterName || "");
        fd.append(`person[${idx}].creditOrder`, p.creditOrder || 0);
      });

      // Movie Images (Swagger ghi là MovieImage viết hoa chữ M)
      movieImages.forEach((file) => {
        fd.append("MovieImage", file); 
      });

      const response = await updateMovie(fd);
      
      // AXIOS: Kết quả nằm trong response.data
      const data = response.data;

      if (data.errorCode === 200) {
        setSuccess("Cập nhật phim thành công!");
        setTimeout(() => navigate("/movies"), 2000);
      } else {
        setError(data.errorMessage || "Cập nhật phim thất bại");
      }
    } catch (err) {
      // Axios bắn lỗi vào catch nếu status code >= 400
      const msg = err.response?.data?.errorMessage || err.message;
      setError("Lỗi: " + msg);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" height="100vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Header title="CHỈNH SỬA PHIM" subtitle="Cập nhật thông tin phim" />
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
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 3 }}>
          {success}
        </Alert>
      )}

      <Box component="form" onSubmit={handleSubmit}>
        {/* Thông tin cơ bản */}
        <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
          <CardContent>
            <Typography variant="h5" color={colors.grey[100]} fontWeight="600" mb={3}>
              Thông tin cơ bản
            </Typography>
            <Stack spacing={2}>
              <TextField
                fullWidth
                variant="filled"
                label="Slug"
                value={slug}
                onChange={(e) => setSlug(e.target.value)}
                required
              />
              <TextField
                fullWidth
                variant="filled"
                label="Tên phim"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                required
              />
              <TextField
                fullWidth
                variant="filled"
                label="Tên gốc"
                value={originalTitle}
                onChange={(e) => setOriginalTitle(e.target.value)}
              />

              {/* Poster Image */}
              <Box>
                <Typography variant="body2" color={colors.grey[300]} mb={1}>
                  Poster Image {image && <span style={{ color: colors.greenAccent[500] }}>✓ Đã chọn ảnh mới</span>}
                </Typography>
                
                {/* Show current poster if exists and no new image selected */}
                {imagePoster && !imagePreview && (
                  <Box mb={2}>
                    <Typography variant="caption" color={colors.grey[400]} display="block" mb={1}>
                      Ảnh hiện tại:
                    </Typography>
                    <img
                      src={imagePoster}
                      alt="Current poster"
                      style={{
                        maxWidth: "200px",
                        maxHeight: "300px",
                        borderRadius: "8px",
                        border: `2px solid ${colors.grey[700]}`,
                      }}
                    />
                  </Box>
                )}
                
                <input
                  type="file"
                  accept="image/*"
                  onChange={handlePosterChange}
                  style={{
                    width: "100%",
                    padding: "10px",
                    backgroundColor: colors.primary[500],
                    border: `1px solid ${colors.grey[700]}`,
                    borderRadius: "4px",
                    color: colors.grey[100],
                  }}
                />
                
                {/* Show new image preview */}
                {imagePreview && (
                  <Box mt={2} display="flex" justifyContent="center">
                    <Box position="relative">
                      <img
                        src={imagePreview}
                        alt="New poster preview"
                        style={{
                          maxWidth: "200px",
                          maxHeight: "300px",
                          borderRadius: "8px",
                          border: `2px solid ${colors.greenAccent[500]}`,
                        }}
                      />
                      <Typography
                        variant="caption"
                        sx={{
                          position: "absolute",
                          bottom: -25,
                          left: 0,
                          right: 0,
                          textAlign: "center",
                          color: colors.greenAccent[400],
                        }}
                      >
                        Ảnh mới
                      </Typography>
                    </Box>
                  </Box>
                )}
              </Box>

              <TextField
                fullWidth
                variant="filled"
                label="Mô tả"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                multiline
                rows={4}
              />
            </Stack>
          </CardContent>
        </Card>

        {/* Thông tin phim */}
        <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
          <CardContent>
            <Typography variant="h5" color={colors.grey[100]} fontWeight="600" mb={3}>
              Thông tin phim
            </Typography>
            <Stack spacing={2}>
              <TextField
                fullWidth
                variant="filled"
                select
                label="Loại phim"
                value={movieType}
                onChange={(e) => setMovieType(e.target.value)}
              >
                <MenuItem value="movie">Phim lẻ</MenuItem>
                <MenuItem value="series">Phim bộ</MenuItem>
              </TextField>

              <TextField
                fullWidth
                variant="filled"
                select
                label="Trạng thái"
                value={status}
                onChange={(e) => setStatus(e.target.value)}
              >
                <MenuItem value="ongoing">Đang chiếu</MenuItem>
                <MenuItem value="completed">Hoàn thành</MenuItem>
                <MenuItem value="coming_soon">Sắp ra mắt</MenuItem>
              </TextField>

              <TextField
                fullWidth
                variant="filled"
                type="date"
                label="Ngày phát hành"
                value={releaseDate}
                onChange={(e) => setReleaseDate(e.target.value)}
                InputLabelProps={{ shrink: true }}
              />

              {movieType === "movie" && (
                <TextField
                  fullWidth
                  variant="filled"
                  type="number"
                  label="Thời lượng (giây)"
                  value={durationSeconds}
                  onChange={(e) => setDurationSeconds(e.target.value)}
                />
              )}

              {movieType === "series" && (
                <>
                  <TextField
                    fullWidth
                    variant="filled"
                    type="number"
                    label="Số mùa"
                    value={totalSeasons}
                    onChange={(e) => setTotalSeasons(e.target.value)}
                  />
                  <TextField
                    fullWidth
                    variant="filled"
                    type="number"
                    label="Số tập"
                    value={totalEpisodes}
                    onChange={(e) => setTotalEpisodes(e.target.value)}
                  />
                </>
              )}

              <TextField
                fullWidth
                variant="filled"
                type="number"
                label="Năm"
                value={year}
                onChange={(e) => setYear(e.target.value)}
              />

              <TextField
                fullWidth
                variant="filled"
                label="Rated"
                value={rated}
                onChange={(e) => setRated(e.target.value)}
              />

              <TextField
                fullWidth
                variant="filled"
                type="number"
                label="Region ID *"
                value={regionID}
                onChange={(e) => setRegionID(e.target.value)}
                required
              />

              <TextField
                fullWidth
                variant="filled"
                type="number"
                label="Độ phổ biến"
                value={popularity}
                onChange={(e) => setPopularity(e.target.value)}
                inputProps={{ step: "0.1" }}
              />

              <Box>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Tag IDs (phân cách bằng dấu phẩy)"
                  value={tagIDsInput}
                  onChange={(e) => setTagIDsInput(e.target.value)}
                  placeholder="VD: 1,2,3"
                />
                {parsedTagIDs.length > 0 && (
                  <Typography variant="caption" color={colors.grey[400]} mt={1} display="block">
                    Parsed: [{parsedTagIDs.join(", ")}]
                  </Typography>
                )}
              </Box>
            </Stack>
          </CardContent>
        </Card>

        {/* Người tham gia */}
        <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
          <CardContent>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
              <Typography variant="h5" color={colors.grey[100]} fontWeight="600">
                Người tham gia
              </Typography>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={addPerson}
                sx={{
                  backgroundColor: colors.greenAccent[600],
                  "&:hover": {
                    backgroundColor: colors.greenAccent[700],
                  },
                }}
              >
                Thêm người
              </Button>
            </Box>

            <TableContainer component={Paper} sx={{ backgroundColor: colors.primary[500] }}>
              <Table>
                <TableHead>
                  <TableRow sx={{ backgroundColor: colors.blueAccent[700] }}>
                    <TableCell sx={{ color: colors.grey[100] }}>Person ID</TableCell>
                    <TableCell sx={{ color: colors.grey[100] }}>Vai trò</TableCell>
                    <TableCell sx={{ color: colors.grey[100] }}>Tên nhân vật</TableCell>
                    <TableCell sx={{ color: colors.grey[100] }}>Thứ tự</TableCell>
                    <TableCell sx={{ color: colors.grey[100] }}>Hành động</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {people.map((person, index) => (
                    <TableRow key={index}>
                      <TableCell>
                        <TextField
                          variant="filled"
                          type="number"
                          size="small"
                          value={person.personID}
                          onChange={(e) => updatePerson(index, "personID", e.target.value)}
                          sx={{ width: "120px" }}
                        />
                      </TableCell>
                      <TableCell>
                        <TextField
                          variant="filled"
                          select
                          size="small"
                          value={person.role}
                          onChange={(e) => updatePerson(index, "role", e.target.value)}
                          sx={{ width: "150px" }}
                        >
                          <MenuItem value="cast">Cast</MenuItem>
                          <MenuItem value="director">Director</MenuItem>
                          <MenuItem value="writer">Writer</MenuItem>
                          <MenuItem value="producer">Producer</MenuItem>
                          <MenuItem value="editor">Editor</MenuItem>
                          <MenuItem value="cinematographer">Cinematographer</MenuItem>
                          <MenuItem value="composer">Composer</MenuItem>
                        </TextField>
                      </TableCell>
                      <TableCell>
                        <TextField
                          variant="filled"
                          size="small"
                          value={person.characterName}
                          onChange={(e) => updatePerson(index, "characterName", e.target.value)}
                          placeholder="Optional"
                          sx={{ width: "180px" }}
                        />
                      </TableCell>
                      <TableCell>
                        <TextField
                          variant="filled"
                          type="number"
                          size="small"
                          value={person.creditOrder}
                          onChange={(e) => updatePerson(index, "creditOrder", e.target.value)}
                          placeholder="Optional"
                          sx={{ width: "100px" }}
                        />
                      </TableCell>
                      <TableCell>
                        <IconButton
                          onClick={() => removePerson(index)}
                          disabled={people.length === 1}
                          sx={{
                            color: colors.redAccent[500],
                            "&:hover": {
                              backgroundColor: colors.redAccent[800],
                            },
                          }}
                        >
                          <DeleteOutlineIcon />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>

        {/* Hình ảnh bổ sung */}
        <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
          <CardContent>
            <Typography variant="h5" color={colors.grey[100]} fontWeight="600" mb={2}>
              Hình ảnh bổ sung
            </Typography>
            <Typography variant="caption" color={colors.grey[400]} display="block" mb={2}>
              Chỉ chọn file nếu bạn muốn thêm ảnh mới
            </Typography>
            <input
              type="file"
              accept="image/*"
              multiple
              onChange={handleMovieImagesChange}
              style={{
                width: "100%",
                padding: "10px",
                backgroundColor: colors.primary[500],
                border: `1px solid ${colors.grey[700]}`,
                borderRadius: "4px",
                color: colors.grey[100],
              }}
            />
            {movieImages.length > 0 && (
              <Typography variant="caption" color={colors.grey[400]} mt={1} display="block">
                Đã chọn: {movieImages.length} file(s) mới
              </Typography>
            )}

            {/* Image Previews */}
            {movieImagesPreviews.length > 0 && (
              <Box mt={3}>
                <Typography variant="body2" color={colors.grey[300]} mb={2}>
                  Xem trước ảnh mới:
                </Typography>
                <Grid container spacing={2}>
                  {movieImagesPreviews.map((preview, index) => (
                    <Grid item xs={12} sm={6} md={4} key={index}>
                      <Box
                        position="relative"
                        sx={{
                          border: `2px solid ${colors.greenAccent[500]}`,
                          borderRadius: "8px",
                          overflow: "hidden",
                        }}
                      >
                        <img
                          src={preview}
                          alt={`Preview ${index + 1}`}
                          style={{
                            width: "100%",
                            height: "200px",
                            objectFit: "cover",
                          }}
                        />
                        <IconButton
                          onClick={() => removeMovieImage(index)}
                          sx={{
                            position: "absolute",
                            top: 5,
                            right: 5,
                            backgroundColor: "rgba(0,0,0,0.6)",
                            color: colors.redAccent[500],
                            "&:hover": {
                              backgroundColor: "rgba(0,0,0,0.8)",
                            },
                          }}
                          size="small"
                        >
                          <CloseIcon fontSize="small" />
                        </IconButton>
                      </Box>
                    </Grid>
                  ))}
                </Grid>
              </Box>
            )}
          </CardContent>
        </Card>

        {/* Submit button */}
        <Box display="flex" justifyContent="flex-end" gap={2}>
          <Button
            variant="outlined"
            onClick={() => navigate("/movies")}
            sx={{
              borderColor: colors.grey[400],
              color: colors.grey[100],
            }}
          >
            Hủy
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={submitting}
            sx={{
              backgroundColor: colors.greenAccent[600],
              color: colors.grey[100],
              fontSize: "14px",
              fontWeight: "bold",
              "&:hover": {
                backgroundColor: colors.greenAccent[700],
              },
              "&:disabled": {
                backgroundColor: colors.greenAccent[300],
              },
            }}
          >
            {submitting ? "Đang cập nhật..." : "Cập nhật phim"}
          </Button>
        </Box>
      </Box>
    </Box>
  );
};

export default MovieEdit;