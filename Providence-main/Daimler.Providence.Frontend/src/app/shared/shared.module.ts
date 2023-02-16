import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms'; // <--- JavaScript import from Angular
import { MaterialModule } from '../material.module';
import { LoggedInGuard } from './logged-in.guard';
import { AuthHttp } from './services/authHttp.service';
import { HttpClientModule } from '@angular/common/http';
import { DataService } from './services/data.service';
import { HistoryService } from "./services/history.service";

import { FlexLayoutModule } from '@angular/flex-layout';
import { LoginComponent } from './login/login.component';
import { LogoutComponent } from './logout/logout.component';
import { RouterModule } from '@angular/router';
import { HistoryResolver } from "./resolver/history-resolver.service";
import { UserProfileComponent } from "./dialogs/user-profile/user-profile.component";
import { ErrorDialogComponent } from "./dialogs/error-dialog/error-dialog.component";
import { SlaChartDialogComponent } from './dialogs/sla-chart-dialog/sla-chart-dialog.component';
import { VanNodeService } from "./services/van-node.service";
import { SystemStateService } from "./services/system-state.service";
import { NodeDataResolverService } from './resolver/node-data-resolver.service';
import { BaseComponent } from './base-component/base.component';
import { SignalRService, createConfig } from './services/signal-r.service';
// import { SignalRModule } from 'ng2-signalr';
import { BreadcrumbModule } from '../van-nodes-view/breadcrumb/breadcrumb.module';
import { JsonViewDialogComponent } from './dialogs/json-view-dialog/json-view-dialog.component';
import { SettingsDialogComponent } from './dialogs/settings-dialog/settings-dialog.component';
import { DeploymentWindowService } from './services/deployment-window.service';
import { DeploymentWindowComponent } from './dialogs/deployment-window-dialog/deployment-window-dialog.component';
import { CrudToolbarComponent } from './crud-toolbar/crud-toolbar.component';
import { ConfirmationDialogComponent } from './dialogs/confirmation-dialog/confirmation-dialog.component';
import { DashboardEditComponent } from './dashboard-edit/dashboard-edit.component';
import { DashboardNodeEditComponent } from './dashboard-edit/dashboard-node-edit/dashboard-node-edit.component';
import { MasterDataService } from './services/masterdata.service';
import { NgxLoadingModule } from 'ngx-loading';
import { AgGridModule } from 'ag-grid-angular';
import { CancelRequestDialogComponent } from './dialogs/cancel-request-dialog/cancel-request-dialog.component';

import { ButtonRendererComponent } from './button-renderer/button-renderer.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    FlexLayoutModule,
    HttpClientModule,
    RouterModule,
   // SignalRModule.forRoot(createConfig),
    NgxLoadingModule.forRoot({}),
    BreadcrumbModule,
    AgGridModule.withComponents([ButtonRendererComponent])
  ],
  declarations: [
    UserProfileComponent,
    LoginComponent,
    LogoutComponent,
    ErrorDialogComponent,
    CancelRequestDialogComponent,
    BaseComponent,
    JsonViewDialogComponent,
    SettingsDialogComponent,
    DeploymentWindowComponent,
    CrudToolbarComponent,
    ConfirmationDialogComponent,
    DashboardEditComponent,
    DashboardNodeEditComponent,
    SlaChartDialogComponent,
    ButtonRendererComponent
  ],
  providers: [
    LoggedInGuard,
    AuthHttp,
    DataService,
    HistoryService,
    HistoryResolver,
    UserProfileComponent,
    LoginComponent,
    LogoutComponent,
    VanNodeService,
    SystemStateService,
    NodeDataResolverService,
    SignalRService,
    DeploymentWindowService,
    MasterDataService
  ],
  exports: [
    FlexLayoutModule,
    UserProfileComponent,
    LoginComponent,
    LogoutComponent,
    BreadcrumbModule,
    CrudToolbarComponent,
    DashboardEditComponent,
    DashboardNodeEditComponent,
    ButtonRendererComponent
  ]
})
export class SharedModule { }
