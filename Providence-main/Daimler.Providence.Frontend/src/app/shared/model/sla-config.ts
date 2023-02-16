export class SLAConfig {
    environmentSubscriptionId: string;
    id: string;
    key: string;
    value: string;

    public constructor(init?: Partial<SLAConfig>) {
        Object.assign(this, init);
    }
}