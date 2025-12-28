import { Box, Typography, useTheme, IconButton, Chip } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { tokens } from "../../theme";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import AdminPanelSettingsOutlinedIcon from "@mui/icons-material/AdminPanelSettingsOutlined";
import PersonOutlineIcon from "@mui/icons-material/PersonOutline";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import VerifiedIcon from "@mui/icons-material/Verified";
import CancelIcon from "@mui/icons-material/Cancel";
import Header from "../../components/Header";
import { getAllUsers, deleteUser } from "../../services/api";

const Team = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();
  
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [pageSize, setPageSize] = useState(10);

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    try {
      const response = await getAllUsers();
      if (response.data.errorCode === 200) {
        // Transform data để phù hợp với DataGrid
        const transformedData = response.data.data.map((user) => ({
          id: user.userID,
          userID: user.userID,
          userName: user.userName,
          email: user.email,
          status: user.status,
          isEmailVerified: user.isEmailVerified,
          firstName: user.profile?.firstName || "",
          lastName: user.profile?.lastName || "",
          fullName: `${user.profile?.firstName || ""} ${user.profile?.lastName || ""}`.trim() || user.userName,
          gender: user.profile?.gender || "N/A",
          roles: user.roles || [],
          sessionsCount: user.sessions?.length || 0,
          lastSeen: user.sessions?.[0]?.lastSeenAt || null,
        }));
        setUsers(transformedData);
      }
    } catch (error) {
      console.error("Error fetching users:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (userId) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa user này?")) {
      try {
        const response = await deleteUser({ userId });
        if (response.data.errorCode === 200) {
          // Refresh danh sách
          fetchUsers();
          alert("Xóa user thành công!");
        } else {
          alert(response.data.errorMessage || "Xóa user thất bại");
        }
      } catch (error) {
        console.error("Error deleting user:", error);
        alert("Có lỗi xảy ra khi xóa user");
      }
    }
  };

  const handleViewDetail = (userId) => {
    navigate(`/users/${userId}`);
  };

  const columns = [
    { 
      field: "userID", 
      headerName: "ID",
      width: 70,
    },
    {
      field: "fullName",
      headerName: "Họ và Tên",
      flex: 1,
      minWidth: 150,
      cellClassName: "name-column--cell",
    },
    {
      field: "userName",
      headerName: "Username",
      flex: 1,
      minWidth: 120,
    },
    {
      field: "email",
      headerName: "Email",
      flex: 1,
      minWidth: 200,
    },
    {
      field: "isEmailVerified",
      headerName: "Email",
      width: 100,
      renderCell: ({ row }) => {
        return (
          <Box display="flex" alignItems="center">
            {row.isEmailVerified ? (
              <Chip
                icon={<VerifiedIcon />}
                label="Verified"
                size="small"
                sx={{
                  backgroundColor: colors.greenAccent[600],
                  color: colors.grey[100],
                }}
              />
            ) : (
              <Chip
                icon={<CancelIcon />}
                label="Chưa"
                size="small"
                sx={{
                  backgroundColor: colors.redAccent[600],
                  color: colors.grey[100],
                }}
              />
            )}
          </Box>
        );
      },
    },
    {
      field: "status",
      headerName: "Trạng thái",
      width: 120,
      renderCell: ({ row }) => {
        return (
          <Chip
            label={row.status}
            size="small"
            sx={{
              backgroundColor: 
                row.status === "Active" 
                  ? colors.greenAccent[700] 
                  : colors.redAccent[700],
              color: colors.grey[100],
            }}
          />
        );
      },
    },
    {
      field: "roles",
      headerName: "Vai trò",
      flex: 1,
      minWidth: 150,
      renderCell: ({ row }) => {
        const hasAdmin = row.roles.some(role => 
          role.toLowerCase().includes('admin')
        );
        const hasVip = row.roles.some(role => 
          role.toLowerCase().includes('vip')
        );

        return (
          <Box display="flex" gap="5px" flexWrap="wrap">
            {hasAdmin && (
              <Chip
                icon={<AdminPanelSettingsOutlinedIcon />}
                label="Admin"
                size="small"
                sx={{
                  backgroundColor: colors.blueAccent[600],
                  color: colors.grey[100],
                }}
              />
            )}
            {hasVip && (
              <Chip
                label="VIP"
                size="small"
                sx={{
                  backgroundColor: colors.greenAccent[600],
                  color: colors.grey[100],
                }}
              />
            )}
            {!hasAdmin && !hasVip && (
              <Chip
                icon={<PersonOutlineIcon />}
                label="User"
                size="small"
                sx={{
                  backgroundColor: colors.grey[700],
                  color: colors.grey[100],
                }}
              />
            )}
          </Box>
        );
      },
    },
    {
      field: "sessionsCount",
      headerName: "Sessions",
      width: 100,
      align: "center",
      headerAlign: "center",
    },
    {
      field: "actions",
      headerName: "Hành động",
      width: 120,
      sortable: false,
      renderCell: ({ row }) => {
        return (
          <Box display="flex" gap="5px">
            <IconButton
              onClick={() => handleViewDetail(row.userID)}
              sx={{
                color: colors.blueAccent[500],
                "&:hover": {
                  backgroundColor: colors.blueAccent[800],
                },
              }}
            >
              <VisibilityOutlinedIcon />
            </IconButton>
            <IconButton
              onClick={() => handleDelete(row.userID)}
              sx={{
                color: colors.redAccent[500],
                "&:hover": {
                  backgroundColor: colors.redAccent[800],
                },
              }}
            >
              <DeleteOutlineIcon />
            </IconButton>
          </Box>
        );
      },
    },
  ];

  return (
    <Box m="20px">
      <Header 
        title="QUẢN LÝ USERS" 
        subtitle="Danh sách tất cả người dùng trong hệ thống" 
      />
      <Box
        m="40px 0 0 0"
        height="75vh"
        sx={{
          "& .MuiDataGrid-root": {
            border: "none",
          },
          "& .MuiDataGrid-cell": {
            borderBottom: "none",
          },
          "& .name-column--cell": {
            color: colors.greenAccent[300],
            fontWeight: "bold",
          },
          "& .MuiDataGrid-columnHeaders": {
            backgroundColor: colors.blueAccent[700],
            borderBottom: "none",
          },
          "& .MuiDataGrid-virtualScroller": {
            backgroundColor: colors.primary[400],
          },
          "& .MuiDataGrid-footerContainer": {
            borderTop: "none",
            backgroundColor: colors.blueAccent[700],
          },
          "& .MuiDataGrid-toolbarContainer .MuiButton-text": {
            color: `${colors.grey[100]} !important`,
          },
        }}
      >
        <DataGrid
          rows={users}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          onPageSizeChange={(newPageSize) => setPageSize(newPageSize)}
          rowsPerPageOptions={[5, 10, 20, 50]}
          disableSelectionOnClick
        />
      </Box>
    </Box>
  );
};

export default Team;