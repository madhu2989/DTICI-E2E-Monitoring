export class StateIncreaseRule {
    name: string;
    id: string;
    description: string;
    environmentSubscriptionId: string;
    checkId: string;
    alertName: string;
    componentId: string;
    triggerTime: number;
    isActive: boolean;

    public constructor(init?: Partial<StateIncreaseRule>) {
        Object.assign(this, init);
    }
}