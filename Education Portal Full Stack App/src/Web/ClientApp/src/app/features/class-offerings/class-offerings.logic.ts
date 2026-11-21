/** The academic year: four digits, or a school year such as 2026-27 (the same rule as the server). */
export const ACADEMIC_YEAR_PATTERN = /^\d{4}(-\d{2})?$/;

/** An ISO 4217 currency code: three letters (the server keeps it in capitals). */
export const CURRENCY_PATTERN = /^[A-Za-z]{3}$/;

/** The part of a class offering the list needs: a school-wide offering has no campus (`null`). */
export interface CampusScopedRow {
  readonly campusId: string | null | undefined;
}

/**
 * The rows the campus filter lets through (AC4.2). "All campuses" (`selectedCampusId` null) shows every row. A campus shows
 * its own rows and the school-wide rows, because a school-wide offering applies to every campus.
 */
export function rowsForCampus<T extends CampusScopedRow>(rows: readonly T[], selectedCampusId: string | null): readonly T[] {
  if (selectedCampusId === null) {
    return rows;
  }
  return rows.filter((row) => row.campusId == null || row.campusId === selectedCampusId);
}
