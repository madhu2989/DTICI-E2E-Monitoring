import { Injectable } from '@angular/core';
import { EnvironmentService } from '../services/config.service';
import { Environment } from "../../shared/model/environment-config";


// Application specific configuration
@Injectable({
    providedIn: "root",
  })
export class SecretService {
    private environmentConfig:Environment =null;
    constructor(environmentService: EnvironmentService) {
        this.environmentConfig= environmentService.getConfiguration();
    }
    public get adalConfig(): any {
        return {

            tenant: this.environmentConfig.aadTenant,
            clientId: this.environmentConfig.aadClientId,
            redirectUri: window.location.origin + '/',
            postLogoutRedirectUri: window.location.origin + '/',
            instance: this.environmentConfig.instance,
            endpoints: {
                [this.environmentConfig.configEndpoint]: this.environmentConfig.aadClientId,
                [this.environmentConfig.dataEndpoint]: this.environmentConfig.aadClientId
            },
            cacheLocation: 'localStorage'
        };
    }
}