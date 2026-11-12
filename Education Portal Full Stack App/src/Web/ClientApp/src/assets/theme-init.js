// Applies the saved light or dark mode before the first paint, so the page does not flash. It is a file rather than an
// inline script, because the Content-Security-Policy allows scripts only from the application itself (SEC-WEB-04).
// The key matches LAYOUT_STORAGE_KEY in src/app/core/layout/layout.service.ts.
(function () {
  try {
    var saved = JSON.parse(localStorage.getItem('education_portal_layout_prefs_v1') || '{}');
    if (saved.themeMode === 'dark') {
      document.documentElement.classList.add('dark-theme');
    }
  } catch (e) {
    // Storage can be unavailable or hold an old value, for example in some private modes. The light theme then applies.
  }
})();
