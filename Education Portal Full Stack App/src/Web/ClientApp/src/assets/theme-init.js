// Applies the saved colour theme before the first paint. It is a file rather than an inline script, because the
// Content-Security-Policy allows scripts only from the application itself (SEC-WEB-04).
(function () {
  try {
    var theme = localStorage.getItem('picoColorScheme');
    if (theme && theme !== 'auto') {
      document.documentElement.setAttribute('data-theme', theme);
    }
  } catch (e) {
    // Storage can be unavailable, for example in some private modes. The default theme then applies.
  }
})();
