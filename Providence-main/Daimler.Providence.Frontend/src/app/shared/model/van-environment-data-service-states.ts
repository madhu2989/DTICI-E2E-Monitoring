export class VanEnvironmentDataServiceStates {
    name: string;
    value: number;

    public constructor(init?: Partial<VanEnvironmentDataServiceStates>) {
        Object.assign(this, init);
    }
}