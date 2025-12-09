import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Invoice } from '../../app/manage/booking/booking';

interface InvoiceState {
  invoices: Invoice[];
  statusFilter: string;
  dateFilter: string;
}

const initialState: InvoiceState = {
  invoices: [],
  statusFilter: 'All',
  dateFilter: '',
};

const invoiceSlice = createSlice({
  name: 'invoice',
  initialState,
  reducers: {
    updateInvoices(state, action: PayloadAction<Invoice[]>) {
      state.invoices = action.payload;
    },
    setStatusFilter(state, action: PayloadAction<string>) {
      state.statusFilter = action.payload;
    },
    setDateFilter(state, action: PayloadAction<string>) {
      state.dateFilter = action.payload;
    },
  },
});

export const { updateInvoices, setStatusFilter, setDateFilter } = invoiceSlice.actions;
export default invoiceSlice.reducer;