import React, { useState, useEffect } from "react";
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  Alert,
  IconButton,
  useTheme,
  LinearProgress,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import EditIcon from "@mui/icons-material/Edit";
import { tokens } from "../theme";
import { updateEpisode } from "../services/api";

const UpdateEpisodeModal = ({ open, onClose, episode, onSuccess }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  // State form
  const [seasonNumber, setSeasonNumber] = useState(0);
  const [episodeNumber, setEpisodeNumber] = useState(0);
  const [title, setTitle] = useState("");
  const [synopsis, setSynopsis] = useState("");
  const [description, setDescription] = useState("");
  const [durationSeconds, setDurationSeconds] = useState(0);
  const [releaseDate, setReleaseDate] = useState("");

  const [submitting, setSubmitting] = useState(false);
  const [errorMsg, setErrorMsg] = useState(null);
  const [successMsg, setSuccessMsg] = useState(null);

  // Load dữ liệu từ props episode vào form khi mở modal
  useEffect(() => {
    if (open && episode) {
      setSeasonNumber(episode.seasonNumber);
      setEpisodeNumber(episode.episodeNumber);
      setTitle(episode.title || "");
      setSynopsis(episode.synopsis || "");
      setDescription(episode.description || "");
      setDurationSeconds(episode.durationSeconds || 0);
      // Format date để hiển thị trong input type="date"
      const dateStr = episode.releaseDate ? new Date(episode.releaseDate).toISOString().split("T")[0] : "";
      setReleaseDate(dateStr);
      
      setErrorMsg(null);
      setSuccessMsg(null);
    }
  }, [open, episode]);

  const handleSubmit = async () => {
    setSubmitting(true);
    setErrorMsg(null);
    setSuccessMsg(null);

    try {
      const payload = {
        episodeID: episode.episodeID,
        movieID: episode.movieID,
        seasonNumber: Number(seasonNumber),
        episodeNumber: Number(episodeNumber),
        title: title,
        synopsis: synopsis,
        description: description,
        durationSeconds: Number(durationSeconds),
        releaseDate: new Date(releaseDate).toISOString()
      };

      // Gọi API
      const result = await updateEpisode(payload);
      
      // --- [FIX] LOG ĐỂ KIỂM TRA DỮ LIỆU ---
      console.log("Update Result:", result);

      // --- [FIX] XỬ LÝ LINH HOẠT AXIOS HOẶC FETCH ---
      // Nếu dùng Axios, dữ liệu nằm trong result.data. Nếu fetch, nằm ngay trong result.
      const data = result.data || result; 

      if (data.errorCode === 200) {
        setSuccessMsg("Cập nhật thành công!");
        setTimeout(() => {
            if (onSuccess) onSuccess(data); 
            onClose();
        }, 1500);
      } else {
        // Nếu API trả về lỗi logic (vd: trùng tập phim)
        setErrorMsg(data.errorMessage || "Có lỗi xảy ra khi cập nhật.");
      }

    } catch (err) {
      console.error(err);
      // Nếu API trả về lỗi mạng (404, 500, cors...)
      const message = err.response?.data?.errorMessage || err.message || "Lỗi kết nối đến server.";
      setErrorMsg(message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={!submitting ? onClose : undefined} maxWidth="md" fullWidth>
      <DialogTitle sx={{ backgroundColor: colors.blueAccent[700], color: colors.grey[100] }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Box display="flex" alignItems="center" gap={1}>
            <EditIcon /> Cập Nhật Tập Phim
          </Box>
          <IconButton onClick={onClose} disabled={submitting}>
            <CloseIcon sx={{ color: colors.grey[100] }} />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent sx={{ backgroundColor: colors.primary[400], pt: 3 }}>
        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          
          <Grid item xs={6}>
            <TextField
              fullWidth variant="filled" type="number" label="Mùa (Season)"
              value={seasonNumber} onChange={(e) => setSeasonNumber(e.target.value)}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth variant="filled" type="number" label="Tập (Episode)"
              value={episodeNumber} onChange={(e) => setEpisodeNumber(e.target.value)}
            />
          </Grid>

          <Grid item xs={12}>
            <TextField
              fullWidth variant="filled" label="Tên tập phim" required
              value={title} onChange={(e) => setTitle(e.target.value)}
            />
          </Grid>

          <Grid item xs={6}>
            <TextField
              fullWidth variant="filled" type="number" label="Thời lượng (giây)"
              value={durationSeconds} onChange={(e) => setDurationSeconds(e.target.value)}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth variant="filled" type="date" label="Ngày phát hành"
              InputLabelProps={{ shrink: true }}
              value={releaseDate} onChange={(e) => setReleaseDate(e.target.value)}
            />
          </Grid>

          <Grid item xs={12}>
            <TextField
              fullWidth variant="filled" label="Synopsis (Tóm tắt)"
              value={synopsis} onChange={(e) => setSynopsis(e.target.value)}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth variant="filled" label="Mô tả chi tiết" multiline rows={3}
              value={description} onChange={(e) => setDescription(e.target.value)}
            />
          </Grid>

          <Grid item xs={12}>
            {submitting && <LinearProgress sx={{ mb: 2 }} />}
            {errorMsg && <Alert severity="error">{errorMsg}</Alert>}
            {successMsg && <Alert severity="success">{successMsg}</Alert>}
          </Grid>

        </Grid>
      </DialogContent>

      <DialogActions sx={{ backgroundColor: colors.blueAccent[700], p: 2 }}>
        <Button onClick={onClose} variant="outlined" sx={{ color: colors.grey[100], borderColor: colors.grey[400] }} disabled={submitting}>
          Hủy
        </Button>
        <Button onClick={handleSubmit} variant="contained" sx={{ backgroundColor: colors.greenAccent[600], fontWeight: "bold" }} disabled={submitting}>
          {submitting ? "Đang lưu..." : "Lưu Thay Đổi"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default UpdateEpisodeModal;