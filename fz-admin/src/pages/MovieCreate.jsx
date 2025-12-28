import { useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
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
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import { createMovie } from "../services/api";

const MovieCreate = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  // Form states
  const [slug, setSlug] = useState("");
  const [title, setTitle] = useState("");
  const [image, setImage] = useState(null); // poster
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

  const [submitting, setSubmitting] = useState(false);
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

    // Validation
    if (!image) {
      setError("Poster image là bắt buộc");
      setSubmitting(false);
      return;
    }
    if (movieImages.length === 0) {
      setError("Vui lòng chọn ít nhất 1 ảnh phụ");
      setSubmitting(false);
      return;
    }
    if (!regionID) {
      setError("Region ID là bắt buộc");
      setSubmitting(false);
      return;
    }

    try {
      const fd = new FormData();

      // Scalars
      fd.append("slug", slug);
      fd.append("title", title);
      fd.append("image", image);
      if (originalTitle) fd.append("originalTitle", originalTitle);
      if (description) fd.append("description", description);
      fd.append("movieType", movieType);
      fd.append("status", status);
      if (releaseDate) fd.append("releaseDate", releaseDate);

      if (movieType === "movie" && durationSeconds !== "") {
        fd.append("durationSeconds", String(durationSeconds));
      }
      if (movieType === "series") {
        if (totalSeasons !== "") fd.append("totalSeasons", String(totalSeasons));
        if (totalEpisodes !== "") fd.append("totalEpisodes", String(totalEpisodes));
      }

      if (year !== "") fd.append("year", String(year));
      if (rated) fd.append("rated", rated);
      fd.append("regionID", String(regionID));
      if (popularity !== "") fd.append("popularity", String(popularity));

      // tagIDs[]
      parsedTagIDs.forEach((id, idx) => {
        fd.append(`tagIDs[${idx}]`, String(id));
      });

      // person[]
      const validPeople = people.filter((p) => p.personID !== "");
      validPeople.forEach((p, idx) => {
        fd.append("person.index", String(idx));
        fd.append(`person[${idx}].personID`, String(p.personID));
        fd.append(`person[${idx}].role`, p.role);
        if (p.characterName) fd.append(`person[${idx}].characterName`, p.characterName);
        if (p.creditOrder !== "") fd.append(`person[${idx}].creditOrder`, String(p.creditOrder));
      });

      // movieImages[]
      movieImages.forEach((file, idx) => {
        fd.append("movieImages.index", String(idx));
        fd.append(`movieImages[${idx}].image`, file);
      });

      const response = await createMovie(fd);

      if (response.data.errorCode === 200) {
        setSuccess("Tạo phim thành công!");
        setTimeout(() => {
          navigate("/movies");
        }, 2000);
      } else {
        setError(response.data.errorMessage || "Tạo phim thất bại");
      }
    } catch (err) {
      console.error("Error creating movie:", err);
      setError(err.response?.data?.errorMessage || "Có lỗi xảy ra khi tạo phim");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box m="20px">
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Header title="THÊM PHIM MỚI" subtitle="Tạo phim mới trong hệ thống" />
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
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Slug"
                  value={slug}
                  onChange={(e) => setSlug(e.target.value)}
                  required
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Tên phim"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  required
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Tên gốc"
                  value={originalTitle}
                  onChange={(e) => setOriginalTitle(e.target.value)}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="body2" color={colors.grey[300]} mb={1}>
                  Poster Image *
                </Typography>
                <input
                  type="file"
                  accept="image/*"
                  onChange={(e) => setImage(e.target.files?.[0] || null)}
                  style={{
                    width: "100%",
                    padding: "10px",
                    backgroundColor: colors.primary[500],
                    border: `1px solid ${colors.grey[700]}`,
                    borderRadius: "4px",
                    color: colors.grey[100],
                  }}
                  required
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Mô tả"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  multiline
                  rows={4}
                />
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        {/* Thông tin phim */}
        <Card sx={{ backgroundColor: colors.primary[400], mb: 3 }}>
          <CardContent>
            <Typography variant="h5" color={colors.grey[100]} fontWeight="600" mb={3}>
              Thông tin phim
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
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
              </Grid>
              <Grid item xs={12} md={4}>
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
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  variant="filled"
                  type="date"
                  label="Ngày phát hành"
                  value={releaseDate}
                  onChange={(e) => setReleaseDate(e.target.value)}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>

              {movieType === "movie" && (
                <Grid item xs={12} md={4}>
                  <TextField
                    fullWidth
                    variant="filled"
                    type="number"
                    label="Thời lượng (giây)"
                    value={durationSeconds}
                    onChange={(e) => setDurationSeconds(e.target.value)}
                  />
                </Grid>
              )}

              {movieType === "series" && (
                <>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      variant="filled"
                      type="number"
                      label="Số mùa"
                      value={totalSeasons}
                      onChange={(e) => setTotalSeasons(e.target.value)}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      variant="filled"
                      type="number"
                      label="Số tập"
                      value={totalEpisodes}
                      onChange={(e) => setTotalEpisodes(e.target.value)}
                    />
                  </Grid>
                </>
              )}

              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  variant="filled"
                  type="number"
                  label="Năm"
                  value={year}
                  onChange={(e) => setYear(e.target.value)}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Rated"
                  value={rated}
                  onChange={(e) => setRated(e.target.value)}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  variant="filled"
                  type="number"
                  label="Region ID *"
                  value={regionID}
                  onChange={(e) => setRegionID(e.target.value)}
                  required
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  variant="filled"
                  type="number"
                  label="Độ phổ biến"
                  value={popularity}
                  onChange={(e) => setPopularity(e.target.value)}
                  inputProps={{ step: "0.1" }}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  variant="filled"
                  label="Tag IDs (phân cách bằng dấu phẩy)"
                  value={tagIDsInput}
                  onChange={(e) => setTagIDsInput(e.target.value)}
                  placeholder="VD: 1,2,3"
                />
                {parsedTagIDs.length > 0 && (
                  <Typography variant="caption" color={colors.grey[400]} mt={1}>
                    Parsed: [{parsedTagIDs.join(", ")}]
                  </Typography>
                )}
              </Grid>
            </Grid>
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
              Hình ảnh bổ sung *
            </Typography>
            <input
              type="file"
              accept="image/*"
              multiple
              onChange={(e) => setMovieImages(Array.from(e.target.files || []))}
              style={{
                width: "100%",
                padding: "10px",
                backgroundColor: colors.primary[500],
                border: `1px solid ${colors.grey[700]}`,
                borderRadius: "4px",
                color: colors.grey[100],
              }}
              required
            />
            {movieImages.length > 0 && (
              <Typography variant="caption" color={colors.grey[400]} mt={1}>
                Đã chọn: {movieImages.length} file(s)
              </Typography>
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
            }}
          >
            {submitting ? "Đang tạo..." : "Tạo phim"}
          </Button>
        </Box>
      </Box>
    </Box>
  );
};

export default MovieCreate;