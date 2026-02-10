# User Management Web Application

A simple user management web application built as an academic project.

## Tech Stack
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Bootstrap

## Features
- User registration and authentication
- Admin user management panel
- Multiple selection with checkboxes
- Toolbar-based actions (no per-row buttons)
- Sorting by last login time
- User blocking / unblocking
- Deleting users (including bulk delete)
- Middleware-based user status validation
- Database-level unique e-mail constraint
- Case-insensitive e-mail uniqueness (functional index)

## Database
- Primary key: User Id (GUID)
- Unique index on e-mail (case-insensitive)
- All schema changes managed via EF Core migrations

## Notes
- Non-authenticated users cannot access user management
- Blocked users are automatically signed out and redirected to login
- E-mail uniqueness is enforced at database level, not in application code
