import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DepoymentWindowComponent } from './depoyment-window.component';

describe('DepoymentWindowComponent', () => {
  let component: DepoymentWindowComponent;
  let fixture: ComponentFixture<DepoymentWindowComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DepoymentWindowComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DepoymentWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
