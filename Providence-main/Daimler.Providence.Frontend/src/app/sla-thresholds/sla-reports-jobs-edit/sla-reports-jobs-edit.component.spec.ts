import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SlaReportJobsEditComponent } from './sla-reports-jobs-edit.component';

describe('SlaReportsJobsEditComponent', () => {
  let component: SlaReportJobsEditComponent;
  let fixture: ComponentFixture<SlaReportJobsEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SlaReportJobsEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SlaReportJobsEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
