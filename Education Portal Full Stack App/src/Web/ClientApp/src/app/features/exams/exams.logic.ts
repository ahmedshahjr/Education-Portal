/** The part of a grade band the form and the table work with. */
export interface BandLike {
  readonly minPercent: number;
  readonly maxPercent: number;
  readonly letter: string;
  readonly points: number;
}

/** A scale's bands in one line, from the highest percent down: "A 90-100 (4)  B 80-89.99 (3)". */
export function describeBands(bands: readonly BandLike[]): string {
  return [...bands]
    .sort((a, b) => b.minPercent - a.minPercent)
    .map((band) => `${band.letter} ${band.minPercent}-${band.maxPercent} (${band.points})`)
    .join('  ');
}

/** True when two bands share some percent. The server also checks that the bands leave no gap. */
export function bandsOverlap(bands: readonly Pick<BandLike, 'minPercent' | 'maxPercent'>[]): boolean {
  const ordered = [...bands].sort((a, b) => a.minPercent - b.minPercent);
  return ordered.some((band, index) => index > 0 && band.minPercent <= ordered[index - 1].maxPercent);
}

/** The percent a score is worth, or `null` when the maximum is not above zero. */
export function scorePercent(score: number, maximumScore: number): number | null {
  return maximumScore > 0 ? Math.round((score / maximumScore) * 10000) / 100 : null;
}
