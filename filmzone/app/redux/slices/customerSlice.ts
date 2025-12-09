import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Customer } from '../../app/manage/booking/booking'; // Adjust path as needed

interface CustomerState {
  customers: Customer[];
  statusFilter: string;
  typeFilter: string;
}

const initialState: CustomerState = {
  customers: [],
  statusFilter: 'All',
  typeFilter: 'All',
};

const customerSlice = createSlice({
  name: 'customer',
  initialState,
  reducers: {
    updateCustomers(state, action: PayloadAction<Customer[]>) {
      state.customers = action.payload;
    },
    setStatusFilter(state, action: PayloadAction<string>) {
      state.statusFilter = action.payload;
    },
    setTypeFilter(state, action: PayloadAction<string>) {
      state.typeFilter = action.payload;
    },
  },
});

export const { updateCustomers, setStatusFilter, setTypeFilter } = customerSlice.actions;
export default customerSlice.reducer;