import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { API_BASE_URL, SchoolCalendarDto } from '../../web-api-client';
import { dateKey } from './academic-calendar.logic';

/**
 * Reads the school calendar for a date range. The generated client sends the `from` and `to` query dates with
 * `toISOString()` (a time and a UTC shift), which the API's `DateOnly` parameters refuse and which can move the day; this
 * sends them as plain `yyyy-MM-dd` and reads the answer with the generated DTO. Changes still go through the generated client.
 */
@Injectable({ providedIn: 'root' })
export class SchoolCalendarApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL, { optional: true }) ?? '';

  /** Reads the holidays, calendar events and (unless left out) the school events of a range. */
  read(from: Date, to: Date, campusId: string | null, includeSchoolEvents = true): Observable<SchoolCalendarDto> {
    let params = new HttpParams()
      .set('from', dateKey(from))
      .set('to', dateKey(to))
      .set('includeSchoolEvents', String(includeSchoolEvents));
    if (campusId) {
      params = params.set('campusId', campusId);
    }
    return this.http
      .get<unknown>(`${this.baseUrl}/api/SchoolCalendar`, { params })
      .pipe(map((body) => SchoolCalendarDto.fromJS(body)));
  }
}
