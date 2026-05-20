# ShopWise Recommendations

A small Python microservice that recommends products for ShopWise. It reads the
shared Postgres database (orders, order items, products) — it does not write to
it.

## Stack

- Python 3.12, FastAPI, Uvicorn
- asyncpg (raw SQL, no ORM — the queries are aggregation-heavy)
- Pydantic / pydantic-settings

## Layout

The package is layered to mirror the .NET API, so the two are easy to navigate
side by side:

| Folder / file              | Role               | .NET equivalent |
|----------------------------|--------------------|-----------------|
| `app/routers/`             | HTTP endpoints     | `Controllers/`  |
| `app/service.py`           | Recommendation logic | `Services/`   |
| `app/repository.py`        | SQL / data access  | `Data/`         |
| `app/schemas.py`           | Request/response models | `DTOs/`    |
| `app/config.py`            | Settings           | `appsettings.json` |
| `app/dependencies.py`      | DI wiring          | `Program.cs`    |

## Endpoints

| Method | Path                                | Description |
|--------|-------------------------------------|-------------|
| GET    | `/health`                           | Liveness probe + DB reachability |
| GET    | `/recommendations/popular`          | Best sellers by units sold. Query: `category_id?`, `limit?` |
| GET    | `/recommendations/related/{id}`     | "Customers also bought", with same-category fallback. Query: `limit?` |

Interactive docs: `http://localhost:5200/docs`.

### Recommendation strategy

- **Popular** — ranks products by total units sold across non-cancelled orders.
- **Related** — ranks other products by how often they appear in the same
  order as the seed product (co-purchase). When there is too little
  co-purchase signal, the list is topped up with other products from the same
  category. The `strategy` field reports which path was taken
  (`co_purchase` / `hybrid` / `category`).

Cancelled orders and soft-deleted products are excluded throughout.

## Running

With Docker (from the repo root) the service comes up automatically on
`http://localhost:5200`:

```bash
docker compose up --build            # dev, with --reload
docker compose -f docker-compose.prod.yml up --build   # prod-like
```

Without Docker:

```bash
cd backend/recommendations
pip install -r requirements-dev.txt
export DATABASE_URL="postgresql://shopwise:shopwise123@localhost:5432/shopwise"
uvicorn app.main:app --reload --port 5200
```

## Tests

The suite fakes the SQL layer, so no database is required:

```bash
cd backend/recommendations
pip install -r requirements-dev.txt
pytest
```

## Configuration

| Env var          | Default | Purpose |
|------------------|---------|---------|
| `DATABASE_URL`   | `postgresql://shopwise:shopwise123@localhost:5432/shopwise` | Postgres DSN |
| `POOL_MIN_SIZE`  | `1`     | asyncpg pool minimum |
| `POOL_MAX_SIZE`  | `10`    | asyncpg pool maximum |
| `DEFAULT_LIMIT`  | `5`     | Default number of recommendations |
| `MAX_LIMIT`      | `50`    | Hard cap on `limit` |
