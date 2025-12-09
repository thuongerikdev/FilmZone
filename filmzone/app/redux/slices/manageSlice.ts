import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface ManageState {
  stats: { label: string; value: number | string; color: string }[];
  recentBookings: { id: number; customer: string; time: string; service: string; status: string }[];
}

const initialState: ManageState = {
  stats: [
    { label: "Lịch Đặt Hôm Nay", value: 12, color: "bg-blue-600" },
    { label: "Doanh Thu Ngày", value: "3,500,000 VNĐ", color: "bg-green-600" },
    { label: "Khách Hàng Mới", value: 5, color: "bg-purple-600" },
  ],
  recentBookings: [
    { id: 1, customer: "Nguyễn Văn A", time: "10:00 AM", service: "Cắt Tóc Nam", status: "Confirmed" },
    { id: 2, customer: "Trần Thị B", time: "11:30 AM", service: "Nhuộm Tóc", status: "Pending" },
    { id: 3, customer: "Lê Văn C", time: "2:00 PM", service: "Gội Đầu", status: "Confirmed" },
  ],
};

const manageSlice = createSlice({
  name: 'manage',
  initialState,
  reducers: {
    updateStats: (state, action: PayloadAction<{ label: string; value: number | string; color: string }[]>) => {
      state.stats = action.payload;
    },
    updateRecentBookings: (state, action: PayloadAction<{ id: number; customer: string; time: string; service: string; status: string }[]>) => {
      state.recentBookings = action.payload;
    },
  },
});

export const { updateStats, updateRecentBookings } = manageSlice.actions;
export default manageSlice.reducer;