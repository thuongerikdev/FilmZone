import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface ServiceState {
    services: { id: number; name: string; category: string; price: number; duration: string;  status: 'Active' | 'Inactive'} [];
    categoryFilter: string;
   
}

const initialState: ServiceState = {
    services: [
        { id: 1, name: "Cắt Tóc Nam", category: "Cắt Tóc", price: 100000, duration: "30 phút", status: "Active" },
        { id: 2, name: "Nhuộm Tóc", category: "Nhuộm", price: 300000, duration: "60 phút", status: "Active" },
        { id: 3, name: "Gội Đầu", category: "Gội", price: 50000, duration: "20 phút", status: "Active" },
        { id: 4, name: "Cạo Mặt", category: "Cạo", price: 70000, duration: "15 phút", status: "Inactive" },
        { id: 5, name: "Cắt Tóc Nữ", category: "Cắt Tóc", price: 150000, duration: "45 phút", status: "Active" },
    ],
    categoryFilter: "All",
   
};

const serviceManageSlice = createSlice({
    name: 'service',
    initialState,
    reducers: {
        updateServices: (state, action: PayloadAction<{ id: number; name: string; category: string; price: number; duration: string; status: 'Active' | 'Inactive' }[]>) => {
            state.services = action.payload;
        },
        setServiceCategoryFilter: (state, action: PayloadAction<string>) => {
            state.categoryFilter = action.payload;
        },
       
    },
});

export const { updateServices, setServiceCategoryFilter } = serviceManageSlice.actions;
export default serviceManageSlice.reducer;