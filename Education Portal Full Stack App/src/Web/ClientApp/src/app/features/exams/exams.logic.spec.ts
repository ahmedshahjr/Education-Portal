import { describe, expect, it } from 'vitest';
import { bandsOverlap, describeBands, scorePercent } from './exams.logic';

describe('describeBands', () => {
  it('lists the bands from the highest percent down', () => {
    const text = describeBands([
      { minPercent: 0, maxPercent: 49.99, letter: 'F', points: 0 },
      { minPercent: 50, maxPercent: 100, letter: 'A', points: 4 },
    ]);

    expect(text).toBe('A 50-100 (4)  F 0-49.99 (0)');
  });

  it('is empty for no bands', () => {
    expect(describeBands([])).toBe('');
  });
});

describe('bandsOverlap', () => {
  it('accepts bands that follow each other', () => {
    expect(bandsOverlap([{ minPercent: 0, maxPercent: 49 }, { minPercent: 50, maxPercent: 100 }])).toBe(false);
  });

  it('finds two bands that share a percent, in any order', () => {
    expect(bandsOverlap([{ minPercent: 50, maxPercent: 100 }, { minPercent: 0, maxPercent: 50 }])).toBe(true);
  });

  it('accepts one band or none', () => {
    expect(bandsOverlap([{ minPercent: 0, maxPercent: 100 }])).toBe(false);
    expect(bandsOverlap([])).toBe(false);
  });
});

describe('scorePercent', () => {
  it('turns a score into a percent with two decimals', () => {
    expect(scorePercent(45, 60)).toBe(75);
    expect(scorePercent(1, 3)).toBe(33.33);
  });

  it('has no percent when the maximum is not above zero', () => {
    expect(scorePercent(5, 0)).toBeNull();
  });
});
