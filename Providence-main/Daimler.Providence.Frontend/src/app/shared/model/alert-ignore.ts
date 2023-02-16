import { AlertIgnoreCondition } from "./alert-ignore-condition";

export class AlertIgnore {
    id: string;
    environmentSubscriptionId: string;
    environmentName: string; // read only --> at creation is environmentSubscriptionId important
    name: string;
    creationDate: string;
    expirationDate: string;
    ignoreCondition: AlertIgnoreCondition;

    public constructor(init?: Partial<AlertIgnore>) {
        Object.assign(this, init);
    }
}