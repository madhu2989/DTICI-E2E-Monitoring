import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SlaThresholdsComponent } from './sla-thresholds.component';

describe('SlaThresholdsComponent', () => {
  let component: SlaThresholdsComponent;
  let fixture: ComponentFixture<SlaThresholdsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SlaThresholdsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SlaThresholdsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
