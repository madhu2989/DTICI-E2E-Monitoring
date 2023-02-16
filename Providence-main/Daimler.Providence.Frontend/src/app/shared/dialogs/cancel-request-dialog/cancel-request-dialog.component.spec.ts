import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CancelRequestDialogComponent } from './cancel-request-dialog.component';

describe('CancelRequestDialogComponent', () => {
  let component: CancelRequestDialogComponent;
  let fixture: ComponentFixture<CancelRequestDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CancelRequestDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CancelRequestDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
