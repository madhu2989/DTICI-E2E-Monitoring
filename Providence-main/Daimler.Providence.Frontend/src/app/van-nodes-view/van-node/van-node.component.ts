import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { MatDialog } from "@angular/material/dialog";
import { MatSnackBar } from "@angular/material/snack-bar";
import { ActivatedRoute, Params, Router } from "@angular/router";
import { BaseComponent } from '../../shared/base-component/base.component';
import { NodeBase } from "../../shared/model/node-base";
import { SystemStateService } from "../../shared/services/system-state.service";
import { VanNodeService } from '../../shared/services/van-node.service';
import { CheckDetailsComponent } from "./check-details/check-details.component";
import { Subscription } from 'rxjs';
import { SettingsService } from '../../shared/services/settings.service';
import { DataService } from '../../shared/services/data.service';

@Component({
  selector: 'app-van-node',
  templateUrl: './van-node.component.html',
  styleUrls: ['./van-node.component.scss']
})
export class VanNodeComponent implements OnInit, OnDestroy {

  @Input() public elementId: string;

  elementNodeId: string;
  elementNode: NodeBase;
  results: object[];
  view: number[];
  viewOfElement: number[];
  viewDW: number[];
  environmentName: string;
  logSystemState: string;
  shouldRedirect: boolean;
  iconName: string;
  IconType: string;
  nodeType: string;
  private urlSubscription: Subscription;
  currentNode: NodeBase;
  nodeChildStatesSummary: object;
  nodeChildStatesTotal: number;
  nodeGrandChildStatesTotal: number;
  isCurrentUserAdmin: boolean = false;
  isredirectToDashBoardClicked: boolean = false;
  showUnassignedComponentsForContributor: boolean = true;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    public dialog: MatDialog,
    private systemStateService: SystemStateService,
    private vanNodeService: VanNodeService,
    private dataService: DataService,
    public settingsService: SettingsService,
    private snackbar: MatSnackBar) { }

  ngOnInit() {
    const me = this;
    me.shouldRedirect = true;
    me.results = [];
    me.view = [325, 25];
    me.viewOfElement = [325, 35];
    me.viewDW = [325, 5];

    this.urlSubscription = me.route.params.subscribe((params: Params) => {
      me.environmentName = params['environmentId'];
      if (me.settingsService.currentUserRoles.includes("Monitoring_admin")) {
        me.isCurrentUserAdmin = true;
      }
      
      me.vanNodeService.getNodeByElementId(me.environmentName, me.elementId).then((node) => {
        me.currentNode = node;

        //Enabling redirect dashboard link btn if dashboard is available for respective service/actions on SLA env
        if (me.environmentName.toLowerCase().includes('sla')) {
          me.currentNode.isSLA = true;
          me.dataService.environments.forEach((item) => {
            if (item.name.match(me.currentNode.name)) {
              me.currentNode.isDashboardAvailable = true;
            }
          });
        }

        me.vanNodeService.getNodeChildStatesSummary(me.elementId, me.environmentName).then((result) => {
          me.nodeChildStatesSummary = result;
          if (me.nodeChildStatesSummary[0]["series"]) {
            me.nodeChildStatesTotal = me.nodeChildStatesSummary[0]["series"][0].value + me.nodeChildStatesSummary[0]["series"][1].value
              + me.nodeChildStatesSummary[0]["series"][2].value;
          }
          if (me.nodeChildStatesSummary[1]["series"]) {
            me.nodeGrandChildStatesTotal = me.nodeChildStatesSummary[1]["series"][0].value + me.nodeChildStatesSummary[1]["series"][1].value
              + me.nodeChildStatesSummary[1]["series"][2].value;
          }
        }
        );

        me.nodeType = me.getNodeType();
      }, (error) => { console.log("error determining node " + me.environmentName + "/" + me.elementId); });

      me.systemStateService.getEnvironmentLogSystemState(me.environmentName).then((result) => {
        me.logSystemState = result;
      });
    });

    this.urlSubscription.unsubscribe();
  }

  ngOnDestroy(): void {
    this.urlSubscription.unsubscribe();
  }

  redirect(rootNode: NodeBase, childNode: NodeBase) {
    if (this.shouldRedirect && rootNode && !this.isredirectToDashBoardClicked) {

      let url = decodeURIComponent(this.getUrlWithoutOptionalParameter(this.router.url)) + '/' + rootNode.name.replace(/\//g, "%2F");

      if (childNode && ((childNode.getChildNodes() && childNode.getChildNodes().length > 0) || childNode.getNodeTitle() === "Component")) {
        url = decodeURIComponent(url) + '/' + childNode.name.replace(/\//g, "%2F");

      }
      //TODO: create central handling for url -> encode/decode logic
      url = url.replace(/\(/g, '%28');
      url = url.replace(/\)/g, '%29');
      //Removing encoding to resolve redirection issue
      //url = url.replace(new RegExp(' ', 'g'), '%20');
      url = url.replace(new RegExp('%25', 'g'), '%');
      // url = url.replace('%2F', '/');
      
      this.router.navigate([url], { queryParamsHandling: 'preserve' });
    }
    this.shouldRedirect = true;
  }

  showCheckDetails(element: NodeBase) {
    this.shouldRedirect = false;
    const dialogRef = this.dialog.open(CheckDetailsComponent, {
      width: '900px',
      maxWidth: '100vw',
      disableClose: false,
      data: { check: element }
    });
  }

  getIconName(): string {
    if (this.currentNode && this.currentNode.state) {
      switch (this.currentNode.state.state) {
        case ("ERROR"):
          return "cancel";
        case ("WARNING"):
          return "info";
        case ("OK"):
          return "check_circle";
        default:
          return "help";
      }
    }
    return "help";
  }

  getNodeType(): string {
    if (this.currentNode) {
      switch (this.currentNode.getNodeTitle()) {
        case ("Environment"):
          return "public";
        case ("Action"):
          return "directions_run";
        case ("Component"):
          return "extension";
        case ("Service"):
          return "category";
        case ("Check"):
          return "assignment_turned_in";
        default:
          return "help";
      }
    }

    return "help";
  }

  hasChildElements(): boolean {
    return this.currentNode && this.currentNode.getChildNodes() && this.currentNode.getChildNodes().length > 0;
  }

  getUrlWithoutOptionalParameter(url: string): string {
    if (url.indexOf("?") !== -1) {
      return url.substr(0, url.indexOf("?"));
    } else if (url.indexOf(";recordId") !== -1) {
      return url.substr(0, url.indexOf(";recordId"));
    } else if (url.indexOf(";elementId") !== -1) {
      return url.substr(0, url.indexOf(";elementId"));
    } else {
      return url;
    }
  }

  checkIgnoreok(listOfNodes): boolean {
    let showNodeList = false;
    if (!this.settingsService.ignoreokMode) {
      showNodeList = true;
    } else {
      for (const node of listOfNodes) {
        if (node.state.state === 'WARNING' || node.state.state === 'ERROR') {
          showNodeList = true;
          break;
        }
      }
    }
    return showNodeList;
  }

  redirectToDashBoard() {
    this.isredirectToDashBoardClicked = true;
    const url = decodeURIComponent('/' + this.currentNode.name);
    this.router.navigate([url], { queryParamsHandling: 'preserve' });
  }
}
