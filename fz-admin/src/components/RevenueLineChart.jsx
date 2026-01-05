import { ResponsiveLine } from "@nivo/line";
import { useTheme } from "@mui/material";
import { tokens } from "../theme";
import { useState, useEffect } from "react";
import axios from "axios";
import { getAllInvoices } from "../services/api";

const RevenueLineChart = ({ isDashboard = false }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const [chartData, setChartData] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchRevenueData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const fetchRevenueData = async () => {
    try {
      setLoading(true);
      // Gọi API lấy tất cả invoices
      const response = await getAllInvoices();

      if (response.data && response.data.errorCode === 200) {
        const invoices = response.data.data;

        console.log("📊 Invoices từ API:", invoices);
        console.log("📊 Số lượng invoices:", invoices.length);

        if (!invoices || invoices.length === 0) {
          console.warn("⚠️ Không có invoices");
          setChartData([
            {
              id: "Doanh thu tích lũy",
              color: colors.greenAccent[500],
              data: [],
            },
          ]);
          setLoading(false);
          return;
        }

        // Sắp xếp invoices theo thời gian
        const sortedInvoices = [...invoices].sort((a, b) => {
          return new Date(a.issuedAt) - new Date(b.issuedAt);
        });

        console.log("📊 Invoices sau khi sort:", sortedInvoices);

        // Tính doanh thu tích lũy
        let cumulativeRevenue = 0;
        const chartDataPoints = sortedInvoices.map((invoice, index) => {
          cumulativeRevenue += invoice.total;
          const date = new Date(invoice.issuedAt);
          const dateLabel = date.toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
          });

          const point = {
            x: dateLabel,
            y: Math.round(cumulativeRevenue / 1000),
          };
          console.log(`📊 Point ${index}:`, point, "| Total:", invoice.total);
          return point;
        });

        console.log("📊 Chart Data Points:", chartDataPoints);

        setChartData([
          {
            id: "Doanh thu tích lũy",
            color: colors.greenAccent[500],
            data: chartDataPoints,
          },
        ]);
      } else {
        console.error("❌ Error:", response.data.errorMessage);
        setChartData([
          {
            id: "Doanh thu tích lũy",
            color: colors.greenAccent[500],
            data: [],
          },
        ]);
      }
    } catch (error) {
      console.error("❌ Error fetching revenue data:", error);
      setChartData([
        {
          id: "Doanh thu tích lũy",
          color: colors.greenAccent[500],
          data: [],
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          height: "100%",
          color: colors.grey[100],
        }}
      >
        Đang tải dữ liệu...
      </div>
    );
  }

  return (
    <ResponsiveLine
      data={chartData}
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
        tooltip: { container: { color: colors.primary[500] } },
      }}
      colors={{ datum: "color" }}
      margin={{ top: 50, right: 110, bottom: 50, left: 70 }}
      xScale={{ type: "point" }}
      yScale={{
        type: "linear",
        min: 0,
        max: "auto",
        stacked: false,
        reverse: false,
      }}
      // Định dạng hiển thị trên trục Y (ví dụ: 99k)
      yFormat={(value) => `${value.toLocaleString()}k`}
      curve="linear"
      axisTop={null}
      axisRight={null}
      axisBottom={{
        orient: "bottom",
        tickSize: 5,
        tickPadding: 5,
        tickRotation: 45,
        legend: isDashboard ? undefined : "Ngày",
        legendOffset: 50,
        legendPosition: "middle",
      }}
      axisLeft={{
        orient: "left",
        tickSize: 5,
        tickPadding: 5,
        tickRotation: 0,
        legend: isDashboard ? undefined : "Doanh thu tích lũy (nghìn VNĐ)",
        legendOffset: -60,
        legendPosition: "middle",
        format: (value) => `${value}k`,
      }}
      enableGridX={false}
      enableGridY={true}
      gridYValues={5}
      pointSize={8}
      pointColor={{ theme: "background" }}
      pointBorderWidth={2}
      pointBorderColor={{ from: "serieColor" }}
      pointLabelYOffset={-12}
      useMesh={true}
      legends={
        isDashboard
          ? []
          : [
              {
                anchor: "bottom-right",
                direction: "column",
                justify: false,
                translateX: 100,
                translateY: 0,
                itemsSpacing: 0,
                itemDirection: "left-to-right",
                itemWidth: 120,
                itemHeight: 20,
                itemOpacity: 0.75,
                symbolSize: 12,
                symbolShape: "circle",
                effects: [{ on: "hover", style: { itemOpacity: 1 } }],
              },
            ]
      }
      tooltip={({ point }) => (
        <div
          style={{
            background: colors.primary[400],
            padding: "12px 16px",
            border: `1px solid ${colors.grey[700]}`,
            borderRadius: "4px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.3)",
          }}
        >
          <strong style={{ color: colors.grey[100] }}>{point.data.xFormatted}</strong>
          <br />
          <span style={{ color: colors.greenAccent[500] }}>
            Tích lũy: {(point.data.y * 1000).toLocaleString("vi-VN")} VNĐ
          </span>
        </div>
      )}
    />
  );
};

export default RevenueLineChart;