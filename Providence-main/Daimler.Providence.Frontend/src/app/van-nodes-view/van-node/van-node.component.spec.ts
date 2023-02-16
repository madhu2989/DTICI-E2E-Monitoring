import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VanNodeComponent } from './van-node.component';

describe('VanNodeComponent', () => {
  let component: VanNodeComponent;
  let fixture: ComponentFixture<VanNodeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VanNodeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VanNodeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
