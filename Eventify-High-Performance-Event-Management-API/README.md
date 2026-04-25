# Eventify-High-Performance-Event-Management-API
Eventify is a robust, scalable Backend API designed to handle real-time event discovery, user registrations, and ticket management. Built with ASP.NET Core and C#, the system is optimized for high-speed data retrieval and strict data integrity, ensuring a seamless experience for both event organizers and attendees.
Technical Stack
Language: C# 12 / .NET 8

Database: SQL Server

ORM: Dapper (Micro-ORM) – Chosen for maximum performance and fine-grained control over complex SQL queries.

Architecture: Clean Architecture (Separation of Concerns between Domain, Application, and Infrastructure layers).

Design Patterns: Repository Pattern, Unit of Work, and DTO (Data Transfer Objects).

Key Features:
Dynamic Event Discovery: Advanced filtering and searching for events by category, date, and location using optimized SQL indexing.

Secure Booking System: A transaction-safe registration flow that prevents "over-booking" by handling concurrent database requests.

Role-Based Access Control (RBAC): Distinct permissions for 'Admins' (event creation/management) and 'Attendees' (browsing/booking).

Data Integrity: Implementation of SQL Constraints and C# Validations to ensure business rules (e.g., event capacity, valid dates) are never violated.

"I built Eventify to master high-performance data access in C#. I chose Dapper over EF Core because I wanted total control over the execution plan and to ensure the system could handle high-concurrency booking scenarios without data corruption.
# 🎟️ Eventify - High-Performance Event Management API

A robust, secure, and high-performance RESTful API built with **.NET Core** and **Dapper**, designed to manage events, categories, and user bookings. This project demonstrates backend best practices, including Role-Based Access Control (RBAC), raw SQL optimization, and strict data integrity.

## 🚀 Tech Stack
* **Framework:** C# / .NET Core 8.0 (Web API)
* **Database:** Microsoft SQL Server
* **ORM:** Dapper (Micro-ORM chosen for maximum query execution speed)
* **Security:** JWT (JSON Web Tokens) Authentication & Authorization

## ✨ Key Features & Engineering Highlights

### 🔐 Security & Identity (JWT)
* Secure User Registration and Login with encrypted tokens.
* **Role-Based Access Control (RBAC):** Distinct endpoints for `Admin` and standard `User` using Token Claims.
* Zero trust architecture: `UserId` is securely extracted directly from the JWT claims, never from the request body, preventing ID manipulation.

### 📅 Smart Booking System
* **Capacity Management:** Real-time validation checks event capacity (`MaxAttendees`) before confirming a booking.
* **Double-Booking Prevention:** Handled at both the application level and database level using `UNIQUE Constraints` and `try-catch` blocks.
* **Status Tracking:** Bookings maintain state (Confirmed, Cancelled) without hard-deleting records.

### 🛡️ Data Integrity & Admin Controls
* **Orphaned Data Prevention:** Deleting a Category automatically checks for associated active events, blocking the deletion to maintain database integrity.
* **Optimized Database Calls:** Merged validation and update logic into single trips to the database using sub-queries and efficient `WHERE` clauses.

### 📊 Complex Data Retrieval
* **My Tickets Endpoint:** Utilizes complex **Triple SQL Joins** (`Bookings` ➔ `Events` ➔ `Categories`) to deliver a rich, flattened data view to the frontend in a single query.

Execute the SQL scripts located in the /Scripts folder to create the Event schema and necessary tables (Users, Categories, Events, Bookings).

Configure Settings:

Open appsettings.json and update your DefaultConnection string.

Ensure the AppSettings:PasswordKey is set for JWT generation.

Run the API:

Bash
dotnet watch run
Explore via Swagger:

Navigate to http://localhost:5000/swagger. Use the /api/Users/Login endpoint to get a token, click the Authorize button, and paste your token (prefix with Bearer ).
<img width="1915" height="988" alt="Screenshot 2026-04-18 122454" src="https://github.com/user-attachments/assets/9a09dd01-1977-4677-9137-acbbf8b2bed6" />
<img width="1914" height="987" alt="Screenshot 2026-04-18 122437" src="https://github.com/user-attachments/assets/fe6e8882-a9bd-47ff-8ff5-b26887d3a084" />
