import { BrowserModule } from '@angular/platform-browser';
import { SecretService } from './shared/services/secret.service';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppComponent } from './app.component';
import { MaterialModule } from './material.module';
import { SharedModule } from './shared/shared.module';
import { DashboardModule } from './dashboard/dashboard.module';
import { AdalService, AdalInterceptor } from 'adal-angular4';
import { NgxLoadingModule } from 'ngx-loading';
import { VanNodesViewModule } from './van-nodes-view/van-nodes-view.module';
import { AppRoutingModule } from './app-routing.module';
import { AgGridModule } from 'ag-grid-angular';
import { DatePipe } from '@angular/common';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { DeploymentsModule } from './deployments/deployments.module';
import { ChecksModule } from './checks/checks.module';
import { AlertIgnoresModule } from './alert-ignores/alert-ignores.module';
import { ChangesModule } from './changelog/changes.module';
import { NotificationRulesModule } from './notification-rules/notificationRules.module';
import { SlaThresholdsModule } from './sla-thresholds/sla-thresholds.module';
import { StateIncreaseRulesModule } from './stateIncreaseRules/stateIncreaseRules.module';
import { LicensesModule } from './licenses/licenses.module';

import { UrlSerializer } from '@angular/router';
import { CustomUrlSerializer } from './CustomUrlSerializer';

import { EnvironmentService } from './shared/services/config.service';
import { AppInsightsMonitoringService } from './shared/services/app-insights-monitoring.service';
import { SettingsService } from './shared/services/settings.service';

const customUrlSerializer = new CustomUrlSerializer();
const CustomUrlSerializerProvider = {
provide: UrlSerializer,
useValue: customUrlSerializer
};

export function init_application(configLoader: EnvironmentService) : () => Promise<any> {
  return () : Promise<any> => {
    return new Promise(async (resolve) => {        
        var config = await configLoader.initConfiguration();
        await configLoader.setConfiguration(config); 
        resolve(true); 
    })
  }
}

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    SharedModule,
    DashboardModule,
    NgxLoadingModule.forRoot({}),
    HttpClientModule,
    VanNodesViewModule,
    AppRoutingModule,
    AgGridModule.withComponents([]),
    DeploymentsModule,
    AlertIgnoresModule,
    ChangesModule,
    ChecksModule,
    NotificationRulesModule,
    StateIncreaseRulesModule,
    SlaThresholdsModule,
    LicensesModule
  ],
  providers: [{
    provide: APP_INITIALIZER, 
    useFactory: init_application, 
    deps: [ EnvironmentService ], 
    multi: true
    }, 
    AdalService,
    SecretService,
    EnvironmentService,
    AppInsightsMonitoringService,
    SettingsService,
    DatePipe,
    { provide: HTTP_INTERCEPTORS, useClass: AdalInterceptor, multi: true },
    CustomUrlSerializerProvider ],
  bootstrap: [AppComponent]
})
export class AppModule { }
