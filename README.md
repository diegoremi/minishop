# MiniShop

MiniShop is a learning project built with .NET to practice backend architecture, event-driven communication, Docker-based local infrastructure, testing, and production-readiness concepts.

The project models a small ecommerce flow where orders are created, placed, paid asynchronously, and updated through integration events.

## Tech Stack

* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server / Azure SQL Edge for local Docker
* Redis
* Kafka
* Docker Compose
* xUnit
* WebApplicationFactory
* SQLite in-memory for integration tests

## Solution Structure

```txt
MiniShop
├── MiniShop.WebApi
├── MiniShop.ApplicationCore
├── MiniShop.Infrastructure
├── MiniShop.PaymentWorker
├── MiniShop.Tests
└── MiniShop.IntegrationTests
```

## Main Architecture Concepts

MiniShop includes:

* Clean Architecture-style project separation
* Domain entities
* Repositories
* Application services
* Domain events
* Integration events
* Outbox Pattern
* Inbox Pattern
* Idempotent event processing
* Background services
* Kafka-based asynchronous messaging
* Redis caching
* SQL Server persistence
* Health checks
* Docker Compose local environment
* Unit tests
* Integration tests

## Event Flow

```txt
Create Brand
→ Create Product
→ Create Order
→ Add Order Items
→ Place Order
→ OrderPlaced domain event
→ OrderPlaced integration event
→ OutboxMessages
→ Kafka topic: minishop.order-placed.v1
→ PaymentWorker consumes OrderPlaced
→ PaymentWorker simulates payment
→ Kafka topic: minishop.payment-completed.v1
→ WebApi consumes PaymentCompleted
→ InboxMessages for idempotency
→ Order is marked as Paid
→ OrderPaid integration event
→ Kafka topic: minishop.order-paid.v1
```

## Requirements

* Docker Desktop
* .NET SDK 10
* Git

Optional:

* Rider / Visual Studio / VS Code
* Kafka UI through Docker Compose
* SQL client for inspecting the database

## Environment Variables

Create a `.env` file in the root folder:

```env
SQL_PASSWORD=Your_password123
```

Do not commit the real `.env` file.

Use `.env.example` as the template for required local variables.

## Run with Docker Compose

From the root folder:

```bash
docker compose --env-file .env up --build
```

To run in detached mode:

```bash
docker compose --env-file .env up --build -d
```

To stop all containers without deleting volumes:

```bash
docker compose down
```

To stop and delete volumes, including the SQL Server data volume:

```bash
docker compose down -v
```

Use `down -v` only when you intentionally want to reset local data.

## Local URLs

```txt
WebApi Swagger:
http://localhost:5001/swagger

Health live:
http://localhost:5001/health/live

Health ready:
http://localhost:5001/health/ready

Kafka UI:
http://localhost:8080

SQL Server:
localhost,1433

Redis:
localhost:6379

Kafka from host:
localhost:9094

Kafka from containers:
kafka:9092
```

## Health Checks

The WebApi exposes:

```txt
/health/live
/health/ready
```

`/health/live` verifies that the application process is alive.

`/health/ready` verifies that the application is ready to receive traffic and can connect to required infrastructure such as the database.

## Database Migrations

In Docker and Development environments, the WebApi applies EF Core migrations automatically on startup.

The application:

1. Waits for SQL Server to become available.
2. Creates the `MiniShop` database if it does not exist.
3. Applies pending EF Core migrations.

Migrations are not applied automatically in Production.

## Kafka Topics

Kafka topics are created automatically by the `kafka-init` service in Docker Compose.

Topics:

```txt
minishop.order-placed.v1
minishop.payment-completed.v1
minishop.order-paid.v1
minishop.payment-completed.dlq.v1
```

## Run Tests

From the root folder:

```bash
dotnet test
```

The test suite includes:

* Unit tests
* Integration tests
* WebApplicationFactory-based API tests
* SQLite in-memory database for integration tests

## Useful Docker Commands

View running services:

```bash
docker compose ps
```

Follow WebApi logs:

```bash
docker compose logs -f webapi
```

Follow PaymentWorker logs:

```bash
docker compose logs -f payment-worker
```

View Kafka topic initialization logs:

```bash
docker compose logs kafka-init
```

Check for Kafka bootstrap issues:

```bash
docker compose logs webapi | grep -i "bootstrap"
docker compose logs payment-worker | grep -i "bootstrap"
```

Check recent PaymentWorker logs:

```bash
docker compose logs payment-worker --since=2m
```

## Manual End-to-End Test Flow

Open Swagger:

```txt
http://localhost:5001/swagger
```

Then execute:

```txt
1. Create Brand
2. Create Product
3. Create Order
4. Add Item to Order
5. Place Order
6. Get Order
```

Expected result:

```txt
The order eventually becomes Paid.
```

The expected event-driven flow is:

```txt
WebApi publishes OrderPlaced
PaymentWorker consumes OrderPlaced
PaymentWorker publishes PaymentCompleted
WebApi consumes PaymentCompleted
WebApi marks the Order as Paid
```

## Troubleshooting

### WebApi is unhealthy

Check:

```bash
docker compose logs webapi
docker compose logs sqlserver
```

Then verify:

```txt
http://localhost:5001/health/live
http://localhost:5001/health/ready
```

### Kafka topic does not exist

Check:

```bash
docker compose logs kafka-init
```

You should see:

```txt
Kafka topics created.
```

### PaymentWorker starts before Kafka is ready

This should be handled by the `kafka-init` service. If needed, restart the worker:

```bash
docker compose restart payment-worker
```

### Reset everything locally

This deletes containers, networks, and volumes:

```bash
docker compose down -v
docker compose --env-file .env up --build
```

Use this only when you want a clean local database.

## Current Status

Implemented:

* WebApi
* PaymentWorker
* SQL Server persistence
* Redis cache
* Kafka messaging
* Outbox Pattern
* Inbox Pattern
* Docker Compose
* Kafka topic initialization
* Health checks
* Automatic migrations in Docker/Development
* Unit tests
* Integration tests
* End-to-end order payment flow

Next planned areas:

* Azure deployment strategy
* Terraform
* GitHub Actions
* Auth and JWT
* Observability
* GraphQL
* gRPC
* AI features for enterprise backend scenarios
