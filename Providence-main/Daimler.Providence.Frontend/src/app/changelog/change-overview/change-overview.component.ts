import { Component, OnInit, OnDestroy } from '@angular/core';
import { DatePipe, JsonPipe } from '@angular/common';
import { UntypedFormControl, Validators, UntypedFormGroup, UntypedFormBuilder } from '@angular/forms';
import { GridOptions } from 'ag-grid-community';
import { DataService } from '../../shared/services/data.service';
import { BaseComponent } from '../../shared/base-component/base.component';
import { MatDialog } from '@angular/material/dialog';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { CancelRequestDialogComponent } from '../../shared/dialogs/cancel-request-dialog/cancel-request-dialog.component';
import { Changelog } from '../../shared/model/changelog';
import { JsonViewDialogComponent } from '../../shared/dialogs/json-view-dialog/json-view-dialog.component';

@Component({
  selector: 'app-change-overview',
  templateUrl: './change-overview.component.html',
  styleUrls: ['./change-overview.component.scss']
})
export class ChangeOverviewComponent extends BaseComponent implements OnInit, OnDestroy {
  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;

  public startDate: Date;
  public startTime: string;
  public endDate: Date;
  public endTime: string;
  public timeRegEx = '([0-1]?[0-9]|2[0-3]):[0-5][0-9]';
  startTimeFormGroup: UntypedFormGroup;
  endTimeFormGroup: UntypedFormGroup;
  formControlStartTime: UntypedFormControl;
  formControlEndTime: UntypedFormControl;

  rowData: Changelog[] = [];
  showloadingIndicator = false;

  public toolBarTitle = 'Changelog History';
  public toolBarSubTitle = 'Monitor changes';
  public settingsButtonVisible = false;
  public deleteButtonVisible = false;
  public editButtonVisible = false;
  public editButtonActive = false;
  public addButtonVisible = false;
  public addButtonActive = false;

  panelOpenState = false;


  constructor(
    private dataService: DataService,
    public dialog: MatDialog,
    private datePipe: DatePipe,
    private jsonPipe: JsonPipe,
    private _formBuilder: UntypedFormBuilder
  ) {
    super();

    this.formControlStartTime = new UntypedFormControl({disabled: false}, [
      Validators.pattern(this.timeRegEx), Validators.minLength(3), Validators.maxLength(5)]);
    this.formControlEndTime = new UntypedFormControl({disabled: false}, [
      Validators.pattern(this.timeRegEx), Validators.minLength(3), Validators.maxLength(5)]);

    this.columnDefs = [
      {
        headerName: 'Environment Name', field: 'environmentName', suppressSizeToFit: true },
      { headerName: 'User Name', field: 'userName', suppressSizeToFit: true, width: 230 },
      { headerName: 'Change Date', field: 'changeDate', suppressSizeToFit: true,  width: 170, sort: 'desc'},
      { headerName: 'Element Type', field: 'elementType', suppressSizeToFit: true,  width: 140  },
      { headerName: 'Element Id', field: 'elementId', suppressSizeToFit: true,  width: 125  },
      { headerName: 'Operation', field: 'operation', suppressSizeToFit: true,  width: 125  },
      { headerName: 'Old Value', field: 'valueOld', valueFormatter: 'JSON.stringify(value)' , suppressSizeToFit: true  },
      { headerName: 'New Value', field: 'valueNew', valueFormatter: 'JSON.stringify(value)', suppressSizeToFit: true   },
      { headerName: 'Diff', field: 'diff', valueFormatter: 'JSON.stringify(value)', suppressSizeToFit: true  }
    ];

    this.gridOptions = <GridOptions>{
      enableColResize: true,
      cacheOverflowSize: 2,
      onGridReady: function (params) {
        this.onGridReady(params);
      }.bind(this),
      onCellDoubleClicked: this.onCellDoubleClicked.bind(this),
      suppressRowClickSelection: true
    };

  }

  ngOnInit() {
    const me = this;

    this.startTimeFormGroup = this._formBuilder.group({
      startTimeCtrl: this.formControlStartTime
    });
    me.endTimeFormGroup = this._formBuilder.group({
      endTimeCtrl: me.formControlEndTime
    });

    me.startTimeFormGroup.get('startTimeCtrl').setValue("");
    me.endTimeFormGroup.get('endTimeCtrl').setValue("");
    me.setRowData();
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
  }

  onCellDoubleClicked(params) {
    if (params && params.colDef && params.colDef.field) {
      if (params.colDef.field === 'valueOld' || params.colDef.field === 'valueNew' || params.colDef.field === 'diff') {
        const message = this.jsonPipe.transform(params.value);
        const title = 'JSON Value of ' + params.colDef.headerName;
        const dialogRef = this.dialog.open(JsonViewDialogComponent, {
          disableClose: false,
          width: '600px',
          data: {
            "title": title,
            "message": message
          }
        });
      }
    }
  }

  setRowData(startDate?, endDate? ): void {
    const me = this;
    me.rowData = [];


    let dialogRef = null;
    setTimeout(() => {
    dialogRef = this.dialog.open(CancelRequestDialogComponent, {
       width: '300px',
       disableClose: true,
       data: { title: "Loading Changelog Entries", dataService: this.dataService }
     });
    });
    this.dataService.getChangelogs(startDate, endDate).then((changelogs: Changelog[]) => {
      me.showloadingIndicator = false;
      if (changelogs) {
        if (dialogRef) {
            dialogRef.close('loaded');
        }
        for (let i = 0; i < changelogs.length; i++) {
          changelogs[i].changeDate = this.datePipe.transform(new Date(changelogs[i].changeDate), "yyyy-MM-dd, HH:mm");
          me.rowData.push(changelogs[i]);
        }
        if (me.gridApi) {
          me.gridApi.setRowData(me.rowData);
        }
      }
    }, (error) => {
      if (dialogRef) {
          dialogRef.close('error');
      }
      me.showloadingIndicator = false;
      this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error loading data");
    });
  }

  applyFilter(filter: Boolean) {
    let startDateFilter;
    let endDateFilter;
    if (filter) {
      const chosenStartTime = this.startTimeFormGroup.get('startTimeCtrl').value;
      const chosenEndTime = this.endTimeFormGroup.get('endTimeCtrl').value;
      if (this.endDate && !isNaN(this.endDate.getTime())) {
        if (chosenEndTime && chosenEndTime !== "") {
          const dateTime = chosenEndTime.split(':');
          this.endDate.setHours(parseInt(dateTime[0], 10));
          this.endDate.setMinutes(parseInt(dateTime[1], 10));
        } else {
          this.endDate.setHours(23, 59, 59, 999);
        }
      }
      if (this.startDate && !isNaN(this.startDate.getTime())) {
        if (chosenStartTime && chosenStartTime !== "") {
          const dateTime = chosenStartTime.split(':');
          this.startDate.setHours(parseInt(dateTime[0], 10));
          this.startDate.setMinutes(parseInt(dateTime[1], 10));
        } else {
          this.startDate.setHours(0, 0);
        }
      }
      startDateFilter = new Date(this.startDate).toISOString();
      endDateFilter = new Date(this.endDate).toISOString();

    } else {
      this.startDate = null;
      this.endDate = null;
      
      this.formControlStartTime.setErrors(null);
      this.formControlStartTime.setValue('');
      
      this.formControlEndTime.setErrors(null);
      this.formControlEndTime.setValue('');
    }

    this.panelOpenState = false;

    this.setRowData(startDateFilter, endDateFilter);
  }

  validate(): boolean {
    let valid = true;
      if (!this.startTimeFormGroup.valid || !this.endTimeFormGroup.valid) {
        valid = false;
      }
      if (!this.endDate || !this.startDate || isNaN(this.endDate.getTime()) || isNaN(this.startDate.getTime())) {
        valid = false;
      }

    return valid;
  }

  protected refreshGrid() {
    this.setRowData();
  }


  showErrorDialog(message: string, title: string) {
    const errorDialog = this.dialog.open(ErrorDialogComponent, {
      width: '400px',
      data: { message, title },
      disableClose: true
    });
  }
}
