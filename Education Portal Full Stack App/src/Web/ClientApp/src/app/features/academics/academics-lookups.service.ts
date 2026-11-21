import { inject, Injectable, signal } from '@angular/core';
import { problemMessages } from '../../core/http/problem-details';
import {
  ClassOfferingDto,
  ClassOfferingsClient,
  CourseDto,
  CoursesClient,
  UserDetailsClient,
  UserDetailsDto,
  UserType,
} from '../../web-api-client';

/**
 * The lists the academic screens pick from: the class offerings, the courses, and the school's teachers and students. A screen
 * asks for what it needs; each list is read again every time, so a screen never shows a list that was changed elsewhere. A
 * failure is kept as a message for the screen to show.
 */
@Injectable({ providedIn: 'root' })
export class AcademicsLookups {
  private readonly offeringsClient = inject(ClassOfferingsClient);
  private readonly coursesClient = inject(CoursesClient);
  private readonly peopleClient = inject(UserDetailsClient);

  readonly offerings = signal<readonly ClassOfferingDto[]>([]);
  readonly courses = signal<readonly CourseDto[]>([]);
  readonly teachers = signal<readonly UserDetailsDto[]>([]);
  readonly students = signal<readonly UserDetailsDto[]>([]);
  readonly error = signal<string | null>(null);

  loadOfferings(): void {
    this.offeringsClient.getClassOfferings(undefined).subscribe({
      next: (rows) => this.offerings.set(rows),
      error: (error: unknown) => this.error.set(problemMessages(error)[0]),
    });
  }

  loadCourses(): void {
    this.coursesClient.getCourses(undefined).subscribe({
      next: (rows) => this.courses.set(rows),
      error: (error: unknown) => this.error.set(problemMessages(error)[0]),
    });
  }

  loadTeachers(): void {
    this.peopleClient.getUserDetails(UserType.Teacher).subscribe({
      next: (rows) => this.teachers.set(rows),
      error: (error: unknown) => this.error.set(problemMessages(error)[0]),
    });
  }

  loadStudents(): void {
    this.peopleClient.getUserDetails(UserType.Student).subscribe({
      next: (rows) => this.students.set(rows),
      error: (error: unknown) => this.error.set(problemMessages(error)[0]),
    });
  }
}
