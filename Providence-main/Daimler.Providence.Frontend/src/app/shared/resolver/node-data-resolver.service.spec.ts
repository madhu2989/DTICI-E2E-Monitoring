import { TestBed, inject } from '@angular/core/testing';

import { NodeDataResolverService } from './node-data-resolver.service';

describe('NodeDataResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [NodeDataResolverService]
    });
  });

  it('should be created', inject([NodeDataResolverService], (service: NodeDataResolverService) => {
    expect(service).toBeTruthy();
  }));
});
