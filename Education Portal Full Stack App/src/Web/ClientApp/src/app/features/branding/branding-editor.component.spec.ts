import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { BrandingDto, ThemePreset } from '../../web-api-client';
import { BrandingChanges } from './branding-changes';
import { BrandingEditorComponent } from './branding-editor.component';

async function render(branding: BrandingDto) {
  TestBed.configureTestingModule({ imports: [BrandingEditorComponent] });
  const fixture = TestBed.createComponent(BrandingEditorComponent);
  fixture.componentRef.setInput('branding', branding);
  const saved: BrandingChanges[] = [];
  fixture.componentInstance.save.subscribe((changes) => saved.push(changes));
  await fixture.whenStable();
  return { fixture, element: fixture.nativeElement as HTMLElement, saved };
}

describe('BrandingEditorComponent', () => {
  const branding = new BrandingDto({ schoolName: 'Demo School', isMaster: false, themePreset: ThemePreset.Cyan, sidebarColor: undefined, logoVersion: undefined, faviconVersion: undefined });

  it('saves the chosen accent colour', async () => {
    const { element, saved, fixture } = await render(branding);

    element.querySelector<HTMLButtonElement>('button[aria-label="Royal violet"]')!.click();
    await fixture.whenStable();
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    expect(saved).toEqual([
      { themePreset: ThemePreset.Purple, sidebarColor: null, logo: null, removeLogo: false, favicon: null, removeFavicon: false },
    ]);
  });

  it('keeps the current sidebar colour and offers to remove the current logo', async () => {
    const { element, saved } = await render(
      new BrandingDto({ ...branding, sidebarColor: '#1f2a3d', logoVersion: 'abc' })
    );

    [...element.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.includes('Remove logo'))!.click();
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    expect(saved[0].sidebarColor).toBe('#1f2a3d');
    expect(saved[0].removeLogo).toBe(true);
  });
});
