/**
 * The size rule for add and edit forms (ADR-024): a form is a dialog only when it fits a modal, otherwise it is a page.
 * A modal holds about five fields and a short list; a grid (a table with a row per item) or more fields goes to a page.
 */
export interface FormSize {
  /** The number of input fields the form shows (text, select, checkbox list counts as one). */
  readonly fields: number;
  /** The number of rows of any grid or table in the form (0 when there is none). */
  readonly gridRows: number;
}

/** The most fields a dialog holds, and the most rows a grid may have inside one. */
export const DIALOG_LIMITS = { maxFields: 5, maxGridRows: 6 } as const;

/** True when the form fits a dialog; false means it should be a page. */
export function fitsDialog(size: FormSize): boolean {
  return size.fields <= DIALOG_LIMITS.maxFields && size.gridRows <= DIALOG_LIMITS.maxGridRows;
}
