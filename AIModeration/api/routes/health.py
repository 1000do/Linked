"""Health check endpoints."""

import logging
from fastapi import APIRouter, Depends
from core.models import HealthResponse
from repositories.cache_repository import CacheRepository
from config.settings import get_settings

logger = logging.getLogger(__name__)

router = APIRouter(tags=["health"])

def get_cache_repository() -> CacheRepository:
    """Get cache repository instance."""
    return CacheRepository()

@router.get("/health", response_model=HealthResponse)
async def health_check(cache_repo: CacheRepository = Depends(get_cache_repository)):
    """
    Health check endpoint for Docker healthcheck and monitoring.
    
    Returns:
        - status: "healthy", "degraded", or "unhealthy"
        - version: API version
        - details: Component-specific status
    """
    settings = get_settings()
    
    try:
        # Check Redis
        redis_healthy = cache_repo.health_check()
        
        status = "healthy" if redis_healthy else "degraded"
        
        return HealthResponse(
            status=status,
            version=settings.APP_VERSION,
            details={
                "redis": "✓ connected" if redis_healthy else "✗ disconnected",
                "app_name": settings.APP_NAME,
                "device": settings.DEVICE,
            }
        )
    
    except Exception as e:
        logger.error(f"Health check failed: {e}")
        return HealthResponse(
            status="unhealthy",
            version=settings.APP_VERSION,
            details={
                "error": str(e),
                "app_name": settings.APP_NAME,
            }
        )
