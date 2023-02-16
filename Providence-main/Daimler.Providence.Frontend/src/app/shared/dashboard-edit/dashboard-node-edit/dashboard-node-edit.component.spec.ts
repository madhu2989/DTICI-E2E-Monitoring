import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardNodeEditComponent } from './dashboard-node-edit.component';

describe('DashboardNodeEditComponent', () => {
  let component: DashboardNodeEditComponent;
  let fixture: ComponentFixture<DashboardNodeEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DashboardNodeEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DashboardNodeEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
