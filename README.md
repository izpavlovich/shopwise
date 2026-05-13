# ShopWise

A small e-commerce web application. Browse products, manage a cart, and place orders.

## Stack

| Layer    | Technology |
|----------|-----------|
| Frontend | React 18, TypeScript, Vite, React Router |
| Backend  | ASP.NET Core 8 Web API, Entity Framework Core, JWT Auth |
| Database | PostgreSQL 15 |

## Quick start

| | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:5100 |
| Swagger | http://localhost:5100/swagger |

### Dev environment (live reload)

The default `docker-compose.yml` runs the API with `dotnet watch` and the
frontend with the Vite dev server. Source is bind-mounted, so edits on the
host reload inside the containers.

```bash
docker compose up --build
```

### Prod-like environment

`docker-compose.prod.yml` builds release artefacts: the API runs from a
published Release build and the frontend is served as a static bundle from
nginx. No source mounts, no watchers.

```bash
docker compose -f docker-compose.prod.yml up --build
```

### Without Docker

**Start the database:**
```bash
docker compose up db -d
```

**Run the API:**
```bash
cd backend/ShopWise.Api
dotnet run
```

**Run the frontend:**
```bash
cd frontend/shopwise-ui
npm install
npm run dev   # proxies /api → http://localhost:5100
```

## Database

Schema and seed data live in `db/init.sql`. Postgres only runs that file
the first time the data volume is created, so to apply changes you either
edit the file and reset the volume, or apply the delta manually.

**Reset (drops all data, re-runs `init.sql`):**
```bash
docker compose down -v
docker compose up -d
```

**Apply an incremental change without wiping data:**
```bash
docker compose exec -T db psql -U shopwise -d shopwise < change.sql
```
Also update `init.sql` so a fresh boot reflects the same schema.

## Test accounts

| Email | Password | Role |
|---|---|---|
| admin@shopwise.com | Admin123! | admin |
| jane@example.com | Jane456! | customer |
| john@example.com | John789! | customer |

## Features

- Product catalogue with category and search
- Shopping cart (add, remove, clear)
- Checkout and order placement
- Order history
- JWT-based authentication (register / login)
- Admin: create and delete products

## Project layout

```
shopwise/
├── backend/ShopWise.Api/
│   ├── Controllers/     # AuthController, ProductsController, CartController, OrdersController
│   ├── Services/        # Business logic
│   ├── Models/          # EF Core entities
│   ├── DTOs/            # Request / response types
│   └── Data/            # AppDbContext
├── frontend/shopwise-ui/
│   ├── src/pages/       # Home, ProductDetail, Cart, Checkout, Orders, Login, Register
│   ├── src/components/  # ProductCard, ProductList, Navbar
│   ├── src/hooks/       # useCart
│   └── src/store/       # AuthContext
└── db/
    └── init.sql         # Schema and seed data
```
