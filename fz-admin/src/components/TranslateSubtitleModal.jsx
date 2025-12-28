import React, { useState } from "react";
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
  MenuItem,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import GTranslateIcon from "@mui/icons-material/GTranslate";
import { tokens } from "../theme";

const TranslateSubtitleModal = ({ open, onClose, sourceId, sourceName, onSuccess }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  // State
  const [externalApiUrl, setExternalApiUrl] = useState("https://e63c1dc514e4.ngrok-free.app");
  const [apiToken, setApiToken] = useState("");
  const [targetLanguage, setTargetLanguage] = useState("vi");
  
  const [submitting, setSubmitting] = useState(false);
  const [errorMsg, setErrorMsg] = useState(null);
  const [successMsg, setSuccessMsg] = useState(null);

  const handleSubmit = async () => {
    setSubmitting(true);
    setErrorMsg(null);
    setSuccessMsg(null);

    // Validate
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
      // Body chuẩn JSON theo curl
      const payload = {
        sourceID: sourceId,
        type: "movie",
        externalApiUrl: externalApiUrl,
        apiToken: apiToken,
        targetLanguage: targetLanguage
      };

      const response = await fetch("https://filmzone-api.koyeb.app/api/MovieSubTitle/TranslateFromSource/Translate/AutoFromSource", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "accept": "*/*"
        },
        body: JSON.stringify(payload),
      });

      const data = await response.json();

      if (data.errorCode === 200) {
        setSuccessMsg(`Yêu cầu dịch thành công! Job ID: ${data.data}`);
        setTimeout(() => {
            onClose(); 
            if(onSuccess) onSuccess();
        }, 2000);
      } else {
        setErrorMsg(data.errorMessage || "Có lỗi xảy ra khi gửi yêu cầu dịch.");
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
          <Typography variant="h5" fontWeight="bold" display="flex" alignItems="center" gap={1}>
            <GTranslateIcon /> Dịch Subtitle (AI)
          </Typography>
          <IconButton onClick={onClose} disabled={submitting}>
            <CloseIcon sx={{ color: colors.grey[100] }} />
          </IconButton>
        </Box>
        <Typography variant="caption" color={colors.grey[300]}>
           Source: {sourceName} (ID: {sourceId})
        </Typography>
      </DialogTitle>

      <DialogContent sx={{ backgroundColor: colors.primary[400], pt: 3 }}>
        <Grid container spacing={3} sx={{ mt: 0.5 }}>
          
          <Grid item xs={12}>
            <TextField
              fullWidth
              select
              variant="filled"
              label="Ngôn ngữ đích (Target Language)"
              value={targetLanguage}
              onChange={(e) => setTargetLanguage(e.target.value)}
            >
                <MenuItem value="vi">Tiếng Việt (vi)</MenuItem>
                <MenuItem value="en">Tiếng Anh (en)</MenuItem>
                <MenuItem value="ja">Tiếng Nhật (ja)</MenuItem>
                <MenuItem value="ko">Tiếng Hàn (ko)</MenuItem>
                <MenuItem value="zh">Tiếng Trung (zh)</MenuItem>
            </TextField>
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

          {/* Feedback */}
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
          {submitting ? "Đang gửi..." : "Dịch ngay"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default TranslateSubtitleModal;