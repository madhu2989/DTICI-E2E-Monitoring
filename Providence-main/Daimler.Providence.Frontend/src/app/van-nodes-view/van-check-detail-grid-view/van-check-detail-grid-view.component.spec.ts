import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VanCheckDetailGridViewComponent } from './van-check-detail-grid-view.component';

describe('VanCheckDetailGridViewComponent', () => {
  let component: VanCheckDetailGridViewComponent;
  let fixture: ComponentFixture<VanCheckDetailGridViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VanCheckDetailGridViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VanCheckDetailGridViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
