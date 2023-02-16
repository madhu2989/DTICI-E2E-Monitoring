export class VanStateTransition {
    id: number;
    sourceTimestamp: string;
    elementId: string;
    alertName: string;
    checkId: string;
    description: string;
    customField1: string;
    customField2: string;
    customField3: string;
    customField4: string;
    customField5: string;
    state: string;
    triggeredByCheckId: string;
    triggeredByElementId: string;
    triggeredByAlertName: string;
    environmentName: string;
    triggerName: string;
    recordId: string;
    progressState: string;

    public constructor(init?: Partial<VanStateTransition>) {
        Object.assign(this, init);
    }
}
