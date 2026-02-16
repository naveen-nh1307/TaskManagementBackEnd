# Task Management System - Backend API

A role-based Task Management System built with .NET 8 MVC, Entity Framework Core, and PostgreSQL with JWT authentication.

## Project Structure

```
TaskManagementSystem/
├── Controllers/          # API Controllers
│   ├── AuthController.cs
│   ├── TasksController.cs
│   ├── TeamsController.cs
│   └── UsersController.cs
├── Data/                 # Database Context
│   └── ApplicationDbContext.cs
├── Models/
│   ├── Entities/         # Domain Models
│   └── DTOs/             # Data Transfer Objects
├── Services/
│   ├── Interfaces/       # Service Interfaces
│   └── Implementations/  # Service Implementations
├── Helpers/              # Utility Classes
│   └── JwtTokenHelper.cs
└── Migrations/           # EF Core Migrations
```

## Features

### Authentication & Authorization
- JWT-based authentication
- Role-based access control (Admin, Manager, User)
- BCrypt password hashing
- Token expiration: 24 hours

### Core Functionalities
- **Users**: Registration, login, role management
- **Tasks**: CRUD operations, status tracking (ToDo, InProgress, Done)
- **Teams**: Team creation, member management
- **Comments**: Task collaboration via comments
- **Notifications**: Mock email notifications for task assignment and status changes

## Prerequisites

- .NET SDK 8.0 or higher
- SQL Server 2019+ (or SQL Server Express)
- IDE (Visual Studio, VS Code, or Rider)

## Setup Instructions

### 1. Database Configuration

Update `appsettings.json` with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaskManagementDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Alternative: SQL Server Authentication**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaskManagementDB;User Id=sa;Password=your_password;TrustServerCertificate=True"
  }
}
```

### 2. Run Migrations

```bash
# Create the database and apply migrations
dotnet ef database update
```

This will create all tables and seed the default admin user:
- **Email**: admin@taskmanagement.com
- **Password**: Admin@123

### 3. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login user | No |
| GET | `/api/auth/me` | Get current user info | Yes |

### Tasks

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/tasks` | Get all tasks (filtered by role) | All |
| GET | `/api/tasks/{id}` | Get task by ID | All |
| POST | `/api/tasks` | Create task | Admin, Manager |
| PATCH | `/api/tasks/{id}/status` | Update task status | All |
| DELETE | `/api/tasks/{id}` | Delete task | Admin, Manager |
| GET | `/api/tasks/my-tasks` | Get current user's tasks | All |
| POST | `/api/tasks/{id}/comments` | Add comment to task | All |
| GET | `/api/tasks/{id}/comments` | Get task comments | All |

### Teams

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/teams` | Get all teams | All |
| GET | `/api/teams/{id}` | Get team by ID | All |
| POST | `/api/teams` | Create team | Admin |
| POST | `/api/teams/{id}/members` | Add team member | Admin, Manager |
| DELETE | `/api/teams/{id}/members/{userId}` | Remove team member | Admin, Manager |

### Users

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/users` | Get all users | Admin |
| GET | `/api/users/{id}` | Get user by ID | All |
| GET | `/api/users/managers` | Get all managers | All |
| PUT | `/api/users/{id}/role` | Update user role | Admin |

## Role-Based Access

### Admin
- Full access to all endpoints
- Can manage teams and users
- Can assign managers to teams
- Can create, update, delete any task

### Manager
- Can create teams (if assigned by Admin)
- Can create and assign tasks to team members
- Can view tasks in their managed teams
- Cannot access other managers' tasks

### User
- Can view only their assigned tasks
- Can update status of their tasks
- Can comment on their tasks
- Cannot create or delete tasks

## Testing with Swagger

1. Navigate to `https://localhost:5001/swagger`
2. Register a new user via `/api/auth/register`
3. Login via `/api/auth/login` to get a JWT token
4. Click "Authorize" button (top right)
5. Enter: `Bearer YOUR_JWT_TOKEN`
6. Now you can test all protected endpoints

## Example API Calls

### 1. Register User

```bash
POST /api/auth/register
Content-Type: application/json

{
  "email": "manager@example.com",
  "password": "Manager@123",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Manager"
}
```

### 2. Login

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@taskmanagement.com",
  "password": "Admin@123"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2026-02-13T21:00:00Z",
  "user": {
    "id": 1,
    "email": "admin@taskmanagement.com",
    "firstName": "System",
    "lastName": "Administrator",
    "role": "Admin"
  }
}
```

### 3. Create Task

```bash
POST /api/tasks
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "title": "Implement Login Page",
  "description": "Create login page with email/password validation",
  "priority": "High",
  "deadline": "2026-02-20T18:00:00Z",
  "assignedToId": 2,
  "teamId": 1
}
```

## Database Schema

### Key Entities

- **User**: Stores user information and credentials
- **Team**: Represents teams with manager assignments
- **TeamMember**: Junction table for team memberships
- **TaskItem**: Task details with status and priority
- **Comment**: Comments on tasks
- **Notification**: System notifications

### Relationships

- One User (Manager) can manage many Teams
- One Team can have many TeamMembers
- One User can be assigned to many Tasks
- One Task can have many Comments
- One User can receive many Notifications

## Notification System

Currently using **mock email** system that logs to console/file.

To enable real SMTP:
1. Update `appsettings.json`:
   ```json
   "EmailSettings": {
     "UseMockEmail": false,
     "SmtpHost": "smtp.gmail.com",
     "SmtpPort": 587
   }
   ```
2. Implement SMTP logic in `NotificationService.cs`

## Security Features

- ✅ Password hashing with BCrypt
- ✅ JWT token authentication
- ✅ Role-based authorization
- ✅ CORS configured for React frontend
- ✅ Input validation with Data Annotations
- ✅ SQL injection prevention via EF Core parameterized queries

## Next Steps

### Backend Enhancements
- [ ] Add refresh token mechanism
- [ ] Implement real SMTP email service
- [ ] Add file upload for task attachments
- [ ] Implement task filtering by date, priority, status
- [ ] Add pagination for large datasets
- [ ] Create comprehensive unit tests

### Frontend Development
- [ ] Set up React project with Vite
- [ ] Create authentication components
- [ ] Build role-specific dashboards
- [ ] Implement task management UI
- [ ] Add real-time updates with SignalR

## Troubleshooting

### Migration Errors
```bash
# Remove all migrations
dotnet ef migrations remove

# Create fresh migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### Connection Issues
- Verify PostgreSQL is running
- Check connection string credentials
- Ensure database exists

### JWT Errors
- Verify SecretKey is at least 32 characters
- Check token expiration
- Ensure Bearer token format: `Bearer <token>`

## License

MIT License - Feel free to use this project for learning or commercial purposes.
