import { Component, OnInit, OnDestroy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { GridOptions } from 'ag-grid-community';
import { DataService } from '../../shared/services/data.service';
import { BaseComponent } from '../../shared/base-component/base.component';
import { LicenseData } from '../../shared/model/license-data';
import { MatDialog } from '@angular/material/dialog';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { ConfirmationDialogComponent } from '../../shared/dialogs/confirmation-dialog/confirmation-dialog.component';


@Component({
  selector: 'app-licenses-overview',
  templateUrl: './licenses-overview.component.html',
  styleUrls: ['./licenses-overview.component.scss']
})
export class LicensesOverviewComponent implements OnInit, OnDestroy {

  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;

  rowData: LicenseData[] = [];
  showloadingIndicator = false;

  public toolBarTitle = 'License Overview';
  public toolBarSubTitle = '';
  public settingsButtonVisible = false;
  public deleteButtonVisible = false;
  public editButtonVisible = false;
  public editButtonActive = false;
  public addButtonVisible = false;
  public addButtonActive = false;


  constructor(
    private dataService: DataService,
    public dialog: MatDialog,
    private datePipe: DatePipe
  ) {
    // super();


    this.columnDefs = [
      {
        headerName: 'Package', field: 'package', headerCheckboxSelection: false,
        headerCheckboxSelectionFilteredOnly: false,
        checkboxSelection: false, sort: 'asc'
      },
      { headerName: 'Version', field: 'version' },
      { headerName: 'License', field: 'license' },
      
   
    ];

    this.gridOptions = <GridOptions>{
      enableColResize: true,
      cacheOverflowSize: 2,
      onGridReady: function (params) {
        this.onGridReady(params);
      }.bind(this),
      //onRowDoubleClicked: this.onRowDoubleClicked.bind(this),
      //onRowSelected: this.onRowSelected.bind(this),
      suppressRowClickSelection: true
    };
    // this.rowSelection = "multiple";

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

  

  

  protected refreshGrid() {
    this.setRowData(true);
    this.editButtonVisible = false;
    this.deleteButtonVisible = false;
  }

  setRowData(forceRefresh): void {
    const me = this;
    me.rowData = [];
    this.dataService.getAllLicenses().then(result => {
      me.showloadingIndicator = false;
      if (result) {
        for (let i = 0; i < result.length; i++) {
          me.rowData.push(result[i]);
        }
        if (me.gridApi) {
          me.gridApi.setRowData(me.rowData);
        }
      }
    }, (error) => {
      me.showloadingIndicator = false;
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
