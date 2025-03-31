# 🛒 Shopping API

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-✓-blue)](https://www.docker.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-red)](https://www.microsoft.com/sql-server)
[![Redis](https://img.shields.io/badge/Redis-✓-red)](https://redis.io/)

A microservices-based shopping cart API with payment integration, built with .NET 8, SQL Server, Redis, and Docker.

## 📋 Features

- Cart management
- Product operations
- Discount application
- Payment processing
- Redis caching
- Docker containerization
- Global error handling

## 🚀 Technologies

- **Backend**: .NET 8 Web API
- **Database**: SQL Server 2022
- **Cache**: Redis
- **Containerization**: Docker
- **ORM**: Dapper

## ⚙️ Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)

## 🐳 Quick Start

```bash
git clone https://github.com/your-username/shopping-api.git
cd shopping-api
docker-compose up -d
```
- Services will be available at:
    . API: http://localhost:8080
    . SQL Server: localhost:1433
    . Redis: localhost:6379

## 🏗️ Project Structure

```
Shopping.API/
  Application/
    Controllers/          (API endpoints)
    Services/            (Business logic implementations)
    Services/Interfaces/ (Service contracts)
  
  Domain/
    Models/              (Core domain entities)
    Models/Request/      (Input DTOs)
    Models/Response/     (Output DTOs)
  
  Infrastructure/
    Data/                (Data context and configurations)
    Repositories/        (Data access implementations)
    Repositories/Interfaces/ (Repository contracts)
  
  Root files:
    Program.cs           (Main application configuration)
    Dockerfile           (Docker container setup)
    docker-compose.yml   (Service orchestration)
```

## 📊 Database Schema

Cart Table
Column	            | Type	        | Description
--------------------|---------------|----------------
CartId	            | INT	          | Primary Key
PayerDocument	      | VARCHAR(20)	  | CPF/CNPJ
CreatedAt	          | DATETIME	    | Creation date
DiscountPercentage	| DECIMAL(5,2)	| Discount %

Product Table
Column	            | Type	        | Description
--------------------|---------------|----------------
ProductId           |	INT	          | Primary Key
CartId	            | INT	          | Foreign Key
ProductName	        | VARCHAR(100)	| Product name
Quantity	          | INT	          | Quantity
Price	              | DECIMAL(10,2)	| Unit price

Order Table
Column	            | Type	        | Description
--------------------|---------------|---------------
OrderId	            | INT	          | Primary Key
CartId	            | INT	          | Foreign Key
PaymentId	          | VARCHAR(50)	  | Payment ID
PaymentStatus	      | VARCHAR(20)	  | Status
CreatedAt	          | DATETIME	    | Creation date

## 🌐 API Endpoints

### 🛒 Cart Endpoints

- **GET** `/api/cart/{cartId}` - Get cart  
- **POST** `/api/cart` - Create cart  
- **DELETE** `/api/cart/{cartId}` - Delete cart  
- **POST** `/api/cart/{cartId}/products` - Add product  
- **PUT** `/api/cart/products/{productId}` - Update product  
- **DELETE** `/api/cart/products/{productId}` - Remove product  
- **PUT** `/api/cart/{cartId}/discount` - Apply discount  

### 💳 Payment Endpoints

- **POST** `/api/payment/{cartId}` - Process payment  

## 🔄 Payment Flow

sequenceDiagram
    participant Client
    participant API
    participant Redis
    participant SQLServer
    participant PaymentAPI
    
    Client->>API: POST /api/payment/{cartId}
    API->>Redis: Get cached total
    alt Cache exists
        Redis-->>API: Return total
    else
        API->>SQLServer: Calculate total
        SQLServer-->>API: Return total
        API->>Redis: Cache total
    end
    API->>PaymentAPI: Process payment
    PaymentAPI-->>API: Confirm payment
    API->>SQLServer: Create order
    API-->>Client: Return result

## ⚠️ Error Handling

Standardized error responses:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Not Found",
  "status": 404,
  "detail": "Cart not found"
}
```
## 🔒 Payment Integration

Example request:

```json
{
  "cart_id": 123,
  "amount": 10.00,
  "payer_document": "12312312389"
}
```
## 📊 Redis Caching

- Key format: cart_total_{cartId}
- TTL: 30 minutes
- Cache invalidation occurs when:
  . Items are added/removed
  . Discount is applied
  . Payment is processed

## 🤝 Contributing

- Fork the project
- Create your branch (git checkout -b feature/fooBar)
- Commit changes (git commit -am 'Add some fooBar')
- Push to branch (git push origin feature/fooBar)
- Open a Pull Request
