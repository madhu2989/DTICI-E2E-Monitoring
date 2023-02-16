import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { UntypedFormControl, Validators, UntypedFormGroup, UntypedFormBuilder, } from '@angular/forms';
import { AlertIgnore } from '../../shared/model/alert-ignore';
import { DataService } from '../../shared/services/data.service';
import { GridOptions, RowNode } from 'ag-grid-community';
import { AlertIgnoreCondition } from '../../shared/model/alert-ignore-condition';
import { VanEnvironment } from '../../shared/model/van-environment';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';

@Component({
  selector: 'app-alert-ignores-edit',
  templateUrl: './alert-ignores-edit.component.html',
  styleUrls: ['./alert-ignores-edit.component.scss']
})
export class AlertIgnoresEditComponent implements OnInit, OnDestroy {

  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;
  public expDate: Date;
  public expTime: string;
  public timeRegEx = '([0-1]?[0-9]|2[0-3]):[0-5][0-9]';
  ignoreAlertConditionItem: object = {
    alertIgnore_key: "",
    alertIgnore_value: ""
  };
  rowData: Array<Object> = [];
  alertIgnoreCondition: AlertIgnoreCondition;

  currentAlertIgnore: AlertIgnore;
  currentAlertIgnoreIgnoreCondition: AlertIgnoreCondition;
  originalAlertIgnore: AlertIgnore;
  showloadingIndicator = false;
  filterCriteriaText = "";
  environments: VanEnvironment[] = [];
  minimumFilterCount = false;
  expTimeFormGroup: UntypedFormGroup;
  formControlExpTime: UntypedFormControl;



  constructor(
    public dialogRef: MatDialogRef<AlertIgnoresEditComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dataService: DataService,
    public dialog: MatDialog,
    private datePipe: DatePipe,
    private _formBuilder: UntypedFormBuilder
  ) {
    this.currentAlertIgnore = data.alertIgnore as AlertIgnore;
    this.currentAlertIgnoreIgnoreCondition = data.alertIgnore.ignoreCondition as AlertIgnoreCondition;

    if (data.mode === "create" || data.mode === "createFromPopup") {
      this.filterCriteriaText = "Add filter criteria";
    } else {
      this.filterCriteriaText = "Add or remove filter criteria";
    }


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
      rowHeight: 48,
      suppressRowClickSelection: true,
      stopEditingWhenGridLosesFocus: true
    };
    this.rowSelection = "multiple";

    this.formControlExpTime = new UntypedFormControl({disabled: false}, [
      Validators.pattern(this.timeRegEx), Validators.minLength(3), Validators.maxLength(5)]);

  }

  ngOnInit() {
    const me = this;
    this.originalAlertIgnore = new AlertIgnore(this.data.alertIgnore);
    me.createRowData();
    this.expTimeFormGroup = this._formBuilder.group({
      expTimeCtrl: this.formControlExpTime

    });
    if (me.currentAlertIgnore.expirationDate !== "") {
      me.expTime = new Date(me.currentAlertIgnore.expirationDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      me.expDate = new Date(me.currentAlertIgnore.expirationDate);
      this.expTimeFormGroup.get('expTimeCtrl').setValue(me.expTime);
    } else {
      this.expTimeFormGroup.get('expTimeCtrl').setValue("");
    }
    me.dataService.getAllEnvironments(false).then(function (result) {
      if (result && result.length > 0) {
        for (let i = 0; i < result.length; i++) {
          me.environments.push(result[i]);
        }
      }
    }
    );
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

    if (this.data.mode === "createFromPopup") {
      this.gridApi.selectAll();
    }

    const allRows = this.getAllRows();
    for (const row of allRows) {
      const currentRow = row as RowNode;
      if (currentRow.data.alertIgnore_value !== "" && currentRow.data.alertIgnore_value !== null) {
        this.minimumFilterCount = true;
      }
    }
  }

  onCellValueChanged() {
    this.minimumFilterCount = false;
    const allRows = this.getAllRows();
    for (const row of allRows) {
      const currentRow = row as RowNode;
      if (currentRow.data.alertIgnore_value !== "" && currentRow.data.alertIgnore_value !== null) {
        this.minimumFilterCount = true;
      }
    }
  }

  createRowData(): void {

    if (this.data.mode === "create" || this.data.mode === "createFromPopup") {
      if (this.data.mode === "create") {
        this.alertIgnoreCondition = new AlertIgnoreCondition({
          alertName: "",
          componentId: "",
          checkId: "",
          description: "",
          customField1: "",
          customField2: "",
          customField3: "",
          customField4: "",
          customField5: "",
          state: ""
        });
      } else if (this.data.mode === "createFromPopup") {
        this.alertIgnoreCondition = new AlertIgnoreCondition({
          alertName: this.data.alertIgnore.ignoreCondition.alertName,
          componentId: this.data.alertIgnore.ignoreCondition.componentId,
          checkId: this.data.alertIgnore.ignoreCondition.checkId,
          description: this.data.alertIgnore.ignoreCondition.description,
          customField1: this.data.alertIgnore.ignoreCondition.customField1,
          customField2: this.data.alertIgnore.ignoreCondition.customField2,
          customField3: this.data.alertIgnore.ignoreCondition.customField3,
          customField4: this.data.alertIgnore.ignoreCondition.customField4,
          customField5: this.data.alertIgnore.ignoreCondition.customField5,
          state: this.data.alertIgnore.ignoreCondition.state
        });
      }
      for (const key of Object.keys(this.alertIgnoreCondition)) {
        this.rowData.push(
          {
            on_off: "On",
            alertIgnore_key: key,
            alertIgnore_value: this.alertIgnoreCondition[key]
          }
        );
      }

    } else {
      // in case of edit mode
      this.alertIgnoreCondition = new AlertIgnoreCondition({
        alertName: "",
        componentId: "",
        checkId: "",
        description: "",
        customField1: "",
        customField2: "",
        customField3: "",
        customField4: "",
        customField5: "",
        state: ""
      });
      if (this.currentAlertIgnoreIgnoreCondition) {
        for (const key of Object.keys(this.alertIgnoreCondition)) {
          // since we get currentAlertIgnoreIgnoreCondition with first letter uppercase
          const firtLetterUpperCaseKey = key.charAt(0).toUpperCase() + key.substring(1);
          if (this.currentAlertIgnoreIgnoreCondition[firtLetterUpperCaseKey]
            && this.currentAlertIgnoreIgnoreCondition[firtLetterUpperCaseKey].length > 0
          ) {
            this.rowData.push(
              {
                on_off: "On",
                alertIgnore_key: key,
                alertIgnore_value: this.currentAlertIgnoreIgnoreCondition[firtLetterUpperCaseKey]
              }
            );
          } else {
            this.rowData.push(
              {
                on_off: "On",
                alertIgnore_key: key,
                alertIgnore_value: this.alertIgnoreCondition[key]
              }
            );
          }
        }
      }
    }

    if (this.gridApi) {
      this.gridApi.setRowData(this.rowData);
    }

  }

  private createColumnDefs() {
    if (this.data.mode === "create" || this.data.mode === "edit") {
      return [
        { headerName: 'Alert', field: 'alertIgnore_key', width: 150, suppressSizeToFit: true, sort: 'asc' },
        { headerName: 'Filter Text', field: 'alertIgnore_value', editable: true }
      ];
    } else {
      return [

        {
          headerName: 'Alert', field: 'alertIgnore_key', width: 210, suppressSizeToFit: true, sort: 'asc',
          headerCheckboxSelection: true,
          headerCheckboxSelectionFilteredOnly: true,
          checkboxSelection: true
        },
        { headerName: 'Filter Text', field: 'alertIgnore_value', editable: true }
      ];
    }

  }


  onSaveClick() {
    this.showloadingIndicator = true;

    // set the correct date
    const chosenTime = this.expTimeFormGroup.get('expTimeCtrl').value;
    if (this.expDate && !isNaN(this.expDate.getTime())) {
      if (chosenTime && chosenTime !== "") {
        const dateTime = chosenTime.split(':');
        this.expDate.setHours(parseInt(dateTime[0], 10));
        this.expDate.setMinutes(parseInt(dateTime[1], 10));
      } else {
        this.expDate.setHours(0, 0);
      }
      this.currentAlertIgnore.expirationDate = new Date(this.expDate).toISOString();
    }

    if (!this.expDate && chosenTime === "") {
      this.currentAlertIgnore.expirationDate = null;
    }

    // iterate over ag-grid and save fields in currentAlertIgnoreCondition
    const currentRows = this.getAllRows();
    this.currentAlertIgnore.ignoreCondition = this.alertIgnoreCondition;
    for (const row of currentRows) {
      const currentRow = row as RowNode;
      if (this.data.mode === 'createFromPopup') {
        if (!currentRow.isSelected()) {
          currentRow.data.alertIgnore_value = null;
        }
      } else {
        if (currentRow.data.alertIgnore_value === "") {
          currentRow.data.alertIgnore_value = null;
        }
      }
      const key = currentRow.data.alertIgnore_key;
      const value = currentRow.data.alertIgnore_value;
      this.currentAlertIgnore.ignoreCondition[key] = value;
    }

    let promise;

    if (this.data.mode === "create" || this.data.mode === "createFromPopup") {
      this.currentAlertIgnore.creationDate = new Date().toISOString();
      promise = this.dataService.createAlertIgnore(this.currentAlertIgnore);
    } else {
      this.currentAlertIgnore.creationDate = new Date(this.currentAlertIgnore.creationDate).toISOString();
      promise = this.dataService.updateAlertIgnore(this.currentAlertIgnore, this.currentAlertIgnore.id);
    }

    promise.then(alertIgnore => {
      this.showloadingIndicator = false;
      this.dialogRef.close('saved');
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
    this.currentAlertIgnore.environmentSubscriptionId = this.originalAlertIgnore.environmentSubscriptionId;
    this.currentAlertIgnore.name = this.originalAlertIgnore.name;
    this.dialogRef.close();
  }

}
