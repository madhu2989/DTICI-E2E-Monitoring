export class Environment {
    aadTenant: string;
    aadClientId: string;
    configEndpoint: string;
    dataEndpoint: string;
    signalREndpoint: string;
    versionReleaseName: string;
    versionBuildNumber: string;
    instrumentationKey: string;
    houseKeepingInterval: number;
    historyDuration: number;
    instance: string


    public constructor(init?: Partial<Environment>) {
        Object.assign(this, init);
    }
}