import { Component, OnInit, OnDestroy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { GridOptions } from 'ag-grid-community';
import { DataService } from '../../shared/services/data.service';
import { BaseComponent } from '../../shared/base-component/base.component';
import { NotificationRule } from '../../shared/model/notification-rule';
import { MatDialog } from '@angular/material/dialog';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { ConfirmationDialogComponent } from '../../shared/dialogs/confirmation-dialog/confirmation-dialog.component';
import { NotificationEditComponent } from '../notification-edit/notification-edit.component';

@Component({
  selector: 'app-notification-overview',
  templateUrl: './notification-overview.component.html',
  styleUrls: ['./notification-overview.component.scss']
})
export class NotificationOverviewComponent implements OnInit, OnDestroy {

  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;

  rowData: NotificationRule[] = [];
  showloadingIndicator = false;

  public toolBarTitle = 'Notification Rule Overview';
  public toolBarSubTitle = 'Manage Notifications';
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
    // super();


    this.columnDefs = [
      {
        headerName: 'Environment Name', field: 'environmentName', headerCheckboxSelection: true,
        headerCheckboxSelectionFilteredOnly: true,
        checkboxSelection: true, sort: 'asc', suppressSizeToFit: true, width: 240        
      },
      { headerName: 'Levels', field: 'levels', suppressSizeToFit: true, width: 320 },
      { headerName: 'States', field: 'states', suppressSizeToFit: true, width: 160 },
      { headerName: 'Addresses', field: 'emailAddresses', suppressSizeToFit: true, width: 225 },
      { headerName: 'Notification Interval (Min)', suppressSizeToFit: true, width: 225, valueGetter: function convertToMinutes(params) {
        return params.data.notificationInterval / 60;
      } },
      { headerName: 'Activation State', field: 'isActive', suppressSizeToFit: true, width: 160 }
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

      const dialogRef = this.dialog.open(NotificationEditComponent, {
        width: '600px',
        data: { notificationRule: item, mode: 'edit' },
        disableClose: true
      });

      dialogRef.afterClosed().toPromise().then(result => {
        this.editButtonVisible = false;
        this.refreshGrid();

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
    const notificationRule = this.gridApi.getSelectedRows()[0];

    const dialogRef = this.dialog.open(NotificationEditComponent, {
      width: '600px',
      data: { notificationRule: notificationRule, mode: 'edit' },
      disableClose: true
    });
    dialogRef.afterClosed().toPromise().then(result => {
      this.refreshGrid();

    });
  }

  openCreateDialog($event) {
    const notificationRule = new NotificationRule();

    const dialogRef = this.dialog.open(NotificationEditComponent, {
      width: '600px',
      data: { notificationRule: notificationRule, mode: 'create' },
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
    this.editButtonVisible = false;
    this.deleteButtonVisible = false;
  }

  setRowData(forceRefresh): void {
    const me = this;
    me.rowData = [];
    this.dataService.getAllNotificationRules(forceRefresh).then((notificationRule: NotificationRule[]) => {
      me.showloadingIndicator = false;
      if (notificationRule) {
        for (let i = 0; i < notificationRule.length; i++) {
          me.rowData.push(notificationRule[i]);
        }
        if (me.gridApi) {
          me.gridApi.setRowData(me.rowData);
        }
      }
    }, (error) => {
      me.showloadingIndicator = false;
    });
  }

  deleteNotificationRule() {
    const message = "Are you sure you want to delete these Notification Rules?";
    const title = "Delete Notification Rules?";
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
        this.editButtonVisible = false;

        for (let i = 0; i < rowsToDelete.length; i++) {
          const id = rowsToDelete[i].id;
          this.dataService.deleteNotificationRule(id).then(res => {
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
