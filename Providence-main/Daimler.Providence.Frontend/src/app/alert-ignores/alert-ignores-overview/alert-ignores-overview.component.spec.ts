import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AlertIgnoresOverviewComponent } from './alert-ignores-overview.component';

describe('AlertIgnoresOverviewComponent', () => {
  let component: AlertIgnoresOverviewComponent;
  let fixture: ComponentFixture<AlertIgnoresOverviewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AlertIgnoresOverviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AlertIgnoresOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
