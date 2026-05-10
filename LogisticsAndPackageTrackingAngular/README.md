# LogisticsAndPackageTrackingAngular

Angular 20 frontend for the Logistics and Package Tracking API. Communicates with the backend at `http://localhost:5096/api/`.

## Features
- **Authentication**: Login/register with JWT, role-based access (admin/customer).
- **Shipment List**: View shipments (filtered by logged-in user's email for customers; admins see all). Inline status editing for admins. "+ New Shipment" button visible only to admins.
- **Tracking Page**: Search by tracking number (public). View shipment details, 3-step progress bar (Pending → In Transit → Delivered) with numbered red circles, Leaflet route map with color-coded markers.
- **Admin Users**: User management page at `/admin/users`.
- **Dashboard**: Overview with stats, recent shipments, and quick links.

## Technologies Used
- **Angular 20** (standalone components, signals, control flow syntax)
- **Tailwind CSS** for styling
- **Leaflet** (via CDN) for map visualization
- **Angular Router** with lazy loading and guards (auth, admin)
- **HttpClient** + auth interceptor for JWT-based API communication

## Screenshots

<kbd><img src="img/01.png" width="80%" height="90%" alt="LogisticsAndPackageTrackingAngular_01"></kbd>  <kbd><img src="img/02.png" width="80%" height="90%" alt="LogisticsAndPackageTrackingAngular_02"></kbd>  <kbd><img src="img/03.png" width="80%" height="90%" alt="LogisticsAndPackageTrackingAngular_03"></kbd>
<kbd><img src="img/04.png" width="80%" height="90%" alt="LogisticsAndPackageTrackingAngular_04"></kbd>  <kbd><img src="img/05.png" width="80%" height="90%" alt="LogisticsAndPackageTrackingAngular_05"></kbd>  <kbd><img src="img/06.png" width="80%" height="90%" alt="LogisticsAndPackageTrackingAngular_06"></kbd>

## Requirements
- [Node.js 20+](https://nodejs.org/)
- [LogisticsAndPackageTrackingApiNet](https://github.com/moraisLuismNet/LogisticsAndPackageTrackingApiNet) backend running on `http://localhost:5096`

## Project Structure

```
src/
├── app/
│   ├── core/
│   │   ├── guards/          # authGuard, adminGuard
│   │   ├── models/          # API interfaces (Shipment, TrackingUpdate, Auth)
│   │   ├── services/        # AuthService, ShipmentService, TrackingService
│   │   └── interceptors/    # JWT auth interceptor
│   ├── features/
│   │   ├── auth/            # Login, Register components
│   │   ├── dashboard/       # Dashboard component
│   │   ├── shipments/       # Shipments list component (admin: inline status + create)
│   │   ├── tracking/        # Tracking page with map, progress bar, shipment details
│   │   └── admin/           # Admin users component
│   └── shared/              # Card, Button, Input, LoadingSpinner, StatusBadge components
├── assets/                  # Static assets (images, etc.)
├── environments/            # API URL configuration
├── index.html               # Leaflet CSS/JS CDN links
└── styles.css               # Tailwind directives
```

## Configuration

Update `src/environments/environment.ts` if needed:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5096/api'
};
```

## Installation

```bash
npm install
npm run dev
```

> **Note**: If `npm install` fails with "Invalid Version", run `npm install --legacy-peer-deps` or use a fresh `node_modules` + `package-lock.json`.

## API Endpoints Used

| Method | Endpoint | Component |
|--------|----------|-----------|
| POST | `/api/auth/login` | LoginComponent |
| POST | `/api/auth/register` | RegisterComponent |
| GET | `/api/auth/users` | AdminUsersComponent, ShipmentsListComponent |
| GET | `/api/shipments` | ShipmentsListComponent, DashboardComponent |
| GET | `/api/shipments/{trackingNumber}` | TrackingComponent |
| PUT | `/api/shipments/{trackingNumber}/status` | ShipmentsListComponent (admin) |

## Routes

| Path | Component | Guard |
|------|-----------|-------|
| `/login` | LoginComponent | — |
| `/register` | RegisterComponent | — |
| `/shipments` | ShipmentsListComponent | authGuard |
| `/tracking/:id` | TrackingComponent | authGuard |
| `/tracking` | TrackingComponent (search) | authGuard |
| `/admin/users` | AdminUsersComponent | adminGuard |
| `/` | redirect to `/shipments` | — |

## UI Notes
- **"+ New Shipment" button** only appears for admin users.
- **Progress bar** shows 3 steps: Pending (1), In Transit (2), Delivered (3). Only the current step's circle is highlighted in red.
- **Map** displays origin (marker or red circle), destination (marker or red circle), and a current position marker from the most recent tracking update. For InTransit status, a red circleMarker is shown at the route midpoint.
- **Login / Register** buttons have extra top margin for better spacing from the password field.
- **"Back" link** on the tracking page navigates to `/tracking` instead of "Track Another" button.

---
[DeepWiki moraisLuismNet/LogisticsAndPackageTrackingAngular](https://deepwiki.com/moraisLuismNet/LogisticsAndPackageTrackingAngular)
