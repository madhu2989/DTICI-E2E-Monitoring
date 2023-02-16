import { Injectable } from '@angular/core';
import { Environment } from '../model/environment-config'

@Injectable({ providedIn: 'root' })

export class EnvironmentService {
  private configObject: Environment;  

  initConfiguration():Promise<any>{
    return fetch('./assets/environment/environment.json', { headers: {
      'Content-Type': 'application/json',
      Accept : 'application/json'
    }});
  }

  printConfig() {
    fetch('./assets/environment/environment.json').then(res => res.json())
    .then(jsonData => {
      console.log("data is " + jsonData);
    });    
  }

  async setConfiguration(config: any)
  {
    var json= await config.json();
    this.configObject = {
      aadTenant : json.aadTenant,
      aadClientId : json.aadClientId,
      configEndpoint: json.configEndpoint,
      dataEndpoint: json.dataEndpoint,
      signalREndpoint: json.signalREndpoint,
      historyDuration: json.historyDuration,
      houseKeepingInterval: json.houseKeepingInterval,
      instrumentationKey: json.instrumentationKey,
      versionBuildNumber: json.versionBuildNumber,
      versionReleaseName: json.versionReleaseName,
      instance: json.instance
    }
  }

   getConfiguration(): any {
    if(this.configObject)
      {
        return this.configObject;
      }
  }
}


