import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable } from 'rxjs';
import { ApiResponse, AuthRequest, RegisterRequest, AuthResponse } from '../models/api.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly tokenKey = 'auth_token';
  private readonly userKey = 'auth_user';

  login(credentials: AuthRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/login`, credentials);
  }

  register(data: RegisterRequest): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/register`, data);
  }

  getUsers(): Observable<ApiResponse<AuthResponse[]>> {
    return this.http.get<ApiResponse<AuthResponse[]>>(`${this.apiUrl}/users`);
  }

  setToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.tokenKey, token);
    }
  }

  getToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem(this.tokenKey);
    }
    return null;
  }

  setUser(user: AuthResponse): void {
    if (isPlatformBrowser(this.platformId)) {
      const role = this.getRoleFromToken();
      const userWithRole = { ...user, role };
      localStorage.setItem(this.userKey, JSON.stringify(userWithRole));
    }
  }

  getUser(): (AuthResponse & { role?: string }) | null {
    if (isPlatformBrowser(this.platformId)) {
      const data = localStorage.getItem(this.userKey);
      return data ? JSON.parse(data) : null;
    }
    return null;
  }

  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.userKey);
    }
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    const user = this.getUser();
    return user?.role === 'Admin';
  }

  isCustomer(): boolean {
    const user = this.getUser();
    return user?.role === 'Customer';
  }

  getUserRole(): string {
    const user = this.getUser();
    return user?.role ?? 'Customer';
  }

  private getRoleFromToken(): string {
    const token = this.getToken();
    if (!token) return 'Customer';

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const roleClaim =
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
        payload['role'] ??
        payload['roles'];

      if (Array.isArray(roleClaim)) {
        return roleClaim[0] || 'Customer';
      }
      return roleClaim || 'Customer';
    } catch {
      return 'Customer';
    }
  }
}
