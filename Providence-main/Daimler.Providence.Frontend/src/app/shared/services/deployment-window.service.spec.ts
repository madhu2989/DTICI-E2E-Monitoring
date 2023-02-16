import { TestBed, inject } from '@angular/core/testing';

import { DeploymentWindowService } from './deployment-window.service';

describe('DeploymentWindowService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DeploymentWindowService]
    });
  });

  it('should be created', inject([DeploymentWindowService], (service: DeploymentWindowService) => {
    expect(service).toBeTruthy();
  }));
});
