import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DeploymentsEditComponent } from './deployments-edit.component';

describe('DeploymentsEditComponent', () => {
  let component: DeploymentsEditComponent;
  let fixture: ComponentFixture<DeploymentsEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DeploymentsEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DeploymentsEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
