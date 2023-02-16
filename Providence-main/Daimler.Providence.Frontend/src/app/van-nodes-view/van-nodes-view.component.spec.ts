import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VanNodesViewComponent } from './van-nodes-view.component';

describe('VanNodesViewComponent', () => {
  let component: VanNodesViewComponent;
  let fixture: ComponentFixture<VanNodesViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VanNodesViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VanNodesViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
