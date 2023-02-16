import { Component, OnInit, Input } from '@angular/core';
import { Router } from "@angular/router";
import { NodeBase } from '../../shared/model/node-base';
import { VanNodeService } from "../../shared/services/van-node.service";
import { BaseComponent } from '../../shared/base-component/base.component';
import { VanEnvironment } from '../../shared/model/van-environment';
import { DataService } from '../../shared/services/data.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SettingsService } from '../../shared/services/settings.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-van-environment',
  templateUrl: './van-environment.component.html',
  styleUrls: ['./van-environment.component.scss']
})
export class VanEnvironmentComponent extends BaseComponent implements OnInit {

  @Input() public vanEnvironmentName: string;


  lastHeartBeatError: boolean;
  // rootNodeId: string;
  rootNodeType: string;
  rootNode: NodeBase;
  rootNodePath: string[] = [];
  rootNodeChildElements: NodeBase[] = [];
  serviceList: NodeBase[] = [];
  isThereMoreServices: boolean;
  childElementCount: number;

  environment: VanEnvironment;



  constructor(private vanNodeService: VanNodeService,
    private router: Router,
    private dataService: DataService,
    private snackbar: MatSnackBar,
    public settingsService: SettingsService,
    private datePipe: DatePipe
  ) {
    super();
  }

  ngOnInit() {
    const me = this;

    me.environment = me.dataService.environments.find(env => env.name === me.vanEnvironmentName);

    if (me.environment) {
      me.rootNodePath.push(me.environment.name);

      me.vanNodeService.getNodeByPath(me.rootNodePath).then((result) => {
        if (result) {
          me.rootNode = result;

          me.rootNodeChildElements = me.rootNode.getChildNodes();
          if (me.rootNodeChildElements) {

            me.childElementCount = me.rootNodeChildElements.length;
            if (me.rootNodeChildElements.length > 0) {

              for (let i = 0; i < me.childElementCount; i++) {
                if (me.rootNodeChildElements[i].state.state === "ERROR") {
                  me.serviceList.push(me.rootNodeChildElements[i]);
                }
                if (me.serviceList.length >= 3) { break; }
              }

              if (me.serviceList.length < 3) {
                for (let j = 0; j < me.childElementCount; j++) {
                  if (me.rootNodeChildElements[j].state.state === "WARNING") {
                    me.serviceList.push(me.rootNodeChildElements[j]);
                  }
                  if (me.serviceList.length >= 3) { break; }
                }
              }

              if (me.serviceList.length < 3) {
                for (let k = 0; k < me.childElementCount; k++) {
                  if (me.rootNodeChildElements[k].state.state === "OK") {
                    me.serviceList.push(me.rootNodeChildElements[k]);
                  }
                  if (me.serviceList.length >= 3) { break; }
                }
              }


            }
          }



        }
      });

      if (!me.environment.logSystemState) {
        me.environment.logSystemState = "ERROR";
      }

      if (!me.environment.lastHeartBeat) {
        me.lastHeartBeatError = true; // "Last heart beat is unknown";
        me.environment.logSystemState = "ERROR";
      } else {
        me.lastHeartBeatError = false; // `Last heart beat:`; // ${me.environmentData.lastHeartBeat}
      }

    } else {
      me.environment = new VanEnvironment({
        name: "-"
      });
    }


  }

  getHeartBeatTooltip(): string {
    if (this.lastHeartBeatError) {
      return 'Last heartbeat is unknown';
    } else if (!this.lastHeartBeatError) {
      return 'Last heartbeat: \n ' + this.datePipe.transform(this.environment.lastHeartBeat, 'medium');
    }
  }

  redirect(environmentId) {
    this.rootNodePath = [];
    this.rootNodePath.push(environmentId);
    this.vanNodeService.getNodeByPath(this.rootNodePath).then((result) => {
      if (result) {
        this.rootNode = result;
        this.rootNodeChildElements = this.rootNode.getChildNodes();
        if (this.rootNodeChildElements) {
          this.childElementCount = this.rootNodeChildElements.length;
          const url = decodeURIComponent('/' + environmentId);
          this.router.navigate([url], { queryParamsHandling: 'preserve' });
        }
      } else {
        this.snackbar.open(`Cannot navigate to next level because there is no ${this.rootNode.getNodeTitle().toLowerCase()}`, '', {
          panelClass: "mysnackbar",
          horizontalPosition: "center",
          duration: 2000
        });
      }
    });    
  }

  redirectToService(environmentId: string, serviceNode: NodeBase) {
    const url = decodeURIComponent('/' + environmentId + '/' + serviceNode.name);
    this.router.navigate([url], { queryParamsHandling: 'preserve' });
  }

}