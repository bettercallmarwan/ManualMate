export interface Item {
  id: number;
  name: string;
  description: string;
  filePath?: string;
}

export interface CreateItemDto {
  name: string;
  description: string;
  filePath?: string;
}

export interface ApiResponse<T> {
  data: T;
}

export interface ApiError {
  error: string;
}
