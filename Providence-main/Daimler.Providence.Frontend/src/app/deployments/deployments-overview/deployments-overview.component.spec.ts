import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DeploymentsOverviewComponent } from './deployments-overview.component';

describe('DeploymentsOverviewComponent', () => {
  let component: DeploymentsOverviewComponent;
  let fixture: ComponentFixture<DeploymentsOverviewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DeploymentsOverviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DeploymentsOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
