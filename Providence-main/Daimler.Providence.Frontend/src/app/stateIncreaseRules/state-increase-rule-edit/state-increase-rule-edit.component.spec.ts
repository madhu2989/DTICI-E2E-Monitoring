import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StateIncreaseRuleEditComponent } from './state-increase-rule-edit.component';

describe('StateIncreaseRuleEditComponent', () => {
  let component: StateIncreaseRuleEditComponent;
  let fixture: ComponentFixture<StateIncreaseRuleEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StateIncreaseRuleEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StateIncreaseRuleEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
