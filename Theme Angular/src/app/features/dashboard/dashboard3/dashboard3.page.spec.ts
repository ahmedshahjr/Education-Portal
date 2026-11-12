import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { routes } from '../../../app.routes';
import { Dashboard3PageComponent } from './dashboard3.page';

describe('Dashboard3PageComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Dashboard3PageComponent],
      providers: [provideRouter(routes)],
    }).compileComponents();
  });

  it('should render all primary Dashboard 3 sections and titles', () => {
    const fixture = TestBed.createComponent(Dashboard3PageComponent);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const text = el.textContent ?? '';

    expect(text).toContain('Overall Balance');
    expect(text).toContain('$2,538,942');
    expect(text).toContain('Return On Investment');
    expect(text).toContain('Total followers');
    expect(text).toContain('Total Income');
    expect(text).toContain('Current Balance');
    expect(text).toContain('Marketing Report');
    expect(text).toContain('Payment Methods');
    expect(text).toContain('Recent Projects');
    expect(text).toContain('Delivery Analytics');
    expect(text).toContain('Earning Reports');
  });
});
