import { Box, useTheme, IconButton, Button, Chip } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { tokens } from "../../theme";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../../components/Header";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import AddIcon from "@mui/icons-material/Add";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import { getAllPlans, deletePlan } from "../../services/api";

const Plans = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(true);
  const [pageSize, setPageSize] = useState(10);

  useEffect(() => {
    fetchPlans();
  }, []);

  const fetchPlans = async () => {
    try {
      const response = await getAllPlans();
      if (response.data.errorCode === 200) {
        const transformedData = response.data.data.map((plan) => ({
          id: plan.planID,
          ...plan,
        }));
        setPlans(transformedData);
      }
    } catch (error) {
      console.error("Error fetching plans:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (planID) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa gói dịch vụ này?")) {
      try {
        const response = await deletePlan(planID);
        if (response.data.errorCode === 200) {
          fetchPlans();
          alert("Xóa gói dịch vụ thành công!");
        } else {
          alert(response.data.errorMessage || "Xóa gói dịch vụ thất bại");
        }
      } catch (error) {
        console.error("Error deleting plan:", error);
        alert("Có lỗi xảy ra khi xóa gói dịch vụ");
      }
    }
  };

  const handleViewDetail = (planID) => {
    navigate(`/plans/${planID}`);
  };

  const handleEdit = (planID) => {
    navigate(`/plans/edit/${planID}`);
  };

  const columns = [
    { 
      field: "planID", 
      headerName: "ID",
      width: 70,
    },
    {
      field: "code",
      headerName: "Mã gói",
      width: 120,
      cellClassName: "name-column--cell",
    },
    {
      field: "name",
      headerName: "Tên gói",
      flex: 1,
      minWidth: 200,
      renderCell: ({ row }) => (
        <Box sx={{ fontWeight: "600", color: colors.greenAccent[300] }}>
          {row.name}
        </Box>
      ),
    },
    {
      field: "description",
      headerName: "Mô tả",
      flex: 2,
      minWidth: 400,
      renderCell: ({ value }) => (
        <Box
          sx={{
            whiteSpace: "nowrap",
            overflow: "hidden",
            textOverflow: "ellipsis",
            maxWidth: "100%",
          }}
          title={value}
        >
          {value || "-"}
        </Box>
      ),
    },
    {
      field: "isActive",
      headerName: "Trạng thái",
      width: 130,
      renderCell: ({ row }) => (
        <Chip
          icon={row.isActive ? <CheckCircleIcon /> : <CancelIcon />}
          label={row.isActive ? "Đang hoạt động" : "Không hoạt động"}
          size="small"
          sx={{
            backgroundColor: row.isActive 
              ? colors.greenAccent[600] 
              : colors.redAccent[600],
            color: colors.grey[100],
            fontWeight: "600",
          }}
        />
      ),
    },
    {
      field: "actions",
      headerName: "Hành động",
      width: 150,
      sortable: false,
      renderCell: ({ row }) => {
        return (
          <Box display="flex" gap="5px">
            <IconButton
              onClick={() => handleViewDetail(row.planID)}
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
              onClick={() => handleEdit(row.planID)}
              sx={{
                color: colors.greenAccent[500],
                "&:hover": {
                  backgroundColor: colors.greenAccent[800],
                },
              }}
            >
              <EditOutlinedIcon />
            </IconButton>
            <IconButton
              onClick={() => handleDelete(row.planID)}
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
      <Box display="flex" justifyContent="space-between" alignItems="center">
        <Header 
          title="QUẢN LÝ GÓI DỊCH VỤ" 
          subtitle="Danh sách tất cả gói dịch vụ trong hệ thống" 
        />
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate("/plans/create")}
          sx={{
            backgroundColor: colors.greenAccent[600],
            color: colors.grey[100],
            fontSize: "14px",
            fontWeight: "bold",
            "&:hover": {
              backgroundColor: colors.greenAccent[700],
            },
          }}
        >
          Thêm gói mới
        </Button>
      </Box>

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
        }}
      >
        <DataGrid
          rows={plans}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          onPageSizeChange={(newPageSize) => setPageSize(newPageSize)}
          rowsPerPageOptions={[5, 10, 20]}
          disableSelectionOnClick
        />
      </Box>
    </Box>
  );
};

export default Plans;