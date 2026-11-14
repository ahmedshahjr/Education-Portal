import { readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { describe, expect, it } from 'vitest';

// Run from the ClientApp folder, like `ng test`.
const LOCALE_DIR = join(process.cwd(), 'src', 'locale');

interface MessagesFile {
  readonly locale: string;
  readonly translations: Record<string, string>;
}

const read = (file: string): MessagesFile => JSON.parse(readFileSync(join(LOCALE_DIR, file), 'utf-8')) as MessagesFile;
const placeholders = (text: string) => (text.match(/\{\$[A-Z_]+\}/g) ?? []).sort();

const source = read('en/messages.json');
const targets = readdirSync(LOCALE_DIR, { withFileTypes: true })
  .filter((entry) => entry.isDirectory() && entry.name !== 'en')
  .map((entry) => `${entry.name}/messages.json`);

/**
 * Every translation file (one folder per language code) covers every message of `en/messages.json` (made by `ng extract-i18n`) with the same
 * placeholders, so no screen falls back to English and no value goes missing from a translated sentence.
 */
describe('translations', () => {
  it('exist for Arabic and Urdu', () => {
    expect(targets).toEqual(expect.arrayContaining(['ar/messages.json', 'ur/messages.json']));
  });

  for (const file of targets) {
    describe(file, () => {
      const target = read(file);

      it('translates every message and nothing else', () => {
        expect(Object.keys(target.translations).sort()).toEqual(Object.keys(source.translations).sort());
      });

      it('keeps every placeholder', () => {
        for (const [id, text] of Object.entries(source.translations)) {
          expect(placeholders(target.translations[id] ?? ''), id).toEqual(placeholders(text));
        }
      });
    });
  }
});
