# ShopWise

A small e-commerce web application. Browse products, manage a cart, and place orders.

## Stack

| Layer    | Technology |
|----------|-----------|
| Frontend | React 18, TypeScript, Vite, React Router |
| Backend  | ASP.NET Core 8 Web API, Entity Framework Core, JWT Auth |
| Database | PostgreSQL 15 |

## Quick start

### With Docker (recommended)

```bash
docker compose up --build
```

| | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:5100 |
| Swagger | http://localhost:5100/swagger |

### Local development

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
