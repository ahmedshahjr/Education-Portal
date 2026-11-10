import { TestBed } from '@angular/core/testing';
import { DashboardDataService } from './dashboard-data.service';

describe('DashboardDataService', () => {
  let service: DashboardDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DashboardDataService);
  });

  it('should return 5 initial projects for the SaaS tab', () => {
    const saas = service.getProjectsForDashboardTab('SaaS');
    expect(saas.length).toBe(5);
    expect(saas[0]?.name).toBe('Photoshop');
  });

  it('should filter Mobile projects correctly', () => {
    const mobile = service.getProjectsForDashboardTab('Mobile');
    expect(mobile.length).toBeGreaterThan(0);
    expect(mobile.every((p) => p.category === 'Mobile')).toBe(true);
  });

  it('should add, update, and delete a project', () => {
    const initialCount = service.projects().length;

    service.addProject({
      name: 'New Telemetry Widget',
      brandMark: 'NT',
      brandColor: '#ffffff',
      brandBg: '#00a1ff',
      budget: 8200,
      extraMembers: 2,
      leader: 'Alex',
      category: 'SaaS',
      trendColor: '#00ceb6',
      status: 'Active',
    });

    expect(service.projects().length).toBe(initialCount + 1);
    const created = service.projects()[0];
    expect(created.name).toBe('New Telemetry Widget');

    service.updateProject(created.id, { budget: 9500 });
    expect(service.projects()[0].budget).toBe(9500);

    service.deleteProject(created.id);
    expect(service.projects().length).toBe(initialCount);
  });
});
