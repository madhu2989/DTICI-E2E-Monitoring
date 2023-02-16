import { Component, OnInit, OnDestroy } from '@angular/core';
import { DatePipe, formatDate } from '@angular/common';
import { BaseComponent } from '../../shared/base-component/base.component';
import { GridOptions } from 'ag-grid-community';
import { MatDialog } from '@angular/material/dialog';
import { DeploymentWindow } from '../../shared/model/deployment-window';
import { DataService } from '../../shared/services/data.service';
import { DeploymentsEditComponent } from '../deployments-edit/deployments-edit.component';
import { ConfirmationDialogComponent } from '../../shared/dialogs/confirmation-dialog/confirmation-dialog.component';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';

@Component({
  selector: 'app-deployments-overview',
  templateUrl: './deployments-overview.component.html',
  styleUrls: ['./deployments-overview.component.scss']
})
export class DeploymentsOverviewComponent extends BaseComponent implements OnInit, OnDestroy {

  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;

  rowData: DeploymentWindow[] = [];
  showloadingIndicator = false;

  public toolBarTitle = 'Deployment Window Overview'; // Deployment Window Maintenance
  public toolBarSubTitle = ''; // text for later on: Maintain your deployment windows here!
  public settingsButtonVisible = false;
  public deleteButtonVisible = false;
  public editButtonVisible = false;
  public addButtonVisible = true;
  public addButtonActive = true;
  public editButtonActive = false;


  constructor(
    private dataService: DataService,
    public dialog: MatDialog,
    private datePipe: DatePipe
  ) {
    super();


    this.columnDefs = [
      {
        headerName: 'Start Date', field: 'startDate', suppressSizeToFit: true, width: 230, sort: 'desc',
        headerCheckboxSelection: true,
        headerCheckboxSelectionFilteredOnly: true,
        checkboxSelection: true,
        valueFormatter: this.dateFormatter
      },
      { headerName: 'Environment Name', field: 'environmentName', suppressSizeToFit: true },
      { headerName: 'Subscription Id', field: 'environmentSubscriptionId', suppressSizeToFit: true, width: 180 },
      { headerName: 'Deployment Id', field: 'id', suppressSizeToFit: true, width: 150 },
      { headerName: 'Description', field: 'description', suppressSizeToFit: true },
      { headerName: 'Short Description', field: 'shortDescription', suppressSizeToFit: true },
      { headerName: 'Close Reason', field: 'closeReason', suppressSizeToFit: true },
      { headerName: 'End Date', field: 'endDate', valueFormatter: this.dateFormatter, suppressSizeToFit: true },
      { headerName: 'Length', field: 'length', valueFormatter: this.convertSecondsToTimeFormatter, suppressSizeToFit: true },
      {
        headerName: 'Occurance', field: 'repeatInformation', suppressSizeToFit: true, cellRenderer: function (params) {
          let icon;
          icon = params.value ? 'repeat' : 'looks_one';
          const div = document.createElement('div');

          div.innerHTML = '<div style="display: flex; flex-direction: row;"><i class="material-icons" style="margin-top: 12px">'
            + icon + '</i></div>';

          return div;
        }
      },
      { headerName: 'ElementIds', field: 'elementIds', suppressSizeToFit: true, valueFormatter: this.removeEnvironmentElementId }
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
  }

  ngOnDestroy(): void {
    if (this.gridApi) {
      this.gridApi.destroy();
    }
  }

  setRowData(forceRefresh): void {
    const me = this;
    me.rowData = [];
    this.dataService.getAllDeploymentWindows(forceRefresh).then((deploymentWindow: DeploymentWindow[]) => {
      me.showloadingIndicator = false;
      if (deploymentWindow) {
        for (let i = 0; i < deploymentWindow.length; i++) {
          me.rowData.push(deploymentWindow[i]);
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

  dateFormatter(params) {
    if (params.value) {
        return formatDate(new Date(params.value), "yyyy-MM-dd, HH:mm", "en-US");
    } else {
        return "Ongoing...";
    }
}

  convertSecondsToTimeFormatter(params) {
    if (params.value) {
      const hours = Math.floor(params.value / 3600);
      const minutes = Math.floor((params.value % 3600) / 60);
      const seconds = Math.floor((params.value % 3600) % 60);
      return (hours > 0 ? hours + "h " : "") + (minutes > 0 ? minutes + "min " : " ") + (seconds > 0 ? seconds + "s" : "");
    }
  }

  removeEnvironmentElementId(params) {
    if (params.value == params.data.environmentSubscriptionId) {
      return "";
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
    const deploymentWindow = this.gridApi.getSelectedRows()[0];
    this.openDialog('edit', deploymentWindow);
  }

  openCreateDialog($event) {
    const deploymentWindow = new DeploymentWindow();
    this.openDialog('create', deploymentWindow);
  }


  deleteDeployment() {
    const message = "Are you sure you want to delete this Deployment?";
    const title = "Delete this Deployment?";
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
          const id = rowsToDelete[i].id;
          const subscriptionId = rowsToDelete[i].environmentSubscriptionId;
          this.dataService.deleteDeploymentWindow(id, subscriptionId).then(res => {
            if (i === rowsToDelete.length - 1) {
              this.deleteButtonVisible = false;
              this.refreshGrid();
            }
          }, (error) => {
            this.showErrorDialog("Reason: " + (error.error || error.message || error), "Deployment could not be deleted");
          });
        }
      }
    });

  }

  openDialog(mode: string, deployment: DeploymentWindow) {
    const dialogRef = this.dialog.open(DeploymentsEditComponent, {
      width: '800px',
      minHeight: '600px',
      data: { deployment: deployment, mode: mode },
      disableClose: true
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result === "refresh") {
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
