import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, TrackingUpdate, AddTrackingUpdateRequest } from '../models/api.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TrackingService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/tracking`;

  getHistory(shipmentId: number): Observable<ApiResponse<TrackingUpdate[]>> {
    return this.http.get<ApiResponse<TrackingUpdate[]>>(`${this.apiUrl}/${shipmentId}/history`);
  }

  addUpdate(shipmentId: number, data: AddTrackingUpdateRequest): Observable<ApiResponse<TrackingUpdate>> {
    return this.http.post<ApiResponse<TrackingUpdate>>(`${this.apiUrl}/${shipmentId}/updates`, data);
  }
}
