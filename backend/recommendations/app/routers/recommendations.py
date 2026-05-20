from fastapi import APIRouter, Depends, HTTPException, Path, Query

from app.config import settings
from app.dependencies import get_service
from app.schemas import PopularResponse, RelatedResponse
from app.service import ProductNotFoundError, RecommendationService

router = APIRouter(prefix="/recommendations", tags=["recommendations"])


@router.get("/popular", response_model=PopularResponse)
async def popular_products(
    category_id: int | None = Query(
        default=None, ge=1, description="Restrict to a single category."
    ),
    limit: int = Query(default=settings.default_limit, ge=1, le=settings.max_limit),
    service: RecommendationService = Depends(get_service),
) -> PopularResponse:
    """Best-selling products by units sold, optionally within a category."""
    return await service.popular(category_id, limit)


@router.get("/related/{product_id}", response_model=RelatedResponse)
async def related_products(
    product_id: int = Path(ge=1),
    limit: int = Query(default=settings.default_limit, ge=1, le=settings.max_limit),
    service: RecommendationService = Depends(get_service),
) -> RelatedResponse:
    """Products frequently bought together with `product_id`."""
    try:
        return await service.related(product_id, limit)
    except ProductNotFoundError as exc:
        raise HTTPException(status_code=404, detail=str(exc)) from exc
