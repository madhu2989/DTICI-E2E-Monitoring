import { Component, OnInit, OnDestroy } from '@angular/core';
import { GridOptions } from 'ag-grid-community';
import { BaseComponent } from '../../shared/base-component/base.component';
import { MatDialog } from '@angular/material/dialog';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { VanChecks } from '../../shared/model/van-checks';
import { CheckEditComponent } from '../check-edit/check-edit.component';
import { ConfirmationDialogComponent } from '../../shared/dialogs/confirmation-dialog/confirmation-dialog.component';
import { MasterDataService, ElementType } from '../../shared/services/masterdata.service';
import { SettingsService } from '../../shared/services/settings.service';

@Component({
  selector: 'app-check-overview',
  templateUrl: './check-overview.component.html',
  styleUrls: ['./check-overview.component.scss']
})
export class CheckOverviewComponent extends BaseComponent implements OnInit, OnDestroy {

  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;

  rowData: VanChecks[] = [];
  showloadingIndicator = false;

  public toolBarTitle = 'Check Overview';
  public toolBarSubTitle = 'Manage Checks';
  public settingsButtonVisible = false;
  public deleteButtonVisible = false;
  public deleteButtonActive = false;
  public editButtonVisible = false;
  public editButtonActive = false;
  public addButtonVisible = true;
  public addButtonActive = true;

  constructor(
    private masterDataService: MasterDataService,
    public settingsService: SettingsService,
    public dialog: MatDialog
  ) {
    super();

    this.columnDefs = [
      {
        headerName: 'Environment Name', field: 'environmentName', sort: 'asc',
        headerCheckboxSelection: true,
        headerCheckboxSelectionFilteredOnly: true,
        checkboxSelection: true
      },
      { headerName: 'Check Id', field: 'elementId' },
      { headerName: 'Check Name', field: 'name' },
      { headerName: 'Description', field: 'description' },
      { headerName: 'Frequency (min)', field: 'frequency', valueFormatter: this.frequencyFormatter, width: 130}
    ];

    this.gridOptions = <GridOptions>{
      enableColResize: true,
      cacheOverflowSize: 2,
      onGridReady: function (params) {
        this.onGridReady(params);
      }.bind(this),
      onRowDoubleClicked: this.onRowDoubleClicked.bind(this),
      onRowSelected: this.updateCrudButtonState.bind(this),
      suppressRowClickSelection: true,
      defaultColDef: {
        comparator: function (a, b) {
          if (typeof a === 'string') {
            return a.localeCompare(b);
          } else {
            return (a > b ? 1 : (a < b ? -1 : 0));
          }
        }
      }
    };
    this.rowSelection = "multiple";

  }

  frequencyFormatter(params) {
    let newValue = params.value;
    if (params.value !== -1) {
      newValue = newValue / 60;
      newValue = +newValue.toFixed(2);
    } else if (params.value == -1) {
      newValue = "none";
    }
    return newValue;
  }


  ngOnInit() {
    this.showloadingIndicator = true;
    this.setRowData();
  }

  ngOnDestroy(): void {
    this.saveGridState();
    this.gridApi.destroy();
  }

  onGridReady(params) {
    const me = this;
    me.gridApi = params.api;
    me.gridColumnApi = params.columnApi;

    if (me.rowData) {
      me.gridApi.setRowData(me.rowData);
    }
    params.api.sizeColumnsToFit();

  }

  onFirstDataRendered(params) {
    // this.restoreGridState();
  }

  private updateCrudButtonState() {
     this.deleteButtonActive = this.gridApi.getSelectedRows().length > 0;
     this.deleteButtonVisible = this.gridApi.getSelectedRows().length > 0;
    if (this.gridApi.getSelectedRows().length === 1) {
      this.editButtonActive = true;
      this.editButtonVisible = true;
    } else if (this.gridApi.getSelectedRows().length === 0) {
      this.deleteButtonVisible = false;
      this.editButtonVisible = false;
    } else {
      this.editButtonActive = false;
    }
  }

  onRowDoubleClicked($event) {
    if ($event.node.data) {
      this.showEditDialog($event.node.data);
    }
  }

  private saveGridState() {

    this.settingsService.checkCrudGridOptions = {};
    if (this.gridApi) {
      this.settingsService.checkCrudGridOptions.colState = this.gridApi.gridOptionsWrapper.columnApi.getColumnState();
      this.settingsService.checkCrudGridOptions.groupState = this.gridApi.gridOptionsWrapper.columnApi.getColumnGroupState();
      this.settingsService.checkCrudGridOptions.sortState = this.gridApi.getSortModel();
      this.settingsService.checkCrudGridOptions.filterState = this.gridApi.getFilterModel();
    }
  }
  private restoreGridState() {
    if (this.settingsService.checkCrudGridOptions) {
      if (this.settingsService.checkCrudGridOptions.colState) {
        this.gridApi.gridOptionsWrapper.columnApi.setColumnState(this.settingsService.checkCrudGridOptions.colState);
      }
      if (this.settingsService.checkCrudGridOptions.groupState) {
        this.gridApi.gridOptionsWrapper.columnApi.setColumnGroupState(this.settingsService.checkCrudGridOptions.groupState);
      }
      if (this.settingsService.checkCrudGridOptions.sortState) {
        this.gridApi.setSortModel(this.settingsService.checkCrudGridOptions.sortState);
      }
      if (this.settingsService.checkCrudGridOptions.filterState) {
        this.gridApi.setFilterModel(this.settingsService.checkCrudGridOptions.filterState);
      }
    }
  }
  setRowData(): void {
    const me = this;
    me.showloadingIndicator = true;

    me.rowData = [];

    me.masterDataService.getAll(ElementType.CHECK).then((vanChecks: VanChecks[]) => {
      me.showloadingIndicator = false;
      if (vanChecks) {
        me.rowData = vanChecks;
      }

      if (me.gridApi) {
        me.gridApi.refreshCells();
        me.restoreGridState();
      }
      me.updateCrudButtonState();
    }, (error) => {
      me.showloadingIndicator = false;
      me.showErrorDialog("Reason: " + (error.error || error.message || error), "Error loading data");
    });

    this.deleteButtonVisible = false;
    this.editButtonVisible = false;
    this.editButtonActive = false;

  }

  openCreateDialog($event) {
    const vanCheck = new VanChecks();

    const dialogRef = this.dialog.open(CheckEditComponent, {
      width: '600px',
      data: { vanCheck: vanCheck, mode: 'create', title: 'New Check' },
      disableClose: true
    });

    dialogRef.afterClosed().toPromise().then(result => {
      if (result === "created") {
        this.refreshGrid();

      }
    });

  }

  openEditDialog() {
    const vanCheck: VanChecks = this.gridApi.getSelectedRows()[0];
    this.showEditDialog(vanCheck);

  }

  private showEditDialog(data: VanChecks) {
    const dialogRef = this.dialog.open(CheckEditComponent, {
      width: '600px',
      data: { vanCheck: data, mode: 'edit', title: 'Edit Check' },
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === "edited") {
        this.refreshGrid();
      }
    });
  }

  deleteCheck() {
    const message = "Are you sure you want to delete the Check(s)?";
    const title = "Delete Check?";
    const action = "Delete";
    const confirmationDialog = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: { message, title, action }
    });

    confirmationDialog.afterClosed().toPromise().then(result => {
      if (result) {
        const me = this;
        const rowsToDelete = this.gridApi.getSelectedRows();

        for (let i = 0; i < rowsToDelete.length; i++) {
          const elementId = rowsToDelete[i].elementId;
          const subscriptionId = rowsToDelete[i].environmentSubscriptionId;
          this.masterDataService.delete(ElementType.CHECK, subscriptionId, elementId).then(res => {
            if (i === rowsToDelete.length - 1) {
              this.refreshGrid();
            }
          },
            (error =>
              this.showErrorDialog(error, "Row(s) could not be deleted.")));
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

  protected refreshGrid() {
    this.saveGridState();
    this.setRowData();
  }

}
