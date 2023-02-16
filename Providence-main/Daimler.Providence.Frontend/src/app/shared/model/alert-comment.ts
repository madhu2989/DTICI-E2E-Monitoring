export class AlertComment {
    id: number;
    timestamp: string;
    comment: string;
    user: string;
    state: string;
    recordId: string;

    public constructor(init?: Partial<AlertComment>) {
        Object.assign(this, init);
    }
}
