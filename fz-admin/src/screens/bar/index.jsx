import { Box } from "@mui/material";
import Header from "../../components/Header";
import BarChart from "../../components/BarChart";
import { useState, useEffect } from "react";
import { getAllInvoices } from "../../services/api"; // Đường dẫn tới file api.js của bạn

const Bar = () => {
  const [chartData, setChartData] = useState([]);

  useEffect(() => {
    const fetchInvoices = async () => {
      try {
        const response = await getAllInvoices();
        if (response.data && response.data.errorCode === 200) {
          const invoices = response.data.data;

          // Xử lý gom nhóm dữ liệu theo ngày
          // Chuyển từ: [{total: 99000, issuedAt: "2025-12-29..."}, {total: 99000, issuedAt: "2025-12-29..."}]
          // Thành: [{date: "2025-12-29", total: 198000}]
          const groupedData = invoices.reduce((acc, curr) => {
            const date = new Date(curr.issuedAt).toLocaleDateString('en-CA'); // Định dạng YYYY-MM-DD
            const existingDate = acc.find((item) => item.date === date);

            if (existingDate) {
              existingDate.total += curr.total;
            } else {
              acc.push({ date: date, total: curr.total });
            }
            return acc;
          }, []);

          // Sắp xếp theo thứ tự ngày tăng dần
          groupedData.sort((a, b) => new Date(a.date) - new Date(b.date));
          
          setChartData(groupedData);
        }
      } catch (error) {
        console.error("Lỗi lấy dữ liệu hóa đơn:", error);
      }
    };

    fetchInvoices();
  }, []);

  return (
    <Box m="20px">
      <Header title="BIỂU ĐỒ DOANH THU" subtitle="Thống kê tổng doanh thu theo ngày" />
      <Box height="75vh">
        <BarChart data={chartData} />
      </Box>
    </Box>
  );
};

export default Bar;