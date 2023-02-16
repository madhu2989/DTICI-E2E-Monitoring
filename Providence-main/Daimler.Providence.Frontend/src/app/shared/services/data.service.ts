import { HttpErrorResponse, HttpHeaders, HttpParams, HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, interval, Observable, of, Subject } from 'rxjs';
import { timeout, takeUntil } from 'rxjs/operators';
import { HeartbeatMsg } from '../model/heartbeat-msg';
import { NodeBase } from '../model/node-base';
import { VanAction } from '../model/van-action';
import { VanChecks } from '../model/van-checks';
import { VanComponent } from '../model/van-component';
import { VanEnvironment } from '../model/van-environment';
import { VanService } from '../model/van-service';
import { VanStateTransition } from '../model/van-statetransition';
import { AuthHttp } from './authHttp.service';
import { SettingsService } from './settings.service';
import { DeploymentWindow } from '../model/deployment-window';
import { AlertIgnore } from '../model/alert-ignore';
import { NotificationRule } from '../model/notification-rule';
import { StateIncreaseRule } from '../model/stateIncreaseRule';
import { AlertComment } from '../model/alert-comment';
import { Changelog } from '../model/changelog';
import { MatDialog } from '@angular/material/dialog';
import { CancelRequestDialogComponent } from '../dialogs/cancel-request-dialog/cancel-request-dialog.component';
import { SLAConfig } from '../model/sla-config';
import { SLAReport } from '../model/sla-report';
import { LicenseData } from '../model/license-data';
import { SLAReportJob } from '../model/sla-report-job';
import { EnvironmentService } from "../services/config.service"
import { Environment } from "../model/environment-config";

@Injectable({
    providedIn: "root",
})
export class DataService {

    protected headers = new HttpHeaders().append(
        'Content-Type', 'application/json'
    );
    protected baseUrl: string = "";
    protected defaultTimeout = 60000;
    protected slaTimeout = 3600000;
    protected entityId: any;

    private ngUnsubscribe = new Subject();

    objectCache: any = new Object();
    public environments: VanEnvironment[] = [];
    public history: object[] = [];
    public deploymentWindows: object[] = [];
    public alertIgnores: object[] = [];
    public notificationRules: object[] = [];
    public changelogs: object[] = [];
    public stateIncreaseRules: object[] = [];
    public slaReportJobs: object[] = [];
    private historyLoaded: any = [];
    private extendedHistoryLoaded: any = [];
    private environmentConfig: Environment = null;
    public environmentDataUpdated: BehaviorSubject<string> = new BehaviorSubject<string>("");
    public slaJobDataUpdated: BehaviorSubject<SLAReportJob> = new BehaviorSubject<SLAReportJob>(null);
    public environmentLogSystemStateChanged: BehaviorSubject<string> = new BehaviorSubject<string>("");
    public environmentDeploymentWindowsDataChanged: BehaviorSubject<string> = new BehaviorSubject<string>("");
    public environmentsTreeChanged: BehaviorSubject<string> = new BehaviorSubject<string>("");
    public dashboardChanged: BehaviorSubject<string> = new BehaviorSubject<string>("");

    constructor(protected http: AuthHttp,
        protected settingsService: SettingsService,
        private httpClient: HttpClient, public dialog: MatDialog,
        envService: EnvironmentService) {
            this.environmentConfig= envService.getConfiguration(); 
            if(this.environmentConfig !=null) {
                interval(this.environmentConfig.houseKeepingInterval).subscribe(() => this.doHouseKeeping());
                this.baseUrl= this.environmentConfig.dataEndpoint;
            }
    }


    public cancelOpenRequests(): void {
        this.ngUnsubscribe.next();
        this.ngUnsubscribe.complete();
        this.ngUnsubscribe = new Subject();
    }

    public clearData() {
        this.environments.length = 0;
        this.deploymentWindows.length = 0;
        this.deploymentWindows["ALL"] = null;
        this.slaReportJobs.length = 0;
        this.slaReportJobs["ALL"] = null;
        this.alertIgnores["ALL"] = null;
        this.notificationRules["ALL"] = null;
        this.clearHistory();
    }

    public clearHistory() {
        this.history.length = 0;
        this.historyLoaded.length = 0;
        this.extendedHistoryLoaded.length = 0;
    }

    private doHouseKeeping() {
        try {
            console.log("Housekeeping triggered");
            const me = this;

            const endDate: Date = new Date(Date.now());
            const startDate: Date = new Date(endDate.valueOf() - this.settingsService.timerange);


            for (let i = 0; i < me.environments.length; i++) {
                // console.log("cleaning up " + me.environments[i].name);

                const environmentName: string = me.environments[i].name;
                const historyOfEnvironment: any = me.history[environmentName];
                if (historyOfEnvironment) {
                    const historyEntryKeys: string[] = Object.keys(historyOfEnvironment);
                    let environmentWasUpdated = false;

                    for (let historyIndex = 0; historyIndex < historyEntryKeys.length; historyIndex++) {
                        const historyEntry: VanStateTransition[] = historyOfEnvironment[historyEntryKeys[historyIndex]];
                        let firstElementOfNewHistoryIndex = -1;
                        for (let historyEntryIndex = historyEntry.length - 1; historyEntryIndex >= 0; historyEntryIndex--) {
                            const sourceTimestamp = new Date(historyEntry[historyEntryIndex].sourceTimestamp);

                            if (sourceTimestamp < startDate) {
                                // keep the first element that is outside of the time range so that we have an initial state for the timeline
                                firstElementOfNewHistoryIndex = historyEntryIndex;
                                break;
                            }
                        }


                        if (firstElementOfNewHistoryIndex > 0) {
                            console.log("Removing " + firstElementOfNewHistoryIndex + " elements from history of " + environmentName + " / " + historyEntryKeys[historyIndex]);
                            historyEntry.splice(0, firstElementOfNewHistoryIndex);
                            environmentWasUpdated = true;
                        }

                    }

                }
            }

            // send update so that timelines move
            this.environmentDataUpdated.next("housekeeping");
        } catch (e) {
            console.log("Error during history data housekeeping");
            console.log(e);
        }

    }

    public updateEnvironmentWithState(environmentName: string, stateTransitions: VanStateTransition[]) {
        const me = this;
        let environmentWasUpdated = false;

        // update current state
        for (let i = 0; i < stateTransitions.length; i++) {
            const stateTransition = stateTransitions[i];
            const updatedNode: NodeBase = me.updateEnvironmentStateRecursive(this.environments.find(env => env.name === environmentName), stateTransition);
            if (updatedNode) {
                console.log("Updated node " + updatedNode.elementId);
                environmentWasUpdated = true;
            } else {
                console.log("Node could not be found for stateTranstion " + JSON.stringify(stateTransition));
            }

            // update history
            if (me.history && me.history[environmentName]) {
                const historyKey = stateTransition.elementId + (stateTransition.checkId && stateTransition.checkId !== stateTransition.elementId ? "###" + stateTransition.checkId : "")
                    + (stateTransition.alertName ? "###" + stateTransition.alertName : "");
                if (me.history[environmentName][historyKey]) {
                    me.history[environmentName][historyKey].push(stateTransition);
                } else {
                    me.history[environmentName][historyKey] = [stateTransition];
                }

                environmentWasUpdated = true;
            }
        }

        if (environmentWasUpdated) {
            this.environmentDataUpdated.next(environmentName);
        } else {
            console.log("no update necessary");
        }

    }

    public updateHeartBeatForEnvironment(heartbeat: HeartbeatMsg) {
        const vanEnvironment: VanEnvironment = this.environments.find(env => env.name === heartbeat.environmentName);

        if (vanEnvironment) {
            if (!vanEnvironment.lastHeartBeat || new Date(vanEnvironment.lastHeartBeat) < new Date(heartbeat.timeStamp)) {
                vanEnvironment.lastHeartBeat = heartbeat.timeStamp;
                if (heartbeat.logSystemState !== vanEnvironment.logSystemState) {
                    vanEnvironment.logSystemState = heartbeat.logSystemState;
                    this.environmentLogSystemStateChanged.next(heartbeat.environmentName);
                }

            }
        }
    }

    public updateDeploymentWindowsForEnvironment(deploymentWindowMsg: string) {
        if (deploymentWindowMsg) {
            this.getDeploymentWindows(deploymentWindowMsg, true).then(response => {
                if (response) {
                    this.environmentDeploymentWindowsDataChanged.next(deploymentWindowMsg);
                }
            }
            );
        }
    }

    public updateTreeForEnvironment(treeUpdateMsg: string) {
        if (treeUpdateMsg) {
            this.getEnvironment(treeUpdateMsg, true).then(response => {
                if (response) {
                    this.clearHistory();
                    this.environmentsTreeChanged.next(treeUpdateMsg);
                    const vanEnvironment: VanEnvironment = this.environments.find(envItem => envItem.name === response.name);
                    if (!vanEnvironment) {
                        this.environments.push(response);
                        this.dashboardChanged.next(treeUpdateMsg);
                    }
                }
            });
        }
    }

    public deleteTree(treeDeleteMsg: string) {
        if (treeDeleteMsg) {
            this.environments.splice(this.environments.findIndex(item => item.subscriptionId === treeDeleteMsg), 1);
            this.dashboardChanged.next(treeDeleteMsg);           
        }
    }

    private updateEnvironmentStateRecursive(startNode: NodeBase, stateTransition: VanStateTransition): NodeBase {
        let result: NodeBase = null;
        if (startNode) {
            if (startNode.elementId === stateTransition.elementId) {
                if (!stateTransition.checkId || stateTransition.checkId === stateTransition.elementId) {
                    // stateTransition is for node with element id (either node or static check leaf)
                    startNode.state = stateTransition;
                    return startNode;
                } else {
                    // stateTransition is for subnode of current node (dynamic / metric check)
                    // todo: smells buggy here. are check updates really working?
                }

            }

            if (startNode.getChildNodes) {
                const childNodes = startNode.getChildNodes();
                if (childNodes) {
                    for (let childIndex = 0; childIndex < childNodes.length; childIndex++) {
                        result = this.updateEnvironmentStateRecursive(childNodes[childIndex], stateTransition);
                        if (result) {
                            break;
                        }
                    }
                }
            }

            if (startNode.checks) {
                for (let checksIndex = 0; checksIndex < startNode.checks.length; checksIndex++) {
                    result = this.updateEnvironmentStateRecursive(startNode.checks[checksIndex], stateTransition);
                    if (result) {
                        break;
                    }
                }
            }
        }

        return result;
    }

    public getStateTransitionById(id: number): Promise<VanStateTransition> {
        const me = this;
        const url = me.baseUrl + "/statetransitions/" + id;

        return me.http.get(url, {
            headers: me.headers,
            observe: 'response'
        }).pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(
                response => {
                    if (response) {
                        return response.body as VanStateTransition;
                    } else {
                        return null;
                    }

                }, (error: HttpErrorResponse) => {
                    return Promise.reject(error);
                })
            .catch(me.handleError.bind(me));
    }

    public getAlertCommentsPerRecordId(recordId: number): Promise<AlertComment[]> {
        const me = this;
        return me.http.get(me.baseUrl + "/alertcomments/" + recordId, {
            headers: me.headers,
            observe: 'response'
        }).pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(
                response => {
                    if (response) {
                        return response.body;
                    } else {
                        return null;
                    }
                }, (error: HttpErrorResponse) => {
                    return Promise.reject(error);
                })
            .catch(me.handleError.bind(me));

    }

    public createAlertComment(newAlertComment: AlertComment): Promise<Object> {
        const me = this;
        return me.httpClient.post(me.baseUrl + "/alertcomments/", newAlertComment)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public updateAlertComment(updateAlertComment: AlertComment, id: number): Promise<Object> {
        const me = this;
        return me.httpClient.put(me.baseUrl + "/alertcomments/" + id, updateAlertComment)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public deleteAlertComment(id: number): Promise<Object> {
        const me = this;
        return me.httpClient.delete(me.baseUrl + "/alertcomments/" + id)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public getHistoryOfElementId(forceRefresh: boolean, environmentName: string, elementId?: string, extendedModel: boolean = false): Promise<object[]> {
        const me = this;

        const endDate = new Date(Date.now() + parseInt("" + (me.settingsService.timerange * 0.01), 10));
        const startDate = new Date(endDate.valueOf() - me.settingsService.timerange);

        let queryParams = new HttpParams();
        queryParams = queryParams.append('startDate', '' + startDate.toISOString());
        queryParams = queryParams.append('endDate', '' + endDate.toISOString());
        queryParams = queryParams.append('includeChecks', 'true'); // hotfix

        if (elementId && elementId.length > 0) {

            queryParams = queryParams.append('elementId', '' + elementId);
        }
       
        const encodedEnvironmentName = environmentName;
        const url = me.baseUrl + "/statetransitions/history/" + encodedEnvironmentName;

        let reloadRequired;

        const environmentHistoryLoaded = me.historyLoaded.includes(environmentName);
        const environmentExtendedHistoryLoaded = me.extendedHistoryLoaded.includes(environmentName);

        if (elementId) {
            const elementHistoryLoaded = me.historyLoaded.includes(elementId);
            const elementExtendedHistoryLoaded = me.extendedHistoryLoaded.includes(elementId);

            reloadRequired = forceRefresh || ((!extendedModel && !environmentHistoryLoaded && !elementHistoryLoaded)
                || (!environmentExtendedHistoryLoaded && !elementExtendedHistoryLoaded));
        } else {
            reloadRequired = forceRefresh || ((!extendedModel && !environmentHistoryLoaded) && !environmentExtendedHistoryLoaded);
        }
        // console.log("history loading required: " + (reloadRequired ? "TRUE" : "FALSE"));
        if (reloadRequired) {            
            let dialogRef = null;
             setTimeout(
                 () => {
                    if (!this.dialog.openDialogs || !this.dialog.openDialogs.length) {
                        dialogRef = this.dialog.open(CancelRequestDialogComponent, {
                            width: '300px',
                            disableClose: true,
                            data: { title: "Loading State History", dataService: this }
                          });
                    }
             }
             );

            return me.http.get(url, {
                headers: me.headers,
                params: queryParams,
                observe: 'response'
            }).pipe(timeout(me.defaultTimeout))
            .pipe(takeUntil(this.ngUnsubscribe))
            .toPromise()
            .then(

                response => {
                    if (dialogRef) {
                        dialogRef.close("loaded");
                    }
                    if (response) {
                        if (!elementId) {
                            // history data for the whole environment
                            me.history[environmentName] = response.body;

                            if (extendedModel) {
                                me.extendedHistoryLoaded.push(environmentName);
                            } else {
                                me.historyLoaded.push(environmentName);
                            }
                        } else {
                            // history data for a subbranch of the environment
                            if (!me.history.hasOwnProperty(environmentName)) {
                                me.history[environmentName][elementId] = {};
                            }
                            if (response && response.body.length > 0) {
                                me.history[environmentName][elementId] = Object.assign(me.history[environmentName][elementId], response.body);
                            } else {
                                me.history[environmentName][elementId] = [];
                            }
                            me.history[environmentName][elementId] = me.history[environmentName][elementId].sort(function(a, b) {
                                a = new Date(a.sourceTimestamp);
                                b = new Date(b.sourceTimestamp);
                                return a < b ? -1 : a < b ? 1 : 0; });

                            if (extendedModel) {
                                me.extendedHistoryLoaded.push(elementId);
                            } else {
                                me.historyLoaded.push(elementId);
                            }
                            if (dialogRef) {
                                dialogRef.close("loaded");
                            }
                            return me.history[environmentName][elementId];
                        }
                        return me.history[environmentName];
                    } else {
                        if (dialogRef) {
                            dialogRef.close("loaded");
                        };
                        return Promise.resolve(null);
                    }

                }, (error: HttpErrorResponse) => {
                    if (dialogRef) {
                        dialogRef.close("error");
                    }
                    return Promise.reject(error);
                })
            .catch(me.handleError.bind(me));
        } else {            
            return elementId ? Promise.resolve(me.history[environmentName][elementId]) : Promise.resolve(me.history[environmentName]);
        }
    }

    public getAllEnvironments(forceRefresh: boolean): Promise<VanEnvironment[]> {

        if (this.environments.length === 0 || forceRefresh) {

            const me = this;

            let showDemoParameter = "";
            if (me.settingsService.showDemo) {
                showDemoParameter = "?showdemo=true";
            }

            return this.http.get(this.baseUrl + "/environments" + showDemoParameter, {
                headers: this.headers,
                observe: 'response'
            })
            .pipe(timeout(this.defaultTimeout))
            .pipe(takeUntil(this.ngUnsubscribe))
            .toPromise()
            .then(

                response => {
                    if (response) {
                        if (this.environments.length > 0) {
                            this.environments.length = 0;
                        }
                        for (let i = 0; i < response.body.length; i++) {
                            const environmentJson = response.body[i];
                            if (environmentJson) {
                                const environmentResponse = new VanEnvironment(this.buildEnvironmentsWithChildren(environmentJson));
                                me.environments.push(environmentResponse);
                            }

                        }
                        // console.log(`${JSON.stringify(me.environments)}`);
                        return me.environments;
                    }
                }, (error: HttpErrorResponse) => {
                    me.environments.length = 0;
                    return Promise.reject(error);
                })
            .catch(this.handleError.bind(this));
        } else {
            return Promise.resolve(this.environments);
        }
    }

    public getEnvironment(environmentName: string, forceRefresh: boolean): Promise<VanEnvironment> {
        const me = this;
        let result;

        if (this.environments) {
            if (forceRefresh) {                
                const url = this.baseUrl + '/environments/' + environmentName;
                return this.http.get(url, {
                    headers: this.headers,
                    observe: 'response'
                }).pipe(timeout(this.defaultTimeout))
                .toPromise()
                .then( response => {
                    const newEnv = new VanEnvironment(this.buildEnvironmentsWithChildren(response.body));

                    for (let i = 0; i < this.environments.length; i++) {
                        if (this.environments[i].name === environmentName) {
                            this.environments[i] = newEnv;
                        }
                    }
                
                    return newEnv;
                })
                .catch(this.handleError.bind(this));

            } else {
                result = this.environments.find(function (vanEnvironment: VanEnvironment) {
                    return vanEnvironment.name === environmentName;
                });

                return Promise.resolve(result);
            }
        }

        return Promise.reject("Environment not found.");
    }

    public buildEnvironmentsWithChildren(environmentJson: VanEnvironment): VanEnvironment {
        const environmentResponse = new VanEnvironment(environmentJson);
        environmentResponse.services = [];

        if (environmentJson.services) {
            for (let j = 0; j < environmentJson.services.length; j++) {
                const serviceJson = environmentJson.services[j];
                const service = new VanService(serviceJson);
                service.actions = [];

                if (serviceJson.actions) {
                    for (let k = 0; k < serviceJson.actions.length; k++) {
                        const actionJson = serviceJson.actions[k];
                        const action = new VanAction(actionJson);
                        action.components = [];

                        if (actionJson.components) {
                            for (let m = 0; m < actionJson.components.length; m++) {
                                const componentJson = actionJson.components[m];
                                const component = new VanComponent(componentJson);
                                action.components.push(component);

                                component.checks = [];
                                if (componentJson.checks) {
                                    for (let p = 0; p < componentJson.checks.length; p++) {
                                        const checksJson = componentJson.checks[p];
                                        const checks = new VanChecks(checksJson);
                                        component.checks.push(checks);
                                    }
                                }
                            }

                        }
                        action.checks = [];
                        if (actionJson.checks) {
                            for (let o = 0; o < actionJson.checks.length; o++) {
                                const checksJson = actionJson.checks[o];
                                const checks = new VanChecks(checksJson);
                                action.checks.push(checks);
                            }
                        }

                        service.actions.push(action);
                    }
                    //   me.environments.push(environment);
                }
                service.checks = [];
                if (serviceJson.checks) {
                    for (let n = 0; n < serviceJson.checks.length; n++) {
                        const checksJson = serviceJson.checks[n];
                        const checks = new VanChecks(checksJson);
                        service.checks.push(checks);
                    }
                }

                environmentResponse.services.push(service);
            }
        }
        environmentResponse.checks = [];
        if (environmentJson.checks) {
            for (let l = 0; l < environmentJson.checks.length; l++) {
                const checksJson = environmentJson.checks[l];
                const checks = new VanChecks(checksJson);
                environmentResponse.checks.push(checks);
            }
        }

        return environmentResponse;

    }

    public getElementsPerEnvironment(environmentName: string, filter: {name: any} = {name: ''}, page = 1): Observable<IElementSearchResult> {
        const me = this;
        let services;
        let actions;
        let components;
        let currentEnvironment;
        const elements: IElementSearchResult = {results: []};

        filter = filter.name.name ? filter.name.name.toLowerCase() : filter.name.toLowerCase();

        for (const env of this.environments) {
            if (env.name === environmentName) {
                currentEnvironment = env;
                services = currentEnvironment.getChildNodes();
            }
        }

        for (const service of services) {
            const serviceElement = {name: service.name, elementId: service.elementId, path: environmentName.replace(/\//g, "%2F")};
            if (service.name.toLowerCase().includes(filter) || service.elementId.toLowerCase().includes(filter)) {
                elements.results.push(serviceElement);
            }
            actions = service.getChildNodes();

            for (const action of actions) {
                const actionElement = {name: action.name, elementId: action.elementId, path: environmentName.replace(/\//g, "%2F") + "/" + serviceElement.name.replace(/\//g, "%2F")};
                if (action.name.toLowerCase().includes(filter) || action.elementId.toLowerCase().includes(filter)) {
                    elements.results.push(actionElement);
                }
                components = action.getChildNodes();

                for (const component of components) {
                    const componentElement = {name: component.name, elementId: component.elementId, path: environmentName.replace(/\//g, "%2F") + "/" + serviceElement.name.replace(/\//g, "%2F") + '/' + actionElement.name.replace(/\//g, "%2F")};
                    if (component.name.toLowerCase().includes(filter) || component.elementId.toLowerCase().includes(filter)) {
                        elements.results.push(componentElement);
                    }
                }
            }
        }

        return of(elements);
    }


    public getDeploymentWindows(environmentName: string, forceRefresh?: boolean): Promise<DeploymentWindow[]> {
        const me = this;

        const endDate = new Date(Date.now() + parseInt("" + (me.settingsService.timerange * 0.01), 10));
        const startDate = new Date(endDate.valueOf() - me.settingsService.timerange);

        let queryParams = new HttpParams();
        queryParams = queryParams.append('startDate', '' + startDate.toISOString());
        queryParams = queryParams.append('endDate', '' + endDate.toISOString());

        if (!me.deploymentWindows[environmentName] || forceRefresh) {
            return me.http.get(me.baseUrl + "/deployments/history/" + environmentName, {
                headers: me.headers,
                params: queryParams,
                observe: 'response'
            }).pipe(timeout(me.defaultTimeout))
                .toPromise()
                .then(
                    response => {
                        if (response) {
                            me.deploymentWindows[environmentName] = response.body;
                            return response.body;
                        } else {
                            return null;
                        }

                    }, (error: HttpErrorResponse) => {
                        return Promise.reject(error);
                    })
                .catch(me.handleError.bind(me));
        } else {
            return Promise.resolve(me.deploymentWindows[environmentName]);
        }
    }

    public getAllDeploymentWindows(forceRefresh?: boolean): Promise<DeploymentWindow[]> {
        const me = this;

        if (!me.deploymentWindows["ALL"] || forceRefresh) {
            return me.http.get(me.baseUrl + "/deployments/", {
                headers: me.headers,
                observe: 'response'
            }).pipe(timeout(me.defaultTimeout))
                .toPromise()
                .then(
                    response => {
                        if (response) {
                            me.deploymentWindows["ALL"] = response.body;
                            return response.body;
                        } else {
                            return null;
                        }
                    }, (error: HttpErrorResponse) => {
                        return Promise.reject(error);
                    })
                .catch(me.handleError.bind(me));
        } else {
            return Promise.resolve(me.deploymentWindows["ALL"]);
        }
    }

    public createDeploymentWindow(newDeploymentWindow: DeploymentWindow): Promise<Object> {
        const me = this;
        return me.httpClient.post(me.baseUrl + "/deployments/", newDeploymentWindow)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public updateDeploymentWindow(updateDeploymentWindow: DeploymentWindow, deploymentId: string): Promise<Object> {
        const me = this;
        const updateUrl = me.baseUrl + "/deployments/" + updateDeploymentWindow.environmentSubscriptionId + '/' + deploymentId;

        return me.httpClient.put(updateUrl, updateDeploymentWindow)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public deleteDeploymentWindow(deploymentId: string, subscriptionId: string): Promise<Object> {
        const me = this;
        const updateUrl = me.baseUrl + "/deployments/" + subscriptionId + '/' + deploymentId;
        return me.httpClient.delete(updateUrl)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public getAllAlertIgnores(forceRefresh?: boolean): Promise<AlertIgnore[]> {
        const me = this;

        if (!me.alertIgnores["ALL"] || forceRefresh) {
            return me.http.get(me.baseUrl + "/alertIgnores/", {
                headers: me.headers,
                observe: 'response'
            }).pipe(timeout(me.defaultTimeout))
                .toPromise()
                .then(
                    response => {
                        if (response) {
                            me.alertIgnores["ALL"] = response.body;
                            return response.body;
                        } else {
                            return null;
                        }
                    }, (error: HttpErrorResponse) => {
                        return Promise.reject(error);
                    })
                .catch(me.handleError.bind(me));
        } else {
            return Promise.resolve(me.alertIgnores["ALL"]);
        }
    }

    public createAlertIgnore(newAlert: AlertIgnore): Promise<Object> {
        const me = this;
        return me.httpClient.post(me.baseUrl + "/alertIgnores/", newAlert)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public updateAlertIgnore(updateAlertIgnore: AlertIgnore, id: string): Promise<Object> {
        const me = this;
        const updateUrl = me.baseUrl + "/alertIgnores/" + id;

        return me.httpClient.put(updateUrl, updateAlertIgnore)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public deleteAlertIgnore(id: string): Promise<Object> {
        const me = this;

        const queryParams = new HttpParams();

        const updateUrl = me.baseUrl + "/alertIgnores/" + id;
        return me.httpClient.delete(updateUrl, {
            headers: me.headers,
            params: queryParams,
            observe: 'response'
        })
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

        public getChangelogs(startDate?: string, endDate?: string): Promise<Changelog[]> {
            const me = this;

            let queryParams = new HttpParams();
            queryParams = queryParams.append('startDate', startDate);
            queryParams = queryParams.append('endDate', endDate);


            return me.http.get(me.baseUrl + "/changelogs", {
                headers: me.headers,
                params: queryParams,
                observe: 'response'
            })
            .pipe(timeout(me.defaultTimeout))
            .pipe(takeUntil(this.ngUnsubscribe))
            .toPromise()
            .then(
                response => {
                    if (response) {
                        me.changelogs["ALL"] = response.body;
                        return response.body;
                    } else {
                        return null;
                    }
                }, (error: HttpErrorResponse) => {
                    return Promise.reject(error);
                })
            .catch(me.handleError.bind(me));
        }


    public getAllNotificationRules(forceRefresh?: boolean): Promise<NotificationRule[]> {
        const me = this;

        if (!me.notificationRules["ALL"] || forceRefresh) {
            return me.http.get(me.baseUrl + "/notificationRules", {
                headers: me.headers,
                observe: 'response'
            }).pipe(timeout(me.defaultTimeout))
                .toPromise()
                .then(
                    response => {
                        if (response) {
                            me.notificationRules["ALL"] = response.body;
                            return response.body;
                        } else {
                            return null;
                        }
                    }, (error: HttpErrorResponse) => {
                        return Promise.reject(error);
                    })
                .catch(me.handleError.bind(me));
        } else {
            return Promise.resolve(me.notificationRules["ALL"]);
        }
    }

    public getNotificationRule(id: string): Promise<Object> {
        const me = this;

        return me.httpClient.get(me.baseUrl + 'notificationRules/' + id, {
            headers: me.headers,
            observe: 'response'
        }).pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(
                response => {
                    return response;
                }, (error: HttpErrorResponse) => {
                    return Promise.reject(error);
                }
            ).catch(me.handleError.bind(me));
    }

    public createNotificationRule(newRule: NotificationRule): Promise<Object> {
        const me = this;
        return me.httpClient.post(me.baseUrl + "/notificationRules", newRule)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public updateNotificationRule(updateNotificationRule: NotificationRule, id: string): Promise<Object> {
        const me = this;
        const updateUrl = me.baseUrl + "/notificationRules/" + id;

        return me.httpClient.put(updateUrl, updateNotificationRule)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public deleteNotificationRule(id: string): Promise<Object> {
        const me = this;
        const updateUrl = me.baseUrl + "/notificationRules/" + id;
        return me.httpClient.delete(updateUrl)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }
   
    public getAllSLAReportJobs(forceRefresh?: boolean): Promise<SLAReportJob[]> {
        const me = this;
        if (!me.slaReportJobs["ALL"] || forceRefresh) {
            return me.http.get(me.baseUrl + "/job", {
                headers: me.headers,
                observe: 'response'
            }).pipe(timeout(me.defaultTimeout))
                .toPromise()
                .then(
                    response => {
                        if (response) {
                            me.slaReportJobs["ALL"] = response.body;
                            return response.body;
                        } else {
                            return null;
                        }
                    }, (error: HttpErrorResponse) => {
                        return Promise.reject(error);
                    })
                .catch(me.handleError.bind(me));
        } else {
            return Promise.resolve(me.slaReportJobs["ALL"]);
        }
    }

   public createSLAReportJob(newJob: SLAReportJob): Promise<Object> {
        const me = this;
        return me.httpClient.post(me.baseUrl + "/job", newJob)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }
    
    public deleteSLAReportJob(id: string): Promise<Object> {
        const me = this;
        const updateUrl = me.baseUrl + "/job/" + id;
        return me.httpClient.delete(updateUrl)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public getSLAReportJobData(id: number, elementId: string, type: boolean): Promise<SLAReport[]> {
        const me = this;

        let queryParams = new HttpParams();
        if(elementId !== null){
            queryParams = queryParams.append('elementId', elementId);
        }
        queryParams = queryParams.append('slaHistory', String(type));

        return me.http.get(me.baseUrl + "/job/" + id, {
            headers: me.headers,
            params: queryParams,
            observe: 'response'
        }).pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(
                response => {
                    if (response) {
                        return response.body;
                    } else {
                        return null;
                    }
                }, (error: HttpErrorResponse) => {
                    return Promise.reject(error);
                })
            .catch(me.handleError.bind(me));
    }

    public updateSLAReportsJobs(job: SLAReportJob) {
        if (job) {
            this.slaJobDataUpdated.next(job);
        } else {
            console.log("no sla job update necessary");
        }
    }

    public getSLAConfig(environmentSubscriptionId: string): Promise<Object> {
        const me = this;
        const getUrl = '/configurations/' + environmentSubscriptionId;

        return me.httpClient.get(me.baseUrl + getUrl, {
            headers: me.headers,
            observe: 'response'
        }).pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(
                response => {
                    return response.body;
                }, (error: HttpErrorResponse) => {
                    return Promise.reject(error.message || error);
                }
            ).catch(me.handleError.bind(me));
    }

    public createSLAConfig(newSLAConfig: SLAConfig): Promise<Object> {
        const me = this;
        return me.httpClient.post(me.baseUrl + "/configurations", newSLAConfig)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error.message || error);
            })
            .catch(me.handleError.bind(me));
    }


    public updateSLAConfig(environmentSubscriptionId: string, updatedSLAConfig: SLAConfig, id: string): Promise<Object> {
        const me = this;
        const updateUrl = me.baseUrl + "/configurations/" + environmentSubscriptionId + '/' + id;

        return me.httpClient.put(updateUrl, updatedSLAConfig)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error.message || error);
            })
            .catch(me.handleError.bind(me));
    }


    public deleteSLAConfig(environmentSubscriptionId: string, id: string): Promise<Object> {
        const me = this;
        const deleteUrl = me.baseUrl + "/configurations/" + environmentSubscriptionId + '/' + id;
        return me.httpClient.delete(deleteUrl)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error.message || error);
            })
            .catch(me.handleError.bind(me));
    }


    public getAllStateIncreaseRules(forceRefresh?: boolean): Promise<StateIncreaseRule[]> {
        const me = this;

        if (!me.stateIncreaseRules["ALL"] || forceRefresh) {
            return me.http.get(me.baseUrl + "/stateIncreaseRules", {
                headers: me.headers,
                observe: 'response'
            }).pipe(timeout(me.defaultTimeout))
                .toPromise()
                .then(
                    response => {
                        if (response) {
                            me.stateIncreaseRules["ALL"] = response.body;
                            return response.body;
                        } else {
                            return null;
                        }
                    }, (error: HttpErrorResponse) => {
                        return Promise.reject(error);
                    })
                .catch(me.handleError.bind(me));
        } else {
            return Promise.resolve(me.stateIncreaseRules["ALL"]);
        }
    }

    public getStateIncreaseRule(id: string): Promise<Object> {
        const me = this;

        return me.httpClient.get(me.baseUrl + '/stateIncreaseRules/' + id, {
            headers: me.headers,
            observe: 'response'
        }).pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(
                response => {
                    return response;
                }, (error: HttpErrorResponse) => {
                    return Promise.reject(error);
                }
            ).catch(me.handleError.bind(me));
    }

    public createStateIncreaseRule(newStateIncreaseRule: StateIncreaseRule): Promise<Object> {
        const me = this;
        return me.httpClient.post(me.baseUrl + "/stateIncreaseRules", newStateIncreaseRule)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public updateStateIncreaseRule(updateStateIncreaseRule: StateIncreaseRule, id: string): Promise<Object> {
        const me = this;
        const updateUrl = me.baseUrl + "/stateIncreaseRules/" + id;

        return me.httpClient.put(updateUrl, updateStateIncreaseRule)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    public deleteStateIncreaseRule(id: string): Promise<Object> {
        const me = this;
        const updateUrl = me.baseUrl + "/stateIncreaseRules/" + id;
        return me.httpClient.delete(updateUrl)
            .pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(response => {
                return response;
            }, (error: HttpErrorResponse) => {
                return Promise.reject(error);
            })
            .catch(me.handleError.bind(me));
    }

    protected handleError(error: HttpErrorResponse) {
        if (error instanceof ErrorEvent) {
            return Promise.reject(error);
        } else {
            return Promise.reject(error.error || error);
        }

    }

    public getComponentElementIdsPerEnvironment(environmentSubscriptionId: string): string[] {
        const me = this;
        let services;
        let actions;
        let components;
        let currentEnvironment;
        const componentElementIds: string[] = [];

        for (const env of me.environments) {
            if (env.subscriptionId === environmentSubscriptionId) {
                currentEnvironment = env;
                services = currentEnvironment.getChildNodes();
            }
        }

        if (services && services.length > 0) {
            for (const service of services) {
                actions = service.getChildNodes();    
                for (const action of actions) {                  
                    components = action.getChildNodes();    
                    for (const component of components) {

                        // check if element id is already in the list
                        if (!componentElementIds.includes(component.elementId)) {
                            componentElementIds.push(component.elementId);     
                        }                                            
                    }

                       
                }

            } 
        }
               

        return componentElementIds;
    }


    public getComponentElementIdsMapPerEnvironment(environmentName: string): Map<string, string[]> {
        const me = this;
        let services;
        let actions;
        let components;
        let currentEnvironment;
        
        

        const componentElementIdMap = new Map<string, string[]>(); 

        for (const env of this.environments) {
            if (env.name === environmentName) {
                currentEnvironment = env;
                services = currentEnvironment.getChildNodes();
            }
        }

        if (services && services.length > 0) {
            
            let environmentComponentElementIds: string[] = [];
            
            for (const service of services) {
                actions = service.getChildNodes();    
                
                let serviceComponentElementIds: string[] = [];
                
                for (const action of actions) {                  
                    components = action.getChildNodes();    
                    const componentElementIds: string[] = [];
                    for (const component of components) {
                         componentElementIds.push(component.elementId);
                         // store componentId to this component 
                         componentElementIdMap.set(component.elementId.toUpperCase(), component.elementId);
                         

                    }
                    // store list of compnents to this action
                    componentElementIdMap.set(action.elementId.toUpperCase(), componentElementIds);
                    
                    // add list of components to serviceList
                    serviceComponentElementIds = serviceComponentElementIds.concat(componentElementIds);
                }

                // store componentsList to this service
                componentElementIdMap.set(service.elementId.toUpperCase(), serviceComponentElementIds);
                // add list of components to environmentList
                environmentComponentElementIds = environmentComponentElementIds.concat(serviceComponentElementIds);
            }
            // store componentsList to Environment
            componentElementIdMap.set(currentEnvironment.elementId.toUpperCase(),environmentComponentElementIds );
        }        
        
        // console.log("getComponentElementIdsMapPerEnvironment" + JSON.stringify(componentElementIdMap));
        
        return componentElementIdMap;
    }

    public getAllLicenses(): Promise<LicenseData[]> {
        const me = this;
        return me.http.get(me.baseUrl + "/licenses", {
            headers: me.headers,
            observe: 'response'
        }).pipe(timeout(me.defaultTimeout))
            .toPromise()
            .then(
                response => {
                    if (response) {                      
                        return response.body;
                    } else {
                        return null;
                    }
                }, (error: HttpErrorResponse) => {
                    return Promise.reject(error);
                })
            .catch(me.handleError.bind(me));
    }
    
}

export interface IElementSearchResult {
    results: Element[];
}

export interface ICheckIdSearchResult {
    results: String [];
}

export interface IComponentIdSearchResult {
    results: String [];
}

export class Element {
    constructor(public name: string, public elementId: string, public path: string) {}
}
