import { Injectable } from '@angular/core';
import { AuthHttp } from './authHttp.service';
import { HeartbeatMsg } from '../model/heartbeat-msg';
import { DataService } from './data.service';
import { AdalService } from "adal-angular4";
import { VanStateTransition } from '../model/van-statetransition';
import { SLAReportJob } from '../model/sla-report-job';
import * as signalR from "@microsoft/signalr";
import { EnvironmentService } from '../services/config.service';
import { Environment } from "../model/environment-config";

@Injectable({
  providedIn: "root",
})
export class SignalRService {

  public isConnecting: boolean = false;
  public isConnected: boolean = false;
  public statusText: string = "Not connected";

  private baseUrl: string = "";
  private environmentConfig: Environment = null;

  constructor(protected http: AuthHttp,
    private dataService: DataService,
    private adalService: AdalService,
    envService: EnvironmentService
  ) {
      this.environmentConfig= envService.getConfiguration(); 
      if(this.environmentConfig !=null) {
          this.baseUrl= this.environmentConfig.signalREndpoint;
      }
  }


  public checkConnection() {

   if (!this.isConnected && !this.isConnecting) {
       this.reconnect(0);
   }

  }

  private reconnect(waitingTime?: number) {
    const me = this;
    me.statusText = "Checking availability of SignalR endpoint...";
    setTimeout(() => {
      console.log("Checking connectivity...");
      this.http.post(this.baseUrl + "/signalr/negotiate", "").toPromise()
        .then(function (response) {
          if (response && response.hasOwnProperty("connectionId")) {
            console.log("Backend available. Starting reconnect...");
            me.connect();
          } else {
            console.log("Backend not available. Retrying in 5s.");
            me.reconnect();
          }
        }, function (error) {
          console.log("Backend not available. Retrying in 5s.");
          me.reconnect();
        });
    }, (waitingTime ? waitingTime : 5000));
  }


  private connect(): any {

    if (!this.isConnected && !this.isConnecting) {
      const me = this;
      this.isConnecting = true;
      console.log('Connecting to SignalR endpoint...');

      this.adalService.acquireToken(this.adalService.config.clientId).toPromise().then((loginToken) => {

          const connection = new signalR.HubConnectionBuilder()
          .withUrl(this.baseUrl + "/signalr", { accessTokenFactory: () => loginToken })
          .withAutomaticReconnect()
          .build();

          connection
            .start()
            .catch(err => {
              console.error("Error while establishing connection: " + err.toString());
              me.isConnected = false;
              me.isConnecting = false;
              me.statusText = "Live data from the monitoring backend is not available.";
              this.reconnect();
            });

          me.isConnected = true;
          me.isConnecting = false;
          me.statusText = "Receiving real-time data from monitoring backend.";
          console.log('Connected to SignalR endpoint.');

          // create listeners on hub methods
          connection.on("updateHeartbeat", (heartbeatMessage: HeartbeatMsg) => {
            this.handleHeartBeatUpdates(heartbeatMessage);
          });
          connection.on("updateStateTransitions", (updateStateTransitionsMessage: VanStateTransition[]) => {
            this.handleStateUpdates(updateStateTransitionsMessage);
          });
          connection.on("updateDeploymentWindows", (updateDeploymentWindowsMessage: string) => {
            this.handleDeploymentWindowUpdates(updateDeploymentWindowsMessage);
          });
          connection.on("updateTree", (updateTreeMessage: string) => {
            this.handleTreeUpdateMsgUpdates(updateTreeMessage);
          });
          connection.on("deleteTree", (deleteTreeMessage: string) => {
            this.handleTreeDeleteMsgUpdates(deleteTreeMessage);
          });  
          connection.on("internalJobUpdated", (job: SLAReportJob) => {
            this.handleJobStateUpdate(job);
         });   
        }, (error) => {
          console.error("Error getting token for SignalR connection " + error.toString());
          me.reconnect();
        });
      }
  }

  private handleStateUpdates(transitionMessages: VanStateTransition[]): void {
    const me = this;

    if (Array.isArray(transitionMessages) && transitionMessages.length > 0) {
      // integrate Transitionmessages into cached tree and history list
      console.log(transitionMessages.length + " SignalR state update(s) received for environment " + transitionMessages[0].environmentName);
      me.dataService.updateEnvironmentWithState(transitionMessages[0].environmentName, transitionMessages);
    }
  }

  private handleHeartBeatUpdates(heartbeat: HeartbeatMsg) {
    if (heartbeat && heartbeat.environmentName && heartbeat.logSystemState && heartbeat.timeStamp) {
      console.log("Heartbeat update received: " + heartbeat.environmentName + " is " + heartbeat.logSystemState);
      this.dataService.updateHeartBeatForEnvironment(heartbeat);
    }
  }

  private handleDeploymentWindowUpdates(deploymentWindowMessage: string) {
    if (deploymentWindowMessage) {
      console.log("Deployment Windows change event received for environment: " + deploymentWindowMessage);
      this.dataService.updateDeploymentWindowsForEnvironment(deploymentWindowMessage);
    }
  }

  private handleTreeUpdateMsgUpdates(treeUpdateMessage: string) {
    if (treeUpdateMessage) {
      console.log("Tree update event received for environment: " + treeUpdateMessage);
      this.dataService.updateTreeForEnvironment(treeUpdateMessage);
    }
  }

  private handleTreeDeleteMsgUpdates(treeDeleteMessage: string) {
    if (treeDeleteMessage) {
      console.log("Tree delete event received for environment: " + treeDeleteMessage);
      this.dataService.deleteTree(treeDeleteMessage);
    }
  }

  private handleJobStateUpdate(job: SLAReportJob): void {
    if (job) {
      
      console.log("SignalR JobState received for job: " + job['Id'] + " state:" + job['State']);
      
      this.dataService.updateSLAReportsJobs(job);
    }
  }
}

export function createConfig(): any { // SignalRConfiguration {

  // const c = new SignalRConfiguration();
  // c.hubName = 'DeviceEventHub';
  // c.url = environment.signalREndpoint;
  // c.logging = false;
  // c.executeEventsInZone = true; // optional, default is true
  // c.executeErrorsInZone = false; // optional, default is false
  // c.executeStatusChangeInZone = true; // optional, default is true
  // return c;
}

