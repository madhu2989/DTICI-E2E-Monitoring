import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangeOverviewComponent } from './change-overview.component';

describe('ChangeOverviewComponent', () => {
  let component: ChangeOverviewComponent;
  let fixture: ComponentFixture<ChangeOverviewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChangeOverviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChangeOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
