import { VanEnvironmentDataServiceStates } from "./van-environment-data-service-states";

export class VanEnvironmentData {
    environment: string;
    state: string;
    lastHeartBeat: string;
    logSystemState: string;
    serviceStates: VanEnvironmentDataServiceStates[];

    public constructor(init?: Partial<VanEnvironmentData>) {
        Object.assign(this, init);
    }
}