import { Component, OnInit, OnDestroy } from '@angular/core';
import { BaseComponent } from '../../shared/base-component/base.component';
import { GridOptions } from 'ag-grid-community';
import { MatDialog } from '@angular/material/dialog';
import { StateIncreaseRule } from '../../shared/model/stateIncreaseRule';
import { DataService } from '../../shared/services/data.service';
import { MasterDataService, ElementType } from '../../shared/services/masterdata.service';
import { VanChecks } from '../../shared/model/van-checks';
import { VanComponent } from '../../shared/model/van-component';
import { StateIncreaseRuleEditComponent } from '../state-increase-rule-edit/state-increase-rule-edit.component';
import { ConfirmationDialogComponent } from '../../shared/dialogs/confirmation-dialog/confirmation-dialog.component';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';

@Component({
  selector: 'app-state-increase-rule-overview',
  templateUrl: './state-increase-rule-overview.component.html',
  styleUrls: ['./state-increase-rule-overview.component.scss']
})
export class StateIncreaseRuleOverviewComponent extends BaseComponent implements OnInit, OnDestroy {

  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;

  public checkIds: VanChecks[] = [];
  public componentIds: VanComponent[] = [];

  rowData: StateIncreaseRule[] = [];
  showloadingIndicator = false;

  public toolBarTitle = 'State Increase Rule Overview'; // State Increase Rule Maintenance
  public toolBarSubTitle = ''; // text for later on: Maintain your State Increase Rules here!
  public settingsButtonVisible = false;
  public deleteButtonVisible = false;
  public editButtonVisible = false;
  public addButtonVisible = true;
  public addButtonActive = true;
  public editButtonActive = false;


  constructor(
    private dataService: DataService,
    private masterDataService: MasterDataService,
    public dialog: MatDialog
  ) {
    super();


    this.columnDefs = [
      {
        headerName: 'Active', field: 'isActive', suppressSizeToFit: true, width: 168, sort: 'desc',
        headerCheckboxSelection: true,
        headerCheckboxSelectionFilteredOnly: true,
        checkboxSelection: true
      },
      { headerName: 'Title', field: 'name', suppressSizeToFit: true },
      { headerName: 'Environment Name', field: 'environmentName', suppressSizeToFit: true },
      { headerName: 'Description', field: 'description', width: 230, suppressSizeToFit: true },
      { headerName: 'Component Id', field: 'componentId', suppressSizeToFit: true },
      { headerName: 'Check Id', field: 'checkId', suppressSizeToFit: true },
      { headerName: 'Alert Name', field: 'alertName', suppressSizeToFit: true },
      { headerName: 'Trigger Time [min]', field: 'triggerTime', suppressSizeToFit: true,  width: 168 }
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
    this.setRowData(false);

    me.masterDataService.getAll(ElementType.CHECK).then((vanChecks: VanChecks[]) => {
        this.checkIds = vanChecks;
    }, (error) => {
      me.showloadingIndicator = false;
      me.showErrorDialog("Reason: " + (error.error || error.message || error), "Error loading data");
    });

    me.masterDataService.getAll(ElementType.COMPONENT).then((vanComponents: VanComponent[]) => {
        this.componentIds = vanComponents;
    }, (error) => {
      me.showloadingIndicator = false;
      me.showErrorDialog("Reason: " + (error.error || error.message || error), "Error loading data");
    });
  }

  ngOnDestroy(): void {
    if (this.gridApi) {
      this.gridApi.destroy();
    }
  }

  setRowData(forceRefresh): void {
    const me = this;
    me.rowData = [];
    this.dataService.getAllStateIncreaseRules(forceRefresh).then((stateIncreaseRule: StateIncreaseRule[]) => {
      me.showloadingIndicator = false;
      if (stateIncreaseRule) {
        for (let i = 0; i < stateIncreaseRule.length; i++) {
          me.rowData.push(stateIncreaseRule[i]);
        }
        if (me.gridApi) {
          me.gridApi.setRowData(me.rowData);
        }
      }
    }, (error) => {
      me.showloadingIndicator = false;
      this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error loading data");
    });


  this.settingsButtonVisible = false;
  this.deleteButtonVisible = false;
  this.editButtonVisible = false;
  this.addButtonVisible = true;
  this.addButtonActive = true;
  this.editButtonActive = false;

  }

  protected refreshGrid() {
    this.setRowData(true);
  }

  defaultTextFormatter(params) {
    if (params.value) {
      return params.value;
    } else {
      return "Ongoing...";
    }
  }

  onGridReady(params) {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;
    if (this.rowData) {
      this.gridApi.setRowData(this.rowData);
    }
    params.api.sizeColumnsToFit();
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

  onRowDoubleClicked($event) {
    if ($event.node.data) {
      const item = $event.node.data;
      this.openDialog('edit', item);
    }
  }

  openEditDialog() {
    const stateIncreaseRule = this.gridApi.getSelectedRows()[0];
    this.openDialog('edit', stateIncreaseRule);
  }

  openCreateDialog($event) {
    const stateIncreaseRule = new StateIncreaseRule();
    this.openDialog('create', stateIncreaseRule);
  }


  deleteStateIncreaseRule() {
    const message = "Are you sure you want to delete this State Increase Rule?";
    const title = "Delete this State Increase Rule?";
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
          this.dataService.deleteStateIncreaseRule(id).then(res => {
            if (i === rowsToDelete.length - 1) {
              this.refreshGrid();
            }
          }, (error) => {
            this.showErrorDialog("Reason: " + (error.error || error.message || error), "Deployment could not be deleted");
          });
        }
      }
    });

  }

  openDialog(mode: string, stateIncreaseRule: StateIncreaseRule) {
    const me = this;
    const dialogRef = this.dialog.open(StateIncreaseRuleEditComponent, {
      width: '500px',
      data: { stateIncreaseRule: stateIncreaseRule, mode: mode, checks: me.checkIds, components: me.componentIds },
      disableClose: true
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result === "created" || result === "edited") {
        this.refreshGrid();
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
