-- ShopWise Database Schema + Seed Data

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================
-- TABLES
-- ============================================================

CREATE TABLE categories (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(100) NOT NULL,
    description TEXT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE products (
    id           SERIAL PRIMARY KEY,
    category_id  INT NOT NULL REFERENCES categories(id),
    name         VARCHAR(200) NOT NULL,
    description  TEXT,
    price        FLOAT NOT NULL,
    stock        INT NOT NULL DEFAULT 0,
    image_url    VARCHAR(500),
    is_deleted   BOOLEAN NOT NULL DEFAULT FALSE,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE users (
    id            SERIAL PRIMARY KEY,
    email         VARCHAR(200) NOT NULL UNIQUE,
    password_hash VARCHAR(200) NOT NULL,
    full_name     VARCHAR(200) NOT NULL,
    role          VARCHAR(50) NOT NULL DEFAULT 'customer',
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE carts (
    id         SERIAL PRIMARY KEY,
    user_id    INT NOT NULL REFERENCES users(id) UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE cart_items (
    id         SERIAL PRIMARY KEY,
    cart_id    INT NOT NULL REFERENCES carts(id) ON DELETE CASCADE,
    product_id INT NOT NULL REFERENCES products(id),
    quantity   INT NOT NULL DEFAULT 1,
    UNIQUE (cart_id, product_id)
);

CREATE TABLE orders (
    id           SERIAL PRIMARY KEY,
    user_id      INT NOT NULL REFERENCES users(id),
    status       VARCHAR(50) NOT NULL DEFAULT 'pending',
    total_amount FLOAT NOT NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE order_items (
    id         SERIAL PRIMARY KEY,
    order_id   INT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id INT NOT NULL REFERENCES products(id),
    quantity   INT NOT NULL,
    unit_price FLOAT NOT NULL
);

-- ============================================================
-- FUNCTIONS
-- ============================================================

CREATE OR REPLACE FUNCTION get_user_orders(
    p_user_id   INT,
    p_page      INT DEFAULT 1,
    p_page_size INT DEFAULT 10
)
RETURNS TABLE (
    order_id     INT,
    status       VARCHAR,
    total_amount FLOAT,
    created_at   TIMESTAMPTZ,
    item_count   BIGINT
)
LANGUAGE sql STABLE AS $$
    SELECT
        o.id,
        o.status,
        o.total_amount,
        o.created_at,
        COUNT(oi.id) AS item_count
    FROM orders o
    LEFT JOIN order_items oi ON oi.order_id = o.id
    WHERE o.user_id = p_user_id
    GROUP BY o.id, o.status, o.total_amount, o.created_at
    ORDER BY o.created_at DESC
    OFFSET p_page * p_page_size
    LIMIT p_page_size;
$$;

-- ============================================================
-- SEED DATA
-- ============================================================

INSERT INTO categories (name, description) VALUES
    ('Electronics', 'Gadgets, devices, and accessories'),
    ('Clothing', 'Apparel for all occasions'),
    ('Books', 'Fiction, non-fiction, technical, and more');

INSERT INTO products (category_id, name, description, price, stock, image_url) VALUES
    (1, 'Wireless Headphones', 'Over-ear noise cancelling headphones with 30h battery', 89.99, 45, '/images/1.png'),
    (1, 'USB-C Hub', '7-in-1 hub with HDMI, USB 3.0, SD card reader', 34.99, 120, '/images/2.png'),
    (1, 'Mechanical Keyboard', 'Compact 75% layout, Cherry MX Blue switches', 129.99, 30, '/images/3.png'),
    (1, 'Webcam 1080p', 'Full HD webcam with built-in microphone and autofocus', 59.99, 75, '/images/4.png'),
    (1, 'Smart Watch', 'Fitness tracker with heart rate monitor and GPS', 199.99, 20, '/images/5.png'),
    (1, 'Portable SSD 1TB', 'USB 3.2 Gen 2, up to 1050 MB/s read speed', 109.99, 60, '/images/6.png'),
    (1, 'Bluetooth Speaker', 'Waterproof portable speaker with 360° sound', 49.99, 90, '/images/7.png'),
    (2, 'Classic White Tee', '100% organic cotton, unisex fit', 19.99, 200, '/images/8.png'),
    (2, 'Slim Fit Jeans', 'Dark wash denim, stretch fabric', 54.99, 80, '/images/9.png'),
    (2, 'Hooded Sweatshirt', 'Fleece-lined hoodie, kangaroo pocket', 39.99, 150, '/images/10.png'),
    (2, 'Running Shoes', 'Lightweight mesh upper, cushioned sole', 84.99, 40, '/images/11.png'),
    (2, 'Wool Beanie', '100% merino wool, one size fits all', 14.99, 300, '/images/12.png'),
    (2, 'Leather Belt', 'Full-grain leather, brass buckle', 29.99, 100, '/images/13.png'),
    (3, 'Clean Code', 'A Handbook of Agile Software Craftsmanship — Robert C. Martin', 34.99, 50, '/images/14.png'),
    (3, 'Designing Data-Intensive Applications', 'Martin Kleppmann — the definitive guide to data systems', 44.99, 35, '/images/15.png'),
    (3, 'The Pragmatic Programmer', '20th Anniversary Edition — Hunt & Thomas', 39.99, 45, '/images/16.png'),
    (3, 'Atomic Habits', 'Tiny changes, remarkable results — James Clear', 16.99, 120, '/images/17.png'),
    (3, 'Refactoring', '2nd Edition — Martin Fowler', 49.99, 25, '/images/18.png'),
    (3, 'The Phoenix Project', 'A novel about IT, DevOps, and helping your business win', 14.99, 70, '/images/19.png'),
    (3, 'System Design Interview', 'An Insider''s Guide — Alex Xu, Volume 1', 29.99, 55, '/images/20.png');

-- Passwords are MD5 hashes
-- admin@shopwise.com  / Admin123!
-- jane@example.com    / Jane456!
-- john@example.com    / John789!
INSERT INTO users (email, password_hash, full_name, role) VALUES
    ('admin@shopwise.com',  md5('Admin123!'), 'Admin User', 'admin'),
    ('jane@example.com',    md5('Jane456!'),  'Jane Smith',  'customer'),
    ('john@example.com',    md5('John789!'),  'John Doe',    'customer');

INSERT INTO carts (user_id) VALUES (2), (3);

INSERT INTO cart_items (cart_id, product_id, quantity) VALUES
    (1, 1, 1),
    (1, 14, 2),
    (2, 3, 1);

INSERT INTO orders (user_id, status, total_amount) VALUES
    (2, 'delivered', 144.97),
    (2, 'shipped',   39.99),
    (2, 'pending',   109.99);

INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
    (1, 1, 1, 89.99),
    (1, 14, 3, 34.99),
    (2, 10, 1, 39.99),
    (3, 6, 1, 109.99);

INSERT INTO orders (user_id, status, total_amount) VALUES
    (3, 'delivered', 84.99),
    (3, 'cancelled', 19.99);

INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
    (4, 11, 1, 84.99),
    (5, 8,  1, 19.99);

-- ------------------------------------------------------------
-- Extra customers + order history so the recommendations
-- service has real co-purchase and popularity signal.
-- Passwords are MD5 hashes; all of these share password Customer1!
-- ------------------------------------------------------------
INSERT INTO users (email, password_hash, full_name, role) VALUES
    ('alice@example.com', md5('Customer1!'), 'Alice Brown',  'customer'),  -- id 4
    ('bob@example.com',   md5('Customer1!'), 'Bob Carter',   'customer'),  -- id 5
    ('carol@example.com', md5('Customer1!'), 'Carol Diaz',   'customer'),  -- id 6
    ('dave@example.com',  md5('Customer1!'), 'Dave Evans',   'customer'),  -- id 7
    ('erin@example.com',  md5('Customer1!'), 'Erin Foster',  'customer'),  -- id 8
    ('frank@example.com', md5('Customer1!'), 'Frank Green',  'customer'),  -- id 9
    ('grace@example.com', md5('Customer1!'), 'Grace Hill',   'customer');  -- id 10

-- Orders 6..41. Rows are listed in id order so the SERIAL ids match the
-- order_items references below. A couple are 'cancelled' on purpose — the
-- recommendation queries must ignore those.
INSERT INTO orders (user_id, status, total_amount) VALUES
    ( 2, 'delivered', 254.97),  -- 6   electronics dev kit
    ( 3, 'delivered', 219.98),  -- 7
    ( 4, 'shipped',   224.97),  -- 8
    ( 5, 'delivered', 219.96),  -- 9
    ( 6, 'delivered', 124.98),  -- 10
    ( 4, 'delivered', 189.98),  -- 11
    ( 7, 'pending',   199.98),  -- 12
    ( 8, 'delivered', 144.98),  -- 13
    ( 3, 'shipped',   314.96),  -- 14
    ( 9, 'delivered', 249.98),  -- 15  fitness
    ( 2, 'delivered', 124.97),  -- 16  tech books
    ( 3, 'delivered',  84.98),  -- 17
    ( 4, 'delivered',  74.98),  -- 18
    ( 5, 'shipped',    74.98),  -- 19
    ( 6, 'delivered', 169.96),  -- 20
    ( 7, 'delivered',  89.98),  -- 21
    ( 8, 'pending',    64.98),  -- 22
    ( 2, 'delivered',  48.97),  -- 23
    (10, 'delivered', 109.97),  -- 24
    ( 9, 'shipped',    69.98),  -- 25
    ( 3, 'delivered',  94.97),  -- 26  clothing
    ( 4, 'delivered', 114.97),  -- 27
    ( 5, 'delivered',  94.98),  -- 28
    ( 6, 'shipped',    74.97),  -- 29
    ( 7, 'delivered',  99.98),  -- 30
    ( 8, 'delivered',  89.95),  -- 31
    (10, 'pending',    84.98),  -- 32
    ( 2, 'delivered', 284.98),  -- 33  cross-category
    ( 9, 'delivered',  74.98),  -- 34
    ( 4, 'cancelled',  89.99),  -- 35  cancelled -> ignored
    ( 6, 'cancelled',  74.98),  -- 36  cancelled -> ignored
    ( 5, 'delivered', 234.97),  -- 37
    ( 3, 'delivered',  51.98),  -- 38
    ( 7, 'shipped',   249.98),  -- 39
    (10, 'delivered',  46.97),  -- 40
    ( 8, 'delivered', 254.97);  -- 41

INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
    -- Electronics: headphones/hub/keyboard/webcam frequently bought together
    ( 6,  1, 1,  89.99), ( 6,  2, 1, 34.99), ( 6,  3, 1, 129.99),
    ( 7,  1, 1,  89.99), ( 7,  3, 1, 129.99),
    ( 8,  2, 1,  34.99), ( 8,  3, 1, 129.99), ( 8,  4, 1, 59.99),
    ( 9,  1, 1,  89.99), ( 9,  2, 2, 34.99), ( 9,  4, 1, 59.99),
    (10,  1, 1,  89.99), (10,  2, 1, 34.99),
    (11,  3, 1, 129.99), (11,  4, 1, 59.99),
    (12,  1, 1,  89.99), (12,  6, 1, 109.99),
    (13,  2, 1,  34.99), (13,  6, 1, 109.99),
    (14,  1, 1,  89.99), (14,  2, 1, 34.99), (14, 3, 1, 129.99), (14, 4, 1, 59.99),
    (15,  5, 1, 199.99), (15,  7, 1,  49.99),
    -- Books: Clean Code / Pragmatic / Refactoring / DDIA / System Design
    (16, 14, 1,  34.99), (16, 16, 1, 39.99), (16, 18, 1, 49.99),
    (17, 14, 1,  34.99), (17, 18, 1, 49.99),
    (18, 14, 1,  34.99), (18, 16, 1, 39.99),
    (19, 15, 1,  44.99), (19, 20, 1, 29.99),
    (20, 14, 1,  34.99), (20, 15, 1, 44.99), (20, 16, 1, 39.99), (20, 18, 1, 49.99),
    (21, 16, 1,  39.99), (21, 18, 1, 49.99),
    (22, 14, 1,  34.99), (22, 20, 1, 29.99),
    (23, 17, 2,  16.99), (23, 19, 1, 14.99),
    (24, 14, 1,  34.99), (24, 15, 1, 44.99), (24, 20, 1, 29.99),
    (25, 16, 1,  39.99), (25, 20, 1, 29.99),
    -- Clothing: tee / jeans / hoodie / beanie
    (26,  8, 2,  19.99), (26,  9, 1, 54.99),
    (27,  8, 1,  19.99), (27,  9, 1, 54.99), (27, 10, 1, 39.99),
    (28,  9, 1,  54.99), (28, 10, 1, 39.99),
    (29,  8, 1,  19.99), (29, 10, 1, 39.99), (29, 12, 1, 14.99),
    (30, 11, 1,  84.99), (30, 12, 1, 14.99),
    (31,  8, 3,  19.99), (31, 12, 2, 14.99),
    (32,  9, 1,  54.99), (32, 13, 1, 29.99),
    (33, 11, 1,  84.99), (33,  5, 1, 199.99),
    (34,  8, 1,  19.99), (34,  9, 1, 54.99),
    -- Cancelled (must NOT contribute to recommendations)
    (35,  1, 1,  89.99),
    (36, 14, 1,  34.99), (36, 16, 1, 39.99),
    -- More cross-category mixes
    (37,  6, 1, 109.99), (37,  2, 1, 34.99), (37,  1, 1, 89.99),
    (38, 17, 1,  16.99), (38, 14, 1, 34.99),
    (39,  7, 1,  49.99), (39,  5, 1, 199.99),
    (40, 19, 2,  14.99), (40, 17, 1, 16.99),
    (41,  3, 1, 129.99), (41,  1, 1, 89.99), (41,  2, 1, 34.99);
