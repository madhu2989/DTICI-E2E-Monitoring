import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AlertIgnoresEditComponent } from './alert-ignores-edit.component';

describe('AlertIgnoresEditComponent', () => {
  let component: AlertIgnoresEditComponent;
  let fixture: ComponentFixture<AlertIgnoresEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AlertIgnoresEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AlertIgnoresEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
