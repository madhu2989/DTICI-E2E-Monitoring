import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { SettingsService } from '../../services/settings.service';
import { Router } from '@angular/router';
import { EnvironmentService } from '../../services/config.service';
import { Environment } from "../../model/environment-config";

@Component({
  selector: 'app-settings-dialog',
  templateUrl: './settings-dialog.component.html',
  styleUrls: ['./settings-dialog.component.scss']
})
export class SettingsDialogComponent implements OnInit {

  timerangeValue: number;
  checkedOverview = true;
  checkedIgnoreOk = false;
  oldTimeRangeValue: number;
  historyDuration: number;
  private environmentConfig: Environment = null;

  constructor(@Inject(MAT_DIALOG_DATA) public data: any,
    private settingsService: SettingsService,
    envService: EnvironmentService,
    private router: Router,
    public dialogRef: MatDialogRef<SettingsDialogComponent>
  ) { 
    this.environmentConfig= envService.getConfiguration(); 
    if(this.environmentConfig !=null) {
        this.historyDuration= this.environmentConfig.historyDuration;
    }
  }

  ngOnInit() {
    if (this.settingsService.isOverviewModeSetInUrl) {
      this.checkedOverview = this.savedOverviewMode(this.settingsService.overviewMode);
    } else if (localStorage.getItem("overviewMode") || sessionStorage.getItem("overviewMode")) {
      this.checkedOverview = this.savedOverviewMode(JSON.parse(sessionStorage.getItem("overviewMode")));
    } else {
      this.checkedOverview = true;
    }

    if (this.settingsService.isIgnoreokModeSetInUrl) {
      this.checkedIgnoreOk = this.settingsService.ignoreokMode;
    } else if (localStorage.getItem("ignoreokMode") || sessionStorage.getItem("ignoreokMode")) {
      this.checkedIgnoreOk = (sessionStorage.getItem("ignoreokMode") === 'true');
    } else {
      this.checkedIgnoreOk = false;
    }

    if (this.settingsService.isTimerangeSetInUrl) {
      this.timerangeValue = Math.round(this.settingsService.timerange / 60 / 60 / 1000);
    } else if (!isNaN(parseInt(localStorage.getItem("timerange"), 10)) && !isNaN(parseInt(sessionStorage.getItem("timerange"), 10))) {
      this.timerangeValue = (parseInt(sessionStorage.getItem("timerange"), 10));
    } else {
      this.timerangeValue = Math.round(this.historyDuration / 60 / 60 / 1000);
    }
    this.oldTimeRangeValue = this.timerangeValue;
  }

  formatLabel(timerangeValue: number | null) {
    if (!timerangeValue) {
      return 0;
    } else {
      return timerangeValue + 'h';
    }
  }

  saveSettings() {

    localStorage.setItem("overviewMode", this.checkedOverview ? "false" : "true");
    sessionStorage.setItem("overviewMode", this.checkedOverview ? "false" : "true");
    this.settingsService.overviewMode = this.savedOverviewMode(this.checkedOverview);

    localStorage.setItem("ignoreokMode", this.checkedIgnoreOk ? "true" : "false");
    sessionStorage.setItem("ignoreokMode", this.checkedIgnoreOk ? "true" : "false");
    this.settingsService.ignoreokMode = this.checkedIgnoreOk;

    localStorage.setItem("timerange", "" + this.timerangeValue);
    sessionStorage.setItem("timerange", "" + this.timerangeValue);
    this.settingsService.timerange = this.timerangeValue * 60 * 60 * 1000;
    console.log(localStorage.getItem("timerange"));

    this.dialogRef.close();

    if (this.oldTimeRangeValue !== this.timerangeValue) {
      const url = this.getUrlWithoutOptionalParameter(this.router.url);
      window.location.href = url;
    }
  }

  savedOverviewMode(currentOverviewMode: boolean): boolean {
    let overviewCheck: boolean;
    switch (currentOverviewMode) {
      case true: {
        overviewCheck = false;
        break;
      }
      case false: {
        overviewCheck = true;
        break;
      }
      default: {
        break;
      }
    }
    return overviewCheck;
  }

  getUrlWithoutOptionalParameter(url: string): string {
    if (url.indexOf("?") !== -1) {
      var showDemoTrue = "?showdemo=true";
      if (url.includes(showDemoTrue)) {
        const showdemoParam = url.substr(url.indexOf(showDemoTrue), showDemoTrue.length);
        url = url.substr(0, url.indexOf("?")) + showdemoParam;
        return url;
      } else {
        return url.substr(0, url.indexOf("?"));
      }
    } else {
      return url;
    }
  }

}
