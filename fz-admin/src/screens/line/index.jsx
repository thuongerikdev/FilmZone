import { Box } from "@mui/material";
import Header from "../../components/Header";
import LineChart from "../../components/RevenueLineChart";

const Line = () => {
  return (
    <Box m="20px">
      <Header title="BIỂU ĐỒ TĂNG TRƯỞNG" subtitle="Theo dõi doanh thu theo thời gian" />
      <Box height="75vh">
        <LineChart />
      </Box>
    </Box>
  );
};

export default Line;