# ShopSphere – Full-Stack E-Commerce Order Management System

A production-grade, containerized e-commerce order management platform built with **ASP.NET Core 8**, **Entity Framework Core**, **SQL Server 2022**, **React**, **TypeScript**, **Docker Compose**, and **GitHub Actions**.

## Architectural Highlights
- **Clean 4-Layer Architecture:** Domain, Application, Infrastructure, and API with strict dependency inversion.
- **Optimistic Concurrency Control (RowVersion):** Prevents inventory overselling during simultaneous checkouts via SQL Server rowversion tokens (409 Conflict handling).
- **Atomic Transactions & Stock Restoration:** Uses Repository and Unit of Work transactions so order placement deducts stock atomically and order cancellation restores stock safely.
- **JWT Security & Refresh Token Rotation:** 15-minute access tokens, 7-day rotating refresh tokens, token reuse detection, and Admin / Customer role-based authorization.
- **SQL Performance & Stored Procedures:** Composite indexes plus sp_GetSalesSummary and sp_GetTopSellingProducts for real-time admin analytics.
- **Observability & Automated Testing:** Structured logging with Serilog, centralized ProblemDetails exception middleware, and 32 unit tests (xUnit, Moq, FluentAssertions).
- **Zero-Cost DevOps:** Multi-stage Dockerfiles (Nginx + .NET 8 + SQL Server 2022) and automated GitHub Actions CI.

## Quick Start with Docker Compose
1. Clone the repository and enter the folder.
2. Run: docker compose up --build -d
3. Open http://localhost:3000 in your browser.

## Demo Credentials
- **Admin:** admin@shopsphere.local / Admin@123
- **Customer:** customer@shopsphere.local / Customer@123

## Author
- **Murtunjay Khuntia** – [GitHub Profile](https://github.com/murtunjay-khuntia)
