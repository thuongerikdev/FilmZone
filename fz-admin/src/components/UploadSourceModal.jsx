import React, { useEffect, useRef, useState } from "react";
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControlLabel,
  Checkbox,
  Radio,
  RadioGroup,
  FormControl,
  FormLabel,
  LinearProgress,
  Typography,
  Grid,
  Alert,
  IconButton,
  useTheme,
  Divider,
  Collapse
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import { tokens } from "../theme";
import * as signalR from "@microsoft/signalr";
import { getEpisodeSourcesByEpisodeId, getMovieSourcesByMovieId, uploadArchiveFile, uploadArchiveLink, uploadMovieSubtitle, uploadYoutubeFile } from "../services/api";

// Helper function
function clampPct(n) {
  if (typeof n !== "number" || Number.isNaN(n)) return 0;
  if (n < 0) return 0;
  if (n > 100) return 100;
  return Math.floor(n);
}

const ProgressBar = ({ label, percent, hint, color }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const p = clampPct(percent);

  return (
    <Box width="100%" mb={2}>
      <Box display="flex" justifyContent="space-between" mb={0.5}>
        <Typography variant="caption" color={colors.grey[300]}>{label}</Typography>
        <Typography variant="caption" color={colors.grey[300]}>{p}%</Typography>
      </Box>
      <LinearProgress 
        variant="determinate" 
        value={p} 
        sx={{ 
          height: 10, 
          borderRadius: 5,
          backgroundColor: colors.primary[400],
          "& .MuiLinearProgress-bar": {
            backgroundColor: color || colors.blueAccent[500]
          }
        }} 
      />
      {hint && <Typography variant="caption" color={colors.grey[400]} mt={0.5} display="block">{hint}</Typography>}
    </Box>
  );
};

const UploadSourceModal = ({ open, onClose, movieId, movieTitle, onUploadSuccess, initialScope = "movie" }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  // --- STATE UPLOAD VIDEO ---
  const [provider, setProvider] = useState("archive-file");
  // movieId ở đây đóng vai trò là TargetId (MovieID hoặc EpisodeID)
  const [scope, setScope] = useState(initialScope); 
  const [quality, setQuality] = useState("1080p");
  const [language, setLanguage] = useState("vi");
  const [isVipOnly, setIsVipOnly] = useState(false);
  const [isActive, setIsActive] = useState(true);

  const [file, setFile] = useState(null);
  const [linkUrl, setLinkUrl] = useState("");
  const fileInputRef = useRef(null);

  // --- STATE SUBTITLE ---
  const [isGenerateSub, setIsGenerateSub] = useState(false);
  const [externalApiUrl, setExternalApiUrl] = useState("https://e63c1dc514e4.ngrok-free.app"); 
  const [apiToken, setApiToken] = useState("");

  // --- STATE SYSTEM ---
  const [submitting, setSubmitting] = useState(false); 
  const [stepStatus, setStepStatus] = useState(""); 
  const [errorMsg, setErrorMsg] = useState(null);
  const [successMsg, setSuccessMsg] = useState(null);

  const [clientPct, setClientPct] = useState(0);
  const [serverPct, setServerPct] = useState(0);
  const [serverProgress, setServerProgress] = useState([]);

  const hubRef = useRef(null);
  const BASE_URL = "https://filmzone-api.koyeb.app"; 

  // --- EFFECT ---
  useEffect(() => {
    if (open) {
        setScope(initialScope); // Reset scope khi mở modal
        resetState();
    }
  }, [open, initialScope]);

  useEffect(() => {
    if (provider === "archive-link") {
      setFile(null);
      if (fileInputRef.current) fileInputRef.current.value = "";
    } else {
      setLinkUrl("");
    }
  }, [provider]);

  const resetState = () => {
    setClientPct(0);
    setServerPct(0);
    setServerProgress([]);
    setErrorMsg(null);
    setSuccessMsg(null);
    setStepStatus("");
    // Giữ lại file/setting để user không phải nhập lại nếu lỗi
  };

  useEffect(() => {
    return () => {
      if (hubRef.current) hubRef.current.stop().catch(() => {});
    };
  }, [open]);

  // --- SIGNALR LOGIC ---
  function pushLog(entry) {
    setServerProgress((prev) => [...prev, { ts: Date.now(), ...entry }]);
  }

  async function connectHubAndJoin(jobId) {
    if (hubRef.current) {
      try { await hubRef.current.stop(); } catch {}
      hubRef.current = null;
    }

    const hubUrl = `${BASE_URL}/hubs/upload`;
    const conn = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build();

    conn.on("upload.progress", (payload) => {
      const pct = clampPct(payload?.percent);
      const text = payload?.text ?? "Uploading...";
      setServerPct(pct);
      pushLog({ type: "progress", text, percent: pct });
    });

    // === XỬ LÝ KHI UPLOAD VIDEO HOÀN TẤT ===
    conn.on("upload.done", async (payload) => {
      setServerPct(100);
      const text = payload?.playerUrl ? `Video Done. Player: ${payload.playerUrl}` : "Video Done.";
      pushLog({ type: "completed", text, percent: 100 });
      
      // Nếu có chọn tạo Subtitle -> Tiếp tục xử lý
      if (isGenerateSub && file) {
        await handleSubtitleGenerationFlow();
      } else {
        setSuccessMsg(text);
        setSubmitting(false); // Enable nút đóng
        if (onUploadSuccess) onUploadSuccess();
      }
    });

    conn.on("upload.error", (payload) => {
      const text = payload?.error || "Server error";
      pushLog({ type: "error", text });
      setErrorMsg(text);
      setSubmitting(false); // Enable nút đóng
    });

    await conn.start();
    await conn.invoke("JoinJob", jobId);
    hubRef.current = conn;
  }

  // --- LOGIC TẠO SUBTITLE ---
  const handleSubtitleGenerationFlow = async () => {
    setStepStatus("Đang xử lý Subtitle...");
    pushLog({ type: "info", text: "Đang tìm Source ID vừa tạo..." });

    try {
        // 1. Xác định API endpoint dựa trên Scope
        const isMovie = scope === 'movie';

        const sourcesRes = isMovie
            ? await getMovieSourcesByMovieId(movieId)
            : await getEpisodeSourcesByEpisodeId(movieId);
        
        // --- SỬA Ở ĐÂY: Bỏ .json(), dùng .data ---
        // SAI: const sourcesData = await sourcesRes.json();
        const sourcesData = sourcesRes.data; 
        
        if (sourcesData.errorCode !== 200 || !sourcesData.data || sourcesData.data.length === 0) {
            throw new Error("Không tìm thấy Source ID để tạo Subtitle.");
        }

        // 2. Tìm Source mới nhất (giữ nguyên)
        const idKey = isMovie ? 'movieSourceID' : 'episodeSourceID';
        const latestSource = sourcesData.data.sort((a, b) => b[idKey] - a[idKey])[0];
        const sourceId = latestSource[idKey];

        pushLog({ type: "info", text: `Đã tìm thấy ${isMovie ? 'Movie' : 'Episode'} Source ID: ${sourceId}. Đang gửi video để tạo sub...` });

        // 3. Gọi API Upload Subtitle (giữ nguyên)
        const subFd = new FormData();
        subFd.append("sourceID", sourceId);
        subFd.append("videoFile", file);
        subFd.append("externalApiUrl", externalApiUrl);
        subFd.append("apiToken", apiToken);
        subFd.append("type", scope);

        console.log("Đang gọi API Subtitle...");
        const subResponse = await uploadMovieSubtitle(subFd);
        
        // Log để kiểm tra (như bạn vừa làm)
        console.log("Sub Response Raw:", subResponse);

        // --- SỬA LẠI ĐOẠN NÀY ---
        // Vì log cho thấy đây là 'Response' object của Fetch API
        // Chúng ta MẮT BUỘC phải dùng .json() để lấy dữ liệu
        const subResult = await subResponse.json(); 

        console.log("Sub Result Parsed:", subResult);

        // Kiểm tra kết quả sau khi parse
        if (subResult && subResult.errorCode === 200) {
            pushLog({ type: "completed", text: `Subtitle Job Created! ID: ${subResult.data}` });
            setSuccessMsg("Hoàn tất! Video đã lên và yêu cầu tạo Subtitle đã được gửi.");
        } else {
            throw new Error(subResult?.errorMessage || "Lỗi khi gọi API Subtitle");
        }

    } catch (err) {
        // ... giữ nguyên phần catch lỗi
        const msg = err.message || "Lỗi quy trình tạo Subtitle";
        pushLog({ type: "error", text: msg });
        setErrorMsg(`Video thành công nhưng Subtitle lỗi: ${msg}`);
    } finally {
        setSubmitting(false);
        if (onUploadSuccess) onUploadSuccess();
    }
  };

  // --- SUBMIT MAIN FORM ---
  const onSubmit = async () => {
    setSubmitting(true);
    setStepStatus("Đang Upload Video...");
    setErrorMsg(null);
    setSuccessMsg(null);
    setServerProgress([]);
    setClientPct(0);
    setServerPct(0);

    // --- Validation (giữ nguyên) ---
    if (provider !== "archive-link" && !file) {
      setErrorMsg("Vui lòng chọn file video.");
      setSubmitting(false);
      return;
    }
    // ... các validation khác ...

    try {
      let jobId;
      
      // Cấu hình Axios để theo dõi tiến độ upload
      const axiosConfig = {
        onUploadProgress: (progressEvent) => {
          if (progressEvent.total) {
            const pct = Math.floor((progressEvent.loaded / progressEvent.total) * 100);
            setClientPct(pct);
          }
        }
      };

      // 1. Xử lý Gửi request dựa trên Provider
      if (provider === "archive-link") {
        const body = {
          Scope: scope,
          TargetId: Number(movieId),
          Quality: quality,
          Language: language,
          IsVipOnly: isVipOnly,
          IsActive: isActive,
          LinkUrl: linkUrl.trim(),
        };

        // Sử dụng hàm uploadArchiveLink từ api.js
        const res = await uploadArchiveLink(body);
        jobId = res.data.jobId; // Axios trả về data trực tiếp
        
      } else {
        // Upload File (Dùng cho cả Archive File và Youtube File)
        const fd = new FormData();
        fd.append("Scope", scope);
        fd.append("TargetId", String(movieId));
        fd.append("Quality", quality);
        fd.append("Language", language);
        fd.append("IsVipOnly", String(isVipOnly));
        fd.append("IsActive", String(isActive));
        if (file) fd.append("File", file);

        let response;
        if (provider === "archive-file") {
          // Gọi hàm từ api.js kèm config progress
          response = await uploadArchiveFile(fd, axiosConfig);
        } else {
          // Gọi hàm từ api.js kèm config progress
          response = await uploadYoutubeFile(fd, axiosConfig);
        }
        
        jobId = response.data.jobId;
      }

      pushLog({ type: "info", text: `Video Job created: ${jobId}` });
      
      // 2. Kết nối SignalR để theo dõi tiến độ xử lý trên server (giữ nguyên)
      await connectHubAndJoin(jobId);

    } catch (err) {
      console.error(err);
      // Axios error handling
      const msg = err.response?.data?.errorMessage || err.message || "Request failed";
      setErrorMsg(msg);
      pushLog({ type: "error", text: msg });
      setSubmitting(false); 
    }
  };

  return (
    <Dialog open={open} onClose={!submitting ? onClose : undefined} maxWidth="md" fullWidth>
      <DialogTitle sx={{ backgroundColor: colors.blueAccent[700], color: colors.grey[100] }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight="bold">Upload Source: {movieTitle}</Typography>
          <IconButton onClick={onClose} disabled={submitting}>
            <CloseIcon sx={{ color: colors.grey[100] }} />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent sx={{ backgroundColor: colors.primary[400], pt: 3 }}>
        <Grid container spacing={3} sx={{ mt: 1 }}>
          
          {/* --- PROVIDER --- */}
          <Grid item xs={12}>
            <FormControl>
              <FormLabel id="provider-label" sx={{ color: colors.grey[100], mb: 1 }}>Nhà cung cấp Video</FormLabel>
              <RadioGroup row aria-labelledby="provider-label" value={provider} onChange={(e) => setProvider(e.target.value)}>
                <FormControlLabel value="archive-file" control={<Radio sx={{color: colors.greenAccent[500], '&.Mui-checked': {color: colors.greenAccent[500]}}} />} label="Archive (File)" />
                <FormControlLabel value="archive-link" control={<Radio sx={{color: colors.greenAccent[500], '&.Mui-checked': {color: colors.greenAccent[500]}}} />} label="Archive (Link)" />
                <FormControlLabel value="youtube-file" control={<Radio sx={{color: colors.greenAccent[500], '&.Mui-checked': {color: colors.greenAccent[500]}}} />} label="YouTube (File)" />
              </RadioGroup>
            </FormControl>
          </Grid>

          {/* --- METADATA --- */}
          <Grid item xs={6}><TextField fullWidth variant="filled" label="Scope" value={scope} disabled /></Grid>
          <Grid item xs={6}><TextField fullWidth variant="filled" label="Target ID" value={movieId} disabled /></Grid>
          <Grid item xs={6}><TextField fullWidth variant="filled" label="Quality" value={quality} onChange={(e) => setQuality(e.target.value)} /></Grid>
          <Grid item xs={6}><TextField fullWidth variant="filled" label="Language" value={language} onChange={(e) => setLanguage(e.target.value)} /></Grid>
          <Grid item xs={6}><FormControlLabel control={<Checkbox checked={isVipOnly} onChange={(e) => setIsVipOnly(e.target.checked)} sx={{color: colors.greenAccent[500], '&.Mui-checked': {color: colors.greenAccent[500]}}} />} label="VIP Only" /></Grid>
          <Grid item xs={6}><FormControlLabel control={<Checkbox checked={isActive} onChange={(e) => setIsActive(e.target.checked)} sx={{color: colors.greenAccent[500], '&.Mui-checked': {color: colors.greenAccent[500]}}} />} label="Active" /></Grid>

          {/* --- VIDEO INPUT --- */}
          <Grid item xs={12}>
            {provider === "archive-link" ? (
              <TextField fullWidth variant="filled" label="Link URL" placeholder="https://example.com/video.mp4" value={linkUrl} onChange={(e) => setLinkUrl(e.target.value)} />
            ) : (
              <Box border={`1px dashed ${colors.grey[500]}`} p={3} textAlign="center" borderRadius="4px" sx={{ backgroundColor: colors.primary[500] }}>
                <input ref={fileInputRef} type="file" accept="video/*" style={{ display: "none" }} id="upload-file-input" onChange={(e) => setFile(e.target.files?.[0] || null)} />
                <label htmlFor="upload-file-input">
                  <Button variant="contained" component="span" startIcon={<CloudUploadIcon />} sx={{ backgroundColor: colors.blueAccent[600] }}>Chọn Video</Button>
                </label>
                {file && <Typography variant="body2" mt={2} color={colors.greenAccent[400]}>{file.name} ({(file.size / 1024 / 1024).toFixed(2)} MB)</Typography>}
              </Box>
            )}
          </Grid>

          {/* --- SUBTITLE OPTION --- */}
          <Grid item xs={12}>
             <Divider sx={{ my: 1, backgroundColor: colors.grey[700] }} />
             <FormControlLabel 
                control={
                    <Checkbox 
                        checked={isGenerateSub} 
                        onChange={(e) => setIsGenerateSub(e.target.checked)} 
                        disabled={provider === "archive-link"} 
                        sx={{color: colors.greenAccent[500], '&.Mui-checked': {color: colors.greenAccent[500]}}} 
                    />
                } 
                label={
                    <Typography color={provider === "archive-link" ? "text.disabled" : colors.grey[100]} fontWeight="bold">
                        Tạo Subtitle tự động (AI)
                    </Typography>
                } 
             />
             
             <Collapse in={isGenerateSub}>
                <Grid container spacing={2} sx={{ mt: 1, p: 2, backgroundColor: colors.primary[500], borderRadius: 1, border: `1px solid ${colors.greenAccent[600]}` }}>
                    <Grid item xs={12}>
                        <Typography variant="caption" color={colors.greenAccent[400]}>
                            * Video sẽ được gửi đến AI Server để xử lý sau khi upload.
                        </Typography>
                    </Grid>
                    <Grid item xs={12}>
                        <TextField fullWidth size="small" variant="outlined" label="External API URL" value={externalApiUrl} onChange={(e) => setExternalApiUrl(e.target.value)} />
                    </Grid>
                    <Grid item xs={12}>
                        <TextField fullWidth size="small" variant="outlined" label="API Token" value={apiToken} onChange={(e) => setApiToken(e.target.value)} type="password" />
                    </Grid>
                </Grid>
             </Collapse>
          </Grid>

          {/* --- PROGRESS & LOGS --- */}
          <Grid item xs={12}>
             <ProgressBar label="Video Upload" percent={clientPct} color={colors.blueAccent[500]} />
             <ProgressBar label="Processing" percent={serverPct} color={colors.greenAccent[500]} />
          </Grid>

          <Grid item xs={12}>
            {errorMsg && <Alert severity="error" sx={{ mb: 2 }}>{errorMsg}</Alert>}
            {successMsg && <Alert severity="success" icon={<CheckCircleIcon fontSize="inherit" />} sx={{ mb: 2 }}>{successMsg}</Alert>}
            
            <Box sx={{ maxHeight: 150, overflowY: "auto", backgroundColor: "#000", p: 1, borderRadius: 1, border: `1px solid ${colors.grey[700]}` }}>
                {serverProgress.map((l, i) => (
                    <Typography key={i} variant="caption" display="block" color={l.type === 'error' ? 'error' : l.type === 'completed' ? 'success.main' : 'text.secondary'} sx={{ fontFamily: 'monospace' }}>
                        [{l.percent ? `${l.percent}%` : '--'}] {l.text}
                    </Typography>
                ))}
                {serverProgress.length === 0 && <Typography variant="caption" color="text.secondary">Ready...</Typography>}
            </Box>
          </Grid>

        </Grid>
      </DialogContent>

      <DialogActions sx={{ backgroundColor: colors.blueAccent[700], p: 2 }}>
        <Button onClick={onClose} variant="outlined" sx={{ color: colors.grey[100], borderColor: colors.grey[400] }} disabled={submitting}>
          Đóng
        </Button>
        <Button onClick={onSubmit} variant="contained" sx={{ backgroundColor: colors.greenAccent[600], fontWeight: "bold" }} disabled={submitting}>
          {submitting ? (stepStatus || "Đang xử lý...") : "Bắt đầu Upload"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default UploadSourceModal;