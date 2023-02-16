import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AdalService } from "adal-angular4";
import { AppInsightsMonitoringService } from './services/app-insights-monitoring.service';
import { SignalRService } from './services/signal-r.service';

@Injectable({
  providedIn: "root",
})
export class LoggedInGuard implements CanActivate {
  constructor(
    private adalService: AdalService,
    private router: Router,
    private appInsightsMonitoringService: AppInsightsMonitoringService,
    private signalRService: SignalRService
  ) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.adalService.userInfo.authenticated) {
      if (((state.url === "/deployments") || (state.url === "/ignores")) && (this.adalService.userInfo.profile.roles === undefined || this.adalService.userInfo.profile.roles.includes("access"))) {
        this.router.navigate(['/dashboard']);
        return false;
      }
      this.appInsightsMonitoringService.setUser(this.adalService.userInfo.userName);
      this.signalRService.checkConnection();
      return true;
    }
    else if (this.adalService.userInfo.loginCached) {
      //Assigning token if it is still available in cache
      this.adalService.userInfo.token = this.adalService.getCachedToken(this.adalService.userInfo.profile.aud);
      if (this.adalService.userInfo.token !== null) {
        this.adalService.userInfo.authenticated = true;
        localStorage.setItem('url', state.url);
        return true;
      }
      else {
        this.router.navigate(['/login']);
        return false;
      }
    }
    else {
      if(!state.url.includes('dashboard')){
      localStorage.setItem('url', state.url);
    }
      this.router.navigate(['/login']);
      return false;
    }
  }

  canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    return this.canActivate(childRoute, state);
  }

}
