import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NotificationOverviewComponent } from './notification-overview.component';

describe('NotificationOverviewComponent', () => {
  let component: NotificationOverviewComponent;
  let fixture: ComponentFixture<NotificationOverviewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ NotificationOverviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NotificationOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
