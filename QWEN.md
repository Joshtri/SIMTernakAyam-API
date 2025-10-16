# SIMTernakAyam - Chicken Farming Management System

## Project Overview
SIMTernakAyam is a comprehensive chicken farming management system built with .NET Core. This system helps manage various aspects of chicken farming operations with role-based access control (Petugas, Operator, Pemilik).

## Current Features
- JWT Authentication and Authorization
- Role-based dashboard system with different views for Petugas, Operator, and Pemilik
- Multiple modules: Ayam (Chickens), Kandang (Cages), Panen (Harvest), Biaya (Expenses), Vaksin (Vaccines), Pakan (Feed), Mortalitas (Mortality), Operasional (Operations)
- Extensive dashboard with charting capabilities (revenue vs expenses, mortality trends, production performance, feed consumption, etc.)
- PostgreSQL database with Entity Framework Core
- Swagger API documentation

## Enhancement Ideas
Here are specific enhancements that could improve your system:

### 1. **Notification System**
   - Implement real-time notifications for critical events (high mortality, vaccination due, feed running low)
   - Email/SMS alerts for farmers
   - In-app notification center

### 2. **Inventory Management Enhancement**
   - Track feed supplies with low-stock alerts
   - Monitor medication/vaccine inventory
   - Equipment maintenance scheduling

### 3. **Advanced Analytics & Reporting**
   - Predictive analytics for optimal farming
   - AI-powered suggestions for improving yield
   - Advanced forecasting for expenses and revenue
   - Custom report generation and export (PDF, Excel)

### 4. **Mobile-Friendly Interface**
   - Responsive design optimization
   - Progressive Web App (PWA) capabilities for offline use
   - Mobile-optimized forms for field data entry

### 5. **Data Import/Export Features**
   - Bulk import for initial data setup
   - Data backup and synchronization
   - Integration with external systems

### 6. **Enhanced User Experience**
   - Interactive farm layout visualization
   - Batch operations for common tasks
   - Quick action dashboard widgets
   - Personalized user preferences

### 7. **Security & Compliance**
   - Two-factor authentication
   - Audit logging for all operations
   - Data encryption for sensitive information
   - GDPR compliance features

### 8. **API Improvements**
   - Rate limiting for API endpoints
   - Caching for frequently accessed data
   - WebSocket support for real-time updates
   - API versioning

## Technical Architecture
- .NET 8.0 Web API
- Entity Framework Core with PostgreSQL (Npgsql)
- JWT Authentication
- Repository and Service patterns
- Role-based authorization
- Swagger/OpenAPI documentation
- BCrypt for password hashing

## Development Guidelines
- Follow .NET coding standards
- Use meaningful variable and method names
- Implement proper error handling and logging
- Add unit tests for critical functionality
- Document API endpoints with XML comments
- Follow SOLID principles and clean architecture patterns

## Getting Started
1. Clone the repository
2. Open the solution file (SIMTernakAyam.sln)
3. Update the connection string in appsettings.json to your PostgreSQL instance
4. Run database migrations: `dotnet ef database update`
5. Build and run the application
6. Access the API documentation at `/swagger` endpoint