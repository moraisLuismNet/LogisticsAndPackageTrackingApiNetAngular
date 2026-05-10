import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { ShipmentsListComponent } from './features/shipments/shipments-list/shipments-list';
import { AdminUsersComponent } from './features/admin/users/users';
import { TrackingComponent } from './features/tracking/tracking/tracking';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent, title: 'Sign In - ShipTrack' },
  { path: 'register', component: RegisterComponent, title: 'Create Account - ShipTrack' },
  { path: 'shipments', component: ShipmentsListComponent, canActivate: [authGuard], title: 'Shipments - ShipTrack' },
  { path: 'tracking/:id', component: TrackingComponent, canActivate: [authGuard], title: 'Shipment Details - ShipTrack' },
  {
    path: 'admin',
    canActivate: [adminGuard],
    children: [
      { path: 'users', component: AdminUsersComponent, title: 'Users - ShipTrack' }
    ]
  },
  { path: '', redirectTo: 'shipments', pathMatch: 'full' },
  { path: '**', redirectTo: 'shipments' }
];
