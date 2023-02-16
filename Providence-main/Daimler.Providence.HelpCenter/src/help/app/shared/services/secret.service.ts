import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

// Application specific configuration
@Injectable()
export class SecretService {

    public get adalConfig(): any {
        return {

            tenant: environment.aadTenant,
            clientId: environment.aadClientId,
            redirectUri: window.location.origin + '/',
            postLogoutRedirectUri: window.location.origin + '/',
            endpoints: {
                [environment.configEndpoint]: environment.aadClientId,
                [environment.dataEndpoint]: environment.aadClientId
            },
            cacheLocation: 'localStorage'
        };
    }
}
