"""End-to-end tests for the recommendation endpoints (DB layer faked)."""


async def test_popular_returns_items_ranked_by_units_sold(client):
    resp = await client.get("/recommendations/popular")
    assert resp.status_code == 200
    body = resp.json()
    assert body["category_id"] is None
    assert [i["id"] for i in body["items"]] == [14, 1, 6]
    assert [i["units_sold"] for i in body["items"]] == [3, 1, 1]


async def test_popular_can_be_scoped_to_a_category(client):
    resp = await client.get("/recommendations/popular", params={"category_id": 1})
    assert resp.status_code == 200
    body = resp.json()
    assert body["category_id"] == 1
    assert [i["id"] for i in body["items"]] == [1, 6]


async def test_popular_respects_limit(client):
    resp = await client.get("/recommendations/popular", params={"limit": 1})
    assert resp.status_code == 200
    assert [i["id"] for i in resp.json()["items"]] == [14]


async def test_related_uses_co_purchase_only_when_enough_signal(client):
    resp = await client.get("/recommendations/related/1", params={"limit": 2})
    assert resp.status_code == 200
    body = resp.json()
    assert body["strategy"] == "co_purchase"
    assert [i["id"] for i in body["items"]] == [14, 2]
    assert [i["score"] for i in body["items"]] == [3, 1]


async def test_related_falls_back_to_category_as_hybrid(client):
    resp = await client.get("/recommendations/related/1", params={"limit": 5})
    assert resp.status_code == 200
    body = resp.json()
    assert body["strategy"] == "hybrid"
    # co-purchase first (with scores), then same-category filler with score 0,
    # never repeating the seed (1) or the co-purchase hits (14, 2).
    assert [i["id"] for i in body["items"]] == [14, 2, 6, 7]
    assert [i["score"] for i in body["items"]] == [3, 1, 0, 0]


async def test_related_is_pure_category_when_no_co_purchase(client):
    resp = await client.get("/recommendations/related/6", params={"limit": 3})
    assert resp.status_code == 200
    body = resp.json()
    assert body["strategy"] == "category"
    assert [i["id"] for i in body["items"]] == [1, 2, 7]
    assert all(i["score"] == 0 for i in body["items"])


async def test_related_unknown_product_returns_404(client):
    resp = await client.get("/recommendations/related/999")
    assert resp.status_code == 404


async def test_limit_bounds_are_validated(client):
    assert (await client.get("/recommendations/popular?limit=0")).status_code == 422
    assert (await client.get("/recommendations/popular?limit=999")).status_code == 422


async def test_health_reports_database_status(client, fake_repo):
    assert (await client.get("/health")).json() == {
        "status": "ok",
        "database": "ok",
    }

    fake_repo.healthy = False
    assert (await client.get("/health")).json() == {
        "status": "ok",
        "database": "unreachable",
    }
