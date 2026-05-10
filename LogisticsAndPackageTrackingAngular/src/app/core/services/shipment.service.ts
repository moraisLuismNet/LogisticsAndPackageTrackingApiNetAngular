import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, Shipment, CreateShipmentRequest, UpdateShipmentStatusRequest } from '../models/api.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ShipmentService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/shipments`;

  getAll(): Observable<ApiResponse<Shipment[]>> {
    return this.http.get<ApiResponse<Shipment[]>>(this.apiUrl);
  }

  getByTrackingNumber(trackingNumber: string): Observable<ApiResponse<Shipment>> {
    return this.http.get<ApiResponse<Shipment>>(`${this.apiUrl}/${trackingNumber}`);
  }

  create(data: CreateShipmentRequest): Observable<ApiResponse<Shipment>> {
    return this.http.post<ApiResponse<Shipment>>(this.apiUrl, data);
  }

  updateStatus(trackingNumber: string, data: UpdateShipmentStatusRequest): Observable<ApiResponse<Shipment>> {
    return this.http.put<ApiResponse<Shipment>>(`${this.apiUrl}/${trackingNumber}/status`, data);
  }
}
