import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box, Typography, useTheme, Button, TextField, Card, CardContent,
  Alert, MenuItem, Select, FormControl, InputLabel, IconButton,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, Chip, OutlinedInput, Divider, Stack
} from "@mui/material";
import { tokens } from "../theme";
import Header from "../components/Header";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";
import ImageIcon from "@mui/icons-material/Image";
import { createMovie, getAllRegions, getAllTags } from "../services/api";

const MovieCreate = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [regions, setRegions] = useState([]);
  const [tags, setTags] = useState([]);

  // Form states
  const [slug, setSlug] = useState("");
  const [title, setTitle] = useState("");
  const [image, setImage] = useState(null);
  const [imagePreview, setImagePreview] = useState(null);
  const [originalTitle, setOriginalTitle] = useState("");
  const [description, setDescription] = useState("");
  const [movieType, setMovieType] = useState("movie");
  const [status, setStatus] = useState("completed");
  const [releaseDate, setReleaseDate] = useState(new Date().toISOString().split('T')[0]);
  const [durationSeconds, setDurationSeconds] = useState(0);
  const [totalSeasons, setTotalSeasons] = useState(0);
  const [totalEpisodes, setTotalEpisodes] = useState(0);
  const [year, setYear] = useState(new Date().getFullYear());
  const [rated, setRated] = useState("1");
  const [regionID, setRegionID] = useState("");
  const [popularity, setPopularity] = useState(1);
  const [selectedTagIDs, setSelectedTagIDs] = useState([]);
  const [people, setPeople] = useState([
    { personID: "", role: "cast", characterName: "", creditOrder: 0 },
  ]);
  const [movieImages, setMovieImages] = useState([]);

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    const fetchMetadata = async () => {
      try {
        const [regionRes, tagRes] = await Promise.all([getAllRegions(), getAllTags()]);
        if (regionRes.data.errorCode === 200) setRegions(regionRes.data.data);
        if (tagRes.data.errorCode === 200) setTags(tagRes.data.data);
      } catch (err) {
        console.error("Error fetching metadata:", err);
      }
    };
    fetchMetadata();
  }, []);

  const handleImageChange = (e) => {
    const file = e.target.files?.[0] || null;
    setImage(file);
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => setImagePreview(reader.result);
      reader.readAsDataURL(file);
    }
  };

  const handleMovieImagesChange = (e) => {
    setMovieImages(Array.from(e.target.files ?? []));
  };

  const updatePerson = (index, key, value) => {
    setPeople((prev) => prev.map((p, i) => (i === index ? { ...p, [key]: value } : p)));
  };

  const addPerson = () => {
    setPeople((prev) => [...prev, { personID: "", role: "cast", characterName: "", creditOrder: 0 }]);
  };

  const removePerson = (index) => {
    setPeople((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    setSuccess("");

    if (!image || movieImages.length === 0 || !regionID) {
      setError("Vui lòng điền đủ: Poster, Additional Images và Region.");
      setSubmitting(false);
      return;
    }

    try {
      const fd = new FormData();

      fd.append("slug", slug);
      fd.append("title", title);
      fd.append("originalTitle", originalTitle);
      fd.append("description", description);
      fd.append("movieType", movieType);
      fd.append("status", status);
      fd.append("releaseDate", new Date(releaseDate).toISOString());
      fd.append("durationSeconds", parseInt(durationSeconds));
      fd.append("totalSeasons", parseInt(totalSeasons));
      fd.append("totalEpisodes", parseInt(totalEpisodes));
      fd.append("year", parseInt(year));
      fd.append("rated", rated);
      fd.append("regionID", parseInt(regionID));
      fd.append("popularity", parseFloat(popularity));
      fd.append("image", image);

      selectedTagIDs.forEach((id) => {
        fd.append("tagIDs", id);
      });

      people.filter(p => p.personID).forEach((p, idx) => {
        fd.append(`person[${idx}].personID`, parseInt(p.personID));
        fd.append(`person[${idx}].role`, p.role);
        fd.append(`person[${idx}].characterName`, p.characterName || "");
        fd.append(`person[${idx}].creditOrder`, parseInt(p.creditOrder || 0));
      });

      movieImages.forEach((file, idx) => {
        fd.append('movieImages.index', String(idx));
        fd.append(`movieImages[${idx}].image`, file);
      });

      const response = await createMovie(fd);

      if (response.data.errorCode === 200) {
        setSuccess("Tạo phim thành công!");
        setTimeout(() => navigate("/movies"), 2000);
      } else {
        setError(response.data.errorMessage || "Tạo phim thất bại");
      }
    } catch (err) {
      console.error("Submit error:", err);
      setError(err.response?.data?.title || "Lỗi dữ liệu đầu vào. Vui lòng kiểm tra lại.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box m="20px" maxWidth="800px" mx="auto">
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={4}>
        <Header title="THÊM PHIM MỚI" subtitle="Tạo phim mới dựa trên API" />
        <Button 
          startIcon={<ArrowBackIcon />} 
          onClick={() => navigate("/movies")} 
          variant="outlined"
          size="large"
          sx={{ 
            color: colors.grey[100], 
            borderColor: colors.grey[400],
            fontSize: "15px",
            fontWeight: 600,
            px: 3
          }}
        >
          Quay lại
        </Button>
      </Box>

      {/* Alerts */}
      {error && <Alert severity="error" sx={{ mb: 3, fontSize: "15px" }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 3, fontSize: "15px" }}>{success}</Alert>}

      {/* Form */}
      <Card sx={{ backgroundColor: colors.primary[400], boxShadow: 3 }}>
        <CardContent sx={{ p: 4 }}>
          <Box component="form" onSubmit={handleSubmit}>
            
            {/* SECTION 1: Poster Preview */}
            {imagePreview && (
              <Box mb={4} display="flex" justifyContent="center">
                <Box 
                  component="img" 
                  src={imagePreview} 
                  sx={{ 
                    width: 240, 
                    height: 360, 
                    objectFit: "cover", 
                    borderRadius: 2,
                    border: `3px solid ${colors.greenAccent[500]}`,
                    boxShadow: 4
                  }} 
                />
              </Box>
            )}

            {/* SECTION 2: Basic Info */}
            <Box mb={4}>
              <Typography 
                variant="h3" 
                color={colors.greenAccent[400]} 
                fontWeight={700}
                mb={3}
                sx={{ fontSize: "22px" }}
              >
                1. THÔNG TIN CƠ BẢN
              </Typography>
              <Divider sx={{ mb: 3, borderColor: colors.grey[700] }} />
              
              <Stack spacing={3}>
                <TextField 
                  fullWidth 
                  variant="filled" 
                  label="Slug (Định danh URL)" 
                  value={slug} 
                  onChange={(e) => setSlug(e.target.value)} 
                  required
                  InputProps={{ style: { fontSize: "16px" } }}
                  InputLabelProps={{ style: { fontSize: "16px" } }}
                />
                
                <TextField 
                  fullWidth 
                  variant="filled" 
                  label="Tiêu đề phim" 
                  value={title} 
                  onChange={(e) => setTitle(e.target.value)} 
                  required
                  InputProps={{ style: { fontSize: "16px" } }}
                  InputLabelProps={{ style: { fontSize: "16px" } }}
                />
                
                <TextField 
                  fullWidth 
                  variant="filled" 
                  label="Tên gốc (Original Title)" 
                  value={originalTitle} 
                  onChange={(e) => setOriginalTitle(e.target.value)}
                  InputProps={{ style: { fontSize: "16px" } }}
                  InputLabelProps={{ style: { fontSize: "16px" } }}
                />
                
                <Box>
                  <Typography variant="h6" mb={1} sx={{ fontSize: "16px", fontWeight: 600 }}>
                    Ảnh Poster <span style={{ color: colors.redAccent[500] }}>*</span>
                  </Typography>
                  <Button
                    variant="contained"
                    component="label"
                    startIcon={<ImageIcon />}
                    sx={{ 
                      fontSize: "15px", 
                      py: 1.5, 
                      px: 3,
                      backgroundColor: colors.blueAccent[600],
                      color: "#fff",
                      fontWeight: 600,
                      "&:hover": {
                        backgroundColor: colors.blueAccent[700]
                      }
                    }}
                  >
                    Chọn ảnh poster
                    <input type="file" accept="image/*" onChange={handleImageChange} hidden required />
                  </Button>
                  {image && (
                    <Typography variant="body2" mt={1} color={colors.greenAccent[400]} sx={{ fontSize: "14px" }}>
                      ✓ Đã chọn: {image.name}
                    </Typography>
                  )}
                </Box>
                
                <TextField 
                  fullWidth 
                  variant="filled" 
                  label="Mô tả phim" 
                  value={description} 
                  onChange={(e) => setDescription(e.target.value)} 
                  multiline 
                  rows={4}
                  InputProps={{ style: { fontSize: "16px" } }}
                  InputLabelProps={{ style: { fontSize: "16px" } }}
                />
              </Stack>
            </Box>

            {/* SECTION 3: Region & Tags */}
            <Box mb={4}>
              <Typography 
                variant="h3" 
                color={colors.greenAccent[400]} 
                fontWeight={700}
                mb={3}
                sx={{ fontSize: "22px" }}
              >
                2. PHÂN LOẠI & QUỐC GIA
              </Typography>
              <Divider sx={{ mb: 3, borderColor: colors.grey[700] }} />
              
              <Stack spacing={3}>
                <FormControl fullWidth variant="filled" required>
                  <InputLabel sx={{ fontSize: "16px" }}>Quốc gia (Region)</InputLabel>
                  <Select 
                    value={regionID} 
                    onChange={(e) => setRegionID(e.target.value)}
                    sx={{ fontSize: "16px" }}
                  >
                    {regions.map(r => (
                      <MenuItem key={r.regionID} value={r.regionID} sx={{ fontSize: "16px" }}>
                        {r.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
                
                <FormControl fullWidth variant="filled">
                  <InputLabel sx={{ fontSize: "16px" }}>Thể loại (Tags)</InputLabel>
                  <Select
                    multiple 
                    value={selectedTagIDs}
                    onChange={(e) => setSelectedTagIDs(e.target.value)}
                    input={<OutlinedInput label="Tags" />}
                    sx={{ fontSize: "16px" }}
                    renderValue={(selected) => (
                      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                        {selected.map((val) => (
                          <Chip 
                            key={val} 
                            label={tags.find(t => t.tagID === val)?.tagName || val}
                            sx={{ fontSize: "14px" }}
                          />
                        ))}
                      </Box>
                    )}
                  >
                    {tags.map(t => (
                      <MenuItem key={t.tagID} value={t.tagID} sx={{ fontSize: "16px" }}>
                        {t.tagName}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Stack>
            </Box>

            {/* SECTION 4: Technical Details */}
            <Box mb={4}>
              <Typography 
                variant="h3" 
                color={colors.greenAccent[400]} 
                fontWeight={700}
                mb={3}
                sx={{ fontSize: "22px" }}
              >
                3. THÔNG SỐ KỸ THUẬT
              </Typography>
              <Divider sx={{ mb: 3, borderColor: colors.grey[700] }} />
              
              <Stack spacing={3}>
                <FormControl fullWidth variant="filled">
                  <InputLabel sx={{ fontSize: "16px" }}>Loại phim (Type)</InputLabel>
                  <Select 
                    value={movieType} 
                    onChange={(e) => setMovieType(e.target.value)}
                    sx={{ fontSize: "16px" }}
                  >
                    <MenuItem value="movie" sx={{ fontSize: "16px" }}>Phim lẻ (Movie)</MenuItem>
                    <MenuItem value="series" sx={{ fontSize: "16px" }}>Phim bộ (Series)</MenuItem>
                  </Select>
                </FormControl>
                
                <TextField 
                  fullWidth 
                  variant="filled" 
                  label="Năm phát hành" 
                  type="number" 
                  value={year} 
                  onChange={(e) => setYear(e.target.value)}
                  InputProps={{ style: { fontSize: "16px" } }}
                  InputLabelProps={{ style: { fontSize: "16px" } }}
                />
                
                <TextField 
                  fullWidth 
                  variant="filled" 
                  label="Phân loại độ tuổi (Rated)" 
                  value={rated} 
                  onChange={(e) => setRated(e.target.value)}
                  InputProps={{ style: { fontSize: "16px" } }}
                  InputLabelProps={{ style: { fontSize: "16px" } }}
                />
                
                <TextField 
                  fullWidth 
                  variant="filled" 
                  label="Độ phổ biến (Popularity)" 
                  type="number" 
                  value={popularity} 
                  onChange={(e) => setPopularity(e.target.value)}
                  InputProps={{ style: { fontSize: "16px" } }}
                  InputLabelProps={{ style: { fontSize: "16px" } }}
                />
              </Stack>
            </Box>

            {/* SECTION 5: People */}
            <Box mb={4}>
              <Typography 
                variant="h3" 
                color={colors.greenAccent[400]} 
                fontWeight={700}
                mb={3}
                sx={{ fontSize: "22px" }}
              >
                4. NHÂN SỰ (DIỄN VIÊN & ĐẠO DIỄN)
              </Typography>
              <Divider sx={{ mb: 3, borderColor: colors.grey[700] }} />
              
              <TableContainer 
                component={Paper} 
                sx={{ 
                  backgroundColor: colors.primary[500],
                  boxShadow: 2
                }}
              >
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell sx={{ fontSize: "15px", fontWeight: 700 }}>Person ID</TableCell>
                      <TableCell sx={{ fontSize: "15px", fontWeight: 700 }}>Vai trò</TableCell>
                      <TableCell sx={{ fontSize: "15px", fontWeight: 700 }}>Tên nhân vật</TableCell>
                      <TableCell sx={{ fontSize: "15px", fontWeight: 700 }}>Hành động</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {people.map((p, idx) => (
                      <TableRow key={idx}>
                        <TableCell>
                          <TextField 
                            type="number" 
                            value={p.personID} 
                            onChange={(e) => updatePerson(idx, "personID", e.target.value)}
                            InputProps={{ style: { fontSize: "15px" } }}
                          />
                        </TableCell>
                        <TableCell>
                          <Select 
                            value={p.role} 
                            onChange={(e) => updatePerson(idx, "role", e.target.value)}
                            sx={{ fontSize: "15px" }}
                          >
                            <MenuItem value="cast" sx={{ fontSize: "15px" }}>Diễn viên</MenuItem>
                            <MenuItem value="director" sx={{ fontSize: "15px" }}>Đạo diễn</MenuItem>
                            <MenuItem value="writer" sx={{ fontSize: "15px" }}>Biên kịch</MenuItem>
                          </Select>
                        </TableCell>
                        <TableCell>
                          <TextField 
                            value={p.characterName} 
                            onChange={(e) => updatePerson(idx, "characterName", e.target.value)}
                            InputProps={{ style: { fontSize: "15px" } }}
                          />
                        </TableCell>
                        <TableCell>
                          <IconButton onClick={() => removePerson(idx)} color="error">
                            <DeleteIcon />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
                <Box p={2}>
                  <Button 
                    startIcon={<AddIcon />} 
                    onClick={addPerson}
                    variant="contained"
                    sx={{ 
                      fontSize: "15px",
                      backgroundColor: colors.greenAccent[600],
                      color: "#fff",
                      fontWeight: 600,
                      px: 3,
                      py: 1,
                      "&:hover": {
                        backgroundColor: colors.greenAccent[700]
                      }
                    }}
                  >
                    Thêm nhân sự
                  </Button>
                </Box>
              </TableContainer>
            </Box>

            {/* SECTION 6: Additional Images */}
            <Box mb={4}>
              <Typography 
                variant="h3" 
                color={colors.greenAccent[400]} 
                fontWeight={700}
                mb={3}
                sx={{ fontSize: "22px" }}
              >
                5. ẢNH BỔ SUNG
              </Typography>
              <Divider sx={{ mb: 3, borderColor: colors.grey[700] }} />
              
              <Box 
                sx={{ 
                  p: 3, 
                  border: `2px dashed ${colors.grey[600]}`,
                  borderRadius: 2,
                  textAlign: "center"
                }}
              >
                <Button
                  variant="contained"
                  component="label"
                  startIcon={<ImageIcon />}
                  sx={{ 
                    fontSize: "15px", 
                    py: 1.5, 
                    px: 4,
                    backgroundColor: colors.blueAccent[600]
                  }}
                >
                  Chọn nhiều ảnh
                  <input type="file" accept="image/*" multiple onChange={handleMovieImagesChange} hidden />
                </Button>
                <Typography 
                  variant="body1" 
                  mt={2} 
                  color={movieImages.length > 0 ? colors.greenAccent[400] : colors.grey[300]}
                  sx={{ fontSize: "15px", fontWeight: 600 }}
                >
                  {movieImages.length > 0 
                    ? `✓ Đã chọn ${movieImages.length} ảnh` 
                    : "Chưa chọn ảnh nào"}
                </Typography>
              </Box>
            </Box>

            {/* Action Buttons */}
            <Divider sx={{ my: 4, borderColor: colors.grey[700] }} />
            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button 
                variant="outlined" 
                onClick={() => navigate("/movies")}
                size="large"
                sx={{ fontSize: "16px", px: 4, py: 1.5 }}
              >
                Hủy
              </Button>
              <Button 
                type="submit" 
                variant="contained" 
                disabled={submitting}
                size="large"
                sx={{ 
                  backgroundColor: colors.greenAccent[600],
                  fontSize: "16px",
                  fontWeight: 700,
                  px: 4,
                  py: 1.5,
                  "&:hover": {
                    backgroundColor: colors.greenAccent[700]
                  }
                }}
              >
                {submitting ? "Đang xử lý..." : "TẠO PHIM NGAY"}
              </Button>
            </Stack>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default MovieCreate;