import { DeploymentWindowRepeatInformation } from './deploymentRepeatInformation';

export class DeploymentWindow {
    environmentName: string;
    environmentSubscriptionId: string;
    elementIds: string[];
    id: string;
    description: string;
    shortDescription: string;
    closeReason: string;
    startDate: string;
    endDate: string;
    length: number;
    repeatInformation: DeploymentWindowRepeatInformation;

    public constructor(init?: Partial<DeploymentWindow>) {
        Object.assign(this, init);
    }
}