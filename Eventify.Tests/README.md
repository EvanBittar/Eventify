Eventify: High-Performance Event Management API 🚀
Eventify is a sophisticated Event Management API built with .NET 10.0. This project focuses on high-performance data access, clean architecture, and robust security practices.

🛠 Tech Stack
Framework: .NET 10.0 (Web API)

ORM: Dapper (Chosen for its raw performance and low overhead)

Database: SQL Server

Authentication: JWT (JSON Web Tokens)

Security: BCrypt.Net for secure password hashing

Testing: xUnit & Moq (Unit Testing suite)

🏗 Architecture & Design Patterns
The project implements the Repository Pattern to ensure a clean separation of concerns and to make the codebase highly testable and maintainable:

Controllers: Handle HTTP requests and orchestrate the flow of data.

Repositories: Encapsulate all data access logic using Dapper, keeping SQL queries out of the business logic.

Dependency Injection (DI): Utilized throughout the project for decoupled and modular code.

DTOs (Data Transfer Objects): Used to define precise data structures for client-server communication.

🚀 Key Features
Secure Authentication: Full JWT implementation with claims-based authorization.

Optimized Data Access: Hand-crafted SQL queries with Dapper for maximum efficiency.

Test-Driven Foundation: Unit tests cover critical business logic (like User Management) to ensure reliability.

Clean Code: Adheres to SOLID principles and industry-standard naming conventions.

📂 Project Structure
Plaintext
api/
├── Eventify-API/       # Main Web API project
└── Eventify.Tests/     # xUnit Test project (Unit Testing)
⚙️ Getting Started
Clone the repository:

Bash
git clone https://github.com/EvanBittar/Eventify.git
Configure Database: Update the DefaultConnection in appsettings.json with your SQL Server credentials.

Run the API:

Bash
dotnet run --project Eventify-API
Run Tests:

Bash
dotnet test
