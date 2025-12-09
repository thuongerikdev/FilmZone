import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Appointment } from '../../app/manage/booking/booking';

interface AppointmentState {
  appointments: Appointment[];
  statusFilter: string;
  dateFilter: string;
}

const initialState: AppointmentState = {
  appointments: [],
  statusFilter: 'All',
  dateFilter: '',
};

const appointmentSlice = createSlice({
  name: 'appointment',
  initialState,
  reducers: {
    updateAppointments: (state, action: PayloadAction<Appointment[]>) => {
      state.appointments = action.payload;
    },
    setStatusFilter: (state, action: PayloadAction<string>) => {
      state.statusFilter = action.payload;
    },
    setDateFilter: (state, action: PayloadAction<string>) => {
      state.dateFilter = action.payload;
    },
  },
});

export const { updateAppointments, setStatusFilter, setDateFilter } = appointmentSlice.actions;
export default appointmentSlice.reducer;