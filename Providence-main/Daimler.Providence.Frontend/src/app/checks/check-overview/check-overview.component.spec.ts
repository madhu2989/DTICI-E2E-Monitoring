import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckOverviewComponent } from './check-overview.component';

describe('CheckOverviewComponent', () => {
  let component: CheckOverviewComponent;
  let fixture: ComponentFixture<CheckOverviewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CheckOverviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
