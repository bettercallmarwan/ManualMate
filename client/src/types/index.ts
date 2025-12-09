export interface Product {
  id: number;
  name: string;
  description: string;
  manualPath?: string;
}

export interface CreateProductDto {
  name: string;
  description: string;
  manualPath?: string;
}

export interface ApiResponse<T> {
  data: T;
}

export interface ApiError {
  error: string;
}
