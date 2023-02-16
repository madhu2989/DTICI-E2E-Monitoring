import { Component, ViewEncapsulation, OnInit, NgZone } from '@angular/core';
import { SecretService } from './shared/services/secret.service';
import { Router, NavigationEnd, ActivatedRoute, RouterEvent, NavigationCancel, NavigationError, NavigationStart, Scroll } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { UserProfileComponent } from "./shared/dialogs/user-profile/user-profile.component";
import { SettingsService } from './shared/services/settings.service';
import { AppInsightsMonitoringService } from './shared/services/app-insights-monitoring.service';
import { DataService } from './shared/services/data.service';
import { HttpErrorResponse } from "@angular/common/http";
import { AdalService } from "adal-angular4";
import { interval } from 'rxjs';
import { filter } from 'rxjs/operators';
import { SettingsDialogComponent } from './shared/dialogs/settings-dialog/settings-dialog.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  encapsulation: ViewEncapsulation.None,
  preserveWhitespaces: false
})



export class AppComponent implements OnInit {

  title = 'E2E Monitoring Service ';
  currentUser = null;
  currentUserName = "Not logged in.";
  //  isCurrentUserOnlyViewer: boolean;
  environmentName = "Loading...";
  notificationsButtonActive = false;
  lastRouterEvent: Date = null;
  lastRouteUrl: string;
  public showloadingIndicator: boolean;
  private zone: NgZone;
  scrollYPosition: any;

  constructor(
    private adalService: AdalService,
    private secretService: SecretService,
    private router: Router,
    private route: ActivatedRoute,
    public dialog: MatDialog,
    public settingsService: SettingsService,
    private appInsightsMonitoringService: AppInsightsMonitoringService,
    private dataService: DataService,
    zone: NgZone
  ) {
    this.adalService.init(this.secretService.adalConfig);
    // try to refresh token every 30min
    interval(1000 * 60 * 30).subscribe(() => this.checkToken());
    // check if page reload is necessary every 30min
    interval(1000 * 60 * 30).subscribe(() => this.handlePageReload());
    this.zone = zone;
  }

  ngOnInit(): void {


    // extract query parameters from localstorage as they were removed from callback URL during login
    let redirectUrlAfterLogin: string = null;
    if (window.location.hash && window.location.hash.length > 0) {
      redirectUrlAfterLogin = localStorage.getItem("adal.login.request").replace(window.location.origin, '').replace(window.location.href, '');
      let queryParams = redirectUrlAfterLogin.split('?')[1];
      if (queryParams) {
        // looks something like the encoded version of: "returnUrl=/Dev01?overview=true"
        queryParams = decodeURIComponent(queryParams).split('?')[1];
        if (queryParams) {
          redirectUrlAfterLogin = window.location.pathname + "?" + queryParams;
        }
      }
    }

    this.adalService.handleWindowCallback();
    this.adalService.getUser();
    this.currentUser = this.adalService.userInfo;


    if (this.currentUser && this.currentUser.profile) {
      this.currentUserName = this.currentUser.profile.name;
      this.settingsService.currentUserRoles = this.currentUser.profile.roles;
      if (this.currentUser.profile.roles && this.currentUser.profile.roles.length > 0 &&
        (this.currentUser.profile.roles.includes("Monitoring_admin") || this.currentUser.profile.roles.includes("Monitoring_contributor"))
      ) {
        // this.isCurrentUserOnlyViewer = false;
        this.settingsService.isCurrentUserOnlyViewer = false;
        if (this.currentUser.profile.roles.includes("Monitoring_admin")) {
          this.settingsService.isCurrentUserAdmin = true;
        } else {
          this.settingsService.isCurrentUserAdmin = false;
        }
      } else {
        // this.isCurrentUserOnlyViewer = true;
        this.settingsService.isCurrentUserOnlyViewer = true;
        this.settingsService.isCurrentUserAdmin = false;
      }
    }

    // enable reload page after second click on routerLink
    this.router.routeReuseStrategy.shouldReuseRoute = function () {
      return false;
    };

    this.router.events.subscribe((event: RouterEvent) => {
      if (event instanceof NavigationStart) {
        this.showloadingIndicator = true;
      }
      if (event instanceof NavigationCancel || event instanceof NavigationError) {
        this.showloadingIndicator = false;
      }

      if (event instanceof NavigationEnd) {
        this.showloadingIndicator = false;
        if (this.lastRouteUrl !== this.router.url) {
          this.lastRouterEvent = new Date();
        }
        this.lastRouteUrl = this.router.url;
        const contentContainer = document.querySelector('.mat-sidenav-content') || window;
        contentContainer.scrollTo(0, 0);
      }
    });

    const parsedUrl = this.router.parseUrl(redirectUrlAfterLogin ? redirectUrlAfterLogin : window.location.pathname + window.location.search);
    if (parsedUrl && parsedUrl.queryParams) {
      this.settingsService.handleQueryParameters(parsedUrl.queryParams);
    }




    this.dataService.environmentDataUpdated.subscribe((environmentName) => {
      if (environmentName && environmentName.length > 0) {
        let doRefresh = false;
        if (environmentName === "housekeeping") {
          if ((!this.router.url.includes("/dashboard")) && (!this.router.url.includes("/deployments")) && (!this.router.url.includes("/ignores")) &&
          (!this.router.url.includes("/checks")) && (!this.router.url.includes("/notifications")) && (!this.router.url.includes("/stateIncreaseRules")) && (!this.router.url.includes("/changelog")) &&
          (!this.router.url.includes("/slaReportsJobs")) && (!this.router.url.includes("/slaThresholds"))) {
            // only refresh if we're not on the dashboard / deployments /ignore / checks / notifications view as nothing will change due to housekeeping there
            console.log("Refreshing view after housekeeping");
            doRefresh = true;
          }
        } else {
          console.log("Environment data updated for " + environmentName);

          if (this.router.url.indexOf(environmentName) > -1) {
            console.log("Refreshing view after data change");
            doRefresh = true;
          } else {
            console.log("Not relevant for current screen");
          }
        }


        if (doRefresh) {
          const url = decodeURIComponent(this.getUrlWithoutOptionalParameter(this.router.url));
          this.router.navigate([url], { queryParamsHandling: 'preserve', skipLocationChange: true });
        }

      }
    });

    this.zone.onUnstable.subscribe(() => {
      this.scrollYPosition = document.querySelector('.mat-sidenav-content').scrollTop;
    });

    this.zone.onStable.subscribe(() => {
      document.querySelector('.mat-sidenav-content').scrollTop = this.scrollYPosition;
    });

    this.dataService.environmentLogSystemStateChanged.subscribe((environmentName) => {
      if (environmentName && environmentName.length > 0) {
        console.log("LogSystemStateChanged for " + environmentName);

        if (this.router.url.indexOf(environmentName) > -1) {
          console.log("Refreshing view");
          const url = decodeURIComponent(this.getUrlWithoutOptionalParameter(this.router.url));
          this.router.navigate([url], { queryParamsHandling: 'preserve' });
        } else {
          console.log("Not relevant for current screen");
        }

      }
    });

    
      this.dataService.environmentsTreeChanged.subscribe((environmentName) => {
        if (environmentName && environmentName.length > 0) {
          console.log("Environment tree has changed for " + environmentName);
          if (this.router.url.indexOf(environmentName) > -1) {
            console.log("Refreshing view");
            const url = decodeURIComponent(this.getUrlWithoutOptionalParameter(this.router.url));
            this.router.navigate([url], { queryParamsHandling: 'preserve' });
          } else {
            console.log("Not relevant for current screen");
          }
  
        }
      });
       
      this.dataService.dashboardChanged.subscribe((environmentName) => {
        if (environmentName && environmentName.length > 0) {
        console.log("Dashboard view has changed");

        if (this.router.url.indexOf("dashboard") > -1) {
          console.log("Refreshing dashboard");
          const url = decodeURIComponent(this.getUrlWithoutOptionalParameter(this.router.url));
          this.router.navigate([url], { queryParamsHandling: 'preserve' });
        } else {
          console.log("Not relevant for current screen");
        }

        }
      });


    this.dataService.environmentDeploymentWindowsDataChanged.subscribe((environmentName) => {
      if (environmentName) {
        console.log("Deployment Windows data changed for " + environmentName);

        if (this.router.url.indexOf(environmentName) > -1) {
          console.log("Refreshing view");
          const url = decodeURIComponent(this.getUrlWithoutOptionalParameter(this.router.url));
          this.router.navigate([url], { queryParamsHandling: 'preserve' });
        } else {
          console.log("Not relevant for current screen");
        }

      }
    });

    if (redirectUrlAfterLogin) {
      this.router.navigate([redirectUrlAfterLogin], { queryParamsHandling: 'preserve' });
    }

  }


  toggleUserProfilePanel() {
    const dialogRef = this.dialog.open(UserProfileComponent, {
      width: '400px',
      height: '160px',
      position: {
        top: '64px',
        right: '32px',
      },
      disableClose: false,
      data: { userProfile: this.currentUser.profile }
    });
  }

  refreshView() {
    const me = this;
    
    const url = decodeURIComponent(me.getUrlWithoutOptionalParameter(me.router.url));
    // const environmentId = url.split('/');
    // const forceRefresh = true;
    const refreshPromises = [];
    me.showloadingIndicator = true;
    
    /*
    if (environmentId[1] === 'dashboard') {
      refreshPromises.push(me.dataService.getAllEnvironments(forceRefresh));
      me.dataService.history = [];
    } else {
      refreshPromises.push(me.dataService.getAllEnvironments(forceRefresh));
      refreshPromises.push(me.dataService.getHistoryOfElementId(forceRefresh, environmentId[1]));
    }*/

    me.dataService.clearData();

    Promise.all(refreshPromises).then(
      response => {
        me.showloadingIndicator = false;
        me.router.navigate([url], { queryParamsHandling: 'preserve', replaceUrl: true });
      }, (error: HttpErrorResponse) => {
        me.showloadingIndicator = false;
        return Promise.reject(error.message || error);
      }
    );

  }


  getUrlWithoutOptionalParameter(url: string): string {
    if (url.indexOf("?") !== -1) {
      return url.substr(0, url.indexOf("?"));
    } else if (url.indexOf(";checkId") !== -1) {
      return url.substr(0, url.indexOf(";checkId"));
    } else if (url.indexOf(";elementId") !== -1) {
      return url.substr(0, url.indexOf(";elementId"));
    } else {
      return url;
    }
  }

  toogleSettingsPanel() {
    const dialogRef = this.dialog.open(SettingsDialogComponent, {
      width: '400px',
      height: '210px',
      position: {
        top: '64px',
        right: '32px',
      },
      disableClose: false,
      data: { userProfile: this.currentUser.profile }
    });
  }

  navigateHome() {
    const me = this;
    me.router.navigate(['/dashboard'], { queryParamsHandling: 'preserve' });
  }

  private handlePageReload() {
    if (this.lastRouterEvent) {

      const dateDiff = new Date().getTime() - this.lastRouterEvent.getTime();
      const dateDiffInMinutes = Math.floor(dateDiff / (1000 * 60));

      // console.log("Idle for " + Math.floor(dateDiff / (1000)) + " seconds");

      if (dateDiffInMinutes > (60 * 4)) {
        console.log("Refreshing Page after 4h of inactivity...");
        setTimeout(() => {
          window.location.reload();
        }, 5000);
      }
    }

  }

  private checkToken() {
    const oldToken = this.adalService.userInfo ? this.adalService.userInfo.token : null;
    this.adalService.acquireToken(this.adalService.config.clientId).subscribe((token: string) => {
      if (token !== oldToken) {
        console.log("AAD token Refreshed");
      }
    }, (error: string) => {
      // retry getting the token
      this.adalService.acquireToken(this.adalService.config.clientId).subscribe((token: string) => {
        if (token !== oldToken) {
          console.log("AAD token Refreshed");
        }
      }, (secondError: string) => {
        window.location.reload();
      });
    });
  }



}

