export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[] | null;
}

export interface AuthRequest {
  mail: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  mail: string;
  address: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  email: string;
  fullName: string;
  mail: string;
  address?: string;
  role?: string;
}

export enum ShipmentStatus {
  Pending = 'Pending',
  InTransit = 'InTransit',
  Delivered = 'Delivered',
  Cancelled = 'Cancelled'
}

export interface Shipment {
  id: number;
  trackingNumber: string;
  mail: string;
  originAddress: string;
  originLatitude: number;
  originLongitude: number;
  receiverName: string;
  destinationAddress: string;
  destinationLatitude: number;
  destinationLongitude: number;
  status: ShipmentStatus;
  weight: number;
  trackingUpdates: TrackingUpdate[];
  createdAt: string;
  updatedAt: string | null;
}

export interface TrackingUpdate {
  id: number;
  location: string;
  description: string;
  latitude: number;
  longitude: number;
}

export interface CreateShipmentRequest {
  mail: string;
  receiverName: string;
  destinationAddress: string;
  weight: number;
}

export interface UpdateShipmentStatusRequest {
  status: ShipmentStatus;
}

export interface AddTrackingUpdateRequest {
  description: string;
}
