import { Component, OnInit, OnDestroy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { GridOptions } from 'ag-grid-community';
import { DataService } from '../../shared/services/data.service';
import { BaseComponent } from '../../shared/base-component/base.component';
import { AlertIgnore } from '../../shared/model/alert-ignore';
import { MatDialog } from '@angular/material/dialog';
import { AlertIgnoresEditComponent } from '../alert-ignores-edit/alert-ignores-edit.component';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { ConfirmationDialogComponent } from '../../shared/dialogs/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-alert-ignores-overview',
  templateUrl: './alert-ignores-overview.component.html',
  styleUrls: ['./alert-ignores-overview.component.scss']
})
export class AlertIgnoresOverviewComponent extends BaseComponent implements OnInit, OnDestroy {

  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;

  rowData: AlertIgnore[] = [];
  showloadingIndicator = false;

  public toolBarTitle = 'Alert Filter Overview';
  public toolBarSubTitle = 'Ignore incoming alerts';
  public settingsButtonVisible = false;
  public deleteButtonVisible = false;
  public editButtonVisible = false;
  public editButtonActive = false;
  public addButtonVisible = true;
  public addButtonActive = true;


  constructor(
    private dataService: DataService,
    public dialog: MatDialog,
    private datePipe: DatePipe
  ) {
    super();


    this.columnDefs = [
      {
        headerName: 'Name', field: 'name', headerCheckboxSelection: true,
        headerCheckboxSelectionFilteredOnly: true,
        checkboxSelection: true,
        suppressSizeToFit: true
      },
      { headerName: 'Environment Name', field: 'environmentName', suppressSizeToFit: true },
      { headerName: 'Creation Date', field: 'creationDate', suppressSizeToFit: true, width: 230, sort: 'desc' },
      { headerName: 'Expiration Date', field: 'expirationDate', suppressSizeToFit: true },
      { headerName: 'Component Id', field: 'ignoreCondition.ComponentId', suppressSizeToFit: true },
      { headerName: 'Check Id', field: 'ignoreCondition.CheckId', suppressSizeToFit: true },
      { headerName: 'Alert Name', field: 'ignoreCondition.AlertName', suppressSizeToFit: true },
      { headerName: 'Ignore Conditions', field: 'ignoreCondition', valueFormatter: 'JSON.stringify(value)', suppressSizeToFit: true }
    ];

    this.gridOptions = <GridOptions>{
      enableColResize: true,
      cacheOverflowSize: 2,
      onGridReady: function (params) {
        this.onGridReady(params);
      }.bind(this),
      onRowDoubleClicked: this.onRowDoubleClicked.bind(this),
      onRowSelected: this.onRowSelected.bind(this),
      suppressRowClickSelection: true
    };
    this.rowSelection = "multiple";

  }

  ngOnInit() {
    const me = this;
    me.showloadingIndicator = true;
    me.setRowData(false);
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

  onRowDoubleClicked($event) {
    if ($event.node.data) {
      console.log('onRowDoubleClicked: ' + $event.node.data);
      const item = $event.node.data;
      if (item.expirationDate !== "") {
        item.expirationDate = this.datePipe.transform(new Date(item.expirationDate), "yyyy-MM-ddTHH:mm");
      }

      const dialogRef = this.dialog.open(AlertIgnoresEditComponent, {
        width: '1200px',
        data: { alertIgnore: item, mode: 'edit' },
        disableClose: true
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.refreshGrid();
        }
      });
    }
  }

  onRowSelected($event) {
    this.deleteButtonVisible = true;
    this.editButtonVisible = true;
    if (this.gridApi.getSelectedRows().length === 1) {
      this.editButtonActive = true;
    } else if (this.gridApi.getSelectedRows().length === 0) {
      this.deleteButtonVisible = false;
      this.editButtonVisible = false;
    } else {
      this.editButtonActive = false;
    }
  }

  openEditDialog() {
    const alertIgnore = this.gridApi.getSelectedRows()[0];
    if (alertIgnore.expirationDate !== "") {
      alertIgnore.expirationDate = this.datePipe.transform(new Date(alertIgnore.expirationDate), "yyyy-MM-ddTHH:mm");
    }

    const dialogRef = this.dialog.open(AlertIgnoresEditComponent, {
      width: '1200px',
      data: { alertIgnore: alertIgnore, mode: 'edit' },
      disableClose: true
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.refreshGrid();
      }
    });
  }

  openCreateDialog($event) {
    const alertIgnore = new AlertIgnore();
    alertIgnore.expirationDate = "";

    const dialogRef = this.dialog.open(AlertIgnoresEditComponent, {
      width: '1200px',
      data: { alertIgnore: alertIgnore, mode: 'create' },
      disableClose: true
    });

    dialogRef.afterClosed().toPromise().then(result => {
      if (result) {
        this.refreshGrid();
      }
    });

  }

  protected refreshGrid() {
    this.setRowData(true);
  }

  setRowData(forceRefresh): void {
    const me = this;
    me.rowData = [];
    this.dataService.getAllAlertIgnores(forceRefresh).then((alertIgnore: AlertIgnore[]) => {
      me.showloadingIndicator = false;
      if (alertIgnore) {
        for (let i = 0; i < alertIgnore.length; i++) {
          if (alertIgnore[i].expirationDate === "9999-12-31T23:59:59.997Z" || alertIgnore[i].expirationDate === "") {
            alertIgnore[i].expirationDate = "";
          } else {
            alertIgnore[i].expirationDate = this.datePipe.transform(new Date(alertIgnore[i].expirationDate), "yyyy-MM-dd, HH:mm");
          }
          alertIgnore[i].creationDate = this.datePipe.transform(new Date(alertIgnore[i].creationDate), "yyyy-MM-dd, HH:mm");
          me.rowData.push(alertIgnore[i]);
        }
        if (me.gridApi) {
          me.gridApi.setRowData(me.rowData);
        }
      }
    }, (error) => {
      me.showloadingIndicator = false;
      this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error loading data");
    });
  }

  deleteAlertIgnore() {
    const message = "Are you sure you want to delete the Alert Ignore(s)?";
    const title = "Delete Alert Ignore?";
    const action = "Delete";
    const confirmationDialog = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: { message, title, action }
    });

    confirmationDialog.afterClosed().toPromise().then(result => {
      if (result) {
        const me = this;
        const rowsToDelete = this.gridApi.getSelectedRows();
        this.deleteButtonVisible = false;

        for (let i = 0; i < rowsToDelete.length; i++) {
          const id = rowsToDelete[i].id;
          this.dataService.deleteAlertIgnore(id).then(res => {
            if (i === rowsToDelete.length - 1) {
              this.refreshGrid();
            }
          },
            (error =>
              this.showErrorDialog(error, "Row could not be deleted")));
        }
      }
    });

  }

  showErrorDialog(message: string, title: string) {
    const errorDialog = this.dialog.open(ErrorDialogComponent, {
      width: '400px',
      data: { message, title },
      disableClose: true
    });
  }

}
