import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SlaReportsComponent } from './sla-reports.component';

describe('SlaReportsComponent', () => {
  let component: SlaReportsComponent;
  let fixture: ComponentFixture<SlaReportsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SlaReportsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SlaReportsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
