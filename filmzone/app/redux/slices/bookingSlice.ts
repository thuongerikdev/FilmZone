import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Booking } from '../../app/manage/booking/booking';

interface BookingState {
  bookings: Booking[];
  statusFilter: string;
  dateFilter: string;
}

const initialState: BookingState = {
  bookings: [],
  statusFilter: 'All',
  dateFilter: '',
};

const bookingSlice = createSlice({
  name: 'booking',
  initialState,
  reducers: {
    updateBookings: (state, action: PayloadAction<Booking[]>) => {
      state.bookings = action.payload;
    },
    setStatusFilter: (state, action: PayloadAction<string>) => {
      state.statusFilter = action.payload;
    },
    setDateFilter: (state, action: PayloadAction<string>) => {
      state.dateFilter = action.payload;
    },
  },
});

export const { updateBookings, setStatusFilter, setDateFilter } = bookingSlice.actions;
export default bookingSlice.reducer;