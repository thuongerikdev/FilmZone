import { Box, useTheme, IconButton, Button, Chip } from "@mui/material";
import { DataGrid, GridToolbar } from "@mui/x-data-grid";
import { tokens } from "../../theme";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../../components/Header";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import AddIcon from "@mui/icons-material/Add";
import { getAllPersons, deletePerson } from "../../services/api";

const Persons = () => {
  const theme = useTheme();
  const colors = tokens(theme.palette.mode);
  const navigate = useNavigate();

  const [persons, setPersons] = useState([]);
  const [loading, setLoading] = useState(true);
  const [pageSize, setPageSize] = useState(10);

  useEffect(() => {
    fetchPersons();
  }, []);

  const fetchPersons = async () => {
    try {
      const response = await getAllPersons();
      if (response.data.errorCode === 200) {
        const transformedData = response.data.data.map((person) => ({
          id: person.personID,
          ...person,
        }));
        setPersons(transformedData);
      }
    } catch (error) {
      console.error("Error fetching persons:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (personId) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa diễn viên này?")) {
      try {
        const response = await deletePerson(personId);
        if (response.data.errorCode === 200) {
          fetchPersons();
          alert("Xóa diễn viên thành công!");
        } else {
          alert(response.data.errorMessage || "Xóa diễn viên thất bại");
        }
      } catch (error) {
        console.error("Error deleting person:", error);
        alert("Có lỗi xảy ra khi xóa diễn viên");
      }
    }
  };

  const handleViewDetail = (personId) => {
    navigate(`/persons/${personId}`);
  };

  const handleEdit = (personId) => {
    navigate(`/persons/edit/${personId}`);
  };

  const formatDate = (dateString) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString('vi-VN');
  };

  const columns = [
    { 
      field: "personID", 
      headerName: "ID",
      width: 70,
    },
    {
      field: "avatar",
      headerName: "Avatar",
      width: 100,
      renderCell: ({ row }) => (
        <Box
          component="img"
          src={row.avatar}
          alt={row.fullName}
          sx={{
            width: "60px",
            height: "60px",
            objectFit: "cover",
            borderRadius: "50%",
            my: 1,
          }}
        />
      ),
    },
    {
      field: "fullName",
      headerName: "Họ và tên",
      flex: 1,
      minWidth: 200,
      cellClassName: "name-column--cell",
    },
    {
      field: "knownFor",
      headerName: "Vai trò",
      width: 150,
      renderCell: ({ row }) => {
        const getRoleColor = (role) => {
          const roleLower = role.toLowerCase();
          if (roleLower.includes('director')) return colors.blueAccent[600];
          if (roleLower.includes('cast')) return colors.greenAccent[600];
          if (roleLower.includes('writer')) return colors.grey[600];
          return colors.grey[700];
        };

        return (
          <Chip
            label={row.knownFor}
            size="small"
            sx={{
              backgroundColor: getRoleColor(row.knownFor),
              color: colors.grey[100],
              fontWeight: "600",
            }}
          />
        );
      },
    },
    {
      field: "birthDate",
      headerName: "Ngày sinh",
      width: 130,
      renderCell: ({ row }) => formatDate(row.birthDate),
    },
    {
      field: "biography",
      headerName: "Tiểu sử",
      flex: 1,
      minWidth: 300,
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
      field: "createdAt",
      headerName: "Ngày tạo",
      width: 130,
      renderCell: ({ row }) => formatDate(row.createdAt),
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
              onClick={() => handleViewDetail(row.personID)}
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
              onClick={() => handleEdit(row.personID)}
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
              onClick={() => handleDelete(row.personID)}
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
          title="QUẢN LÝ DIỄN VIÊN" 
          subtitle="Danh sách tất cả diễn viên trong hệ thống" 
        />
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate("/persons/create")}
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
          Thêm diễn viên mới
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
          "& .MuiDataGrid-toolbarContainer .MuiButton-text": {
            color: `${colors.grey[100]} !important`,
          },
          "& .MuiDataGrid-row": {
            minHeight: "80px !important",
            maxHeight: "80px !important",
          },
          "& .MuiDataGrid-cell": {
            minHeight: "80px !important",
            maxHeight: "80px !important",
            display: "flex",
            alignItems: "center",
          },
        }}
      >
        <DataGrid
          rows={persons}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          onPageSizeChange={(newPageSize) => setPageSize(newPageSize)}
          rowsPerPageOptions={[5, 10, 20, 50]}
          disableSelectionOnClick
          components={{ Toolbar: GridToolbar }}
          getRowHeight={() => 80}
        />
      </Box>
    </Box>
  );
};

export default Persons;