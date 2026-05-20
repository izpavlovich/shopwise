from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Service configuration, read from environment variables.

    `DATABASE_URL` points at the shared ShopWise Postgres instance. The default
    targets a local Postgres so the service runs outside Docker too.
    """

    database_url: str = "postgresql://shopwise:shopwise123@localhost:5432/shopwise"

    # asyncpg connection pool bounds.
    pool_min_size: int = 1
    pool_max_size: int = 10

    # How many recommendations to return when the caller does not ask.
    default_limit: int = 5
    # Upper bound the caller cannot exceed.
    max_limit: int = 50

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")


settings = Settings()
