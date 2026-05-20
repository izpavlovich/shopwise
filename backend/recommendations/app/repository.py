"""Data-access layer: raw SQL against the shared ShopWise database.

Recommendation queries are aggregation-heavy (co-occurrence, ranking), so they
are expressed as plain SQL rather than through an ORM. Cancelled orders are
excluded everywhere — they are not a signal of intent — and soft-deleted
products (`is_deleted = TRUE`) are never recommended.
"""

import asyncpg

from app.schemas import PopularProduct, ProductSummary, RelatedProduct


class RecommendationRepository:
    def __init__(self, pool: asyncpg.Pool) -> None:
        self._pool = pool

    async def ping(self) -> bool:
        """Return True if the database answers a trivial query."""
        try:
            async with self._pool.acquire() as conn:
                await conn.fetchval("SELECT 1")
            return True
        except (asyncpg.PostgresError, OSError):
            return False

    async def get_product(self, product_id: int) -> ProductSummary | None:
        row = await self._pool.fetchrow(
            """
            SELECT id, category_id, name, price, image_url
            FROM products
            WHERE id = $1 AND is_deleted = FALSE
            """,
            product_id,
        )
        return ProductSummary(**dict(row)) if row else None

    async def related_by_copurchase(
        self, product_id: int, limit: int
    ) -> list[RelatedProduct]:
        """Products most often bought in the same order as `product_id`."""
        rows = await self._pool.fetch(
            """
            SELECT p.id,
                   p.category_id,
                   p.name,
                   p.price,
                   p.image_url,
                   COUNT(DISTINCT oi1.order_id) AS score
            FROM order_items oi1
            JOIN order_items oi2
              ON oi2.order_id = oi1.order_id
             AND oi2.product_id <> oi1.product_id
            JOIN orders o   ON o.id = oi1.order_id
            JOIN products p ON p.id = oi2.product_id
            WHERE oi1.product_id = $1
              AND o.status <> 'cancelled'
              AND p.is_deleted = FALSE
            GROUP BY p.id, p.category_id, p.name, p.price, p.image_url
            ORDER BY score DESC, p.id
            LIMIT $2
            """,
            product_id,
            limit,
        )
        return [RelatedProduct(**dict(r)) for r in rows]

    async def same_category(
        self, category_id: int, exclude_ids: list[int], limit: int
    ) -> list[ProductSummary]:
        """Best-stocked products in a category, used as a fallback."""
        rows = await self._pool.fetch(
            """
            SELECT id, category_id, name, price, image_url
            FROM products
            WHERE category_id = $1
              AND is_deleted = FALSE
              AND id <> ALL($2::int[])
            ORDER BY stock DESC, id
            LIMIT $3
            """,
            category_id,
            exclude_ids,
            limit,
        )
        return [ProductSummary(**dict(r)) for r in rows]

    async def popular(
        self, category_id: int | None, limit: int
    ) -> list[PopularProduct]:
        """Top sellers by units sold, optionally scoped to a category."""
        rows = await self._pool.fetch(
            """
            SELECT p.id,
                   p.category_id,
                   p.name,
                   p.price,
                   p.image_url,
                   SUM(oi.quantity) AS units_sold
            FROM products p
            JOIN order_items oi ON oi.product_id = p.id
            JOIN orders o       ON o.id = oi.order_id
            WHERE p.is_deleted = FALSE
              AND o.status <> 'cancelled'
              AND ($1::int IS NULL OR p.category_id = $1)
            GROUP BY p.id, p.category_id, p.name, p.price, p.image_url
            ORDER BY units_sold DESC, p.id
            LIMIT $2
            """,
            category_id,
            limit,
        )
        return [PopularProduct(**dict(r)) for r in rows]
