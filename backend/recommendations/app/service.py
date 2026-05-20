"""Recommendation logic.

Kept deliberately separate from both the HTTP layer (`routers`) and the SQL
layer (`repository`) so the strategy can be unit-tested with a fake repository.
"""

from app.repository import RecommendationRepository
from app.schemas import (
    PopularResponse,
    RelatedProduct,
    RelatedResponse,
)


class ProductNotFoundError(Exception):
    """Raised when a recommendation is requested for an unknown product."""

    def __init__(self, product_id: int) -> None:
        super().__init__(f"product {product_id} not found")
        self.product_id = product_id


class RecommendationService:
    def __init__(self, repo: RecommendationRepository) -> None:
        self._repo = repo

    async def popular(self, category_id: int | None, limit: int) -> PopularResponse:
        items = await self._repo.popular(category_id, limit)
        return PopularResponse(category_id=category_id, items=items)

    async def related(self, product_id: int, limit: int) -> RelatedResponse:
        """Co-purchase recommendations, topped up with same-category products.

        Raises `ProductNotFoundError` if the seed product does not exist (or is
        soft-deleted), so the caller can return a 404.
        """
        product = await self._repo.get_product(product_id)
        if product is None:
            raise ProductNotFoundError(product_id)

        items = await self._repo.related_by_copurchase(product_id, limit)
        strategy = "co_purchase"

        # New or rarely-ordered products have little co-purchase signal. Fill
        # the gap with other products from the same category so the response
        # is never empty for a valid product.
        if len(items) < limit:
            exclude = [product_id] + [i.id for i in items]
            fallback = await self._repo.same_category(
                product.category_id, exclude, limit - len(items)
            )
            if fallback:
                strategy = "hybrid" if items else "category"
                items += [
                    RelatedProduct(score=0, **p.model_dump()) for p in fallback
                ]

        return RelatedResponse(
            product_id=product_id, strategy=strategy, items=items
        )
