import { Box, useTheme } from "@mui/material";
import Header from "../../components/Header";
import Accordion from "@mui/material/Accordion";
import AccordionSummary from "@mui/material/AccordionSummary";
import AccordionDetails from "@mui/material/AccordionDetails";
import Typography from "@mui/material/Typography";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import { tokens } from "../../theme";

const FAQ = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  return (
    <Box m="20px">
      <Header title="CÂU HỎI THƯỜNG GẶP" subtitle="Trang Câu hỏi Thường gặp" />

      <Accordion defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography color={colors.greenAccent[500]} variant="h5">
            Một Câu Hỏi Quan Trọng
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Typography>
            Đây là một câu trả lời mẫu cho câu hỏi quan trọng của bạn. Chúng tôi sẽ hỗ trợ bạn tốt nhất có thể.
          </Typography>
        </AccordionDetails>
      </Accordion>
      <Accordion defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography color={colors.greenAccent[500]} variant="h5">
            Một Câu Hỏi Quan Trọng Khác
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Typography>
            Câu trả lời cho câu hỏi này có thể thay đổi tùy theo ngữ cảnh. Hãy liên hệ nếu cần thêm chi tiết.
          </Typography>
        </AccordionDetails>
      </Accordion>
      <Accordion defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography color={colors.greenAccent[500]} variant="h5">
            Câu Hỏi Yêu Thích Của Bạn
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Typography>
            Chúng tôi rất vui khi bạn hỏi điều này. Đây là câu trả lời ngắn gọn và hữu ích.
          </Typography>
        </AccordionDetails>
      </Accordion>
      <Accordion defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography color={colors.greenAccent[500]} variant="h5">
            Một Câu Hỏi Ngẫu Nhiên
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Typography>
            Ngẫu nhiên nhưng vẫn quan trọng! Hãy xem xét các tùy chọn sau để giải quyết vấn đề.
          </Typography>
        </AccordionDetails>
      </Accordion>
      <Accordion defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography color={colors.greenAccent[500]} variant="h5">
            Câu Hỏi Cuối Cùng
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Typography>
            Đây là câu trả lời cuối cùng cho hôm nay. Cảm ơn bạn đã hỏi và hy vọng giúp ích được!
          </Typography>
        </AccordionDetails>
      </Accordion>
    </Box>
  );
};

export default FAQ;