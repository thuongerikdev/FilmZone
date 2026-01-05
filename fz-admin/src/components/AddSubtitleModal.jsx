import React, { useState, useRef } from "react";
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Typography,
  Grid,
  Alert,
  IconButton,
  useTheme,
  LinearProgress,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import { tokens } from "../theme";
import { uploadMovieSubtitle } from "../services/api"; // ✅ Import từ api.js

const AddSubtitleModal = ({ open, onClose, sourceId, sourceName, onSuccess }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  const [file, setFile] = useState(null);
  const [externalApiUrl, setExternalApiUrl] = useState("https://e63c1dc514e4.ngrok-free.app");
  const [apiToken, setApiToken] = useState("");
  
  const [submitting, setSubmitting] = useState(false);
  const [errorMsg, setErrorMsg] = useState(null);
  const [successMsg, setSuccessMsg] = useState(null);
  const fileInputRef = useRef(null);

  const handleSubmit = async () => {
    setSubmitting(true);
    setErrorMsg(null);
    setSuccessMsg(null);

    if (!file) {
      setErrorMsg("Vui lòng chọn file video để tạo sub.");
      setSubmitting(false);
      return;
    }
    if (!externalApiUrl) {
      setErrorMsg("Vui lòng nhập External API URL.");
      setSubmitting(false);
      return;
    }
    if (!apiToken) {
      setErrorMsg("Vui lòng nhập API Token.");
      setSubmitting(false);
      return;
    }

    try {
      const fd = new FormData();
      fd.append("sourceID", sourceId);
      fd.append("videoFile", file);
      fd.append("externalApiUrl", externalApiUrl);
      fd.append("apiToken", apiToken);
      fd.append("type", "movie");

      // ✅ Sử dụng function từ api.js
      const response = await uploadMovieSubtitle(fd);
      
      // uploadMovieSubtitle trả về Promise từ fetch, cần .json()
      const data = await response.json();

      if (data.errorCode === 200) {
        setSuccessMsg(`Yêu cầu tạo sub thành công! Job ID: ${data.data}`);
        setTimeout(() => {
          onClose();
          if(onSuccess) onSuccess();
        }, 2000);
      } else {
        setErrorMsg(data.errorMessage || "Có lỗi xảy ra khi gửi yêu cầu.");
      }

    } catch (err) {
      console.error(err);
      setErrorMsg("Lỗi kết nối đến server.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={!submitting ? onClose : undefined} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ backgroundColor: colors.blueAccent[700], color: colors.grey[100] }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h5" fontWeight="bold">
            Tạo Subtitle cho Source #{sourceId}
          </Typography>
          <IconButton onClick={onClose} disabled={submitting}>
            <CloseIcon sx={{ color: colors.grey[100] }} />
          </IconButton>
        </Box>
        <Typography variant="caption" color={colors.grey[300]}>
           Source: {sourceName}
        </Typography>
      </DialogTitle>

      <DialogContent sx={{ backgroundColor: colors.primary[400], pt: 3 }}>
        <Grid container spacing={3} sx={{ mt: 0.5 }}>
          
          <Grid item xs={12}>
            <Box border={`1px dashed ${colors.grey[500]}`} p={2} textAlign="center" borderRadius="4px" sx={{ backgroundColor: colors.primary[500] }}>
              <input
                ref={fileInputRef}
                type="file"
                accept="video/*"
                style={{ display: "none" }}
                id="sub-video-file"
                onChange={(e) => setFile(e.target.files?.[0] || null)}
              />
              <label htmlFor="sub-video-file">
                <Button variant="contained" component="span" startIcon={<CloudUploadIcon />} sx={{ backgroundColor: colors.blueAccent[600] }}>
                  Chọn Video để Scan Sub
                </Button>
              </label>
              {file && (
                <Typography variant="body2" mt={2} color={colors.greenAccent[400]}>
                  {file.name} ({(file.size / 1024 / 1024).toFixed(2)} MB)
                </Typography>
              )}
            </Box>
          </Grid>

          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              variant="filled"
              label="External API URL"
              value={externalApiUrl}
              onChange={(e) => setExternalApiUrl(e.target.value)}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              variant="filled"
              label="API Token"
              type="password"
              value={apiToken}
              onChange={(e) => setApiToken(e.target.value)}
            />
          </Grid>

          <Grid item xs={12}>
            {submitting && <LinearProgress sx={{ mb: 2, backgroundColor: colors.blueAccent[800], "& .MuiLinearProgress-bar": { backgroundColor: colors.blueAccent[400] } }} />}
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
          {submitting ? "Đang gửi..." : "Gửi yêu cầu"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default AddSubtitleModal;