import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StateIncreaseRuleOverviewComponent } from './state-increase-rule-overview.component';

describe('StateIncreaseRuleOverviewComponent', () => {
  let component: StateIncreaseRuleOverviewComponent;
  let fixture: ComponentFixture<StateIncreaseRuleOverviewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StateIncreaseRuleOverviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StateIncreaseRuleOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
