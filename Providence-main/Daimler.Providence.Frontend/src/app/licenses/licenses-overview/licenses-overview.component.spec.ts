import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LicensesOverviewComponent } from './licenses-overview.component';

describe('NotificationOverviewComponent', () => {
  let component: LicensesOverviewComponent;
  let fixture: ComponentFixture<LicensesOverviewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LicensesOverviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LicensesOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
