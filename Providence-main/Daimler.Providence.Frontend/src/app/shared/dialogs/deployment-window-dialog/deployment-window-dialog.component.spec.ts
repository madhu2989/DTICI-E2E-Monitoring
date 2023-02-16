import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DeploymentWindowComponent } from './deployment-window.component';

describe('DeploymentWindowComponent', () => {
  let component: DeploymentWindowComponent;
  let fixture: ComponentFixture<DeploymentWindowComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DeploymentWindowComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DeploymentWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
