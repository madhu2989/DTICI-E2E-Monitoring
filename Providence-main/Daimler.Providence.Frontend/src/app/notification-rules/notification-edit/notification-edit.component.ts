import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { UntypedFormControl, Validators, UntypedFormGroup, UntypedFormBuilder, } from '@angular/forms';
import { AlertIgnore } from '../../shared/model/alert-ignore';
import { NotificationRule } from '../../shared/model/notification-rule';
import { DataService } from '../../shared/services/data.service';
import { GridOptions, RowNode } from 'ag-grid-community';
import { AlertIgnoreCondition } from '../../shared/model/alert-ignore-condition';
import { VanEnvironment } from '../../shared/model/van-environment';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';

@Component({
  selector: 'app-notification-edit',
  templateUrl: './notification-edit.component.html',
  styleUrls: ['./notification-edit.component.scss']
})
export class NotificationEditComponent implements OnInit, OnDestroy {

  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;
  rowData: Array<Object> = [];

  currentNotificationRule: NotificationRule;
  showloadingIndicator = false;
  filterCriteriaText = "";
  environments: VanEnvironment[] = [];
  minimumFilterCount = false;
  newAddressFormGroup: UntypedFormGroup;
  _formBuilder: UntypedFormBuilder;
  formControlAddress: UntypedFormControl;
  notificationRuleProps = {environment: false, service: false, action: false, component: false, error: false, warning: false, ok: false};
  addressToAdd: string;
  emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

  public deleteButtonVisible = false;
  public addButtonVisible = true;
  public addButtonActive = true;



  constructor(
    public dialogRef: MatDialogRef<NotificationEditComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dataService: DataService,
    public dialog: MatDialog,
    private datePipe: DatePipe
  ) {
    this.currentNotificationRule = data.notificationRule as NotificationRule;


    this.gridOptions = <GridOptions>{
      enableColResize: true,
      cacheOverflowSize: 2,
      columnDefs: this.createColumnDefs(),
      onGridReady: function (params) {
        this.onGridReady(params);
      }.bind(this),
      onCellValueChanged: function onCellValueChanged() {
        this.onCellValueChanged();
      }.bind(this),
      onRowSelected: this.onRowSelected.bind(this),
      rowHeight: 48,
      suppressRowClickSelection: true,
      stopEditingWhenGridLosesFocus: true
    };
    this.rowSelection = "multiple";


    this.formControlAddress = new UntypedFormControl();

  }

  ngOnInit() {
    const me = this;
    me.createRowData();
    this.newAddressFormGroup = new UntypedFormGroup({
      newAdressCtrl: this.formControlAddress

    });
    me.createRowData();
    me.dataService.getAllEnvironments(false).then(function (result) {
      if (result && result.length > 0) {
        for (let i = 0; i < result.length; i++) {
          me.environments.push(result[i]);
        }
      }
    }
    );

    if (this.data.mode === 'edit') {
      for (const lvl of this.currentNotificationRule.levels) {
        switch (lvl) {
          case 'Environment': {
            this.notificationRuleProps.environment = true;
            break;
          }
          case 'Service': {
            this.notificationRuleProps.service = true;
            break;
          }
          case 'Action': {
            this.notificationRuleProps.action = true;
            break;
          }
          case 'Component': {
            this.notificationRuleProps.component = true;
            break;
          }
          default: {
            break;
          }
        }
      }
      for (const state of this.currentNotificationRule.states) {
        switch (state) {
          case 'ERROR': {
            this.notificationRuleProps.error = true;
            break;
          }
          case 'WARNING': {
            this.notificationRuleProps.warning = true;
            break;
          }
          default: {
            break;
          }
        }
      }

      this.currentNotificationRule.notificationInterval = this.currentNotificationRule.notificationInterval / 60;
    } else {
      this.currentNotificationRule.notificationInterval = 30; }
  }

  ngOnDestroy(): void {
    this.gridApi.destroy();
  }


  onGridReady(params) {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;
    if (this.rowData) {
      this.gridApi.setRowData(this.rowData);
    }
    params.api.sizeColumnsToFit();

    const allRows = this.getAllRows();
    for (const row of allRows) {
      const currentRow = row as RowNode;
      if (currentRow.data !== null) {
        this.minimumFilterCount = true;
      }
    }
  }

  onCellValueChanged() {
    this.minimumFilterCount = false;
    const allRows = this.getAllRows();
    for (const row of allRows) {
      const currentRow = row as RowNode;
      if (currentRow.data !== null) {
        this.minimumFilterCount = true;
      }
    }
  }

  onRowSelected($event) {
    this.deleteButtonVisible = true;
    if (this.gridApi.getSelectedRows().length === 0) {
      this.deleteButtonVisible = false;
    }
  }

  createRowData(): void {
    this.rowData = [];
    if ( this.data.mode === 'edit') {
      const mailingList = this.data.notificationRule.emailAddresses.split(';');
      for (const address of mailingList) {
        const addressItem = {email: address};
        this.rowData.push(addressItem);
      }
    }

    if (this.gridApi) {
      this.gridApi.setRowData(this.rowData);
    }

  }

  private createColumnDefs() {
      return [
        //{ headerName: 'E-Mail Address', field: 'email', width: 504, suppressSizeToFit: true, sort: 'asc',
        { headerName: 'Webhook URL', field: 'email', width: 504, suppressSizeToFit: true, sort: 'asc',
        headerCheckboxSelection: true,
        headerCheckboxSelectionFilteredOnly: true,
        checkboxSelection: true,
        editable: true }
      ];
  }

  addNewMail() {
    if (this.newAddressFormGroup.get('newAdressCtrl').value) {
      const newAdress = {email: this.newAddressFormGroup.get('newAdressCtrl').value};
      this.rowData.push(newAdress);
      this.gridApi.setRowData(this.rowData);
      this.addressToAdd = null;
      this.onCellValueChanged();
      this.formControlAddress.reset();
    }
  }

  deleteMail() {
    const mailsToDelete = this.gridApi.getSelectedRows();
    for (let i = 0; i < this.rowData.length; i++) {
      for (const mail of mailsToDelete) {
        if (this.rowData[i] === mail) {
          this.rowData.splice(i, 1);
        }
      }
    }
    this.gridApi.setRowData(this.rowData);
    this.deleteButtonVisible = false;
    this.onCellValueChanged();
  }


  onSaveClick() {
    this.showloadingIndicator = true;

    this.currentNotificationRule.notificationInterval = this.currentNotificationRule.notificationInterval * 60;

    // save levels
    this.currentNotificationRule.levels = [];
    if (this.notificationRuleProps.environment === true) {
      this.currentNotificationRule.levels.push('Environment');
    }
    if (this.notificationRuleProps.service === true) {
      this.currentNotificationRule.levels.push('Service');
    }
    if (this.notificationRuleProps.action === true) {
      this.currentNotificationRule.levels.push('Action');
    }
    if (this.notificationRuleProps.component === true) {
      this.currentNotificationRule.levels.push('Component');
    }

    // save states
    this.currentNotificationRule.states = [];
    if (this.notificationRuleProps.error === true) {
      this.currentNotificationRule.states.push('ERROR');
    }
    if (this.notificationRuleProps.warning === true) {
      this.currentNotificationRule.states.push('WARNING');
    }

    // iterate over ag-grid and save fields in currentNotificationRule
    const currentRows = this.getAllRows();
    this.currentNotificationRule.emailAddresses = '';
    for (const row of currentRows ) {
      const currentRow = row as RowNode;
      this.currentNotificationRule.emailAddresses = this.currentNotificationRule.emailAddresses + currentRow.data.email + ';';
    }
    this.currentNotificationRule.emailAddresses = this.currentNotificationRule.emailAddresses.substr(0, this.currentNotificationRule.emailAddresses.length - 1);

    let promise;

    if (this.data.mode === "create") {
      promise = this.dataService.createNotificationRule(this.currentNotificationRule);
    } else {
      promise = this.dataService.updateNotificationRule(this.currentNotificationRule, this.currentNotificationRule.id);
    }

    promise.then(notificationRule => {
      this.showloadingIndicator = false;
      this.dialogRef.close(notificationRule);
    }).catch(error => {
      this.showloadingIndicator = false;
      this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error creating/updating data.");
    });
  }

  getAllRows(): Object[] {
    const rows: Object[] = [];
    const count = this.gridApi.getDisplayedRowCount();
    for (let i = 0; i < count; i++) {
      const row = this.gridApi.getDisplayedRowAtIndex(i);
      rows.push(row);
    }
    return rows;
  }

  showErrorDialog(message: string, title: string) {
    const errorDialog = this.dialog.open(ErrorDialogComponent, {
      width: '400px',
      data: { message, title },
      disableClose: true
    });
  }

  onCancelClick() {
    this.dialogRef.close();
  }

  checkMinimumLevels() {
    if (!this.notificationRuleProps.environment
      && !this.notificationRuleProps.service
      && !this.notificationRuleProps.action
      && !this.notificationRuleProps.component) {
        return false;
      } else {
        return true;
      }
  }

  checkMinimumStates() {
    if (!this.notificationRuleProps.error && !this.notificationRuleProps.warning) {
      return false;
    } else {
      return true;
    }
  }

}
