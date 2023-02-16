import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { DataService } from '../shared/services/data.service';
// import { SignalRConnection } from 'ng2-signalr';
import { BaseComponent } from '../shared/base-component/base.component';
import { DashboardNodeEditComponent } from '../shared/dashboard-edit/dashboard-node-edit/dashboard-node-edit.component';
import { ElementType } from '../shared/services/masterdata.service';
import { MatDialog } from '@angular/material/dialog';
import { SettingsService } from '../shared/services/settings.service';
import { NodeBase } from '../shared/model/node-base';
import { CancelRequestDialogComponent } from '../shared/dialogs/cancel-request-dialog/cancel-request-dialog.component';


@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent extends BaseComponent implements OnInit, OnDestroy {

  public vanEnvironmentInfoFailed: boolean;
  public vanEnvironmentInfoLoaded: boolean;
  public vanEnvironmentInfoCancelled: boolean;
  isManuallyRefreshed = false;
  public showloadingIndicator = false;
  isUnassignedCompRemoved: boolean = false;

  environmentNameList: string[] = [];
  // private _connection: SignalRConnection;
  private environmentDataSubscription = null;

  constructor(private router: Router,
    private dataService: DataService,
    public settingsService: SettingsService,
    public dialog: MatDialog) {
    super();
  }

  ngOnInit() {
    const me = this;
    me.environmentNameList.length = 0;
    this.loadData(me);
    this.redirectLoggedOutIfUrlOtherThanDashboard();
  }

  loadData(me) {
    setTimeout(() => {
      const dialogRef = this.dialog.open(CancelRequestDialogComponent, {
        width: '300px',
        disableClose: true,
        data: { title: "Loading Environments", dataService: this.dataService }
      });

      me.dataService.getAllEnvironments(this.isManuallyRefreshed).then(function (result) {

        if (result) {
          if (result.length > 0) {
            for (let i = 0; i < result.length; i++) {
              me.environmentNameList.push(result[i].name);

              //logic only for SLA
              if (result[i].name.toLowerCase().includes('sla')) {
                result[i].isSLA = true;
                for (let j = 0; j < result[i].services.length; j++) {
                  result[i].services[j].isSLA = true;

                  //invisibling unassigned components for SLA contributor role
                  me.isUnassignedCompRemoved = false;
                  if (result[i].services[j].name.includes('UnassignedComponents') && me.settingsService.currentUserRoles.includes("Monitoring_contributor")) {
                    result[i].services.splice(j, 1);
                    me.isUnassignedCompRemoved = true;
                  }

                  if (!me.isUnassignedCompRemoved) {
                    for (let k = 0; k < result[i].services[j].actions.length; k++) {
                      result[i].services[j].actions[k].isSLA = true;

                      //Disabling components for Contributor
                      if (result[i].services[j].actions[k].components !== [] && me.settingsService.currentUserRoles.includes("Monitoring_contributor")) {
                        result[i].services[j].actions[k].components = [];
                      }
                    }
                  }
                }
              }
            }

            //once environmentNamelist has all environments name make redirect link logic visible or not
            // result.forEach((res, e) => {
            //   if (res.name.includes('SLA')) {
            //     res.services.forEach((ser, s) => {
            //       if (me.environmentNameList.find(x => x == ser.name)) {
            //         result[e].services[s].isDashboardAvailable = true;
            //       }
            //       ser.actions.forEach((act, a) => {
            //         if (me.environmentNameList.find(x => x == act.name)) {
            //           result[e].services[s].actions[a].isDashboardAvailable = true;
            //         }
            //       });
            //     });
            //   }
            // });

            // for (let i = 0; i < me.environmentNameList.length; i++) {
            //   // me.environmentNameList.push(result[i].name);

            //   //once environmentNamelist has all environments name make redirect link logic visible or not
            //   for(let m = 0; m < result[i].services.length; m++)
            //     { 
            //         me.environmentNameList.forEach(envName => {
            //           if(envName == result[i].services[m].name){
            //             result[i].services[m].isDashboardAvailable = true;
            //           }
            //       });  
            //     }    
            // }
            me.vanEnvironmentInfoLoaded = true;

          } else {
            me.vanEnvironmentInfoLoaded = true; // empty environment

          }

        } else {
          if (result === undefined) {
            me.vanEnvironmentInfoCancelled = true;

          } else {
            me.vanEnvironmentInfoFailed = true;

          }
        }




        dialogRef.close("loaded");
      },
        (err) => {
          me.vanEnvironmentInfoFailed = true;
          console.log('ErrEnvFailed:' + me.vanEnvironmentInfoFailed);
          dialogRef.close("error");
        }
      );

      me.environmentDataSubscription = me.dataService.environmentDataUpdated.subscribe((environmentName) => {
        if (environmentName && environmentName.length > 0 && !(environmentName === "housekeeping")) {
          me.dataService.getEnvironment(environmentName, false).then(function (environment) {
            if (environment) {
              const idx: number = me.environmentNameList.findIndex(env => env === environmentName);
              if (idx > -1) {
                console.log("Updating dashboard entry for " + environmentName);
                me.environmentNameList[idx] = null;
                setTimeout(() => {
                  me.environmentNameList[idx] = environmentName;
                }, 0);
              }
            }
          });
        }
      });
    });
  }

  gotoModule(moduleName: string) {
    this.router.navigate(['/' + moduleName], { queryParamsHandling: 'preserve' });
  }

  ngOnDestroy() {
    this.environmentDataSubscription.unsubscribe();
  }

  public addNewElement() {

    const dialogRef = this.dialog.open(DashboardNodeEditComponent, {
      disableClose: false,
      width: '600px',
      data: {
        mode: "create",
        elementTypeName: "environment",
        elementType: ElementType.ENVIRONMENT,
        parentNode: null,
        environmentName: ""
      }
    });
  }

  public onEditButtonPress(environmentName: string): void {
    const nodeToEdit: NodeBase = this.dataService.environments.find(environment => environment.name === environmentName);

    const dialogRef = this.dialog.open(DashboardNodeEditComponent, {
      disableClose: false,
      width: '600px',
      data: {
        mode: "update",
        view: "default",
        elementTypeName: nodeToEdit.getNodeTitle(),
        elementType: ElementType.ENVIRONMENT,
        currentNode: nodeToEdit,
        environmentName: environmentName
      }
    });
  }

  public onDeleteButtonPress(environmentName: string): void {
    const nodeToDelete: NodeBase = this.dataService.environments.find(environment => environment.name === environmentName);

    const dialogRef = this.dialog.open(DashboardNodeEditComponent, {
      disableClose: false,
      width: '600px',
      data: {
        mode: "delete",
        elementTypeName: nodeToDelete.getNodeTitle(),
        elementType: ElementType.ENVIRONMENT,
        currentNode: nodeToDelete,
        environmentName: environmentName
      }
    });
  }

  private redirectLoggedOutIfUrlOtherThanDashboard(): void { 
    let url: string = localStorage.getItem('url');
    if(url !== '' && url !== null)
      {
        url = decodeURIComponent(url);
        console.log('Redirecting url to : ('+url+')');
        this.router.navigate([url]);
        localStorage.removeItem('url');
      }
  }
}
