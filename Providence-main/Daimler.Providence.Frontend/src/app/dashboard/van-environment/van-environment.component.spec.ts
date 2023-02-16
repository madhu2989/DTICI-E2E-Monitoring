import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VanEnvironmentComponent } from './van-environment.component';

describe('VanEnvironmentComponent', () => {
  let component: VanEnvironmentComponent;
  let fixture: ComponentFixture<VanEnvironmentComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VanEnvironmentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VanEnvironmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
