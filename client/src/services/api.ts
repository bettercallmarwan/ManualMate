import axios from 'axios';
import type { Item, CreateItemDto, ApiResponse, ApiError } from '../types';

// Use proxy in development, or full URL in production
// In development, always use the proxy to avoid CORS issues
const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Items API
export const itemApi = {
  getAll: async (): Promise<Item[]> => {
    const response = await api.get<ApiResponse<Item[]>>('/item');
    return response.data.data;
  },

  getById: async (id: number): Promise<Item> => {
    const response = await api.get<ApiResponse<Item>>(`/item/${id}`);
    return response.data.data;
  },

  create: async (item: CreateItemDto): Promise<CreateItemDto> => {
    const response = await api.post<ApiResponse<CreateItemDto>>('/item', item);
    return response.data.data;
  },

  update: async (id: number, item: CreateItemDto): Promise<Item> => {
    const response = await api.put<ApiResponse<Item>>(`/item/${id}`, item);
    return response.data.data;
  },

  delete: async (id: number): Promise<boolean> => {
    const response = await api.delete<ApiResponse<boolean>>(`/item/${id}`);
    return response.data.data;
  },

  uploadFile: async (id: number, file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post<ApiResponse<string>>(
      `/item/upload-file/${id}`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data.data;
  },

  processFile: async (id: number): Promise<boolean> => {
    const response = await api.get<ApiResponse<boolean>>(`/item/process-file/${id}`);
    return response.data.data;
  },

  ask: async (itemId: number, question: string): Promise<string> => {
    const response = await api.get<ApiResponse<string>>(
      `/item/ask/${itemId}`,
      {
        params: { question },
      }
    );
    return response.data.data;
  },

  deleteEmbeddings: async (id: number): Promise<boolean> => {
    const response = await api.delete<ApiResponse<boolean>>(`/item/embeddings/${id}`);
    return response.data.data;
  },
};

export default api;
