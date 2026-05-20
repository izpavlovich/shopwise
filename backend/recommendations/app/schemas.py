from pydantic import BaseModel, Field


class ProductSummary(BaseModel):
    """Trimmed product view returned by the recommendation endpoints."""

    id: int
    category_id: int
    name: str
    price: float
    image_url: str | None = None


class RelatedProduct(ProductSummary):
    # How many distinct orders contained this product alongside the seed
    # product. Zero means the product was added as a same-category fallback.
    score: int


class PopularProduct(ProductSummary):
    # Total units sold across all non-cancelled orders.
    units_sold: int


class RelatedResponse(BaseModel):
    product_id: int
    # "co_purchase" — purely from order co-occurrence.
    # "category"    — no co-purchase data, same-category fallback only.
    # "hybrid"      — co-purchase results topped up with category fallback.
    strategy: str
    items: list[RelatedProduct]


class PopularResponse(BaseModel):
    category_id: int | None = Field(
        default=None, description="Category filter that was applied, if any."
    )
    items: list[PopularProduct]
