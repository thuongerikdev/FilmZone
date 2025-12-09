import { configureStore } from '@reduxjs/toolkit';
import manageReducer from './slices/manageSlice';
import bookingReducer from './slices/bookingSlice';
import serviceReducer from './slices/serviceSlice';
import invoiceReducer from './slices/invoiceSlice';
import appointmentReducer from './slices/appointmentSlice'
import customerReducer from './slices/customerSlice'

export const store = configureStore({
  reducer: {
    manage: manageReducer,
    booking: bookingReducer,
    service: serviceReducer,
    invoice: invoiceReducer,
    appointment: appointmentReducer,
    customer: customerReducer
  },
});

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;