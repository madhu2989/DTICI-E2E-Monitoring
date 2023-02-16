import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChartsLegendComponent } from './charts-legend.component';

describe('ChartsLegendComponent', () => {
  let component: ChartsLegendComponent;
  let fixture: ComponentFixture<ChartsLegendComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChartsLegendComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChartsLegendComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
