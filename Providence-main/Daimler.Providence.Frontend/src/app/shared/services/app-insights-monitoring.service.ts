import { Injectable, Inject } from '@angular/core';
import { AppInsights } from 'applicationinsights-js';
import { EnvironmentService } from "../services/config.service"
import { Environment } from "../model/environment-config";

@Injectable({
  providedIn: "root",
})
export class AppInsightsMonitoringService {
  private environmentConfig: Environment;
  private config: Microsoft.ApplicationInsights.IConfig;

  private userId: string;

  constructor(public envService: EnvironmentService) {
    this.environmentConfig= envService.getConfiguration(); 
    {
      if(this.environmentConfig !=null) {
        this.config={
          instrumentationKey: this.environmentConfig.instrumentationKey
        };
      }      
    }
    if (!AppInsights.config) {
      AppInsights.downloadAndSetup(this.config);
    }
  }

  logPageView(name?: string, url?: string, properties?: any,
    measurements?: any, duration?: number) {
    AppInsights.trackPageView(name, url, properties, measurements, duration);
  }

  logEvent(name: string, properties?: any, measurements?: any) {
    AppInsights.trackEvent(name, properties, measurements);
  }

  setUser(userID: string): void {
    // Called when my app has identified the user.
    if (userID && userID !== this.userId) {
      this.userId = userID;
      const validatedId = userID.replace(/[,;=| ]+/g, "_");
      AppInsights.setAuthenticatedUserContext(validatedId);
    }

  }

}
