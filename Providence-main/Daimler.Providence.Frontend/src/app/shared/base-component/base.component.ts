import { Component, Injector, inject } from '@angular/core';
import { AppInsightsMonitoringService } from '../services/app-insights-monitoring.service';
import { SettingsService } from '../services/settings.service';
import { EnvironmentService } from "../services/config.service";

@Component({
  templateUrl: './base.component.html',
  providers: [EnvironmentService]
})
export class BaseComponent {
  // Manually retrieve the monitoring service from the injector
  // so that constructor has no dependencies that must be passed in from child
    
  private appInsightsMonitoringService = inject(AppInsightsMonitoringService);
  public settingsService = inject(SettingsService);

  constructor() {    
    this.logNavigation();
  }

  protected ngO

  private logNavigation() {
    this.appInsightsMonitoringService.logPageView();
  }
}