import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LoggedInGuard } from './shared/logged-in.guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { VanNodesViewComponent } from './van-nodes-view/van-nodes-view.component';
import { LoginComponent } from './shared/login/login.component';
import { LogoutComponent } from './shared/logout/logout.component';
import { HistoryResolver } from "./shared/resolver/history-resolver.service";
import { VanCheckDetailGridViewComponent } from './van-nodes-view/van-check-detail-grid-view/van-check-detail-grid-view.component';
import { NodeDataResolverService } from './shared/resolver/node-data-resolver.service';
import { AlertIgnoresOverviewComponent } from './alert-ignores/alert-ignores-overview/alert-ignores-overview.component';
import { ChangeOverviewComponent } from './changelog/change-overview/change-overview.component';
import { CheckOverviewComponent } from './checks/check-overview/check-overview.component';
import { DeploymentsOverviewComponent } from './deployments/deployments-overview/deployments-overview.component';
import { NotificationOverviewComponent } from './notification-rules/notification-overview/notification-overview.component';
import { SlaThresholdsComponent } from './sla-thresholds/sla-thresholds/sla-thresholds.component';
import { SlaReportJobsOverviewComponent } from './sla-thresholds/sla-reports-jobs-overview/sla-reports-jobs-overview.component';
import { StateIncreaseRuleOverviewComponent } from './stateIncreaseRules/state-increase-rule-overview/state-increase-rule-overview.component';
import { LicensesOverviewComponent } from './licenses/licenses-overview/licenses-overview.component';



const appRoutes: Routes = [
  { path: 'dashboard', component: DashboardComponent, canActivate: [LoggedInGuard] },
  { path: 'deployments', component: DeploymentsOverviewComponent, canActivate: [LoggedInGuard] },
  { path: 'checks', component: CheckOverviewComponent, canActivate: [LoggedInGuard] },
  { path: 'changelog', component: ChangeOverviewComponent, canActivate: [LoggedInGuard] },
  { path: 'ignores', component: AlertIgnoresOverviewComponent, canActivate: [LoggedInGuard] },
  { path: 'notifications', component: NotificationOverviewComponent, canActivate: [LoggedInGuard] },
  { path: 'stateIncreaseRules', component: StateIncreaseRuleOverviewComponent, canActivate: [LoggedInGuard] },
  { path: 'slaThresholds', component: SlaThresholdsComponent, canActivate: [LoggedInGuard] },
  { path: 'licenses', component: LicensesOverviewComponent, canActivate: [LoggedInGuard] },
  { path: 'slaReportsJobs', component: SlaReportJobsOverviewComponent, canActivate: [LoggedInGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'logout', component: LogoutComponent },
  { path: ':environmentId', component: VanNodesViewComponent, resolve: { historyData: HistoryResolver }, canActivate: [LoggedInGuard], runGuardsAndResolvers: 'always' },
  { path: ':environmentId/:serviceId', component: VanNodesViewComponent, resolve: { historyData: HistoryResolver }, canActivate: [LoggedInGuard], runGuardsAndResolvers: 'always' },
  { path: ':environmentId/:serviceId/:actionId', component: VanNodesViewComponent, resolve: { historyData: HistoryResolver }, canActivate: [LoggedInGuard], runGuardsAndResolvers: 'always' },
  // tslint:disable-next-line:max-line-length
  { path: ':environmentId/:serviceId/:actionId/:componentId', component: VanCheckDetailGridViewComponent, resolve: { historyData: HistoryResolver, nodeData: NodeDataResolverService }, canActivate: [LoggedInGuard], runGuardsAndResolvers: 'always' }
  , {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(appRoutes, { onSameUrlNavigation: 'reload', enableTracing: false})],
  exports: [RouterModule]
})
export class AppRoutingModule { }