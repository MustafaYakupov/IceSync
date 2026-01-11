# IceSync – Workflow Synchronization App

IceSync is an ASP.NET Core MVC web application built for **The Ice Cream Company**.  
It integrates with the **Universal Loader API** to monitor, synchronize, and manually execute workflows.

The application periodically synchronizes workflow data into a SQL Server database and provides a web-based UI for monitoring and triggering workflows.

---

## 🛠️ Tech Stack

- **.NET 10**
- **ASP.NET Core MVC**
- **Entity Framework Core**
- **SQL Server**
- **Bootstrap 5**
- **NUnit & Moq** (unit testing)

---

## 🧱 Solution Architecture

The solution follows a **layered architecture** with clear separation of concerns.

```text
/Web
├─ IceSync.Web
└─ IceSync.Web.ViewModels

/Services
├─ IceSync.Services
├─ IceSync.Services.Contracts
└─ IceSync.Infrastructure

/Data
├─ IceSync.Data
└─ IceSync.Data.Models
```

### Layer Responsibilities

- **Web**
  - MVC controllers and Razor views
  - UI logic and ViewModels
  - Dependency Injection composition root

- **Services**
  - Business logic and workflow synchronization
  - Orchestration between API and database
  - Interfaces exposed via Contracts

- **Infrastructure**
  - Universal Loader API client
  - JWT authentication and token caching
  - Background hosted service

- **Data**
  - EF Core DbContext
  - Repository and Unit of Work pattern
  - SQL Server persistence

---

## 🔐 Universal Loader API Integration

IceSync connects to the Universal Loader API using **JWT authentication**.

- Authentication endpoint:  
  `POST /authenticate`
- Tokens are cached in memory and refreshed only when expired
- Workflow endpoints:
  - `GET /workflows`
  - `POST /workflows/{workflowId}/run`

The API returns JSON payloads with `text/plain` content type, which is handled explicitly in the HTTP client.

---

## 🔄 Background Synchronization

A background hosted service runs every **30 minutes** and:

1. Retrieves workflows from the Universal Loader API
2. Inserts new workflows into the database
3. Updates existing workflows
4. Deletes workflows no longer present in the API

This ensures the database always mirrors the external system.

---

## 🧪 Unit Testing

Unit tests are written using **NUnit** and **Moq**.

### What is tested
- Workflow synchronization logic
- Insert / update / delete behavior
- Manual workflow execution logic
- API client behavior (mocked)
