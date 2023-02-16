import { Injectable } from '@angular/core';
import { EnvironmentService } from '../services/config.service';
import { Environment } from "../model/environment-config";

@Injectable({
  providedIn: "root",
})
export class SettingsService {

  public debugMode: boolean;
  public isdebugModeSetInUrl: boolean;
  public overviewMode: boolean;
  public isOverviewModeSetInUrl: boolean;
  public ignoreokMode: boolean;
  public isIgnoreokModeSetInUrl: boolean;
  public timerange: number;
  public isTimerangeSetInUrl: boolean;
  public showDemo: boolean;
  public isCurrentUserOnlyViewer: boolean;
  public isCurrentUserAdmin: boolean;
  public checkDetailGridOptions: any;
  public checkCrudGridOptions: any;
  public currentUserRoles: string[] = [];
  public editModeActive: boolean;
  public version: string = "";
  private environmentConfig: Environment = null;

  constructor(public envService: EnvironmentService) {
    
    this.environmentConfig= envService.getConfiguration(); 
    if(this.environmentConfig !=null) {
        this.version= this.environmentConfig.versionBuildNumber;
    }
   }

  handleQueryParameters(queryParams: any): void {
    // this.debugMode = queryParams['debug'] === 'true';
    if (queryParams['debug'] === 'true') {
      this.debugMode = true;
      this.isdebugModeSetInUrl = true;
    } else if (queryParams['debug'] === 'false') {
      this.debugMode = false;
      this.isdebugModeSetInUrl = true;
    } else if (sessionStorage.getItem("debugMode")) {
      this.debugMode = JSON.parse(sessionStorage.getItem("debugMode"));
      this.isdebugModeSetInUrl = false;
    } else {
      this.debugMode = false;
      this.isdebugModeSetInUrl = false;
    }

    //  this.overviewMode = queryParams['overview'] === 'true';
    if (queryParams['overview'] === 'true') {
      this.overviewMode = true;
      this.isOverviewModeSetInUrl = true;
    } else if (queryParams['overview'] === 'false') {
      this.overviewMode = false;
      this.isOverviewModeSetInUrl = true;
    } else if (sessionStorage.getItem("overviewMode")) {
      this.overviewMode = JSON.parse(sessionStorage.getItem("overviewMode"));
      this.isOverviewModeSetInUrl = false;
    } else {
      this.overviewMode = false;
      this.isOverviewModeSetInUrl = false;
    }

    if (queryParams['ignoreok'] === 'true') {
      this.ignoreokMode = true;
      this.isIgnoreokModeSetInUrl = true;
    } else if (queryParams['ignoreok'] === 'false') {
      this.ignoreokMode = false;
      this.isIgnoreokModeSetInUrl = true;
    } else if (sessionStorage.getItem("ignoreokMode")) {
      this.ignoreokMode = JSON.parse(sessionStorage.getItem("ignoreokMode"));
      this.isOverviewModeSetInUrl = false;
    } else {
      this.ignoreokMode = false;
      this.isIgnoreokModeSetInUrl = false;
    }

    /*  this.timerange = queryParams['timerange'] &&
        (((queryParams['timerange'] * 60 * 60 * 1000) <= 72 * 60 * 60 * 1000) &&
          ((queryParams['timerange'] * 60 * 60 * 1000) >= 1 * 60 * 60 * 1000))
        ? queryParams['timerange'] * 60 * 60 * 1000 : environment.historyDuration;
    */

    if (queryParams['timerange'] &&
      (((queryParams['timerange'] * 60 * 60 * 1000) <= 72 * 60 * 60 * 1000) &&
        ((queryParams['timerange'] * 60 * 60 * 1000) >= 1 * 60 * 60 * 1000))) {
      this.timerange = queryParams['timerange'] * 60 * 60 * 1000;
      this.isTimerangeSetInUrl = true;
    } else if (localStorage.getItem("timerange")) {
      this.timerange = (parseInt(localStorage.getItem("timerange"), 10) * 60 * 60 * 1000);
    } else {
      this.timerange = this.environmentConfig.historyDuration;
      this.isTimerangeSetInUrl = false;
    }

    this.showDemo = (queryParams['showdemo'] === 'true');

  }


}