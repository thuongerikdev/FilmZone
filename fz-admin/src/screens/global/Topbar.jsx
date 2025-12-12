import React, { useState } from 'react';
import { Box, IconButton, useTheme, Menu, MenuItem, Typography, Divider } from "@mui/material";
import { useContext } from "react";
import { useNavigate } from "react-router-dom";
import { ColorModeContext, tokens } from "../../theme";
import InputBase from "@mui/material/InputBase";
import LightModeOutlinedIcon from "@mui/icons-material/LightModeOutlined";
import DarkModeOutlinedIcon from "@mui/icons-material/DarkModeOutlined";
import NotificationsOutlinedIcon from "@mui/icons-material/NotificationsOutlined";
import SettingsOutlinedIcon from "@mui/icons-material/SettingsOutlined";
import PersonOutlinedIcon from "@mui/icons-material/PersonOutlined";
import SearchIcon from "@mui/icons-material/Search";
import AccountCircleIcon from "@mui/icons-material/AccountCircle";
import LogoutIcon from "@mui/icons-material/Logout";
import { logout } from "../../services/api";

function Topbar() {
    const theme = useTheme();
    const colorMode = useContext(ColorModeContext);
    const colors = tokens(theme.palette.mode);
    const navigate = useNavigate();
    
    // State cho menu
    const [anchorEl, setAnchorEl] = useState(null);
    const open = Boolean(anchorEl);
    
    // Lấy thông tin user từ localStorage
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    
    const handleClick = (event) => {
        setAnchorEl(event.currentTarget);
    };
    
    const handleClose = () => {
        setAnchorEl(null);
    };
    
    const handleProfile = () => {
        handleClose();
        navigate('/profile'); // Chuyển đến trang profile
    };
    
    const handleLogout = async () => {
        handleClose();
        try {
            // Gọi API logout
            await logout({});
            
            // Xóa token và user info
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            
            // Chuyển về trang login
            navigate('/login');
        } catch (error) {
            console.error('Logout error:', error);
            // Vẫn xóa local storage và chuyển về login nếu API fail
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            navigate('/login');
        }
    };

    return (
        <Box
            display="flex"
            justifyContent="space-between"
            p={2}
        >
            <Box
                display="flex"
                sx={{
                    backgroundColor: colors.primary[400],
                    borderRadius: "3px"
                }}
            >
                <InputBase sx={{ ml: 2, flex: 1 }} placeholder='Search'></InputBase>
                <IconButton type="button" sx={{ p: 1 }}>
                    <SearchIcon />
                </IconButton>
            </Box>
            
            <Box display="flex">
                <IconButton onClick={colorMode.toggleColorMode}>
                    {theme.palette.mode === "dark" ? (
                        <DarkModeOutlinedIcon />
                    ) : (
                        <LightModeOutlinedIcon />
                    )}
                </IconButton>
                <IconButton>
                    <NotificationsOutlinedIcon />
                </IconButton>
                <IconButton>
                    <SettingsOutlinedIcon />
                </IconButton>
                <IconButton
                    onClick={handleClick}
                    aria-controls={open ? 'user-menu' : undefined}
                    aria-haspopup="true"
                    aria-expanded={open ? 'true' : undefined}
                >
                    <PersonOutlinedIcon />
                </IconButton>
                
                {/* Menu dropdown */}
                <Menu
                    id="user-menu"
                    anchorEl={anchorEl}
                    open={open}
                    onClose={handleClose}
                    MenuListProps={{
                        'aria-labelledby': 'user-button',
                    }}
                    PaperProps={{
                        sx: {
                            backgroundColor: colors.primary[400],
                            minWidth: 200,
                        }
                    }}
                >
                    {/* Hiển thị thông tin user */}
                    <Box sx={{ px: 2, py: 1 }}>
                        <Typography variant="body1" color={colors.grey[100]} fontWeight="bold">
                            {user.userName || user.email || 'User'}
                        </Typography>
                        <Typography variant="body2" color={colors.grey[300]} fontSize="12px">
                            {user.email}
                        </Typography>
                    </Box>
                    
                    <Divider sx={{ backgroundColor: colors.grey[700] }} />
                    
                    <MenuItem 
                        onClick={handleProfile}
                        sx={{
                            '&:hover': {
                                backgroundColor: colors.primary[300],
                            }
                        }}
                    >
                        <AccountCircleIcon sx={{ mr: 1, color: colors.grey[100] }} />
                        <Typography color={colors.grey[100]}>Thông tin cá nhân</Typography>
                    </MenuItem>
                    
                    <MenuItem 
                        onClick={handleLogout}
                        sx={{
                            '&:hover': {
                                backgroundColor: colors.primary[300],
                            }
                        }}
                    >
                        <LogoutIcon sx={{ mr: 1, color: colors.redAccent[500] }} />
                        <Typography color={colors.redAccent[500]}>Đăng xuất</Typography>
                    </MenuItem>
                </Menu>
            </Box>
        </Box>
    );
}

export default Topbar;