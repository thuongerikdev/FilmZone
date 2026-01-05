import { useTheme } from "@mui/material";
import { ResponsiveBar } from "@nivo/bar";
import { tokens } from "../theme";

const BarChart = ({ data, isDashboard = false }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  return (
    <ResponsiveBar
      data={data}
      theme={{
        axis: {
          domain: { line: { stroke: colors.grey[100] } },
          legend: { text: { fill: colors.grey[100] } },
          ticks: {
            line: { stroke: colors.grey[100], strokeWidth: 1 },
            text: { fill: colors.grey[100] },
          },
        },
        legends: { text: { fill: colors.grey[100] } },
      }}
      keys={["total"]} // Key là 'total' từ bước xử lý dữ liệu
      indexBy="date"   // Trục X là 'date'
      margin={{ top: 50, right: 50, bottom: 50, left: 100 }}
      padding={0.3}
      valueScale={{ type: "linear" }}
      indexScale={{ type: "band", round: true }}
      colors={colors.blueAccent[500]} // Màu xanh cho tài chính
      borderColor={{ from: "color", modifiers: [["darker", 1.6]] }}
      axisTop={null}
      axisRight={null}
      axisBottom={{
        tickSize: 5,
        tickPadding: 5,
        tickRotation: -30, // Xoay ngày một chút
        legend: isDashboard ? undefined : "Ngày thanh toán",
        legendPosition: "middle",
        legendOffset: 40,
      }}
      axisLeft={{
        tickSize: 5,
        tickPadding: 5,
        tickRotation: 0,
        legend: isDashboard ? undefined : "Doanh thu (VND)",
        legendPosition: "middle",
        legendOffset: -80,
        // Format số tiền trên trục Y
        format: (v) => new Intl.NumberFormat("vi-VN").format(v),
      }}
      enableLabel={true}
      // Hiển thị số tiền trên đầu mỗi cột
      labelFormat={(v) => new Intl.NumberFormat("vi-VN").format(v)}
      labelSkipWidth={12}
      labelSkipHeight={12}
      labelTextColor={colors.grey[900]}
      role="application"
      barAriaLabel={(e) => `Ngày ${e.indexValue}: ${e.formattedValue} VND`}
    />
  );
};

export default BarChart;