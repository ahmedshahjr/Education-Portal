import {
  AcademicYearStatus,
  CalendarEventAudience,
  CalendarEventType,
  HolidayAudience,
  HolidayType,
} from '../../web-api-client';

/** One choice of a closed list, with its translated label. */
export interface LabelledOption<T> {
  readonly value: T;
  readonly label: string;
}

export function yearStatusOptions(): readonly LabelledOption<AcademicYearStatus>[] {
  return [
    { value: AcademicYearStatus.Planned, label: $localize`:@@academicCalendar.statusPlanned:Planned` },
    { value: AcademicYearStatus.Active, label: $localize`:@@academicCalendar.statusActive:Active` },
    { value: AcademicYearStatus.Closed, label: $localize`:@@academicCalendar.statusClosed:Closed` },
  ];
}

export function holidayTypeOptions(): readonly LabelledOption<HolidayType>[] {
  return [
    { value: HolidayType.Public, label: $localize`:@@academicCalendar.holidayPublic:Public holiday` },
    { value: HolidayType.Religious, label: $localize`:@@academicCalendar.holidayReligious:Religious holiday` },
    { value: HolidayType.Vacation, label: $localize`:@@academicCalendar.holidayVacation:Vacation` },
    { value: HolidayType.School, label: $localize`:@@academicCalendar.holidaySchool:School holiday` },
  ];
}

export function holidayAudienceOptions(): readonly LabelledOption<HolidayAudience>[] {
  return [
    { value: HolidayAudience.Both, label: $localize`:@@academicCalendar.audienceBoth:Students and staff` },
    { value: HolidayAudience.Students, label: $localize`:@@academicCalendar.audienceStudents:Students` },
    { value: HolidayAudience.Staff, label: $localize`:@@academicCalendar.audienceStaff:Staff` },
  ];
}

export function eventTypeOptions(): readonly LabelledOption<CalendarEventType>[] {
  return [
    { value: CalendarEventType.ParentTeacherMeeting, label: $localize`:@@academicCalendar.eventPtm:Parent-teacher meeting` },
    { value: CalendarEventType.Sports, label: $localize`:@@academicCalendar.eventSports:Sports` },
    { value: CalendarEventType.Exam, label: $localize`:@@academicCalendar.eventExam:Exam` },
    { value: CalendarEventType.Function, label: $localize`:@@academicCalendar.eventFunction:Function` },
    { value: CalendarEventType.Other, label: $localize`:@@academicCalendar.eventOther:Other` },
  ];
}

export function eventAudienceOptions(): readonly LabelledOption<CalendarEventAudience>[] {
  return [
    { value: CalendarEventAudience.All, label: $localize`:@@academicCalendar.audienceAll:Everyone` },
    { value: CalendarEventAudience.Students, label: $localize`:@@academicCalendar.audienceStudents:Students` },
    { value: CalendarEventAudience.Staff, label: $localize`:@@academicCalendar.audienceStaff:Staff` },
    { value: CalendarEventAudience.Parents, label: $localize`:@@academicCalendar.audienceParents:Parents` },
  ];
}

/** The label of a value from one of the lists above; an unknown value (a newer server) shows as it came. */
export function labelOf<T>(options: readonly LabelledOption<T>[], value: T | undefined): string {
  return options.find((option) => option.value === value)?.label ?? String(value ?? '');
}
