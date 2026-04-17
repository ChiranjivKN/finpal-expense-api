# FinPal - Expense Management API

## 🚀 Overview

FinPal is a secure and scalable backend REST API for managing personal finances including expenses, categories, and budgets. It supports JWT-based authentication and follows a clean layered architecture.

---

## 🛠️ Tech Stack

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* JWT Authentication
* BCrypt Password Hashing

---

## ✨ Features

* User Registration & Login
* Secure JWT Authentication
* Category Management (CRUD)
* Expense Tracking with Date Filtering
* Budget Management
* Budget vs Expense Summary (Aggregation)
* Soft Delete Support
* Input Validation using Data Annotations
* Global Exception Handling Middleware
* Structured Logging

---

## 🏗️ Architecture

Controller → Service → DbContext → Database

Additional Layers:

* Infrastructure (Current User Context)
* Common (API Response Wrapper)

---

## 🔐 Security

* Passwords hashed using BCrypt
* JWT-based authentication
* User-specific data isolation (no direct userId input)

---

## 📌 API Modules

* Auth
* Categories
* Expenses
* Budgets

---

## ⚙️ Setup Instructions

1. Clone the repository
2. Configure SQL Server connection string in `appsettings.json`
3. Run database scripts
4. Run the API using Visual Studio or `dotnet run`
5. Access Swagger UI for testing

---

## 📊 Key Highlights

* Clean Architecture with Service Layer
* Secure Authentication Flow
* Complex LINQ Queries (aggregation & filtering)
* Production-ready design patterns

---

## 🔮 Future Enhancements

* Pagination for large datasets
* Unit testing
* Docker support
* Cloud deployment (Azure)

---
