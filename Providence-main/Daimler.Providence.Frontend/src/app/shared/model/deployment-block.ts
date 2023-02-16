import { DeploymentWindow } from './deployment-window';


export class DeploymentBlock {
    environmentName: string;
    environmentSubscriptionId: string;
    id: string;
    startDate: string;
    endDate: string;
    deployments: DeploymentWindow[];
    shortDescription: string;
    

    public constructor(init?: Partial<DeploymentBlock>) {
        Object.assign(this, init);
    }
}