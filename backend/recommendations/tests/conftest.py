"""Test fixtures.

The recommendation strategy is tested through the real `RecommendationService`
and HTTP layer, with only the SQL layer replaced by an in-memory
`FakeRepository`. This means no database is needed to run the suite.
"""

import pytest
from httpx import ASGITransport, AsyncClient

from app.dependencies import get_repository
from app.main import app
from app.schemas import PopularProduct, ProductSummary, RelatedProduct

# Mirror of the seed data relevant to recommendations.
PRODUCTS = {
    1: ProductSummary(id=1, category_id=1, name="Wireless Headphones", price=89.99, image_url="/images/1.png"),
    2: ProductSummary(id=2, category_id=1, name="USB-C Hub", price=34.99, image_url="/images/2.png"),
    6: ProductSummary(id=6, category_id=1, name="Portable SSD 1TB", price=109.99, image_url="/images/6.png"),
    7: ProductSummary(id=7, category_id=1, name="Bluetooth Speaker", price=49.99, image_url="/images/7.png"),
    14: ProductSummary(id=14, category_id=3, name="Clean Code", price=34.99, image_url="/images/14.png"),
}

# product 1 is co-purchased with 14 (3 orders) and 2 (1 order); product 6 never.
COPURCHASE = {
    1: [
        RelatedProduct(id=14, category_id=3, name="Clean Code", price=34.99, image_url="/images/14.png", score=3),
        RelatedProduct(id=2, category_id=1, name="USB-C Hub", price=34.99, image_url="/images/2.png", score=1),
    ],
    6: [],
}

POPULAR = [
    PopularProduct(id=14, category_id=3, name="Clean Code", price=34.99, image_url="/images/14.png", units_sold=3),
    PopularProduct(id=1, category_id=1, name="Wireless Headphones", price=89.99, image_url="/images/1.png", units_sold=1),
    PopularProduct(id=6, category_id=1, name="Portable SSD 1TB", price=109.99, image_url="/images/6.png", units_sold=1),
]


class FakeRepository:
    """In-memory stand-in honouring the `RecommendationRepository` contract."""

    def __init__(self, healthy: bool = True) -> None:
        self.healthy = healthy

    async def ping(self) -> bool:
        return self.healthy

    async def get_product(self, product_id: int):
        return PRODUCTS.get(product_id)

    async def related_by_copurchase(self, product_id: int, limit: int):
        return list(COPURCHASE.get(product_id, []))[:limit]

    async def same_category(self, category_id, exclude_ids, limit):
        candidates = sorted(
            (
                p
                for p in PRODUCTS.values()
                if p.category_id == category_id and p.id not in exclude_ids
            ),
            key=lambda p: p.id,
        )
        return candidates[:limit]

    async def popular(self, category_id, limit):
        rows = POPULAR
        if category_id is not None:
            rows = [r for r in rows if r.category_id == category_id]
        return rows[:limit]


@pytest.fixture
def fake_repo() -> FakeRepository:
    return FakeRepository()


@pytest.fixture
async def client(fake_repo: FakeRepository):
    app.dependency_overrides[get_repository] = lambda: fake_repo
    transport = ASGITransport(app=app)
    async with AsyncClient(transport=transport, base_url="http://test") as c:
        yield c
    app.dependency_overrides.clear()
