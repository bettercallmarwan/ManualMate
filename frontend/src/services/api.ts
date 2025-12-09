import axios from 'axios';
import type { Product, CreateProductDto, ApiResponse, ApiError } from '../types';

// Use proxy in development, or full URL in production
// In development, always use the proxy to avoid CORS issues
const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Products API
export const productApi = {
  getAll: async (): Promise<Product[]> => {
    const response = await api.get<ApiResponse<Product[]>>('/product');
    return response.data.data;
  },

  getById: async (id: number): Promise<Product> => {
    const response = await api.get<ApiResponse<Product>>(`/product/${id}`);
    return response.data.data;
  },

  create: async (product: CreateProductDto): Promise<CreateProductDto> => {
    const response = await api.post<ApiResponse<CreateProductDto>>('/product', product);
    return response.data.data;
  },

  update: async (id: number, product: CreateProductDto): Promise<Product> => {
    const response = await api.put<ApiResponse<Product>>(`/product/${id}`, product);
    return response.data.data;
  },

  delete: async (id: number): Promise<boolean> => {
    const response = await api.delete<ApiResponse<boolean>>(`/product/${id}`);
    return response.data.data;
  },

  uploadManual: async (id: number, file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post<ApiResponse<string>>(
      `/product/upload-manual/${id}`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data.data;
  },

  processManual: async (id: number): Promise<boolean> => {
    const response = await api.get<ApiResponse<boolean>>(`/product/process-manual/${id}`);
    return response.data.data;
  },

  ask: async (productId: number, question: string): Promise<string> => {
    const response = await api.get<ApiResponse<string>>(
      `/product/ask/${productId}`,
      {
        params: { question },
      }
    );
    return response.data.data;
  },
};

export default api;
