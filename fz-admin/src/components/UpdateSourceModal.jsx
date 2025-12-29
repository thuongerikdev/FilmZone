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
  FormControlLabel,
  Checkbox,
  MenuItem
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import EditIcon from "@mui/icons-material/Edit";
import { tokens } from "../theme";
import { updateMovieSource, updateEpisodeSource } from "../services/api";

// scope: "movie" | "episode"
const UpdateSourceModal = ({ open, onClose, source, scope = "movie", onSuccess }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  // State form
  const [sourceName, setSourceName] = useState("");
  const [sourceType, setSourceType] = useState("mp4");
  const [sourceUrl, setSourceUrl] = useState("");
  const [sourceIdStr, setSourceIdStr] = useState(""); // field 'sourceId' (string) trong DB
  const [quality, setQuality] = useState("");
  const [language, setLanguage] = useState("");
  const [isVipOnly, setIsVipOnly] = useState(false);
  const [isActive, setIsActive] = useState(true);
  
  // Riêng cho Movie
  const [rawSubTitle, setRawSubTitle] = useState("");

  const [submitting, setSubmitting] = useState(false);
  const [errorMsg, setErrorMsg] = useState(null);
  const [successMsg, setSuccessMsg] = useState(null);

  // Load data khi mở modal
  useEffect(() => {
    if (open && source) {
      setSourceName(source.sourceName || "");
      setSourceType(source.sourceType || "mp4");
      setSourceUrl(source.sourceUrl || "");
      setSourceIdStr(source.sourceID || ""); // Lưu ý: API trả về field tên là sourceID (string)
      setQuality(source.quality || "");
      setLanguage(source.language || "");
      setIsVipOnly(source.isVipOnly || false);
      setIsActive(source.isActive !== undefined ? source.isActive : true);
      
      if (scope === "movie") {
        setRawSubTitle(source.rawSubTitle || "");
      }
      
      setErrorMsg(null);
      setSuccessMsg(null);
    }
  }, [open, source, scope]);

  const handleSubmit = async () => {
    setSubmitting(true);
    setErrorMsg(null);
    setSuccessMsg(null);

    try {
      let res;
      
      if (scope === "movie") {
        // Payload cho Movie Source
        const payload = {
          movieID: source.movieID,
          sourceName: sourceName,
          sourceType: sourceType,
          sourceUrl: sourceUrl,
          sourceId: sourceIdStr, // string ID (vd: avengers-1)
          quality: quality,
          language: language,
          rawSubTitle: rawSubTitle,
          isVipOnly: isVipOnly,
          isActive: isActive,
          sourceID: source.movieSourceID // int ID (PK)
        };
        res = await updateMovieSource(payload);
      } else {
        // Payload cho Episode Source
        const payload = {
          episodeID: source.episodeID,
          sourceName: sourceName,
          sourceType: sourceType,
          sourceUrl: sourceUrl,
          sourceId: sourceIdStr,
          quality: quality,
          language: language,
          isVipOnly: isVipOnly,
          isActive: isActive,
          episodeSourceID: source.episodeSourceID // PK
        };
        res = await updateEpisodeSource(payload);
      }

      const data = res.data || res; 

      console.log("Update Source Response:", data); // Bật F12 xem log để chắc chắn

      if (data && data.errorCode === 200) {
        setSuccessMsg("Cập nhật source thành công!");
        setTimeout(() => {
            if (onSuccess) onSuccess(); 
            onClose();
        }, 1500);
      } else {
        // Lấy errorMessage từ data, nếu không có mới dùng chuỗi mặc định
        setErrorMsg(data?.errorMessage || "Có lỗi xảy ra khi cập nhật.");
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
            <EditIcon /> Cập Nhật Source ({scope === "movie" ? "Phim Lẻ" : "Tập Phim"})
          </Box>
          <IconButton onClick={onClose} disabled={submitting}>
            <CloseIcon sx={{ color: colors.grey[100] }} />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent sx={{ backgroundColor: colors.primary[400], pt: 3 }}>
        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          
          <Grid item xs={6}>
            <TextField fullWidth variant="filled" label="Tên Source" value={sourceName} onChange={(e) => setSourceName(e.target.value)} />
          </Grid>
          <Grid item xs={6}>
            <TextField fullWidth select variant="filled" label="Loại (Type)" value={sourceType} onChange={(e) => setSourceType(e.target.value)}>
                <MenuItem value="mp4">MP4</MenuItem>
                <MenuItem value="m3u8">M3U8</MenuItem>
                <MenuItem value="iframe">Iframe</MenuItem>
            </TextField>
          </Grid>

          <Grid item xs={12}>
            <TextField fullWidth variant="filled" label="Source URL (Link)" value={sourceUrl} onChange={(e) => setSourceUrl(e.target.value)} />
          </Grid>

          <Grid item xs={4}>
            <TextField fullWidth variant="filled" label="Chất lượng (Quality)" value={quality} onChange={(e) => setQuality(e.target.value)} />
          </Grid>
          <Grid item xs={4}>
            <TextField fullWidth variant="filled" label="Ngôn ngữ (Language)" value={language} onChange={(e) => setLanguage(e.target.value)} />
          </Grid>
          <Grid item xs={4}>
            <TextField fullWidth variant="filled" label="Source Identifier (String ID)" value={sourceIdStr} onChange={(e) => setSourceIdStr(e.target.value)} helperText="Mã định danh riêng (nếu có)" />
          </Grid>

          {scope === "movie" && (
             <Grid item xs={12}>
                <TextField fullWidth variant="filled" label="Raw Subtitle (JSON)" multiline rows={2} value={rawSubTitle} onChange={(e) => setRawSubTitle(e.target.value)} />
             </Grid>
          )}

          <Grid item xs={6}>
             <FormControlLabel control={<Checkbox checked={isVipOnly} onChange={(e) => setIsVipOnly(e.target.checked)} sx={{color: colors.greenAccent[500], '&.Mui-checked': {color: colors.greenAccent[500]}}} />} label="VIP Only" />
          </Grid>
          <Grid item xs={6}>
             <FormControlLabel control={<Checkbox checked={isActive} onChange={(e) => setIsActive(e.target.checked)} sx={{color: colors.greenAccent[500], '&.Mui-checked': {color: colors.greenAccent[500]}}} />} label="Active (Kích hoạt)" />
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

export default UpdateSourceModal;