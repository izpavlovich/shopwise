from contextlib import asynccontextmanager

import asyncpg
from fastapi import Depends, FastAPI

from app.config import settings
from app.dependencies import get_repository
from app.repository import RecommendationRepository
from app.routers import recommendations


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Open the connection pool on startup, close it on shutdown."""
    app.state.pool = await asyncpg.create_pool(
        settings.database_url,
        min_size=settings.pool_min_size,
        max_size=settings.pool_max_size,
    )
    try:
        yield
    finally:
        await app.state.pool.close()


app = FastAPI(
    title="ShopWise Recommendations",
    description="Product recommendations (related & popular) for ShopWise.",
    version="1.0.0",
    lifespan=lifespan,
)

app.include_router(recommendations.router)


@app.get("/health", tags=["health"])
async def health(
    repo: RecommendationRepository = Depends(get_repository),
) -> dict[str, str]:
    """Liveness/readiness probe. Reports database reachability."""
    db_ok = await repo.ping()
    return {"status": "ok", "database": "ok" if db_ok else "unreachable"}
