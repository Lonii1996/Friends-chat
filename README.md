# Friends Chat Server (Real-time Backend)  [Читать на русском языке](README.RU.md)

This project is a real-time messaging backend built on the .NET platform. It focuses on performance, binary network traffic serialization, and secure data handling.

Note: The included .NET MAUI client application is designed strictly as a lightweight visualizer (UI client) to demonstrate the operation of the API and the SignalR hub in real time.

## Key Architectural Decisions and Optimizations

* **MessagePack Protocol:** For real-time communication via SignalR, the default JSON protocol is replaced with MessagePackProtocol. This minimizes network packet sizes and reduces CPU overhead during serialization.
* **Cursor-based Pagination:** Message history retrieval is implemented using cursors (cursorMessageId) instead of traditional Skip/Take offset pagination. This prevents duplicate or skipped messages during infinite scrolling in a constantly growing database.
* **Case-Insensitive Authentication:** The citext extension is enabled at the PostgreSQL database level for unique user indices. Usernames like "User" and "user" are treated as identical, preventing account name collisions during registration.
* **Modern Error Handling:** Centralized exception handling is implemented using GlobalExceptionHandler (IExceptionHandler interface) producing standard RFC 7807 (ProblemDetails) responses.
* **Infrastructure as Code:** A docker-compose environment is configured with automated EF Core migration execution on server startup and an embedded healthcheck for PostgreSQL.

## Technology Stack

* **Backend:** .NET 10, ASP.NET Core Web API, SignalR, Entity Framework Core (PostgreSQL), Serilog, xUnit / Moq.
* **DevOps & DB:** Docker, Docker Compose, PostgreSQL 16 (Alpine).
* **UI Visualizer:** .NET MAUI (Windows / Android).

## Quick Start (Docker)

To spin up the backend and the database out of the box, follow these steps:

1. Create a `.env` file in the root directory:
   ```env
   DB_PASSWORD=your_secure_password
   JWT_SECRET_KEY=your_super_secret_jwt_key_at_least_32_chars
   ```
2. Run the command to build and start the containers:
   ```bash
   docker compose up --build
   ```
   *Note: If you prefer to run the containers in the background and free up your terminal, add the `-d` flag: `docker compose up -d --build`.*

Once running:
* The server will accept incoming requests at http://localhost:8080.
* Interactive API documentation (Scalar) is available in development mode at: http://localhost:8080/scalar/v1.

## Running the UI Client (For Testing)

The client application is built with .NET MAUI and dynamically switches its base URL depending on the target platform: http://10.0.2.2:8080 for the Android emulator and http://localhost:8080 for Windows.

1. Open `ChatServer.slnx` in Visual Studio 2022.
2. Run the `ChatClient` project (select either Windows Machine or an emulator).
3. Use the built-in forms to register a new account and log in.
