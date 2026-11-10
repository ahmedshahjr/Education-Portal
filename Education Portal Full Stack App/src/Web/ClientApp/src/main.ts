import { bootstrapApplication } from '@angular/platform-browser';
import { loadAppLocale, provideAppLocale } from './app/core/i18n/app-locale';

// The language comes first: Angular's runtime translations must be loaded before any app code that holds a $localize
// message is imported, so the app is imported only afterwards.
loadAppLocale()
  .then(async (locale) => {
    const [{ App }, { appConfig }] = await Promise.all([import('./app/app'), import('./app/app.config')]);
    await bootstrapApplication(App, { providers: [...appConfig.providers, ...provideAppLocale(locale)] });
  })
  .catch((error: unknown) => console.error(error));
