import { Component, OnInit, OnDestroy } from '@angular/core';
import { NodeBase } from '../../shared/model/node-base';
import { ActivatedRoute, Params } from '@angular/router';
import { VanChecks } from '../../shared/model/van-checks';
import { VanComponent } from '../../shared/model/van-component';
import { GridOptions, ICellRendererParams } from 'ag-grid-community';
import { DatePipe, Location } from '@angular/common';
import { BaseComponent } from '../../shared/base-component/base.component';
import { VanStateTransition } from '../../shared/model/van-statetransition';
import { CheckDetailsComponent } from '../van-node/check-details/check-details.component';
import { MatDialog } from '@angular/material/dialog';
import { VanNodeService } from '../../shared/services/van-node.service';
import { Subscription } from 'rxjs';
import { SystemStateService } from '../../shared/services/system-state.service';
import { SettingsService } from '../../shared/services/settings.service';
import { DataService } from '../../shared/services/data.service';
import { AppComponent } from '../../app.component';

@Component({
  selector: 'app-van-check-detail-grid-view',
  templateUrl: './van-check-detail-grid-view.component.html',
  styleUrls: ['./van-check-detail-grid-view.component.scss']
})
export class VanCheckDetailGridViewComponent extends BaseComponent implements OnInit, OnDestroy {

  private gridApi;
  private gridColumnApi;
  public rowSelection;
  public gridOptions: GridOptions;
  rowNodeIndex: number;

  results: object[];
  view: number[];
  viewDW: number[];
  rootNodePath: string[] = [];
  vanNodeData: NodeBase;
  elementId: string;
  data: any;
  vanComponentChecks: VanChecks[];
  componentRootNode: VanComponent;
  rowData: VanStateTransition[] = [];
  checkElementsOfHistoryData: VanStateTransition[];
  nodeElementsOfHistoryData: VanStateTransition[] = [];
  selectedRow: any;
  selectedRowTimestamp: string;
  triggerTimestamp: any;
  checkIdToSelect: string;
  alertNameToSelect: string;
  sourceTimestampToSelect: string;
  stateToSelect: string;

  nodeCheck: NodeBase;
  private urlSubscription: Subscription;
  logSystemState: string;
  showloadingIndicator = false;

  columnDefs = [
    {
      headerName: 'Timestamp UTC', field: 'sourceTimestamp', suppressSizeToFit: true, sort: 'desc', width: 215
    },  
    {
      headerName: 'Localtime', field: 'sourceTimestamp', suppressSizeToFit: true, width: 210, cellRenderer: (params: ICellRendererParams) => params.value ?
        `<div>${this.datepipe.transform(params.value, 'yyyy-MM-ddTHH:mm:ss.SSS', '', 'en-US')}</div>` : ''
    },
    { headerName: 'State', field: 'state', suppressSizeToFit: true, width: 115 },
    { headerName: 'Alert Description', field: 'description',  suppressSizeToFit: true, width: 280 },
    { headerName: 'Alert Name', field: 'triggeredByAlertName', suppressSizeToFit: true, width: 350},  
    { headerName: 'Element Id', field: 'elementId', suppressSizeToFit: true, width: 240 },
    { headerName: 'Check Id', field: 'triggeredByCheckId', suppressSizeToFit: true, width: 250 },
    { headerName: 'Record Id', field: 'recordId' , suppressSizeToFit: true },
    { headerName: 'customField1', field: 'customField1', suppressSizeToFit: true, cellRenderer: function(grafanalink) {
      if(grafanalink.value != null)
      {
        const actionLink = document.createElement('a');
        actionLink.innerHTML = `<a  target=”_blank” href="${grafanalink.value}" >Take me to alert</a>`
        return actionLink;
      }
    }},
    { headerName: 'customField2', field: 'customField2', suppressSizeToFit: true },
    { headerName: 'customField3', field: 'customField3', suppressSizeToFit: true },
    { headerName: 'customField4', field: 'customField4', suppressSizeToFit: true },
    { headerName: 'customField5', field: 'customField5', suppressSizeToFit: true },
    { headerName: 'Service Progress', field: 'progressState', cellRenderer: function(params) {
      let progressIcon;
      const div = document.createElement('div');
      switch (params.value) {
        case "OPEN":
          progressIcon = "priority_high";
          break;
        case "IN PROGRESS":
          progressIcon = "autorenew";
          break;
        case "DONE":
          progressIcon = "done_outline";
          break;
        case "NONE":
          progressIcon = "not_interested";
          break;
        default:
          progressIcon = "";
          break;
      }
      div.innerHTML = '<div style="display: flex; flex-direction: row;"><i class="material-icons" style="margin-top: 12px">'
                      + progressIcon + '</i><div style="margin: auto">' + params.value + '</div></div>';
      return div; }}
  ];

  constructor(private route: ActivatedRoute,
    public datepipe: DatePipe,
    public dialog: MatDialog,
    private vanNodeService: VanNodeService,
    private dataService: DataService,
    private systemStateService: SystemStateService,
    public settingsService: SettingsService,
    private location: Location,
    private appComponent: AppComponent
  ) {
    super();

    this.gridOptions = <GridOptions>{
      enableColResize: true,
      cacheOverflowSize: 2,
      onGridReady: function (params) {
        this.onGridReady(params);
      }.bind(this),
      onRowDoubleClicked: this.onRowDoubleClicked.bind(this)
    };
    this.rowSelection = "multiple";

    this.checkIdToSelect = this.route.snapshot.paramMap.get('checkId');
    this.alertNameToSelect = this.route.snapshot.paramMap.get('alertName');
    this.sourceTimestampToSelect = this.route.snapshot.paramMap.get('sourceTimestamp');
    this.stateToSelect = this.route.snapshot.paramMap.get('state');
    this.location.go(window.location.pathname.split(";checkId")[0]);
  }

  ngOnInit() {
    const me = this;
    me.results = [];
    me.view = [window.innerWidth - 100, 40];
    me.viewDW = [window.innerWidth - 100, 7];

    me.data = this.route.snapshot.data;


    // this version is with the history list of a component
    if (me.data && me.data.nodeData) {
      me.elementId = me.data.nodeData.elementId;
      me.componentRootNode = me.data.nodeData;
    }

    this.setRowData(false);

  }

  setRowData(forceRefresh: boolean) {
    const me = this;
    let environmentName: string;
    me.rowData = [];
    me.nodeElementsOfHistoryData = [];

    this.urlSubscription = me.route.params.subscribe((params: Params) => {
      environmentName = params['environmentId'];
    });

    me.systemStateService.getEnvironmentLogSystemState(environmentName).then((result) => {
      me.logSystemState = result;
    });
    this.dataService.getHistoryOfElementId(forceRefresh, environmentName, me.elementId, true).then((historyData: VanStateTransition[]) => {
      if (historyData) {
        me.nodeElementsOfHistoryData = historyData;

        if (me.nodeElementsOfHistoryData && me.nodeElementsOfHistoryData.length > 0) {
          for (let j = 0; j < me.nodeElementsOfHistoryData.length; j++) {
            if (me.nodeElementsOfHistoryData[j].progressState) {
              me.rowData.push(me.nodeElementsOfHistoryData[j]);
           
            }
          }
          if (me.gridApi) {
            me.gridApi.setRowData(me.rowData);
            if (this.checkIdToSelect) {
              this.selectRowInGrid(me.getClosestDate(me.elementId, me.checkIdToSelect, me.alertNameToSelect, me.stateToSelect, me.sourceTimestampToSelect));
            }
            me.gridApi.sizeColumnsToFit();
          }
        }
      }
    }, (error) => {
    });
  }

  ngOnDestroy(): void {
    this.urlSubscription.unsubscribe();

    // save current column state temporarily
    this.settingsService.checkDetailGridOptions = {};
    if (this.gridApi) {
      this.settingsService.checkDetailGridOptions.colState = this.gridApi.gridOptionsWrapper.columnApi.getColumnState();
      this.settingsService.checkDetailGridOptions.groupState = this.gridApi.gridOptionsWrapper.columnApi.getColumnGroupState();
      this.settingsService.checkDetailGridOptions.sortState = this.gridApi.getSortModel();
      this.settingsService.checkDetailGridOptions.filterState = this.gridApi.getFilterModel();
      if (this.gridApi.getSelectedRows() && this.gridApi.getSelectedRows().length > 0) {
        this.settingsService.checkDetailGridOptions.selectedSourceTimestamp = this.gridApi.getSelectedRows()[0].sourceTimestamp;
      } else {
        this.settingsService.checkDetailGridOptions.selectedSourceTimestamp = null;
      }

    }

  }

  onGridReady(params) {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;
    if (this.rowData) {
      this.gridApi.setRowData(this.rowData);
    }
    params.api.sizeColumnsToFit();
    if (this.checkIdToSelect) {

      this.selectRowInGrid(this.getClosestDate(this.elementId, this.checkIdToSelect, this.alertNameToSelect, this.stateToSelect, this.sourceTimestampToSelect));

    }

    if (this.settingsService.checkDetailGridOptions) {
      if (this.settingsService.checkDetailGridOptions.colState) {
        this.gridApi.gridOptionsWrapper.columnApi.setColumnState(this.settingsService.checkDetailGridOptions.colState);
      }
      if (this.settingsService.checkDetailGridOptions.groupState) {
        this.gridApi.gridOptionsWrapper.columnApi.setColumnGroupState(this.settingsService.checkDetailGridOptions.groupState);
      }
      if (this.settingsService.checkDetailGridOptions.sortState) {
        this.gridApi.setSortModel(this.settingsService.checkDetailGridOptions.sortState);
      }
      if (this.settingsService.checkDetailGridOptions.filterState) {
        this.gridApi.setFilterModel(this.settingsService.checkDetailGridOptions.filterState);
      }
      if (this.settingsService.checkDetailGridOptions.selectedSourceTimestamp && !this.checkIdToSelect) {
        this.selectRowInGrid(this.settingsService.checkDetailGridOptions.selectedSourceTimestamp);
      }


    }
  }

  receiveEvent($event) {
    this.selectedRow = $event;
    const me = this;

    if (this.selectedRow) {

      // tslint:disable-next-line:max-line-length
      this.selectRowInGrid(me.getClosestDate(me.selectedRow[4].triggeredByElementId, me.selectedRow[4].triggeredByCheckId, me.selectedRow[4].triggeredByAlertName, me.selectedRow[4].state, me.selectedRow[4].sourceTimestamp));
    }
  }

  getClosestDate(triggeredByElementId: string, triggeredByCheckId: string, triggeredByAlertName: string, state: string, sourceTimestamp: string): string {


    if (triggeredByElementId && triggeredByCheckId && sourceTimestamp && this.rowData && this.rowData.length > 0) {
      const rowsInGrid = this.rowData;

      const sortedGridRows = rowsInGrid.sort((one, two) => (one.sourceTimestamp > two.sourceTimestamp ? -1 : 1));

      for (let i = 0; i < sortedGridRows.length; i++) {
        if ((triggeredByElementId === sortedGridRows[i].elementId) &&
          (triggeredByCheckId === sortedGridRows[i].checkId || sortedGridRows[i].checkId == null) &&
          ((!triggeredByAlertName && !sortedGridRows[i].alertName) || (triggeredByAlertName === sortedGridRows[i].alertName)) &&
          (state === sortedGridRows[i].state) &&
          (sortedGridRows[i].sourceTimestamp.valueOf() <= sourceTimestamp.valueOf())) {
          return (sortedGridRows[i].sourceTimestamp.valueOf());
        }
      }
    }

    return null;

  }

  selectRowInGrid(rowTimestamp: string): void {
    const me = this;
    if (this.gridApi) {
      this.gridApi.deselectAll();

      if (rowTimestamp) {

        setTimeout(() => {
          me.gridApi.forEachNode(function (node, selectedRowTimestamp) {
            if (node.data.sourceTimestamp === rowTimestamp) {
              node.setSelected(true);
              me.rowNodeIndex = node.rowIndex;
              me.gridApi.ensureIndexVisible(me.rowNodeIndex, 'top');
            }
          });
        }, 0);

      }

    }
  }

  onRowDoubleClicked($event) {
    if ($event.node.data) {
      console.log('onRowDoubleClicked: ' + $event.node.data.sourceTimestamp);
      const item = $event.node.data;
      this.prepareDataForCheckDetailsDialog(item);
    }
  }

  prepareDataForCheckDetailsDialog(historyData: any) {
    const me = this;
    if (historyData && historyData.environmentName) {
      me.vanNodeService.getNodeByElementId(historyData.environmentName, historyData.elementId).then((node) => {
        me.vanNodeData = node;

        if (me.vanNodeData && me.vanNodeData.checks.length === 0) {
          this.vanNodeService.getNodeByElementId(historyData.environmentName, historyData.triggeredByCheckId).then((nodeChk) => {
            if (nodeChk) {
              me.nodeCheck = nodeChk;
            }
          });
        } else {
          me.nodeCheck = this.vanNodeData.checks.find(function (nodeChk: NodeBase) {
            return nodeChk.elementId === historyData.triggeredByCheckId;
          });
        }
        this.openDetailsCheckDialog(historyData, me.vanNodeData, me.nodeCheck);
      });
    }

  }

  openDetailsCheckDialog(historyData: any, nodeData: NodeBase, nodeCheck: NodeBase) {
    const dialogRef = this.dialog.open(CheckDetailsComponent, {
      width: '900px',
      maxWidth: '100vw',
      disableClose: true,
      data: { historyData, nodeData, nodeCheck }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === "refresh") {
        this.setRowData(true);
      }
    });
  }
}
