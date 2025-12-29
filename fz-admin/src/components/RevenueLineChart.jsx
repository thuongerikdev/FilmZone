import { ResponsiveLine } from "@nivo/line";
import { useTheme } from "@mui/material";
import { tokens } from "../theme";
import { useState, useEffect } from "react";
import axios from "axios";

const RevenueLineChart = ({ isDashboard = false }) => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const [chartData, setChartData] = useState([]);

  useEffect(() => {
    fetchRevenueData();
  }, []);

  const fetchRevenueData = async () => {
    try {
      const response = await axios.get(
        'https://filmzone-api.koyeb.app/api/payment/order/all',
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );

      if (response.data.errorCode === 200) {
        const orders = response.data.data.filter(order => order.status === 'paid');
        
        // Group orders by month
        const revenueByMonth = {};
        
        orders.forEach(order => {
          const date = new Date(order.createdAt);
          const monthKey = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
          const monthLabel = date.toLocaleDateString('vi-VN', { month: 'short', year: 'numeric' });
          
          if (!revenueByMonth[monthKey]) {
            revenueByMonth[monthKey] = {
              month: monthLabel,
              revenue: 0
            };
          }
          
          revenueByMonth[monthKey].revenue += order.amount;
        });

        // Sort by date and convert to array
        const sortedData = Object.keys(revenueByMonth)
          .sort()
          .map(key => ({
            x: revenueByMonth[key].month,
            y: revenueByMonth[key].revenue / 1000000 // Convert to millions
          }));

        // If no data, use sample data
        if (sortedData.length === 0) {
          setChartData([
            {
              id: "Doanh thu",
              color: colors.greenAccent[500],
              data: [
                { x: "Tháng 1", y: 0 },
                { x: "Tháng 2", y: 0 },
                { x: "Tháng 3", y: 0 },
              ]
            }
          ]);
        } else {
          setChartData([
            {
              id: "Doanh thu",
              color: colors.greenAccent[500],
              data: sortedData
            }
          ]);
        }
      }
    } catch (error) {
      console.error("Error fetching revenue data:", error);
      // Use empty data on error
      setChartData([
        {
          id: "Doanh thu",
          color: colors.greenAccent[500],
          data: []
        }
      ]);
    }
  };

  return (
    <ResponsiveLine
      data={chartData}
      theme={{
        axis: {
          domain: {
            line: {
              stroke: colors.grey[100],
            },
          },
          legend: {
            text: {
              fill: colors.grey[100],
            },
          },
          ticks: {
            line: {
              stroke: colors.grey[100],
              strokeWidth: 1,
            },
            text: {
              fill: colors.grey[100],
            },
          },
        },
        legends: {
          text: {
            fill: colors.grey[100],
          },
        },
        tooltip: {
          container: {
            color: colors.primary[500],
          },
        },
      }}
      colors={{ datum: "color" }}
      margin={{ top: 50, right: 110, bottom: 50, left: 60 }}
      xScale={{ type: "point" }}
      yScale={{
        type: "linear",
        min: "auto",
        max: "auto",
        stacked: false,
        reverse: false,
      }}
      yFormat={(value) => `${value.toFixed(1)}M`}
      curve="catmullRom"
      axisTop={null}
      axisRight={null}
      axisBottom={{
        orient: "bottom",
        tickSize: 5,
        tickPadding: 5,
        tickRotation: 0,
        legend: isDashboard ? undefined : "Tháng",
        legendOffset: 36,
        legendPosition: "middle",
      }}
      axisLeft={{
        orient: "left",
        tickValues: 5,
        tickSize: 5,
        tickPadding: 5,
        tickRotation: 0,
        legend: isDashboard ? undefined : "Doanh thu (Triệu VNĐ)",
        legendOffset: -50,
        legendPosition: "middle",
        format: (value) => `${value}M`,
      }}
      enableGridX={false}
      enableGridY={false}
      pointSize={10}
      pointColor={{ theme: "background" }}
      pointBorderWidth={2}
      pointBorderColor={{ from: "serieColor" }}
      pointLabelYOffset={-12}
      useMesh={true}
      legends={[
        {
          anchor: "bottom-right",
          direction: "column",
          justify: false,
          translateX: 100,
          translateY: 0,
          itemsSpacing: 0,
          itemDirection: "left-to-right",
          itemWidth: 80,
          itemHeight: 20,
          itemOpacity: 0.75,
          symbolSize: 12,
          symbolShape: "circle",
          symbolBorderColor: "rgba(0, 0, 0, .5)",
          effects: [
            {
              on: "hover",
              style: {
                itemBackground: "rgba(0, 0, 0, .03)",
                itemOpacity: 1,
              },
            },
          ],
        },
      ]}
      tooltip={({ point }) => (
        <div
          style={{
            background: colors.primary[400],
            padding: "9px 12px",
            border: `1px solid ${colors.grey[700]}`,
            borderRadius: "4px",
          }}
        >
          <strong style={{ color: colors.grey[100] }}>{point.data.xFormatted}</strong>
          <br />
          <span style={{ color: colors.greenAccent[500] }}>
            Doanh thu: {(point.data.y * 1000000).toLocaleString('vi-VN')} VNĐ
          </span>
        </div>
      )}
    />
  );
};

export default RevenueLineChart;