import React, { useState } from "react";
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
import AddIcon from "@mui/icons-material/Add";
import { tokens } from "../theme";

const CreateEpisodeModal = ({ open, onClose, movieId, onSuccess }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  // State form
  const [seasonNumber, setSeasonNumber] = useState(1);
  const [episodeNumber, setEpisodeNumber] = useState(1);
  const [title, setTitle] = useState("");
  const [synopsis, setSynopsis] = useState("");
  const [description, setDescription] = useState("");
  const [durationSeconds, setDurationSeconds] = useState(0);
  const [releaseDate, setReleaseDate] = useState(new Date().toISOString().split("T")[0]);

  const [submitting, setSubmitting] = useState(false);
  const [errorMsg, setErrorMsg] = useState(null);
  const [successMsg, setSuccessMsg] = useState(null);

  const handleSubmit = async () => {
    setSubmitting(true);
    setErrorMsg(null);
    setSuccessMsg(null);

    // Validate đơn giản
    if (!title) {
      setErrorMsg("Vui lòng nhập tên tập phim.");
      setSubmitting(false);
      return;
    }

    try {
      const payload = {
        movieID: Number(movieId),
        seasonNumber: Number(seasonNumber),
        episodeNumber: Number(episodeNumber),
        title: title,
        synopsis: synopsis || "",
        description: description || "",
        durationSeconds: Number(durationSeconds),
        releaseDate: new Date(releaseDate).toISOString() // Convert sang chuẩn ISO
      };

      const response = await fetch("https://filmzone-api.koyeb.app/api/Episode/CreateEpisode", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "accept": "*/*"
        },
        body: JSON.stringify(payload),
      });

      const data = await response.json();

      if (data.errorCode === 200) {
        setSuccessMsg("Tạo tập phim thành công!");
        // Reset form sau khi tạo thành công để có thể tạo tiếp tập sau
        setEpisodeNumber(prev => prev + 1); 
        setTitle("");
        setSynopsis("");
        setDescription("");
        
        setTimeout(() => {
            if (onSuccess) onSuccess(); // Refresh list bên ngoài
            setSuccessMsg(null);
            // onClose(); // Tùy chọn: Đóng modal hoặc giữ lại để nhập tiếp
        }, 1500);
      } else {
        setErrorMsg(data.errorMessage || "Có lỗi xảy ra khi tạo tập phim.");
      }

    } catch (err) {
      console.error(err);
      setErrorMsg("Lỗi kết nối đến server.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={!submitting ? onClose : undefined} maxWidth="md" fullWidth>
      <DialogTitle sx={{ backgroundColor: colors.blueAccent[700], color: colors.grey[100] }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Box display="flex" alignItems="center" gap={1}>
            <AddIcon /> Tạo Tập Phim Mới
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
          Đóng
        </Button>
        <Button onClick={handleSubmit} variant="contained" sx={{ backgroundColor: colors.greenAccent[600], fontWeight: "bold" }} disabled={submitting}>
          {submitting ? "Đang tạo..." : "Tạo Tập Phim"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default CreateEpisodeModal;