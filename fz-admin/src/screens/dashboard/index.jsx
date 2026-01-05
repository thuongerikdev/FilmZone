import { Box, Button, IconButton, Typography, useTheme } from "@mui/material";
import { tokens } from "../../theme";
import { useState, useEffect } from "react";
import DownloadOutlinedIcon from "@mui/icons-material/DownloadOutlined";
import MovieIcon from "@mui/icons-material/Movie";
import PlayCircleOutlineIcon from "@mui/icons-material/PlayCircleOutline";
import PersonAddIcon from "@mui/icons-material/PersonAdd";
import AttachMoneyIcon from "@mui/icons-material/AttachMoney";
import Header from "../../components/Header";
import RevenueLineChart from "../../components/RevenueLineChart";
import BarChart from "../../components/BarChart";
import StatBox from "../../components/StatBox";
import ProgressCircle from "../../components/ProgressCircle";
// ✅ Import tất cả API functions từ api.js
// ✅ Import tất cả API functions từ api.js
import { getAllMovies, getAllPersons, getAllUsers, getAllOrders } from "../../services/api";

const Dashboard = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);

  const [stats, setStats] = useState({
    totalMovies: 0,
    totalPersons: 0,
    totalOrders: 0,
    totalRevenue: 0,
    movieGrowth: "+0%",
    personGrowth: "+0%",
    orderGrowth: "+0%",
    revenueGrowth: "+0%",
  });

  const [recentOrders, setRecentOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [userStats, setUserStats] = useState({
    totalUsers: 0,
    vipUsers: 0,
    regularUsers: 0,
    vipPercentage: 0,
  });

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      // ✅ Fetch movies
      const moviesResponse = await getAllMovies();
      const totalMovies = moviesResponse.data.errorCode === 200 
        ? moviesResponse.data.data.length 
        : 0;

      // ✅ Fetch persons
      const personsResponse = await getAllPersons();
      const totalPersons = personsResponse.data.errorCode === 200 
        ? personsResponse.data.data.length 
        : 0;

      // ✅ Fetch users for customer statistics
      const usersResponse = await getAllUsers();
      let totalUsers = 0;
      let vipUsers = 0;
      let regularUsers = 0;
      let vipPercentage = 0;

      if (usersResponse.data.errorCode === 200) {
        const users = usersResponse.data.data;
        totalUsers = users.length;
        
        // Count VIP users
        vipUsers = users.filter(user => 
          user.roles && user.roles.some(role => role.toLowerCase().includes('vip'))
        ).length;
        
        regularUsers = totalUsers - vipUsers;
        vipPercentage = totalUsers > 0 ? (vipUsers / totalUsers) : 0;
      }

      setUserStats({
        totalUsers,
        vipUsers,
        regularUsers,
        vipPercentage,
      });

      // ✅ Fetch orders - Sử dụng getAllOrders từ api.js
      const ordersResponse = await getAllOrders();

      let totalOrders = 0;
      let totalRevenue = 0;
      let recentOrdersList = [];

      if (ordersResponse.data.errorCode === 200) {
        const orders = ordersResponse.data.data;
        totalOrders = orders.length;
        
        // Calculate total revenue from paid orders
        totalRevenue = orders
          .filter(order => order.status === 'paid')
          .reduce((sum, order) => sum + order.amount, 0);

        // Get 5 most recent orders
        recentOrdersList = orders
          .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
          .slice(0, 5)
          .map(order => ({
            txId: `#${order.orderID}`,
            user: `User ${order.userID}`,
            date: new Date(order.createdAt).toLocaleDateString('vi-VN'),
            cost: new Intl.NumberFormat('vi-VN').format(order.amount),
          }));
      }

      setStats({
        totalMovies,
        totalPersons,
        totalOrders,
        totalRevenue,
        movieGrowth: "+12%",
        personGrowth: "+8%",
        orderGrowth: "+23%",
        revenueGrowth: "+31%",
      });

      setRecentOrders(recentOrdersList);
    } catch (error) {
      console.error("Error fetching dashboard data:", error);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
    }).format(amount);
  };

  return (
    <Box m="20px">
      {/* HEADER */}
      <Box display="flex" justifyContent="space-between" alignItems="center">
        <Header title="BẢNG ĐIỀU KHIỂN" subtitle="Chào mừng đến với hệ thống quản lý phim trực tuyến" />

        {/* <Box>
          <Button
            sx={{
              backgroundColor: colors.blueAccent[700],
              color: colors.grey[100],
              fontSize: "14px",
              fontWeight: "bold",
              padding: "10px 20px",
            }}
          >
            <DownloadOutlinedIcon sx={{ mr: "10px" }} />
            Tải báo cáo
          </Button>
        </Box> */}
      </Box>

      {/* GRID & CHARTS */}
      <Box
        display="grid"
        gridTemplateColumns="repeat(12, 1fr)"
        gridAutoRows="140px"
        gap="20px"
      >
        {/* ROW 1 - Statistics */}
        <Box
          gridColumn="span 3"
          backgroundColor={colors.primary[400]}
          display="flex"
          alignItems="center"
          justifyContent="center"
        >
          <StatBox
            title={stats.totalMovies.toLocaleString()}
            subtitle="Tổng số phim"
            progress="0.85"
            increase={stats.movieGrowth}
            icon={
              <MovieIcon
                sx={{ color: colors.greenAccent[600], fontSize: "26px" }}
              />
            }
          />
        </Box>
        <Box
          gridColumn="span 3"
          backgroundColor={colors.primary[400]}
          display="flex"
          alignItems="center"
          justifyContent="center"
        >
          <StatBox
            title={stats.totalPersons.toLocaleString()}
            subtitle="Diễn viên & Đạo diễn"
            progress="0.72"
            increase={stats.personGrowth}
            icon={
              <PersonAddIcon
                sx={{ color: colors.greenAccent[600], fontSize: "26px" }}
              />
            }
          />
        </Box>
        <Box
          gridColumn="span 3"
          backgroundColor={colors.primary[400]}
          display="flex"
          alignItems="center"
          justifyContent="center"
        >
          <StatBox
            title={stats.totalOrders.toLocaleString()}
            subtitle="Tổng lượt mua"
            progress="0.55"
            increase={stats.orderGrowth}
            icon={
              <PlayCircleOutlineIcon
                sx={{ color: colors.greenAccent[600], fontSize: "26px" }}
              />
            }
          />
        </Box>
        <Box
          gridColumn="span 3"
          backgroundColor={colors.primary[400]}
          display="flex"
          alignItems="center"
          justifyContent="center"
        >
          <StatBox
            title={formatCurrency(stats.totalRevenue)}
            subtitle="Tổng doanh thu"
            progress="0.88"
            increase={stats.revenueGrowth}
            icon={
              <AttachMoneyIcon
                sx={{ color: colors.greenAccent[600], fontSize: "26px" }}
              />
            }
          />
        </Box>

        {/* ROW 2 */}
        <Box
          gridColumn="span 8"
          gridRow="span 2"
          backgroundColor={colors.primary[400]}
        >
          <Box
            mt="25px"
            p="0 30px"
            display="flex "
            justifyContent="space-between"
            alignItems="center"
          >
            <Box>
              <Typography
                variant="h5"
                fontWeight="600"
                color={colors.grey[100]}
              >
                Doanh thu theo thời gian
              </Typography>
              <Typography
                variant="h3"
                fontWeight="bold"
                color={colors.greenAccent[500]}
              >
                {formatCurrency(stats.totalRevenue)}
              </Typography>
            </Box>
            {/* <Box>
              <IconButton>
                <DownloadOutlinedIcon
                  sx={{ fontSize: "26px", color: colors.greenAccent[500] }}
                />
              </IconButton>
            </Box> */}
          </Box>
          <Box height="250px" m="-20px 0 0 0">
            <RevenueLineChart isDashboard={true} />
          </Box>
        </Box>
        <Box
          gridColumn="span 4"
          gridRow="span 2"
          backgroundColor={colors.primary[400]}
          overflow="auto"
        >
          <Box
            display="flex"
            justifyContent="space-between"
            alignItems="center"
            borderBottom={`4px solid ${colors.primary[500]}`}
            colors={colors.grey[100]}
            p="15px"
          >
            <Typography color={colors.grey[100]} variant="h5" fontWeight="600">
              Đơn hàng gần đây
            </Typography>
          </Box>
          {loading ? (
            <Box p="15px" textAlign="center">
              <Typography color={colors.grey[100]}>Đang tải...</Typography>
            </Box>
          ) : recentOrders.length === 0 ? (
            <Box p="15px" textAlign="center">
              <Typography color={colors.grey[100]}>Chưa có đơn hàng</Typography>
            </Box>
          ) : (
            recentOrders.map((order, i) => (
              <Box
                key={`${order.txId}-${i}`}
                display="flex"
                justifyContent="space-between"
                alignItems="center"
                borderBottom={`4px solid ${colors.primary[500]}`}
                p="15px"
              >
                <Box>
                  <Typography
                    color={colors.greenAccent[500]}
                    variant="h5"
                    fontWeight="600"
                  >
                    {order.txId}
                  </Typography>
                  <Typography color={colors.grey[100]}>
                    {order.user}
                  </Typography>
                </Box>
                <Box color={colors.grey[100]}>{order.date}</Box>
                <Box
                  backgroundColor={colors.greenAccent[500]}
                  p="5px 10px"
                  borderRadius="4px"
                >
                  {order.cost}₫
                </Box>
              </Box>
            ))
          )}
        </Box>

        {/* ROW 3 */}
        <Box
          gridColumn="span 6"
          gridRow="span 2"
          backgroundColor={colors.primary[400]}
          p="30px"
        >
          <Typography variant="h5" fontWeight="600" mb={2}>
            Phân loại khách hàng
          </Typography>
          <Box
            display="flex"
            flexDirection="column"
            alignItems="center"
            mt="25px"
          >
            <ProgressCircle size="125" progress={userStats.vipPercentage.toFixed(2)} />
            <Typography
              variant="h5"
              color={colors.greenAccent[500]}
              sx={{ mt: "15px" }}
            >
              {(userStats.vipPercentage * 100).toFixed(1)}% khách hàng VIP
            </Typography>
            <Box display="flex" gap={3} mt={3}>
              <Box textAlign="center">
                <Typography variant="h4" color={colors.greenAccent[500]} fontWeight="600">
                  {userStats.vipUsers.toLocaleString()}
                </Typography>
                <Typography variant="body2" color={colors.grey[400]}>
                  Khách VIP
                </Typography>
              </Box>
              <Box textAlign="center">
                <Typography variant="h4" color={colors.blueAccent[500]} fontWeight="600">
                  {userStats.regularUsers.toLocaleString()}
                </Typography>
                <Typography variant="body2" color={colors.grey[400]}>
                  Khách thường
                </Typography>
              </Box>
            </Box>
          </Box>
        </Box>
      </Box>
    </Box>
  );
};

export default Dashboard;