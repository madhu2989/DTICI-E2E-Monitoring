import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SlaReportJobsOverviewComponent } from './sla-reports-jobs-overview.component';

describe('SlaReportJobsOverviewComponent', () => {
  let component: SlaReportJobsOverviewComponent;
  let fixture: ComponentFixture<SlaReportJobsOverviewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SlaReportJobsOverviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SlaReportJobsOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
