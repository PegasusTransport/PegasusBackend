# Pegasus Transport - Taxi Booking Platform

A full-stack taxi booking and management system for Pegasus Transport AB, operating in Stockholm, Uppsala, and Arlanda regions. The platform provides seamless booking experiences for customers, efficient job management for drivers, and comprehensive administrative tools.

## 🚖 Overview

Pegasus is a modern taxi booking platform that enables:
- **Customers**: Easy booking, real-time price estimates, and trip tracking
- **Drivers**: Job acceptance, route management, and receipt generation
- **Administrators**: System configuration, driver management, and booking oversight

## 🏗️ Architecture

### Backend Stack
- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT with 2FA (Two-Factor Authentication)
- **APIs**: RESTful with Swagger/Scalar documentation
- **External Services**: 
  - Google Maps API (routing, geocoding, autocomplete)
  - Azure OpenAI (AI chatbot)
  - Mailjet (email services)
  - BilUppgifter API (vehicle information)
- **PDF Generation**: QuestPDF
- **Deployment**: Docker + Render

### Frontend Stack
- **Framework**: Vue 3 with TypeScript
- **UI Library**: Headless UI + Heroicons
- **Styling**: Tailwind CSS
- **State Management**: Pinia stores
- **Routing**: Vue Router

## ✨ Key Features

### Customer Features
- 🔍 Address autocomplete with Google Maps integration
- 💰 Real-time price estimation
- 📧 Email confirmation with secure token verification
- 🚗 Optional stops (up to 2 intermediate locations)
- 📱 Flight number tracking for airport pickups
- 📝 Booking history and management
- ✏️ Update bookings (24 hours before pickup)
- ❌ Cancel bookings with policy enforcement
- 👁️ Track driver information and vehicle details

### Driver Features
- 📋 View available bookings
- ✅ Accept/reject jobs
- 🔄 Reassign bookings to other drivers
- ✔️ Complete trips
- 🧾 Generate and send PDF receipts via email
- 👤 Profile management with vehicle registration
- 🚘 Automatic vehicle data fetching via license plate

### Admin Features
- 👥 Driver management (create, update, soft delete)
- 💵 Dynamic taxi pricing configuration
  - Start price
  - Per kilometer rate
  - Per minute rate
  - Zone pricing
- 📊 Comprehensive booking oversight
- ⚙️ System settings management

### Technical Features
- 🔐 JWT authentication with HttpOnly cookies
- 🔑 Two-Factor Authentication (2FA) via email
- 🔄 Refresh token rotation
- 🔒 Role-based authorization (Customer, Driver, Admin)
- ⚡ Rate limiting for security
- 🔁 Idempotency handling for duplicate requests
- ✅ FluentValidation for request validation
- 🎯 Optimistic concurrency control
- 🗺️ Google Maps integration for routing and geocoding
- 🤖 AI-powered customer support chatbot
- 📧 Email notifications with Mailjet templates
- 📄 Professional PDF receipt generation
- 🌐 CORS configuration for frontend integration

## 📦 Project Structure

```
.
├── PegasusBackend/
│   ├── Controllers/          # API endpoints
│   ├── Services/             # Business logic layer
│   │   ├── Implementations/
│   │   └── Interfaces/
│   ├── Repositorys/          # Data access layer
│   ├── Models/               # Entity models
│   ├── DTOs/                 # Data transfer objects
│   ├── Configurations/       # Service configurations
│   ├── Helpers/              # Utility classes
│   ├── Data/                 # DbContext
│   ├── Responses/            # API response wrappers
│   └── Fonts/                # PDF fonts
├── PegasusBackend.Tests/     # Unit tests
└── pegasus-frontend/
    ├── src/
    │   ├── components/       # Vue components
    │   ├── pages/            # Page views
    │   ├── stores/           # Pinia state management
    │   ├── services/         # API services
    │   └── types/            # TypeScript definitions
    └── public/
```

## 🗄️ Database Schema

### Core Entities
- **Users**: Customer and driver accounts (ASP.NET Identity)
- **Drivers**: Driver profiles with vehicle assignments
- **Cars**: Vehicle information
- **Bookings**: Trip bookings with status tracking
- **TaxiSettings**: Dynamic pricing configuration
- **IdempotencyRecords**: Duplicate request prevention

### Booking Status Flow
```
PendingEmailConfirmation → Confirmed → [Cancelled | Completed]
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL
- Node.js 18+ (for frontend)
- Docker (optional, for containerization)

### Backend Setup

1. **Clone the repository**
```bash
git clone <repository-url>
cd PegasusBackend
```

2. **Configure User Secrets**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=pegasus;Username=postgres;Password=yourpassword"
dotnet user-secrets set "JwtSetting:Key" "your-secret-key-min-32-chars"
dotnet user-secrets set "GoogleMaps:ApiKey" "your-google-maps-api-key"
dotnet user-secrets set "BilUppgifterApiKey" "your-biluppgifter-api-key"
dotnet user-secrets set "AzureOpenAI:Endpoint" "your-azure-openai-endpoint"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-azure-openai-key"
dotnet user-secrets set "Mailjet:ApiKey" "your-mailjet-api-key"
dotnet user-secrets set "Mailjet:SecretKey" "your-mailjet-secret-key"
```

3. **Install dependencies**
```bash
dotnet restore
```

4. **Apply migrations**
```bash
dotnet ef database update
```

5. **Run the application**
```bash
dotnet run
```

The API will be available at `https://localhost:5282`

### Frontend Setup

1. **Navigate to frontend directory**
```bash
cd pegasus-frontend
```

2. **Install dependencies**
```bash
npm install
```

3. **Configure environment variables**
Create a `.env` file:
```
VITE_API_URL=https://localhost:5282/api
```

4. **Run development server**
```bash
npm run dev
```

### Docker Deployment

```bash
docker build -t pegasusbackend:latest .
docker run -d -p 8080:8080 -e PORT=8080 pegasusbackend:latest
```

## 🔐 Authentication Flow

1. **Login**: User submits credentials
2. **2FA**: System sends OTP code via email
3. **Verification**: User submits OTP code
4. **Token Issuance**: JWT access token (15 min) + refresh token (7 days)
5. **Token Storage**: Cookies (HttpOnly, Secure, SameSite=None)
6. **Token Refresh**: Automatic refresh before expiration
7. **Logout**: Token invalidation and cookie clearing

## 📧 Email Integration

The system uses Mailjet for email delivery:
- **Booking Confirmations**: Secure token-based verification
- **2FA Codes**: OTP for login authentication
- **Receipts**: PDF attachments with trip details
- **Password Reset**: Secure token-based reset links

## 🗺️ Google Maps Integration

- **Autocomplete**: Real-time address suggestions
- **Geocoding**: Address to coordinates conversion
- **Reverse Geocoding**: Coordinates to address lookup
- **Routing**: Distance and duration calculations
- **Directions**: Multi-stop route optimization

## 🤖 AI Chatbot

Powered by Azure OpenAI, the chatbot provides:
- Professional customer service in multiple languages
- Booking assistance and information
- Pricing information retrieval
- General taxi service inquiries
- Context-aware responses with cached pricing data

## 🧾 Receipt Generation

Professional PDF receipts include:
- Company branding and logo
- Trip details (pickup, stops, destination)
- Driver and vehicle information
- Pricing breakdown
- Time and distance metrics
- Custom fonts (Open Sans, Oswald)

## 🔒 Security Features

- **JWT Authentication**: Secure token-based auth
- **2FA**: Email-based two-factor authentication
- **HttpOnly Cookies**: XSS protection
- **Rate Limiting**: DDoS and brute-force protection
- **CORS**: Configured for production frontend
- **Idempotency**: Duplicate request prevention
- **Password Hashing**: ASP.NET Identity with PBKDF2
- **Secure Password Reset**: Token-based with expiration
- **Role-Based Authorization**: Fine-grained access control

## 📊 API Documentation

API documentation is available at:
- **Swagger UI**: `/swagger`
- **Scalar**: `/scalar/v1` (development only)

### Main Endpoints

#### Authentication
- `POST /api/Auth/Login` - Initiate login with 2FA
- `POST /api/Auth/VerifyTwoFA` - Complete 2FA verification
- `POST /api/Auth/RefreshToken` - Refresh access token
- `POST /api/Auth/Logout` - Logout and invalidate tokens

#### Bookings
- `POST /api/Booking/BookingPreview` - Get price estimate
- `POST /api/Booking/CreateBooking` - Create new booking
- `GET /api/Booking/ConfirmBooking/{token}` - Confirm via email
- `GET /api/Booking/MyBookings` - Get user's bookings
- `PUT /api/Booking/UpdateBooking` - Update booking
- `DELETE /api/Booking/CancelBooking/{id}` - Cancel booking

#### Driver
- `POST /api/Driver/CreateDriver` - Register new driver
- `GET /api/Driver/AvailableBookings` - View available jobs
- `POST /api/Driver/AcceptBooking/{id}` - Accept a job
- `POST /api/Driver/CompleteBooking/{id}` - Complete trip
- `POST /api/Driver/SendReceipt` - Generate and email receipt

#### Admin
- `POST /api/Admin/CreateDriver` - Create driver account
- `PUT /api/Admin/UpdateTaxiPrice` - Update pricing
- `GET /api/Admin/GetDrivers` - List all drivers
- `GET /api/Admin/GetAllBookings` - View all bookings

## 🧪 Testing

Run the test suite:
```bash
dotnet test PegasusBackend.sln --configuration Release
```

Tests include:
- Service layer unit tests
- Map service integration tests
- Authentication flow tests
- Booking validation tests

## 📝 Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSetting": {
    "Issuer": "PegasusBackend",
    "Audience": "PegasusFrontend",
    "AccessTokenExpire": 15,
    "RefreshTokenExpire": 7
  }
}
```

### Booking Rules
```json
{
  "BookingRules": {
    "MinHoursBeforePickupForChange": 24,
    "MinMinutesBeforePickup": 10
  }
}
```

### Pagination Defaults
```json
{
  "Pagination": {
    "DefaultPage": 1,
    "DefaultPageSize": 10,
    "MaxPageSize": 200
  }
}
```

## 🚦 CI/CD Pipeline

GitHub Actions workflow:
1. **Test**: Run all unit tests
2. **Build**: Create Docker image
3. **Deploy**: Trigger Render deployment (on main branch)

## 🌐 Deployment

### Production URLs
- **Backend**: `https://pegasusbackend.onrender.com`
- **Frontend**: `https://portal.pegasustransport.se`
- **API Docs**: `https://pegasusbackend.onrender.com/swagger`

### Environment Variables (Production)
- `ConnectionStrings__DefaultConnection`
- `JwtSetting__Key`
- `GoogleMaps__ApiKey`
- `BilUppgifterApiKey`
- `AzureOpenAI__Endpoint`
- `AzureOpenAI__ApiKey`
- `AzureOpenAI__DeploymentName`
- `Mailjet__ApiKey`
- `Mailjet__SecretKey`
- `Mailjet__SenderEmail`
- `Mailjet__SenderName`
- `PORT` (for Render deployment)

## 📚 Dependencies

### Backend
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.EntityFrameworkCore
- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.AspNetCore.Authentication.JwtBearer
- System.IdentityModel.Tokens.Jwt
- FluentValidation.AspNetCore
- QuestPDF
- Azure.AI.OpenAI
- Mailjet.Api
- Swashbuckle.AspNetCore
- Scalar.AspNetCore

### Frontend
- Vue 3
- TypeScript
- Tailwind CSS
- Headless UI
- Heroicons
- Pinia
- Vue Router

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is proprietary software owned by Pegasus Transport AB.

## 👥 Team

Developed for Pegasus Transport AB

## 📞 Support

- **Email**: info@pegasustransport.se
- **Phone**: 08-123 45 67
- **Service Areas**: Stockholm, Uppsala, Arlanda

---

**Note**: This application requires proper configuration of external API keys for full functionality. Ensure all services (Google Maps, Azure OpenAI, Mailjet, BilUppgifter) are properly configured before deployment.
