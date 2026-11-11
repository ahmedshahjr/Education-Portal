import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { BrandingDto } from '../../web-api-client';
import { BrandingChanges } from './branding-changes';
import { BrandingEditorComponent } from './branding-editor.component';

async function render(branding: BrandingDto, imagesBase?: string) {
  TestBed.configureTestingModule({ imports: [BrandingEditorComponent] });
  const fixture = TestBed.createComponent(BrandingEditorComponent);
  fixture.componentRef.setInput('branding', branding);
  if (imagesBase) {
    fixture.componentRef.setInput('imagesBase', imagesBase);
  }
  const saved: BrandingChanges[] = [];
  fixture.componentInstance.save.subscribe((changes) => saved.push(changes));
  await fixture.whenStable();
  return { fixture, element: fixture.nativeElement as HTMLElement, saved };
}

describe('BrandingEditorComponent', () => {
  const branding = new BrandingDto({ schoolName: 'Demo School', isMaster: false, accentColor: '#00a1ff', sidebarColor: undefined, logoVersion: undefined, faviconVersion: undefined });

  it('saves the accent colour picked as a hex colour', async () => {
    const { element, saved, fixture } = await render(branding);

    const picker = element.querySelector<HTMLInputElement>('input[type="color"][aria-label="Pick the accent colour"]')!;
    picker.value = '#763EBD';
    picker.dispatchEvent(new Event('input'));
    await fixture.whenStable();
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    expect(saved).toEqual([
      { accentColor: '#763ebd', sidebarColor: null, logo: null, removeLogo: false, favicon: null, removeFavicon: false },
    ]);
  });

  it('does not save an accent colour that is not a hex colour', async () => {
    const { element, saved, fixture } = await render(branding);

    const hex = element.querySelector<HTMLInputElement>('input[maxlength="7"]')!;
    hex.value = 'purple';
    hex.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    expect(element.querySelector<HTMLButtonElement>('button[type="submit"]')!.disabled).toBe(true);
    expect(saved).toEqual([]);
  });

  it('previews the accent colour as the user changes it', async () => {
    const { element, fixture } = await render(branding);
    const preview = element.querySelector<HTMLElement>('.mm-branding__preview')!;

    const picker = element.querySelector<HTMLInputElement>('input[type="color"][aria-label="Pick the accent colour"]')!;
    picker.value = '#763ebd';
    picker.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    expect(preview.style.getPropertyValue('--mm-primary')).toBe('#763ebd');
    expect(preview.style.getPropertyValue('--mm-branding-preview-text')).toBe('#ffffff');
  });

  it('shows the current logo and favicon of the school being edited', async () => {
    const { element } = await render(
      new BrandingDto({ ...branding, logoVersion: 'abc', faviconVersion: 'def' }),
      'api/Schools/7/branding/images'
    );

    const sources = [...element.querySelectorAll<HTMLImageElement>('img.mm-branding__thumb')].map((img) => img.getAttribute('src'));
    expect(sources).toEqual(['api/Schools/7/branding/images/Logo?v=abc', 'api/Schools/7/branding/images/Favicon?v=def']);
  });

  it('hides the current favicon once the user removes it', async () => {
    const { element } = await render(new BrandingDto({ ...branding, faviconVersion: 'def' }));

    [...element.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.includes('Remove favicon'))!.click();
    await TestBed.inject(ApplicationRef).whenStable();

    expect(element.querySelector('img[alt="Current favicon"]')).toBeNull();
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
