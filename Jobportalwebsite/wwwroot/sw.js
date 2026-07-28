// Minimal service worker — exists only to satisfy PWA installability requirements.
// Deliberately does NOT cache anything, so users always see fresh job listings,
// applications, and notifications rather than stale cached data.
self.addEventListener('install', () => {
    self.skipWaiting();
});

self.addEventListener('activate', () => {
    self.clients.claim();
});

self.addEventListener('fetch', () => {
    // Always pass through to the network — no caching, no offline fallback.
});