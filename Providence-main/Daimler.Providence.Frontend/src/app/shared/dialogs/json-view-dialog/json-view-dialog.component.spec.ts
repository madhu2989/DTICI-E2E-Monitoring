import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { JsonViewDialogComponent } from './json-view-dialog.component';

describe('JsonViewDialogComponent', () => {
  let component: JsonViewDialogComponent;
  let fixture: ComponentFixture<JsonViewDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ JsonViewDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(JsonViewDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
