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
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import EditIcon from "@mui/icons-material/Edit";
import { tokens } from "../theme";
import { updateMovieSubtitle, updateEpisodeSubtitle } from "../services/api";

const UpdateSubtitleModal = ({ open, onClose, subtitle, scope = "movie", onSuccess }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  // State
  const [subTitleName, setSubTitleName] = useState("");
  const [language, setLanguage] = useState("");
  const [linkSubTitle, setLinkSubTitle] = useState("");
  const [isActive, setIsActive] = useState(true);

  const [submitting, setSubmitting] = useState(false);
  const [errorMsg, setErrorMsg] = useState(null);
  const [successMsg, setSuccessMsg] = useState(null);

  useEffect(() => {
    if (open && subtitle) {
      setSubTitleName(subtitle.subTitleName || "");
      setLanguage(subtitle.language || "");
      setLinkSubTitle(subtitle.linkSubTitle || "");
      setIsActive(subtitle.isActive !== undefined ? subtitle.isActive : true);
      setErrorMsg(null);
      setSuccessMsg(null);
    }
  }, [open, subtitle]);

  const handleSubmit = async () => {
    setSubmitting(true);
    setErrorMsg(null);
    setSuccessMsg(null);

    try {
      let response;
      if (scope === "movie") {
        const payload = {
          movieSubTitleID: subtitle.movieSubTitleID,
          movieSourceID: subtitle.movieSourceID,
          subTitleName,
          language,
          linkSubTitle,
          isActive,
        };
        response = await updateMovieSubtitle(payload);
      } else {
        const payload = {
          episodeSubTitleID: subtitle.episodeSubTitleID || subtitle.movieSubTitleID, // API trả về có thể khác tên, check kỹ data
          episodeSourceID: subtitle.episodeSourceID || subtitle.movieSourceID,
          subTitleName,
          language,
          linkSubTitle,
          isActive,
        };
        response = await updateEpisodeSubtitle(payload);
      }

      // Kiểm tra response (có thể bọc trong data hoặc trả trực tiếp)
      const data = response.data || response;

      if (response.errorCode === 200) {
        setSuccessMsg("Cập nhật subtitle thành công!");
        setTimeout(() => {
          if (onSuccess) onSuccess();
          onClose();
        }, 1500);
      } else {
        setErrorMsg(data?.errorMessage || "Cập nhật thất bại.");
      }
    } catch (err) {
      console.error(err);
      setErrorMsg("Lỗi kết nối server.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={!submitting ? onClose : undefined} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ backgroundColor: colors.blueAccent[700], color: colors.grey[100] }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Box display="flex" alignItems="center" gap={1}>
            <EditIcon /> Cập Nhật Subtitle
          </Box>
          <IconButton onClick={onClose} disabled={submitting}>
            <CloseIcon sx={{ color: colors.grey[100] }} />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent sx={{ backgroundColor: colors.primary[400], pt: 3 }}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextField fullWidth variant="filled" label="Tên Subtitle" value={subTitleName} onChange={(e) => setSubTitleName(e.target.value)} />
          </Grid>
          <Grid item xs={6}>
            <TextField fullWidth variant="filled" label="Ngôn ngữ" value={language} onChange={(e) => setLanguage(e.target.value)} />
          </Grid>
          <Grid item xs={6}>
             <Box mt={1}>
                <FormControlLabel control={<Checkbox checked={isActive} onChange={(e) => setIsActive(e.target.checked)} sx={{color: colors.greenAccent[500], '&.Mui-checked': {color: colors.greenAccent[500]}}} />} label="Active" />
             </Box>
          </Grid>
          <Grid item xs={12}>
            <TextField fullWidth variant="filled" label="Link Subtitle" value={linkSubTitle} onChange={(e) => setLinkSubTitle(e.target.value)} />
          </Grid>

          <Grid item xs={12}>
            {submitting && <LinearProgress sx={{ mb: 2 }} />}
            {errorMsg && <Alert severity="error">{errorMsg}</Alert>}
            {successMsg && <Alert severity="success">{successMsg}</Alert>}
          </Grid>
        </Grid>
      </DialogContent>

      <DialogActions sx={{ backgroundColor: colors.blueAccent[700], p: 2 }}>
        <Button onClick={onClose} variant="outlined" sx={{ color: colors.grey[100] }} disabled={submitting}>Hủy</Button>
        <Button onClick={handleSubmit} variant="contained" sx={{ backgroundColor: colors.greenAccent[600] }} disabled={submitting}>Lưu thay đổi</Button>
      </DialogActions>
    </Dialog>
  );
};

export default UpdateSubtitleModal;