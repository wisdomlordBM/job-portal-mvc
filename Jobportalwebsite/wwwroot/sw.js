// Minimal service worker — exists only to satisfy PWA installability requirements.
// No caching, no offline behavior — every request always goes to the network.
self.addEventListener('install', () => {
    self.skipWaiting();
});

self.addEventListener('activate', () => {
    self.clients.claim();
});