
import { Component, OnInit, OnDestroy } from '@angular/core';
import { MatDialog } from "@angular/material/dialog";
import { Location } from "@angular/common";
import { UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';
import { ActivatedRoute, Router, Params } from "@angular/router";
import { BaseComponent } from '../shared/base-component/base.component';
import { ErrorDialogComponent } from "../shared/dialogs/error-dialog/error-dialog.component";
import { NodeBase } from '../shared/model/node-base';
import { VanNodeService } from "../shared/services/van-node.service";
import { DataService, IElementSearchResult, Element } from '../shared/services/data.service';
import { Subscription, Observable } from 'rxjs';
import { switchMap, debounceTime } from 'rxjs/operators';
import { SettingsService } from '../shared/services/settings.service';
import { SystemStateService } from '../shared/services/system-state.service';
import { DashboardNodeEditComponent } from '../shared/dashboard-edit/dashboard-node-edit/dashboard-node-edit.component';
//import { element } from '@angular/core/src/render3';
import { VanEnvironment } from '../shared/model/van-environment';
import { isParenthesizedTypeNode } from 'typescript';

@Component({
  selector: 'app-van-nodes-view',
  templateUrl: './van-nodes-view.component.html',
  styleUrls: ['./van-nodes-view.component.scss']
})
export class VanNodesViewComponent extends BaseComponent implements OnInit, OnDestroy {

  rootNodePath: string[] = [];
  rootNode: NodeBase;
  originalPathNotFound: boolean;
  view: number[];
  viewDW: number[];
  private urlSubscription: Subscription;
  environmentName: string;
  logSystemState: string;
  searchedElementId: string;

  searchFrom: UntypedFormGroup;
  filteredElements: Observable<IElementSearchResult>;

  constructor(
    private vanNodeService: VanNodeService,
    private router: Router,
    private route: ActivatedRoute,
    private systemStateService: SystemStateService,
    public dialog: MatDialog,
    public settingsService: SettingsService,
    private dataService: DataService,
    private fb: UntypedFormBuilder,
    private location: Location) {
    super();


    this.searchedElementId = this.route.snapshot.paramMap.get('elementId');
    this.location.go(window.location.pathname.split(";elementId")[0]);
  }

  ngOnInit() {
    const me = this;
    me.originalPathNotFound = false;
    this.urlSubscription = me.route.url.subscribe(value => {
      value.forEach(function (element) {
        me.rootNodePath.push(element.path);
      });
    });
    this.urlSubscription.unsubscribe();

    me.getNodeByPath(me.rootNodePath);
    me.view = [window.innerWidth - 100, 40];
    me.viewDW = [window.innerWidth - 100, 7];

    this.urlSubscription = me.route.params.subscribe((params: Params) => {
      me.environmentName = params['environmentId'];

      me.systemStateService.getEnvironmentLogSystemState(me.environmentName).then((result) => {
        me.logSystemState = result;
      });
    });

    this.searchFrom = this.fb.group({
      userInput: null
    });

    this.filteredElements = this.searchFrom.get('userInput').valueChanges
      .pipe(
        debounceTime(300),
        switchMap(value => this.dataService.getElementsPerEnvironment(this.environmentName, { name: value }, 1))
      );

  }

  ngOnDestroy(): void {
    this.urlSubscription.unsubscribe();
  }

  displayFn(element: Element) {
    if (element) { return element.name; }
  }

  getNodeByPath(path) {
    const me = this;
    this.vanNodeService.getNodeByPath(me.rootNodePath).then(result => {
      if (result !== null) {
        me.rootNode = result;
        if (me.originalPathNotFound) {
          me.router.navigate([me.rootNodePath.join("/")], { queryParamsHandling: 'preserve' });
          me.showErrorDialog("The specified element could not be found. You were redirected to the closest valid element.", "Error");
        }
      } else {
        if (me.rootNodePath.length > 1) {
          me.originalPathNotFound = true;
          me.rootNodePath.pop();
          me.getNodeByPath(me.rootNodePath);
        } else {
          me.router.navigate(["dashboard"], { queryParamsHandling: 'preserve' });
          me.showErrorDialog("The specified element could not be found. You are redirected to the dashboard.", "Error");
        }
      }
    });
  }

  showErrorDialog(message: string, title: string) {
    const dialogRef = this.dialog.open(ErrorDialogComponent, {
      width: '400px',
      disableClose: false,
      data: { message, title }
    });
  }

  public addNewElement() {

    const dialogRef = this.dialog.open(DashboardNodeEditComponent, {
      disableClose: false,
      width: '600px',
      data: {
        mode: "create",
        elementTypeName: this.rootNode.getChildNodesTitle().toLocaleLowerCase().substr(0, this.rootNode.getChildNodesTitle().length - 1),
        elementType: this.rootNode.getChildNodesTitle().toLocaleLowerCase(),
        parentNode: this.rootNode,
        environmentName: this.environmentName
      }
    });
  }

  public addExistingElement() {
    const me = this;
    var matchedNode = this.dataService.environments.find(function (environment: VanEnvironment) {
      return environment.name === me.rootNode.name;
    });
    if(!matchedNode)
    {
      matchedNode = this.dataService.environments.find(function (environment: VanEnvironment) {
        return environment.name === me.rootNodePath[1];
      });
    }
      const parentNodePath = me.rootNodePath;
    parentNodePath.pop();
    let parentNode: NodeBase;

    this.vanNodeService.getNodeByPath(parentNodePath).then(result => {
      if (result) {
        parentNode = result;

        if(matchedNode)
        {
          const dialogRef = this.dialog.open(DashboardNodeEditComponent, {
            disableClose: false,
            width: '800px',
            data: {
              mode: "update",
              view: "addExisting",
              addFromOtherEnvironment: true,
              elementTypeName: this.rootNode.getChildNodesTitle().toLocaleLowerCase().substr(0, this.rootNode.getChildNodesTitle().length - 1),
              elementType: this.rootNode.getNodeTitle().toLocaleLowerCase() + "s",
              childElementType: this.rootNode.getChildNodesTitle().toLocaleLowerCase(),
              parentNode: matchedNode,
              currentNode: this.rootNode,
              currentParentNode: parentNode,
              environmentName: matchedNode.name,
              parentEnvironmentName: this.environmentName
            }
          });
        }
        else
        {
        const dialogRef = this.dialog.open(DashboardNodeEditComponent, {
          disableClose: false,
          width: '800px',
          data: {
            mode: "update",
            view: "addExisting",
            elementTypeName: this.rootNode.getChildNodesTitle().toLocaleLowerCase().substr(0, this.rootNode.getChildNodesTitle().length - 1),
            elementType: this.rootNode.getNodeTitle().toLocaleLowerCase() + "s",
            childElementType: this.rootNode.getChildNodesTitle().toLocaleLowerCase(),
            parentNode: parentNode,
            currentNode: this.rootNode,
            environmentName: this.environmentName
          }
        });
      }
      }
    });
  }

  public checkIgnoreOkForNode(node: NodeBase): boolean {
    if (!this.settingsService.ignoreokMode) {
      return true;
    }
    if (node.state.state === 'WARNING' || node.state.state === 'ERROR') {
      return true;
    }
    return false;
  }

  public onEditButtonPress(elementId: string): void {
    const nodeToEdit: NodeBase = this.rootNode.getChildNodes().find(childNode => childNode.elementId === elementId);

    const dialogRef = this.dialog.open(DashboardNodeEditComponent, {
      disableClose: false,
      width: '600px',
      data: {
        mode: "update",
        view: "default",
        elementTypeName: this.rootNode.getChildNodesTitle().toLocaleLowerCase().substr(0, this.rootNode.getChildNodesTitle().length - 1),
        elementType: this.rootNode.getChildNodesTitle().toLocaleLowerCase(),
        parentNode: this.rootNode,
        currentNode: nodeToEdit,
        environmentName: this.environmentName
      }
    });
  }

  public onDeleteButtonPress(elementId: string): void {
    const nodeToEdit: NodeBase = this.rootNode.getChildNodes().find(childNode => childNode.elementId === elementId);

    const dialogRef = this.dialog.open(DashboardNodeEditComponent, {
      disableClose: false,
      width: '600px',
      data: {
        mode: "delete",
        elementTypeName: this.rootNode.getChildNodesTitle().toLocaleLowerCase().substr(0, this.rootNode.getChildNodesTitle().length - 1),
        elementType: this.rootNode.getChildNodesTitle().toLocaleLowerCase(),
        parentNode: this.rootNode,
        currentNode: nodeToEdit,
        environmentName: this.environmentName
      }
    });
  }

  public navigateSearch(searchedElement: Object) {
    const url = JSON.parse(JSON.stringify(searchedElement)).path;
    this.router.navigate([url, { elementId: JSON.parse(JSON.stringify(searchedElement)).elementId }]);
  }

  redirectToDashBoardAction(){
    const url = decodeURIComponent('/' + this.rootNode.name);
    this.router.navigate([url], { queryParamsHandling: 'preserve' });
  }
}