import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MasterDataService, ElementType } from '../../services/masterdata.service';
import { ErrorDialogComponent } from '../../dialogs/error-dialog/error-dialog.component';
import { NodeBase } from '../../model/node-base';
import { DataService } from '../../services/data.service';
import { VanEnvironment } from '../../model/van-environment';
import { VanComponent } from '../../model/van-component';
import { VanAction } from '../../model/van-action';
import { VanService } from '../../model/van-service';
import { VanNodeService } from '../../services/van-node.service';
import { GridOptions, RowNode } from 'ag-grid-community';
import { HttpErrorResponse } from '@angular/common/http';
import { ConfirmationDialogComponent } from '../../dialogs/confirmation-dialog/confirmation-dialog.component';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-dashboard-node-edit',
  templateUrl: './dashboard-node-edit.component.html',
  styleUrls: ['./dashboard-node-edit.component.css']
})
export class DashboardNodeEditComponent implements OnInit, OnDestroy {

  public payload: any;

  public showloadingIndicator = false;
  public dialogTitle: string;
  public isSaveButtonActive = false;
  public isAnyRowSelected = false;
  private elementType: ElementType;
  private currentNode: NodeBase;
  private isAddExistingElementPartActive = false;
  private currentNodeChildElements: NodeBase[];

  newNodeResponseObject: any;
  updatedNodeResponseObject: any;
  clientRefreshResponseObject: any;

  regexId   = '^[a-zA-Z0-9\\/][a-zA-Z0-9_\\-\\.\\/\\:]{4,499}$';
  regexTooltipId   = 'It must begin/end with letter or digit and only is allowed to contain following special characters: _ - . / :';

  regexElementId = '';
  regexTooltipElementId   = '';

  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions;
  rowData: NodeBase[] = [];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<any>,
    private snackbar: MatSnackBar,
    public dialog: MatDialog,
    private masterDataService: MasterDataService,
    private dataService: DataService,
    private vanNodeService: VanNodeService
  ) {
    this.elementType = this.data.elementType;

    if (this.data.mode === "delete") {
      this.dialogTitle = "Delete " + this.data.elementTypeName;
      const me = this;
      dialogRef = this.showConfirmationDialog("Do you really want to delete the element?", "Confirmation", "Delete", "Unassign", this.data.elementTypeName);
      const subscriptionOnDelete = dialogRef.componentInstance.onDelete.subscribe(() => {
        me.handleDeleteConfirmation(null);
      });
      const subscriptionOnCancel = dialogRef.componentInstance.onCancel.subscribe(() => {
        me.dialogRef.close();
      });
      const subscriptionOnUnassigned = dialogRef.componentInstance.onUnassigned.subscribe(() => {
        me.handleUnassignedConfirmation();
      });

      dialogRef.afterClosed().subscribe(() => {
        subscriptionOnDelete.unsubscribe();
        subscriptionOnUnassigned.unsubscribe();
        subscriptionOnCancel.unsubscribe();
      });

      this.dialogRef.close();
    }

    if (this.data.view === "addExisting") {
      this.gridOptions = <GridOptions>{
        enableColResize: true,
        cacheOverflowSize: 2,
        columnDefs: this.createColumnDefs(),
        onGridReady: function (params) {
          this.onGridReady(params);
        }.bind(this),
        onCellValueChanged: function onCellValueChanged() {
          this.onCellValueChanged();
        }.bind(this),
        onRowSelected: this.onRowSelected.bind(this),
        rowHeight: 48,
        rowMultiSelectWithClick: true
      };
      this.rowSelection = "multiple";
    }

  }

  ngOnInit() {
    const me = this;
    this.regexElementId = this.regexId;
    this.regexTooltipElementId = this.regexTooltipId;

    if (this.data.mode === "update") {
      if (this.data.view === "addExisting") {
        this.isAddExistingElementPartActive = true;
        this.createRowData();
        this.dialogTitle = "Add existing " + this.data.elementTypeName + "(s)";
        this.currentNode = this.data.currentNode;
        this.payload = this.currentNode.getUpdatePayload();
      } else {
        this.dialogTitle = "Update " + this.data.elementTypeName;
        this.currentNode = this.data.currentNode;
        this.payload = this.currentNode.getUpdatePayload();
      }
    } else if (this.data.mode === "create") {
      this.dialogTitle = "Create new " + this.data.elementTypeName;
      this.payload = this.createPayloadForElementType(this.elementType);
    } else {
      this.payload = {};
    }

    this.payload.environmentSubscriptionId = this.getNodeEnvironmentSubscriptionId();

    if (me.elementType === ElementType.ACTION) {
      me.payload.serviceElementId = me.data.parentNode.elementId;
    }
  }

  ngOnDestroy(): void {
    if (this.gridApi) {
      this.gridApi.destroy();
    }
  }

  onRowSelected($event) {
    this.isAnyRowSelected = true;
    if (this.gridApi.getSelectedRows().length >= 1) {
      this.isAnyRowSelected = true;
    } else if (this.gridApi.getSelectedRows().length === 0) {
      this.isAnyRowSelected = false;
    } else {
      this.isAnyRowSelected = false;
    }
  }

  onCloseClick() {
    this.dialogRef.close();
  }

  private updateParentNode(parentNode: NodeBase, elementIdOfNewChildNode: string): Promise<any> {
    const me = this;

    if (parentNode) {
      const updatePayload: any = parentNode.getUpdatePayload([elementIdOfNewChildNode]);
      updatePayload.environmentSubscriptionId = updatePayload.environmentSubscriptionId = me.getNodeEnvironmentSubscriptionId();

      // ugly workaround for strange action CRUD API to determine serviceElementID
      const environmentNode = me.dataService.environments.find(function (environment: VanEnvironment) {
        return environment.name === me.data.environmentName;
      });
      const nodePath: NodeBase[] = me.vanNodeService.getPathByElementId(environmentNode, parentNode.elementId, []);
      if (nodePath && nodePath.length > 1) {
        updatePayload.serviceElementId = nodePath[1].elementId;
      }

      return me.masterDataService.update(me.getElementTypeOfNode(parentNode), updatePayload, me.getNodeEnvironmentSubscriptionId()).then(function (data) {
        Promise.resolve(data);
      }, function (error) {
        Promise.resolve(error);
      });
    }
  }

  public onSaveClick() {
    const me = this;
    me.showloadingIndicator = true;
    let currentNodePromise: Promise<any>;

    if (me.data.view === "addExisting") {
      const currentRows = this.getAllRows();
      const componentList: string[] = [];
      for (const row of currentRows) {
        const currentRow = row as RowNode;
        if (currentRow.isSelected()) {
          componentList.push(currentRow.data.elementId);
        }
      }
      me.payload = componentList ? this.currentNode.getUpdatePayload(componentList) : this.currentNode.getUpdatePayload();
      if(this.data.currentParentNode)
      {
      me.payload.environmentSubscriptionId = this.getNodeEnvironmentSubscriptionId();
      var SubscriptionId = this.dataService.environments.find(function (environment: VanEnvironment) {
        return environment.name === me.data.parentEnvironmentName;
      }).subscriptionId;
      me.payload.serviceElementId = me.data.currentParentNode.elementId;
      currentNodePromise = me.masterDataService[me.data.mode](me.elementType, me.payload, SubscriptionId);
      }
      else
      {
        me.payload.environmentSubscriptionId = this.getNodeEnvironmentSubscriptionId();
        me.payload.serviceElementId = me.data.parentNode.elementId;
        currentNodePromise = me.masterDataService[me.data.mode](me.elementType, me.payload, me.getNodeEnvironmentSubscriptionId());
      }

      //If adding existing component from unassignedcomponents then uassign components from unassignedcomponents service which got assigned to other component
      this.dataService.getEnvironment(this.data.environmentName, false).then(environmentData => {
        console.log(environmentData);
        let UnassignedComponents = 'UnassignedComponents';
        let UnassignedComponentsAction = 'UnassignedComponentsAction';
        let envName = this.data.environmentName;
        const unassigncomponentList: string[] = [];
        if (environmentData != null) {
          if (environmentData.services.length > 0) {
            for (let i = 0; i < environmentData.services.length; i++) {
              if (environmentData.services[i].elementId === (UnassignedComponents + envName)) {
                for (let j = 0; j < environmentData.services.length; j++) {
                  if (environmentData.services[i].actions[j].elementId === (UnassignedComponentsAction + envName)) {
                    if (environmentData.services[i].actions[j].components.length > 0 &&
                      this.isAnyRowSelected && componentList.length > 0) {
                      environmentData.services[i].actions[j].components.forEach(component => {
                        unassigncomponentList.push(component.elementId);
                        console.log("No of Components before unassigning - " + environmentData.services[i].actions[j].components.length)
                      })
                      componentList.forEach(elementId => {
                        console.log("Index found for selected component: " + unassigncomponentList.indexOf(elementId) + " for ComponentId '" + elementId + "'");
                        if (unassigncomponentList.indexOf(elementId) > -1) {
                          unassigncomponentList.splice(unassigncomponentList.indexOf(elementId), 1);
                        }
                      });
                      console.log("No of Components after unassigning - " + unassigncomponentList.length)
                      this.unassignCompAfterAssigningCompFromUassignService(unassigncomponentList, currentNodePromise, UnassignedComponents, UnassignedComponentsAction);
                    }
                  }
                }
              }
            }
          }
        }
      });
      
    } else {
      // handle create / update of current node
      currentNodePromise = me.masterDataService[me.data.mode](me.elementType, me.payload, me.getNodeEnvironmentSubscriptionId());
    }

    currentNodePromise.then(
      function (result) {

        let updateParentNodePromise = Promise.resolve();

        // handle update of parent node (if applicable, only needed for n:m relationships)
        if (me.data.parentNode && me.data.mode === "create" && me.getElementTypeOfNode(me.data.parentNode) === ElementType.ACTION) {
          updateParentNodePromise = me.updateParentNode(me.data.parentNode, me.payload.elementId);
        }

        // wait for client refresh a bit so that we get the latest data
        const sleepPromise = new Promise(function (resolve) {
          setTimeout(resolve, 2000);
        });

        Promise.all([sleepPromise, updateParentNodePromise]).then(
          function (updateParentResult) {
            me.showloadingIndicator = false;
            me.dialogRef.close();
            me.snackbar.open("Element " + me.data.mode + "d successfully.", '', {
              horizontalPosition: "center",
              duration: 2000
            });
          }, function (error) {
            me.showloadingIndicator = false;
            me.showErrorDialog("Error updating parent node. Please reload the application and try to add the node via 'Add existing element'. Reason: " + error, "Error");
            return Promise.reject(error);
          });
      }, function (error) {
        me.showloadingIndicator = false;
        me.showErrorDialog("Error creating/updating node. Please check the console output for more info. Reason: " + error, "Error");
        return Promise.reject(error);
      });

  }

  private unassignCompAfterAssigningCompFromUassignService(unassigncomponentList: string[], currentNodePromise: Promise<any>,
    UnassignedComponents: string, UnassignedComponentsAction: string) {
    this.isAnyRowSelected = false;
    console.log("Unassigning unassigned components after assigning")
    this.payload.components = unassigncomponentList;
    this.payload.description = UnassignedComponentsAction;
    this.payload.name = UnassignedComponentsAction;
    this.payload.serviceElementId = UnassignedComponents + this.data.environmentName;
    this.payload.elementId = UnassignedComponentsAction + this.data.environmentName;
    this.payload.environmentSubscriptionId = this.getNodeEnvironmentSubscriptionId();

    currentNodePromise = this.masterDataService[this.data.mode](ElementType.ACTION, this.payload, this.getNodeEnvironmentSubscriptionId());
  }

  private showErrorDialog(message: string, title: string) {
    return this.dialog.open(ErrorDialogComponent, {
      width: '400px',
      disableClose: false,
      data: { message, title }
    });
  }

  private showConfirmationDialog(message: string, title: string, action: string, mode: string, nodeType: string) {
    return this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      disableClose: false,
      data: { message, title, mode, nodeType, action }
    });
  }

  public isFieldVisible(fieldName: string): boolean {
    return this.payload.hasOwnProperty(fieldName);
  }

  public isFieldActive(fieldName: string): boolean {
    return ((fieldName === "elementId" || fieldName === "subscriptionId") && this.data.mode !== "create");
  }

  private getElementTypeOfNode(node: NodeBase) {
    if (node instanceof VanEnvironment) {
      return ElementType.ENVIRONMENT;
    } else if (node instanceof VanService) {
      return ElementType.SERVICE;
    } else if (node instanceof VanAction) {
      return ElementType.ACTION;
    } else if (node instanceof VanComponent) {
      return ElementType.COMPONENT;
    }
  }

  private createPayloadForElementType(nodeElementType: string): any {
    switch (nodeElementType) {
      case (ElementType.ENVIRONMENT):
        return new VanEnvironment().getUpdatePayload();
      case (ElementType.SERVICE):
        return new VanService().getUpdatePayload();
      case (ElementType.ACTION):
        return new VanAction().getUpdatePayload();
      case (ElementType.COMPONENT):
        return new VanComponent().getUpdatePayload();
    }
  }

  private getNodeEnvironmentSubscriptionId(): string {
    const me = this;
    if (this.data.environmentName) {
      const result = this.dataService.environments.find(function (environment: VanEnvironment) {
        return environment.name === me.data.environmentName;
      }).subscriptionId;
      return result;
    } else {
      return this.payload.subscriptionId || this.payload.environmentSubscriptionId;
    }
  }

  handleDeleteConfirmation(result: any): void {
    console.log(result);
    this.deleteNode(this.data.currentNode);
  }

  public deleteNode(node: NodeBase) {
    const me = this;
    me.showloadingIndicator = true;

    // handle create / update of current node
    const currentNodePromise = me.masterDataService.delete(me.elementType, me.getNodeEnvironmentSubscriptionId(), node.elementId);

    currentNodePromise.then(function (result) {
      me.showloadingIndicator = false;
      me.dialogRef.close();
      me.snackbar.open("Element " + me.data.mode + "d successfully.", '', {
        horizontalPosition: "center",
        duration: 2000
      });
    }, function (error) {
      me.showloadingIndicator = false;
      me.showErrorDialog("Error deleting element. Please check the console output for more info. Reason: " + error, "Error");
      return Promise.reject(error);
    });
  }

  handleUnassignedConfirmation() {
    const me = this;
    me.showloadingIndicator = true;
    const selectedNode = me.data.currentNode;
    const selectedNodeParent = me.data.parentNode;
    const environmentNode = me.dataService.environments.find(function (environment: VanEnvironment) {
      return environment.name === me.data.environmentName;
    });

    const elementIdsWithRemovedSelectedNodeId: string[] = selectedNodeParent.getChildNodes().map(childNode => childNode.elementId)
      .filter(elementId => elementId !== selectedNode.elementId);

    const payload = selectedNodeParent.getUpdatePayload();
    payload.components = elementIdsWithRemovedSelectedNodeId ? elementIdsWithRemovedSelectedNodeId : [];
    payload.environmentSubscriptionId = me.getNodeEnvironmentSubscriptionId();
    const nodePath: NodeBase[] = me.vanNodeService.getPathByElementId(environmentNode, selectedNodeParent.elementId, []);
    if (nodePath && nodePath.length > 1) {
      payload.serviceElementId = nodePath[1].elementId;
    }

    const currentNodePromise = me.masterDataService["update"](ElementType.ACTION, payload, me.getNodeEnvironmentSubscriptionId());

    // wait for client refresh a bit so that we get the latest data
    const sleepPromise = new Promise(function (resolve) {
      setTimeout(resolve, 2000);
    });

    Promise.all([sleepPromise, currentNodePromise]).then(
      function (updateParentResult) {
        me.showloadingIndicator = false;
        me.dialogRef.close();
        me.snackbar.open("Element unassigned successfully.", '', {
          horizontalPosition: "center",
          duration: 2000
        });
      }, function (error) {
        me.showloadingIndicator = false;
        me.showErrorDialog("Error unassigning node. Reason: " + error, "Error");
        return Promise.reject(error);
      });
  }

  private createColumnDefs() {
    if (this.data.view === "addExisting") {
      return [
        {
          headerName: 'Name', field: 'name', width: 250, suppressSizeToFit: true, headerCheckboxSelection: true,
          headerCheckboxSelectionFilteredOnly: true,
          checkboxSelection: true, sort: 'asc'
        },
        {
          headerName: 'Element Id', field: 'elementId', width: 250, suppressSizeToFit: true
        },
        { headerName: 'Description', field: 'description', suppressSizeToFit: true },
        { headerName: 'Orphan', field: 'isOrphan', width: 140, suppressSizeToFit: true, valueFormatter: this.defaultTextFormatter },
        { headerName: 'Type', field: 'componentType', suppressSizeToFit: true }
      ];
    }
  }

  defaultTextFormatter(params) {
    if (params.value) {
      return "Yes";
    } else if ((!params.value)) {
      return "No";
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

  createRowData(): void {
    const me = this;
    me.showloadingIndicator = true;
    me.masterDataService.getAll(me.data.childElementType, me.getNodeEnvironmentSubscriptionId()).then(result => {
      me.showloadingIndicator = false;
      if (result) {
        // filter child elements that are already binded to the action
        let filteredResult = result;
        if (me.data.currentNode) {
          me.currentNodeChildElements = me.data.currentNode.getChildNodes() as NodeBase[];
          if (me.currentNodeChildElements && me.currentNodeChildElements.length > 0) {
            me.currentNodeChildElements.forEach(item => {
              filteredResult = filteredResult.filter(node => node.elementId !== item.elementId);
            });
          } else {
            filteredResult = result;
          }
        }
        this.isSaveButtonActive = filteredResult.length > 0;
        me.rowData = Array.isArray(filteredResult) ? filteredResult : [filteredResult];
        if (me.gridApi) {
          me.gridApi.setRowData(me.rowData);
        }
      }
    }, (error: HttpErrorResponse) => {
      me.showloadingIndicator = false;
      if (error && error.status === 404) {
        // tslint:disable-next-line:max-line-length
        me.showErrorDialog("No existing components found for the subscription id: " + me.getNodeEnvironmentSubscriptionId() + ". Please create one first.", "Info");
        return Promise.reject(error);
      } else {
        me.showErrorDialog("Error retrieving components. Please check the console output for more info. Reason: " + JSON.stringify(error.statusText ? error.statusText : error), "Error");
        return Promise.reject(error);
      }

    });
  }

  getAllRows(): Object[] {
    const rows: Object[] = [];
    const count = this.gridApi.getDisplayedRowCount();
    for (let i = 0; i < count; i++) {
      const row = this.gridApi.getDisplayedRowAtIndex(i);
      rows.push(row);
    }
    return rows;
  }

}
