export class Changelog {

        id: number;
        environmentName: string;
        elementId: number;
        elementType: string;
        changeDate: string;
        userName: string;
        operation: string;
        valueOld: Object;
        valueNew: Object;
        diff: Object;

    public constructor(init?: Partial<Changelog>) {
        Object.assign(this, init);
    }
}