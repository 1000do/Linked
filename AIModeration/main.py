"""FastAPI application entry point."""

import logging
import logging.config
from contextlib import asynccontextmanager
from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware

from config.settings import get_settings
from config.logger import setup_logging, LOGGING_CONFIG
from core.exceptions import ModerationException
from api.routes import moderation, health

# Setup logging
settings = get_settings()
setup_logging(settings.LOG_LEVEL)
logger = logging.getLogger(__name__)

# ============================================================================
# LIFESPAN HANDLER (Startup/Shutdown)
# ============================================================================

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Manage application lifecycle."""
    # Startup
    logger.info("=" * 80)
    logger.info(f"🚀 {settings.APP_NAME} v{settings.APP_VERSION} starting...")
    logger.info(f"Device: {settings.DEVICE}")
    logger.info(f"Redis: {settings.REDIS_HOST}:{settings.REDIS_PORT}")
    logger.info("Loading models...")
    
    try:
        # Pre-load models on startup
        from providers.ml_model_provider import MLModelProvider
        provider = MLModelProvider(settings)
        
        logger.info("Pre-loading Spam model...")
        provider.get_spam_model()
        logger.info("✓ Spam model ready")

        logger.info("Pre-loading Toxicity model...")
        provider.get_toxic_model()
        logger.info("✓ Toxicity model ready")
        
        logger.info("Pre-loading Text Embedding model...")
        provider.get_text_embedding_model()
        logger.info("✓ Text Embedding model ready")
        
        logger.info("Pre-loading CLIP model...")
        provider.get_clip_model()
        logger.info("✓ CLIP model ready")

        logger.info("Pre-loading Whisper model...")
        provider.get_whisper_model()
        logger.info("✓ Whisper model ready")

        logger.info("Pre-loading OCR model...")
        provider.get_ocr_reader()
        logger.info("✓ OCR model ready")
        
        logger.info("✓ All models loaded successfully")
        logger.info("=" * 80)
    
    except Exception as e:
        logger.error(f"✗ Startup failed: {e}")
        raise
    
    yield
    
    # Shutdown
    logger.info("🛑 Shutting down...")
    logger.info("Cleanup complete")

# ============================================================================
# FASTAPI APP CREATION
# ============================================================================

app = FastAPI(
    title=settings.APP_NAME,
    description="AI Moderation Service for Course Content",
    version=settings.APP_VERSION,
    docs_url="/docs",
    redoc_url="/redoc",
    openapi_url="/openapi.json",
    lifespan=lifespan,
)

# ============================================================================
# MIDDLEWARE
# ============================================================================

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, restrict to specific origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ============================================================================
# GLOBAL EXCEPTION HANDLERS
# ============================================================================

@app.exception_handler(ModerationException)
async def moderation_exception_handler(request: Request, exc: ModerationException):
    """Handle custom moderation exceptions."""
    logger.warning(f"Moderation exception: {exc.code} - {exc.message}")
    return JSONResponse(
        status_code=422,
        content={
            "error": exc.code,
            "message": exc.message,
            "details": exc.details,
        }
    )

@app.exception_handler(Exception)
async def general_exception_handler(request: Request, exc: Exception):
    """Handle unexpected exceptions."""
    logger.error(f"Unexpected error: {exc}", exc_info=True)
    return JSONResponse(
        status_code=500,
        content={
            "error": "internal_error",
            "message": "An unexpected error occurred",
            "detail": str(exc) if settings.DEBUG else "See logs for details",
        }
    )

# ============================================================================
# REQUEST/RESPONSE LOGGING
# ============================================================================

@app.middleware("http")
async def log_requests(request: Request, call_next):
    """Log all HTTP requests and responses."""
    import time
    
    start_time = time.time()
    response = await call_next(request)
    process_time = time.time() - start_time
    
    logger.info(
        f"{request.method} {request.url.path} - {response.status_code} ({process_time*1000:.2f}ms)"
    )
    
    return response

# ============================================================================
# ROUTE INCLUSION
# ============================================================================

app.include_router(health.router)
app.include_router(moderation.router)

# ============================================================================
# ROOT ENDPOINT
# ============================================================================

@app.get("/")
async def root():
    """Root endpoint."""
    return {
        "service": settings.APP_NAME,
        "version": settings.APP_VERSION,
        "status": "running",
        "docs": "/docs",
        "health": "/health",
    }

# ============================================================================
# MAIN
# ============================================================================

if __name__ == "__main__":
    import uvicorn
    
    uvicorn.run(
        app,
        host=settings.HOST,
        port=settings.PORT,
        log_level=settings.LOG_LEVEL.lower(),
        reload=settings.DEBUG,
    )
