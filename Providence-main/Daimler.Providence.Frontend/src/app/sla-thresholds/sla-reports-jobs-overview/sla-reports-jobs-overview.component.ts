import { Component, OnInit } from '@angular/core';
import { VanEnvironment } from '../../shared/model/van-environment';
import { SLAReportJob } from '../../shared/model/sla-report-job';
import { DataService, IElementSearchResult, Element } from '../../shared/services/data.service';
import { BaseComponent } from '../../shared/base-component/base.component';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { ConfirmationDialogComponent } from '../../shared/dialogs/confirmation-dialog/confirmation-dialog.component';
import { GridOptions } from 'ag-grid-community';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from "@angular/material/snack-bar";
import { DatePipe } from '@angular/common';
import { UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { SlaReportJobsEditComponent } from '../../sla-thresholds/sla-reports-jobs-edit/sla-reports-jobs-edit.component';
import { SlaReportsComponent } from '../../sla-thresholds/sla-reports/sla-reports.component';
import { ButtonRendererComponent } from '../../shared/button-renderer/button-renderer.component';

@Component({
  selector: 'app-sla-reports-jobs',
  templateUrl: './sla-reports-jobs-overview.component.html',
  styleUrls: ['./sla-reports-jobs-overview.component.scss']
})
export class SlaReportJobsOverviewComponent extends BaseComponent implements OnInit {
    private gridApi;
    private gridColumnApi;
    public columnDefs;
    public rowSelection;
    public gridOptions: GridOptions;

    rowData: SLAReportJob[] = [];
    environments: VanEnvironment[] = [];
    showloadingIndicator = false;

    public toolBarTitle = 'SLA Jobs Overview';
    public toolBarSubTitle = '';
    public settingsButtonVisible = false;
    public deleteButtonVisible = false;
    public editButtonVisible = false;
    public addButtonVisible = true;
    public addButtonActive = true;
    public editButtonActive = false;
    public showDataButtonDisabled = true;

    presentationType: string;
    chosenElementId: string;
    searchedElementId: string;

    searchFrom: UntypedFormGroup;
    filteredElements: Observable<IElementSearchResult>;
    warningThreshold: number;
    errorThreshold: number;
    frameworkComponents: any;

    private slaJobDataSubscription = null;

    constructor(
        private dataService: DataService,
        public dialog: MatDialog,
        private datePipe: DatePipe,
        private fb: UntypedFormBuilder,
        private snackbar: MatSnackBar
    ) {
        super();

        this.frameworkComponents = {
          buttonRenderer: ButtonRendererComponent,
        }

        this.columnDefs = [
          {
            headerName: 'Environment Name', field: 'environmentName',suppressSizeToFit: true, width :230,
            headerCheckboxSelection: true,
            headerCheckboxSelectionFilteredOnly: true,
            checkboxSelection: true,
          },
          { headerName: 'Subscription Id', field: 'environmentSubscriptionId', suppressSizeToFit: true , width: 170},
          { headerName: 'Type', field: 'type', suppressSizeToFit: true, cellRenderer: this.jobTypeCellRenderer, width: 120},
          { headerName: 'Username', field: 'userName', suppressSizeToFit: true },           
          { headerName: 'State', field: 'state', suppressSizeToFit: true, cellRenderer: this.jobStateCellRenderer, width: 170 },     
          { headerName: 'StateInformation', field: 'stateInformation', suppressSizeToFit: true, width: 260 },            
          { headerName: 'Startdate', field: 'startDate', suppressSizeToFit: true,  width: 180 },
          { headerName: 'Enddate', field: 'endDate', suppressSizeToFit: true,  width: 180  },
          { headerName: 'QueueDate', field: 'queueDate', suppressSizeToFit: true, width: 190, sort: 'desc' },
          { headerName: 'Filename', field: 'fileName', suppressSizeToFit: true, width: 170},
          { headerName: 'Action', field: 'state', suppressSizeToFit: true, cellRenderer:  'buttonRenderer'              
           , cellRendererParams: { onClick: this.openShowJobResultDialog.bind(this), label: 'Show data'}, width: 140 },
        ];

        this.gridOptions = <GridOptions>{
          enableColResize: true,
          cacheOverflowSize: 2,
          onGridReady: function (params) {
            this.onGridReady(params);
          }.bind(this),
            onRowSelected: this.onRowSelected.bind(this),
            suppressRowClickSelection: true
        };
          this.rowSelection = "multiple";
    }

    ngOnInit() {
        const me = this;
        me.showloadingIndicator = true;
        me.dataService.getAllEnvironments(false).then(function (result) {
            if (result && result.length > 0) {
                for (let i = 0; i < result.length; i++) {
                    me.environments.push(result[i]);
                }
            }
        });
        this.setRowData(false);

        me.slaJobDataSubscription = me.dataService.slaJobDataUpdated.subscribe((job) => {           
            if (job !== null) {
                me.updateJobInTable(job);
            }
        });
    }

    ngOnDestroy(): void {
        this.slaJobDataSubscription.unsubscribe();
        if (this.gridApi) {
            this.gridApi.destroy();
        }
    }

    updateJobInTable(job: SLAReportJob): void {
        const me = this;
        const id = job['Id'];
        const state = job['State']; 
        const stateInformation = job['StateInformation']; 
        const fileName = job['FileName'];

        me.gridOptions.api.forEachNode((rowNode, index) => {
            
            if (id == rowNode.data.id) {
                console.log('Updating Job in Table Id:' + id + " state: " + state );
                
                var updated = rowNode.data;
                updated.state = state;
                updated.stateInformation = stateInformation;
                updated.fileName = fileName;
                rowNode.setData(updated);                
            }
        });             
    }

    setRowData(forceRefresh): void {
        const me = this;
        me.rowData = [];
        this.dataService.getAllSLAReportJobs(forceRefresh).then((slaReportJob: SLAReportJob[]) => {
            me.showloadingIndicator = false;
            if (slaReportJob) {
                for (let i = 0; i < slaReportJob.length; i++) {
                    me.rowData.push(slaReportJob[i]);
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

    onGridReady(params) {
        this.gridApi = params.api;
        this.gridColumnApi = params.columnApi;
          if (this.rowData) {
              this.gridApi.setRowData(this.rowData);
          }
          params.api.sizeColumnsToFit();
    }

    onRowSelected($event) {
        var selectedRows = this.gridApi.getSelectedRows();
        if ((selectedRows.length > 0) && (selectedRows.findIndex(x => x.state == 1) < 0) && (selectedRows.findIndex(x => x.state == 2) < 0)) {
            this.deleteButtonVisible = true;
        } else if (this.gridApi.getSelectedRows().length === 0 || selectedRows.findIndex(x => x.state == 1) >= 0 || selectedRows.findIndex(x => x.state == 2) >= 0) {
            this.deleteButtonVisible = false;
        }
    }

    openCreateDialog() {       
        const dialogRef = this.dialog.open(SlaReportJobsEditComponent, {
         width: '900px',
         data: { mode: 'create' },
         disableClose: false
       });
       dialogRef.afterClosed().subscribe(result => {
         if (result) {
           this.refreshGrid();
         }
       });
    }

    deleteSelectedElements() {
        const message = "Are you sure you want to delete this SLA Report?";
        const title = "Delete this SLA Report?";
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
                    this.dataService.deleteSLAReportJob(id).then(res => {
                        if (i === rowsToDelete.length - 1) {
                            this.deleteButtonVisible = false;
                            this.refreshGrid();
                        }
                    }, (error) => {
                        this.showErrorDialog("Reason: " + (error.error || error.message || error), "SLA Report could not be deleted");
                    });
                }
            }
        });
    }

    

    openShowJobResultDialog(event) {
        let jobId = event.rowData.id;
        let startDate = event.rowData.startDate;
        let endDate = event.rowData.endDate;
        let environmentSubscriptionId = event.rowData.environmentSubscriptionId;
        console.log('open job=' + jobId);
        const dialogRef = this.dialog.open(SlaReportsComponent, {
            width: '1500px',
            height: '750px',
            data: { id: jobId, startDate: startDate, endDate: endDate, environmentSubscriptionId: environmentSubscriptionId },
            disableClose: false
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.refreshGrid();
            }
        });
    }

    jobStateCellRenderer(params) {
        let slaStateString;
        let icon;
        const level = params.data ? params.value : params;
        switch (level) {
            case 1:
                icon = 'post_add';
                slaStateString = 'Queued';
                break;
            case 2:
                icon = 'autorenew';
                slaStateString = 'Running';
                break;
            case 3:
                icon = 'check_box';          
                slaStateString = 'Processed';
                break;
            case 4:
                icon = 'error';
                slaStateString = 'Error';
                break;
            default:
                icon = '';
                slaStateString = '-';
                break;
        }
        const div = document.createElement('div');
        div.innerHTML = '<div style="display: flex; flex-direction: row;">' +
            '<i class="material-icons" style="margin-top: 12px">' + icon + '</i>' + slaStateString +
            '</div>';
        return div;
    }

    jobTypeCellRenderer(params) {
        let slaStateString;
        let icon;
        const level = params.data ? params.value : params;
        switch (level) {
            case 1:
                slaStateString = 'SLA';
                break;
            default:
                slaStateString = '-';
                break;
        }
        return slaStateString;
    }

    displayFn(element: Element) {
        if (element) { return element.name; }
    }

    showErrorDialog(message: string, title: string) {
        const errorDialog = this.dialog.open(ErrorDialogComponent, {
            width: '400px',
            data: { message, title },
            disableClose: true
        });
    }
}
