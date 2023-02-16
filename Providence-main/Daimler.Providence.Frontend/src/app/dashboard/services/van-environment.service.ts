import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { VanEnvironmentData } from "../../shared/model/van-environment-data";
import { VanEnvironmentDataServiceStates } from "../../shared/model/van-environment-data-service-states";
import { DataService } from "../../shared/services/data.service";

@Injectable({
    providedIn: "root",
  })
export class VanEnvironmentService {

    router: Router;
    private vanEnvironmentDataList = [];
    public vanEnvironmentData: VanEnvironmentData;

    constructor(protected dataService: DataService,
        private _router: Router) {
        this.router = _router;
    }

    public getVanEnvironmentPieChartData(env: string, isManuallyRefreshed: boolean): Promise<VanEnvironmentData> {

        const me = this;

        return me.dataService.getAllEnvironments(isManuallyRefreshed).then(function (result) {
            if (result) {
                me.vanEnvironmentData = me.processVanEnvironmentPieChartData(result, env);
                return me.vanEnvironmentData;
            }
        });

    }


    private processVanEnvironmentPieChartData(data: any, env: string) {
        let stateOk: number = 0;
        let stateWarn: number = 0;
        let stateError: number = 0;
        const environment = data.find(envmnt => envmnt.name === env);
        const vanEnvironmentData = new VanEnvironmentData();

        if (environment) {
            vanEnvironmentData.environment = environment.name;
            vanEnvironmentData.state = environment.state.state;
            vanEnvironmentData.serviceStates = [];
            if (environment.services) {
                if (environment.services.length > 0) {
                    for (let i = 0; i < environment.services.length; i++) {
                        if (environment.services[i].state.state === "OK") {
                            stateOk++;
                        }
                        if (environment.services[i].state.state === "WARNING") {
                            stateWarn++;
                        }
                        if (environment.services[i].state.state === "ERROR") {
                            stateError++;
                        }
                    }

                    const serviceStatesOk = new VanEnvironmentDataServiceStates();
                    serviceStatesOk.name = "OK";
                    serviceStatesOk.value = stateOk;
                    vanEnvironmentData.serviceStates.push(serviceStatesOk);

                    const serviceStatesWarn = new VanEnvironmentDataServiceStates();
                    serviceStatesWarn.name = "WARNING";
                    serviceStatesWarn.value = stateWarn;
                    vanEnvironmentData.serviceStates.push(serviceStatesWarn);

                    const serviceStatesError = new VanEnvironmentDataServiceStates();
                    serviceStatesError.name = "ERROR";
                    serviceStatesError.value = stateError;
                    vanEnvironmentData.serviceStates.push(serviceStatesError);
                }
            }
        }
        return vanEnvironmentData;
    }

}
