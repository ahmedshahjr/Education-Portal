import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { HarnessLoader } from '@angular/cdk/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { MatMenuHarness } from '@angular/material/menu/testing';
import { of } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';
import { APP_LOCALE, AppLocale, DEFAULT_LOCALE } from '../../../core/i18n/app-locale';
import { LocaleService } from '../../../core/i18n/locale.service';
import { LanguageMenuComponent } from './language-menu.component';

const pakistan: AppLocale = {
  ...DEFAULT_LOCALE,
  culture: 'en-PK',
  languages: [
    { code: 'en', nativeName: 'English', isRightToLeft: false },
    { code: 'ur', nativeName: 'اردو', isRightToLeft: true },
  ],
};

function setUp(locale: AppLocale) {
  TestBed.configureTestingModule({
    imports: [LanguageMenuComponent],
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: APP_LOCALE, useValue: locale }],
  });
  const fixture = TestBed.createComponent(LanguageMenuComponent);
  const loader: HarnessLoader = TestbedHarnessEnvironment.loader(fixture);
  return { fixture, loader };
}

describe('LanguageMenuComponent', () => {
  it('is hidden when the school offers one language', async () => {
    const { fixture } = setUp(DEFAULT_LOCALE);
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('button')).toBeNull();
  });

  it("lists the school's languages in their own script and switches to the chosen one", async () => {
    const { loader } = setUp(pakistan);
    const change = vi.spyOn(TestBed.inject(LocaleService), 'changeLanguage').mockReturnValue(of(undefined));

    const menu = await loader.getHarness(MatMenuHarness);
    expect(await menu.getTriggerText()).toContain('English');
    await menu.open();
    const items = await menu.getItems();
    const texts = await Promise.all(items.map((item) => item.getText()));
    expect(texts[0]).toContain('English');
    expect(texts[1]).toContain('اردو');

    await items[1].click();
    expect(change).toHaveBeenCalledWith('ur');
  });
});
