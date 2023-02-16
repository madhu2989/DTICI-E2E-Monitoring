import { Component, OnInit, Output, Input, EventEmitter } from '@angular/core';
import { SettingsService } from '../services/settings.service';
import { Router, ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { DashboardNodeEditComponent } from './dashboard-node-edit/dashboard-node-edit.component';

@Component({
  selector: 'app-dashboard-edit',
  templateUrl: './dashboard-edit.component.html',
  styleUrls: ['./dashboard-edit.component.scss']
})
export class DashboardEditComponent implements OnInit {

  @Output() add: EventEmitter<any> = new EventEmitter();
  @Output() addExistingComponent: EventEmitter<any> = new EventEmitter();

  public editModeAvailable: boolean = false;
  isAddExistingComponentButtonVisible: boolean;
  isAddButtonVisible: boolean = true;

  constructor(public settingsService: SettingsService,
    private router: Router,
    private route: ActivatedRoute,
    public dialog: MatDialog) { }

  ngOnInit() {
    const me = this;

    if (me.route.snapshot.paramMap.get('actionId')) {
      me.isAddExistingComponentButtonVisible = me.route.snapshot.paramMap.get('actionId').length > 0;
      if (me.route.snapshot.paramMap.get('environmentId').includes('SLA') &&
        me.settingsService.currentUserRoles.includes("Monitoring_admin")) {
        me.isAddButtonVisible = false;
      }
    }

    // if (me.settingsService.currentUserRoles) {
    //   me.editModeAvailable = me.settingsService.currentUserRoles.includes("Monitoring_admin") || ((me.settingsService.currentUserRoles.includes("Monitoring_contributor")) &&
    //     (me.router.url !== "/dashboard"));
    // }
    if (me.settingsService.currentUserRoles) {
      me.editModeAvailable = me.settingsService.currentUserRoles.includes("Monitoring_admin")
    }

  }

  public addButtonPressed(): void {
    this.add.emit(null);
  }

  public addExistingComponentButtonPressed(): void {
    this.addExistingComponent.emit(null);
  }

}
