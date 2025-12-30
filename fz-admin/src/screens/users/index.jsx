import { useState, useEffect, useCallback, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { 
  Box, 
  useTheme, 
  IconButton, 
  Chip, 
  Tooltip, 
  Typography 
} from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { tokens } from "../../theme";
import Header from "../../components/Header";
import { getAllUsers, deleteUser } from "../../services/api";

// Icons
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import VerifiedIcon from "@mui/icons-material/Verified";
import CancelIcon from "@mui/icons-material/Cancel";

const Team = () => {
const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [pageSize, setPageSize] = useState(10);

  // 1. Dùng useCallback để hàm này không bị tạo lại mỗi lần render
  // Giúp useEffect không bị chạy vô tận
  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true);
      const response = await getAllUsers();

      if (response.data && response.data.errorCode === 200) {
        const transformedData = response.data.data.map((user) => ({
          id: user.userID, 
          userID: user.userID,
          userName: user.userName,
          email: user.email,
          status: user.status,
          isEmailVerified: user.isEmailVerified,
          fullName:
            `${user.profile?.firstName || ""} ${
              user.profile?.lastName || ""
            }`.trim() || user.userName,
          roles: user.roles || [],
          sessionsCount: user.sessions?.length || 0,
        }));
        setUsers(transformedData);
      }
    } catch (error) {
      console.error("Error fetching users:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    let isActive = true;

    fetchUsers().then(() => {
        if (!isActive) return; 
    });

    return () => {
      isActive = false;
    };
  }, [fetchUsers]);

  const handleDelete = async (userId) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa user này?")) {
      try {
        const response = await deleteUser({ userId });
        if (response.data.errorCode === 200) {
          alert("Xóa user thành công!");
          fetchUsers(); 
        } else {
          alert(response.data.errorMessage || "Xóa user thất bại");
        }
      } catch (error) {
        console.error("Error deleting user:", error);
        alert("Có lỗi xảy ra khi xóa user");
      }
    }
  };

  const columns = [
    {
      field: "userID",
      headerName: "ID",
      width: 60,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "fullName",
      headerName: "Họ và Tên",
      flex: 1,
      minWidth: 150,
      renderCell: ({ value }) => (
        <Tooltip title={value} arrow>
          <Box display="flex" alignItems="center" width="100%" height="100%">
            <Typography variant="body2" noWrap>
              {value}
            </Typography>
          </Box>
        </Tooltip>
      ),
    },
    {
      field: "email",
      headerName: "Email",
      flex: 1,
      minWidth: 200,
      renderCell: ({ value }) => (
        <Tooltip title={value} arrow>
          <Box display="flex" alignItems="center" width="100%" height="100%">
            <Typography variant="body2" noWrap>
              {value}
            </Typography>
          </Box>
        </Tooltip>
      ),
    },
    {
      field: "isEmailVerified",
      headerName: "Xác thực",
      width: 100,
      align: "center",
      headerAlign: "center",
      renderCell: ({ value }) => (
        <Box display="flex" justifyContent="center" alignItems="center" height="100%">
          {value ? (
            <VerifiedIcon sx={{ color: colors.greenAccent[500] }} />
          ) : (
            <CancelIcon sx={{ color: colors.redAccent[500] }} />
          )}
        </Box>
      ),
    },
    {
      field: "status",
      headerName: "Trạng thái",
      width: 120,
      align: "center",
      headerAlign: "center",
      renderCell: ({ value }) => (
        <Box display="flex" justifyContent="center" alignItems="center" height="100%">
          <Chip
            label={value}
            size="small"
            sx={{
              backgroundColor: value === "Active" ? colors.greenAccent[700] : colors.redAccent[700],
              color: colors.grey[100],
              minWidth: "80px",
            }}
          />
        </Box>
      ),
    },
    {
      field: "roles",
      headerName: "Vai trò",
      flex: 1,
      minWidth: 180,
      renderCell: ({ value }) => {
        const displayRoles = value.slice(0, 2);
        const remaining = value.length - 2;
        
        return (
          <Box display="flex" gap={0.5} alignItems="center" height="100%" flexWrap="nowrap">
            {displayRoles.map((role, idx) => (
              <Chip
                key={idx}
                label={role}
                size="small"
                sx={{
                  backgroundColor: role.toLowerCase().includes('admin') 
                    ? colors.redAccent[600] 
                    : colors.blueAccent[700],
                  color: colors.grey[100],
                  maxWidth: "100px", 
                }}
              />
            ))}
            {remaining > 0 && (
              <Tooltip title={value.slice(2).join(", ")} arrow>
                <Chip
                  label={`+${remaining}`}
                  size="small"
                  variant="outlined"
                  sx={{ color: colors.grey[100], borderColor: colors.grey[100] }}
                />
              </Tooltip>
            )}
          </Box>
        );
      },
    },
    {
      field: "sessionsCount",
      headerName: "Sessions",
      width: 80,
      align: "center",
      headerAlign: "center",
    },
    {
      field: "actions",
      headerName: "Hành động",
      width: 100,
      sortable: false,
      align: "center",
      headerAlign: "center",
      renderCell: ({ row }) => (
        <Box display="flex" justifyContent="center" alignItems="center" gap={1} height="100%">
          <Tooltip title="Xem chi tiết">
            <IconButton
              onClick={() => navigate(`/users/${row.userID}`)}
              size="small"
              sx={{ color: colors.blueAccent[400] }}
            >
              <VisibilityOutlinedIcon />
            </IconButton>
          </Tooltip>
          
          <Tooltip title="Xóa User">
            <IconButton
              onClick={() => handleDelete(row.userID)}
              size="small"
              sx={{ color: colors.redAccent[500] }}
            >
              <DeleteOutlineIcon />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  return (
    <Box m="20px">
      <Header title="QUẢN LÝ USERS" subtitle="Danh sách người dùng hệ thống" />
      
      <Box
        m="20px 0 0 0"
        height="75vh"
        sx={{
          "& .MuiDataGrid-root": {
            border: "none",
          },
          "& .MuiDataGrid-cell": {
            borderBottom: `1px solid ${colors.primary[400]}`, 
          },
          "& .name-column--cell": {
            color: colors.greenAccent[300],
          },
          "& .MuiDataGrid-columnHeaders": {
            backgroundColor: colors.blueAccent[700],
            borderBottom: "none",
            fontSize: "14px", 
          },
          "& .MuiDataGrid-virtualScroller": {
            backgroundColor: colors.primary[400],
          },
          "& .MuiDataGrid-footerContainer": {
            borderTop: "none",
            backgroundColor: colors.blueAccent[700],
          },
          "& .MuiCheckbox-root": {
            color: `${colors.greenAccent[200]} !important`,
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
          rowHeight={60} 
          // Pagination
          pageSize={pageSize}
          onPageSizeChange={(newPageSize) => setPageSize(newPageSize)}
          rowsPerPageOptions={[10, 20, 50]}
          disableSelectionOnClick
        />
      </Box>
    </Box>
  );
};

export default Team;