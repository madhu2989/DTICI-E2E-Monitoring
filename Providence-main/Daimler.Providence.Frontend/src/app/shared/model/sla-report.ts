export class SLAReport {
    elementId: string;
    value: number;
    level: number;
    elementType: number;
    startDate: string;
    endDate: string;
    errorThreshold: number;
    warningThreshold: number;
    presentationType: string;
    lineChartPoints?: string [];
    lineChartDates?: string[];

    public constructor(init?: Partial<SLAReport>) {
        Object.assign(this, init);
    }
}