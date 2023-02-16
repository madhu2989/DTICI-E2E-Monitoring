import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SlaChartDialogComponent } from './sla-chart-dialog.component';

describe('SlaChartDialogComponent', () => {
  let component: SlaChartDialogComponent;
  let fixture: ComponentFixture<SlaChartDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SlaChartDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SlaChartDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
