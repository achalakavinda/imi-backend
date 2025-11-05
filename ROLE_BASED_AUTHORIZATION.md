# Role-Based Authorization Implementation Guide

## Overview

The system now supports three distinct roles with hierarchical permissions:

### Roles Hierarchy

1. **Guest** - Limited access (read-only or basic features)
2. **User** - Full user features (create, read, update own data)
3. **Administrator** - Full system access (manage all data, control panel access)

## Seeded Default Admin Account

When the application starts (in Development mode), the following admin account is automatically created:

```
Email: admin@migratingassistant.com
Password: Admin@123456
Role: Administrator
```

**⚠️ IMPORTANT:** Change this password immediately in production!

## Registration Endpoints

### 1. Register as User (Default)

```http
POST /api/authentication/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Assigned Role:** `User`

### 2. Register as Guest

```http
POST /api/authentication/register-guest
Content-Type: application/json

{
  "email": "guest@example.com",
  "password": "SecurePass123!"
}
```

**Assigned Role:** `Guest`

### 3. Admin Users

- Admin users are **seeded** during database initialization
- Cannot be created via API registration
- Must be created manually or through seeding

## Using Roles in Endpoints

### Example 1: Protect Endpoint with Single Role

```csharp
public override void Map(RouteGroupBuilder group)
{
    // Only Administrators can access
    group.MapDelete("/{id:guid}", DeleteUser)
        .RequireAuthorization(Policies.RequireAdministratorRole);
}
```

### Example 2: Allow Multiple Roles (Hierarchical)

```csharp
public override void Map(RouteGroupBuilder group)
{
    // Users AND Administrators can access
    group.MapPost("/", CreateBooking)
        .RequireAuthorization(Policies.RequireUserRole);

    // Anyone authenticated (Guest, User, Administrator) can access
    group.MapGet("/", GetBookings)
        .RequireAuthorization(Policies.RequireGuestRole);
}
```

### Example 3: Different Access Levels for Same Resource

```csharp
public override void Map(RouteGroupBuilder group)
{
    // Public - anyone can view
    group.MapGet("/", GetServiceProviders)
        .AllowAnonymous();

    // Authenticated users can view details
    group.MapGet("/{id:guid}", GetServiceProvider)
        .RequireAuthorization(Policies.RequireGuestRole);

    // Only users can create
    group.MapPost("/", CreateServiceProvider)
        .RequireAuthorization(Policies.RequireUserRole);

    // Only admins can delete
    group.MapDelete("/{id:guid}", DeleteServiceProvider)
        .RequireAuthorization(Policies.RequireAdministratorRole);
}
```

## Policy Definitions

Located in `Domain/Constants/Policies.cs`:

```csharp
public const string RequireAdministratorRole = "RequireAdministratorRole";
// Allows: Administrator only

public const string RequireUserRole = "RequireUserRole";
// Allows: User OR Administrator

public const string RequireGuestRole = "RequireGuestRole";
// Allows: Guest OR User OR Administrator (any authenticated user)
```

## Suggested Entity Protection Pattern

### Public Entities (No Authentication Required)

- Job Listings (read-only)
- Service Provider List (read-only)

### Guest-Level Access (Basic Authentication)

- View own profile
- Browse available services
- Read documentation

### User-Level Access (Standard Features)

- **Bookings** - Create, view, update, cancel own bookings
- **Documents** - Upload, manage own documents
- **Job Applications** - Submit, track applications
- **Support Tickets** - Create, view own tickets
- **Payments** - Initiate, view own payment history
- **User Profiles** - Manage own profile

### Administrator-Level Access (Control Panel)

- **All Entity Management** - Full CRUD on all entities
- **User Management** - View all users, assign roles, disable accounts
- **Service Providers** - Verify, approve, manage providers
- **Support Tickets** - View all, assign, resolve tickets
- **Payments** - View all transactions, issue refunds
- **System Configuration** - Manage settings, audit logs

## Example: Protecting All Endpoints

Here's how to implement role-based protection across your entities:

### BookingsEndpoint.cs

```csharp
public override void Map(RouteGroupBuilder group)
{
    // Users can view their own bookings
    group.MapGet("/", GetBookings)
        .RequireAuthorization(Policies.RequireUserRole);

    // Users can create bookings
    group.MapPost("/", CreateBooking)
        .RequireAuthorization(Policies.RequireUserRole);

    // Users can update their own bookings
    group.MapPut("/{id:guid}", UpdateBooking)
        .RequireAuthorization(Policies.RequireUserRole);

    // Only admins can delete any booking
    group.MapDelete("/{id:guid}", DeleteBooking)
        .RequireAuthorization(Policies.RequireAdministratorRole);
}
```

### DocumentsEndpoint.cs

```csharp
public override void Map(RouteGroupBuilder group)
{
    // Users manage their own documents
    group.MapGet("/", GetDocuments)
        .RequireAuthorization(Policies.RequireUserRole);

    group.MapPost("/", CreateDocument)
        .RequireAuthorization(Policies.RequireUserRole);

    // Admins can verify documents
    group.MapPut("/{id:guid}/verify", VerifyDocument)
        .RequireAuthorization(Policies.RequireAdministratorRole);
}
```

### SupportTicketsEndpoint.cs

```csharp
public override void Map(RouteGroupBuilder group)
{
    // Users can create tickets
    group.MapPost("/", CreateSupportTicket)
        .RequireAuthorization(Policies.RequireUserRole);

    // Users can view their own tickets
    group.MapGet("/my-tickets", GetMyTickets)
        .RequireAuthorization(Policies.RequireUserRole);

    // Admins can view all tickets
    group.MapGet("/", GetAllSupportTickets)
        .RequireAuthorization(Policies.RequireAdministratorRole);

    // Admins can resolve tickets
    group.MapPut("/{id:guid}/resolve", ResolveSupportTicket)
        .RequireAuthorization(Policies.RequireAdministratorRole);
}
```

## Checking User Role in Handler Logic

Sometimes you need role-based logic inside handlers:

```csharp
public async Task<Results<Ok<IList<BookingDto>>, UnauthorizedHttpResult>> GetBookings(
    ISender sender,
    ICurrentUserService currentUserService,
    HttpContext httpContext)
{
    var isAdmin = httpContext.User.IsInRole(Roles.Administrator);

    if (isAdmin)
    {
        // Admins see all bookings
        return TypedResults.Ok(await sender.Send(new GetAllBookingsQuery()));
    }
    else
    {
        // Users see only their own bookings
        var userId = currentUserService.UserId;
        return TypedResults.Ok(await sender.Send(new GetUserBookingsQuery { UserId = userId }));
    }
}
```

## Testing with Different Roles

### 1. Login as Admin (Seeded Account)

```http
POST /api/authentication/login
Content-Type: application/json

{
  "email": "admin@migratingassistant.com",
  "password": "Admin@123456"
}
```

### 2. Register as User

```http
POST /api/authentication/register
Content-Type: application/json

{
  "email": "john.user@example.com",
  "password": "User@123456"
}
```

### 3. Register as Guest

```http
POST /api/authentication/register-guest
Content-Type: application/json

{
  "email": "jane.guest@example.com",
  "password": "Guest@123456"
}
```

### 4. Use Token in Requests

```http
GET /api/bookings
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Response Codes

- **200 OK** - Request successful
- **401 Unauthorized** - No token or invalid token
- **403 Forbidden** - Valid token but insufficient permissions
- **404 Not Found** - Resource doesn't exist or user doesn't have access

## Production Considerations

### 1. Change Default Admin Password

Update the seeded admin password or disable the seeding in production:

```csharp
// In ApplicationDbContextInitialiser.cs
if (app.Environment.IsDevelopment())
{
    await initialiser.SeedAsync();
}
```

### 2. Implement Password Reset

Add forgot password/reset password endpoints

### 3. Email Confirmation

Require email confirmation before allowing login

### 4. Two-Factor Authentication (2FA)

Add 2FA for administrator accounts

### 5. Audit Logging

Log all admin actions for compliance

### 6. Role Management Endpoints

Create admin endpoints to:

- Assign/remove roles from users
- Create custom roles
- View user permissions

## Summary

✅ **Implemented:**

- Three-tier role system (Guest, User, Administrator)
- Role-based authorization policies
- Admin account seeding on app startup
- Separate registration endpoints for User and Guest roles
- Token generation includes user roles as claims

✅ **Ready to Use:**

- Protect any endpoint with `.RequireAuthorization(Policies.XYZ)`
- Check roles in business logic with `httpContext.User.IsInRole(Roles.XYZ)`
- Default admin account for immediate control panel access

✅ **Flexible:**

- Easy to add more roles
- Hierarchical permission system
- Can protect individual endpoints or entire route groups
