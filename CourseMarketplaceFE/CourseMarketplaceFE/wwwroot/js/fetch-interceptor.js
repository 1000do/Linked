// Global Fetch Interceptor for Anti-Forgery Token
(function () {
    const originalFetch = window.fetch;
    window.fetch = async function () {
        let [resource, config] = arguments;
        if (config && config.method && ['POST', 'PUT', 'DELETE', 'PATCH'].includes(config.method.toUpperCase())) {
            
            let urlStr = '';
            if (typeof resource === 'string') {
                urlStr = resource;
            } else if (resource instanceof Request) {
                urlStr = resource.url;
            } else if (resource && resource.toString) {
                urlStr = resource.toString();
            }

            // Only send anti-forgery token to internal/allowed origins
            // to prevent CORS preflight errors with third-party integrations
            let isAllowed = false;
            
            if (!urlStr.startsWith('http')) {
                // Relative URLs are inherently internal
                isAllowed = true;
            } else {
                let allowedOrigins = [];
                
                // Add origins injected from the backend configuration (.env)
                if (window.APP_ALLOWED_ORIGINS) {
                    const originsFromEnv = window.APP_ALLOWED_ORIGINS.split(',').map(o => o.trim()).filter(o => o.length > 0);
                    allowedOrigins = allowedOrigins.concat(originsFromEnv);
                }
                
                // Always allow the current frontend origin
                allowedOrigins.push(window.location.origin);
                
                // Allow API_BASE_URL if it's defined (e.g., in _InstructorLayout)
                if (typeof API_BASE_URL !== 'undefined') {
                    try {
                        allowedOrigins.push(new URL(API_BASE_URL).origin);
                    } catch (e) {}
                }

                // Check if the request URL starts with any of the allowed origins
                isAllowed = allowedOrigins.some(origin => urlStr.startsWith(origin));
            }

            if (isAllowed) {
                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenElement) {
                    config.headers = config.headers || {};
                    if (config.headers instanceof Headers) {
                        if (!config.headers.has('RequestVerificationToken')) {
                            config.headers.append('RequestVerificationToken', tokenElement.value);
                        }
                    } else {
                        if (!config.headers['RequestVerificationToken']) {
                            config.headers['RequestVerificationToken'] = tokenElement.value;
                        }
                    }
                }
            }
        }
        return originalFetch(resource, config);
    };
})();
