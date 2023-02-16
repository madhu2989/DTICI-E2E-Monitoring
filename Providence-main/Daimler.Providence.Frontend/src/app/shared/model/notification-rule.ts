export class NotificationRule {
    environmentSubscriptionId: string;
    environmentName: string;
    id: string;
    levels: string[];
    emailAddresses: string;
    states: string[];
    isActive: boolean;
    notificationInterval: number;

    public constructor(init?: Partial<NotificationRule>) {
        Object.assign(this, init);
    }
}