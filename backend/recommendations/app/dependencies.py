"""FastAPI dependency wiring.

The chain is `get_pool -> get_repository -> get_service`, linked with `Depends`
so any link can be replaced in tests via `app.dependency_overrides`
(see `tests/conftest.py`).
"""

import asyncpg
from fastapi import Depends, Request

from app.repository import RecommendationRepository
from app.service import RecommendationService


def get_pool(request: Request) -> asyncpg.Pool:
    return request.app.state.pool


def get_repository(
    pool: asyncpg.Pool = Depends(get_pool),
) -> RecommendationRepository:
    return RecommendationRepository(pool)


def get_service(
    repo: RecommendationRepository = Depends(get_repository),
) -> RecommendationService:
    return RecommendationService(repo)
