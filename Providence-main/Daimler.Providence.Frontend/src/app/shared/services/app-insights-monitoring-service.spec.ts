import { TestBed, inject } from '@angular/core/testing';

import { AppInsightsMonitoringService } from './app-insights-monitoring.service';

describe('AppInsightsMonitoringServiceService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AppInsightsMonitoringService]
    });
  });

  it('should be created', inject([AppInsightsMonitoringService], (service: AppInsightsMonitoringService) => {
    expect(service).toBeTruthy();
  }));
});
