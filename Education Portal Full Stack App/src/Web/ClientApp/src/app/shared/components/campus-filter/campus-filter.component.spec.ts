import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { CampusFilterComponent } from './campus-filter.component';

const campus = (id: string) => ({ id, name: `Campus ${id}` });

// AC4.1: shown only with more than one campus.
describe('CampusFilterComponent (AC4.1)', () => {
  function render(campuses: unknown[]) {
    const fixture = TestBed.createComponent(CampusFilterComponent);
    fixture.componentRef.setInput('campuses', campuses);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('is not rendered for a single campus (AC4.1)', () => {
    const element = render([campus('a')]);
    expect(element.querySelector('mat-select')).toBeNull();
  });

  it('is rendered for two campuses (AC4.1)', () => {
    const element = render([campus('a'), campus('b')]);
    expect(element.querySelector('mat-select')).not.toBeNull();
  });
});
