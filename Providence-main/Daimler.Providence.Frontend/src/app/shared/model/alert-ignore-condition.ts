export class AlertIgnoreCondition {
    alertName: string;
    subscriptionId: string;
    componentId: string;
    checkId: string;
    description: string;
    customField1: string;
    customField2: string;
    customField3: string;
    customField4: string;
    customField5: string;
    state: string;

    public constructor(init?: Partial<AlertIgnoreCondition>) {
        Object.assign(this, init);
    }
}