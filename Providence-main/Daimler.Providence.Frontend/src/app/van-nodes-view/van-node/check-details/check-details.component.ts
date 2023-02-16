import { Component, OnInit, Inject, ViewEncapsulation } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { VanNodeService } from "../../../shared/services/van-node.service";
import { NodeBase } from "../../../shared/model/node-base";
import { MAT_DIALOG_DATA, MatDialogRef, MatDialog } from "@angular/material/dialog";
import { MatSnackBar } from "@angular/material/snack-bar";
import { BaseComponent } from "../../../shared/base-component/base.component";
import { DatePipe } from "@angular/common";
import { JsonViewDialogComponent } from "../../../shared/dialogs/json-view-dialog/json-view-dialog.component";
import { DataService } from "../../../shared/services/data.service";
import { ErrorDialogComponent } from "../../../shared/dialogs/error-dialog/error-dialog.component";
import { SettingsService } from "../../../shared/services/settings.service";
import { AlertIgnoresEditComponent } from "../../../alert-ignores/alert-ignores-edit/alert-ignores-edit.component";
import { AlertIgnore } from "../../../shared/model/alert-ignore";
import { AlertIgnoreCondition } from "../../../shared/model/alert-ignore-condition";
import { GridOptions, ICellRendererParams } from 'ag-grid-community';
import { AlertComment } from '../../../shared/model/alert-comment';
import { ConfirmationDialogComponent } from '../../../shared/dialogs/confirmation-dialog/confirmation-dialog.component';
import { AddCommentComponent } from '../check-details/add-comment/add-comment.component';

@Component({
  selector: 'app-check-details',
  templateUrl: './check-details.component.html',
  styleUrls: ['./check-details.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class CheckDetailsComponent extends BaseComponent implements OnInit {

  public ignoreConditions: AlertIgnoreCondition;
  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;
  public dialogResult;

  rowData: AlertComment[] = [];
  showloadingIndicator = false;

  public toolBarTitle = 'Alert Comments';
  public settingsButtonVisible = false;
  public deleteButtonVisible = false;
  public editButtonVisible = false;
  public editButtonActive = false;
  public addButtonVisible = true;
  public addButtonActive = true;


  constructor(@Inject(MAT_DIALOG_DATA) public data: any, public dialogRef: MatDialogRef<any>,
    private datepipe: DatePipe,
    public dialog: MatDialog,
    private snackbar: MatSnackBar,
    private vanNodeService: VanNodeService,
    public settingsService: SettingsService,
    private router: Router,
    private dataService: DataService) {
    super();

    this.columnDefs = [
      {
        headerName: 'Timestamp', field: 'timestamp', headerCheckboxSelection: true,
        headerCheckboxSelectionFilteredOnly: true,
        checkboxSelection: true, sort: 'desc', width: 30
      },
      { headerName: 'User', field: 'user', width: 20 },
      { headerName: 'State', field: 'state', width: 20 },
      { headerName: 'Comment', field: 'comment', width: 30, cellRenderer: function (params) {
        const urlRegex = /(https?:\/\/[\w\d\/\.\-\_\%\?\!\&\\\#\@\.\(\)]+[^\.\s])/g;
        return params.value.replace(urlRegex, function(url) {
          return '<a target="_blank" rel="noopener noreferrer" href="' + url + '">' + url + '</a>';
        });
      } }
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


  public sourceTimestampLocal: string;
  public extendedHistoryData: any;
  public loading = false;
  public showDetailsButton = false;
  public isFullScreen = false;
  public fullScreenIcon = "fullscreen";
  private urlRegex = /(https?:\/\/[\w\d\/\.\-\_\%\?\!\&\\\#\@\.\(\)]+[^\.\s])/g;

  ngOnInit() {

    this.setRowData();
    if (this.data.historyData.sourceTimestamp) {
      this.sourceTimestampLocal = this.datepipe.transform(this.data.historyData.sourceTimestamp, 'yyyy-MM-ddTHH:mm:ss.SSS', '', 'en-US');
    }

    // if id = 0 it means that this record was received via signalR. It already contains the detailed StateTransition info.
    if (this.data.historyData.id !== 0) {
      this.loading = true;
      
      this.dataService.getStateTransitionById(this.data.historyData.id).then((result) => {
        this.extendedHistoryData = result;
        this.loading = false;
        
        const newIgnoreConditions = {
          alertName: this.extendedHistoryData.AlertName,
          subscriptionId: null,
          componentId: this.extendedHistoryData.ElementId,
          checkId: this.extendedHistoryData.TriggeredByCheckId,
          description: this.extendedHistoryData.Description,
          customField1: this.extendedHistoryData.CustomField1,
          customField2: this.extendedHistoryData.CustomField2,
          customField3: this.extendedHistoryData.CustomField3,
          customField4: this.extendedHistoryData.CustomField4,
          customField5: this.extendedHistoryData.CustomField5,
          state: this.extendedHistoryData.State
        };
        this.ignoreConditions = newIgnoreConditions;
        
        // enable href if custom fields contain a hyperlink
        if (this.extendedHistoryData.CustomField1) {         
            this.extendedHistoryData.CustomField1 = this.extendedHistoryData.CustomField1.replace(this.urlRegex, function(url) {
            return '<a id="insertedHref" style="color:white;" class="insertedHrefStyle" target="_blank" rel="noopener noreferrer" href="' + url + '">' + url + '</a>';            
          });          
        };
        if (this.extendedHistoryData.CustomField2) {         
          this.extendedHistoryData.CustomField2 = this.extendedHistoryData.CustomField2.replace(this.urlRegex, function(url) {
          return '<a id="insertedHref" class="insertedHrefStyle" target="_blank" rel="noopener noreferrer" href="' + url + '">' + url + '</a>';            
        });          
        };
        if (this.extendedHistoryData.CustomField3) {         
          this.extendedHistoryData.CustomField3 = this.extendedHistoryData.CustomField3.replace(this.urlRegex, function(url) {
          return '<a id="insertedHref" class="insertedHrefStyle" target="_blank" rel="noopener noreferrer" href="' + url + '">' + url + '</a>';            
        });          
        };
        if (this.extendedHistoryData.CustomField4) {         
          this.extendedHistoryData.CustomField4 = this.extendedHistoryData.CustomField4.replace(this.urlRegex, function(url) {
          return '<a id="insertedHref" class="insertedHrefStyle" target="_blank" rel="noopener noreferrer" href="' + url + '">' + url + '</a>';            
        });          
        };
        if (this.extendedHistoryData.CustomField5) {         
          this.extendedHistoryData.CustomField5 = this.extendedHistoryData.CustomField5.replace(this.urlRegex, function(url) {
          return '<a id="insertedHref" class="insertedHrefStyle" target="_blank" rel="noopener noreferrer" href="' + url + '">' + url + '</a>';            
        });          
        };

      }, (error) => {
        this.loading = false;
        this.showErrorDialog("Could not load alert details.", "Error");
      });
    } else {
      this.extendedHistoryData = this.data.historyData;
      
      this.ignoreConditions = {
        alertName: this.extendedHistoryData.AlertName,
        subscriptionId: null,
        componentId: this.extendedHistoryData.ElementId,
        checkId: this.extendedHistoryData.TriggeredByCheckId,
        description: this.extendedHistoryData.Description,
        customField1: this.extendedHistoryData.CustomField1,
        customField2: this.extendedHistoryData.CustomField2,
        customField3: this.extendedHistoryData.CustomField3,
        customField4: this.extendedHistoryData.CustomField4,
        customField5: this.extendedHistoryData.CustomField5,
        state: this.extendedHistoryData.State
      };
    }

    this.showDetailsButton = this.router.url.split("/").length !== 5;
  }

  redirectToExternalUrl(url: string) {
    if (url && url.length > 0) {
      window.open(url);
    }
  }

  onCloseClick() {
    this.dialogRef.close(this.dialogResult);
  }

  fullScreen(isFullScreen: boolean) {
    if (!isFullScreen) {
      this.dialogRef.updateSize('100vw');
      this.isFullScreen = !this.isFullScreen;
      this.fullScreenIcon = "fullscreen_exit";
    } else {
      this.dialogRef.updateSize('44vw');
      this.isFullScreen = !this.isFullScreen;
      this.fullScreenIcon = "fullscreen";
    }
    this.gridApi.sizeColumnsToFit();
  }

  showDetails() {
    const startNodes: NodeBase[] = [];
    let url: string;
    const me = this;

    this.dataService.getEnvironment(this.data.historyData.environmentName, false).then(environment => {
      url = '';
      const pathNodes = me.vanNodeService.getPathByElementId(environment, me.data.historyData.triggeredByElementId, startNodes);

      for (const node of pathNodes) {
        url = url + "/" + node.name.replace(/\//g, "%2F");
      }

      me.dialogRef.close();
      // tslint:disable-next-line:max-line-length
      me.router.navigate([url, { checkId: me.data.historyData.triggeredByCheckId, alertName: me.data.historyData.triggeredByAlertName, state: me.data.historyData.state, sourceTimestamp: me.data.historyData.sourceTimestamp }]);
    });
  }

  // Source: https://stackoverflow.com/questions/49102724/angular-5-copy-to-clipboard/49121680#49121680
  copyText(val: string) {
    
    
    let newVal = null;
    
    if (val.includes("<a") && val.endsWith("</a>")) {

         const tmpVal = val.match('(.*?)<a.*?">(.*?)</a>');
         
         newVal = tmpVal[1] + tmpVal[2];
    } else {
      newVal = val;
    }

    const selBox = document.createElement('textarea');
    selBox.style.position = 'fixed';
    selBox.style.left = '0';
    selBox.style.top = '0';
    selBox.style.opacity = '0';
    selBox.value = newVal;
    document.body.appendChild(selBox);
    selBox.focus();
    selBox.select();
    document.execCommand('copy');
    document.body.removeChild(selBox);
    this.snackbar.open(`Text has been copied to clipboard`, '', {
      panelClass: "mysnackbar",
      horizontalPosition: "center",
      duration: 2000
    });
  }

  openJsonViewDialog(title: string, message: string) {
    if (message.indexOf('insertedHref') == -1) {
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

  showErrorDialog(message: string, title: string) {
    const dialogRef = this.dialog.open(ErrorDialogComponent, {
      width: '400px',
      disableClose: false,
      data: { message, title }
    });
  }

  onIgnoreClick() {
    this.dataService.getEnvironment(this.data.historyData.environmentName, false).then(environment => {
      const environmentSubscriptionIdAlertIgnore = environment.subscriptionId;

      const alertIgnore: AlertIgnore = {
        id: null,
        environmentSubscriptionId: environmentSubscriptionIdAlertIgnore,
        environmentName: this.data.historyData.environmentName,
        name: this.data.historyData.triggeredByAlertName,
        creationDate: null,
        expirationDate: "",
        ignoreCondition: this.ignoreConditions
      };

      

      const dialogRef = this.dialog.open(AlertIgnoresEditComponent, {
        width: '1200px',
        data: { alertIgnore: alertIgnore, mode: 'createFromPopup' },
        disableClose: true
      });
    });
  }

  onGridReady(params) {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;
    if (this.rowData) {
      this.gridApi.setRowData(this.rowData);
    }
    params.api.sizeColumnsToFit();
  }
  setRowData(): void {
    const me = this;
    me.rowData = [];
    this.dataService.getAlertCommentsPerRecordId(this.data.historyData.recordId).then((alertComments: AlertComment[]) => {
      me.showloadingIndicator = false;
      if (alertComments) {
        for (let i = 0; i < alertComments.length; i++) {
          me.rowData.push(alertComments[i]);
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
      const dialogRef = this.dialog.open(AddCommentComponent, {
        width: '500px',
        data: { alertComment: item, mode: 'edit', recordId: this.data.historyData.recordId},
        disableClose: true
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result === "refresh") {
          this.refreshGrid();
          this.editButtonVisible = false;
          this.deleteButtonVisible = false;
          this.dialogResult = "refresh";
        }
      });
    }
  }

  openEditDialog() {
    const alertComment = this.gridApi.getSelectedRows()[0];

    const dialogRef = this.dialog.open(AddCommentComponent, {
      width: '500px',
      data: { alertComment: alertComment, mode: 'edit', recordId: this.data.historyData.recordId},
      disableClose: true
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result === "refresh") {
        this.refreshGrid();
        this.editButtonVisible = false;
        this.deleteButtonVisible = false;
        this.dialogResult = "refresh";
      }
    });
  }

  openCreateDialog($event) {
    const alertComment = new AlertComment();

    const dialogRef = this.dialog.open(AddCommentComponent, {
      width: '500px',
      data: { alertComment: alertComment, mode: 'create', recordId: this.data.historyData.recordId},
      disableClose: true
    });

    dialogRef.afterClosed().toPromise().then(result => {
      if (result === "refresh") {
        this.refreshGrid();
        this.editButtonVisible = false;
        this.deleteButtonVisible = false;
        this.dialogResult = "refresh";
      }
    });

  }
  protected refreshGrid() {
    this.setRowData();
  }


  deleteAlertComment() {
    const message = "Are you sure you want to delete the selected Comment(s)?";
    const title = "Delete Alert Comment?";
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
          this.dataService.deleteAlertComment(id).then(res => {
            if (i === rowsToDelete.length - 1) {
              this.refreshGrid();
              this.dialogResult = "refresh";
              this.editButtonVisible = false;
              this.deleteButtonVisible = false;
            }
          },
            (error =>
              this.showErrorDialog(error, "Row could not be deleted")));
        }
      }
    });

  }
}
