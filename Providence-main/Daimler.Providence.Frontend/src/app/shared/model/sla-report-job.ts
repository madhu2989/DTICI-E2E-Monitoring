export class SLAReportJob {
    id: number;
    type: string;
    environmentName: string;
    environmentSubscriptionId: string;
    userName: string;
    state: number;
    stateInformation: string;
    startDate: string;
    endDate: string;
    queueDate: string;
    fileName: string;

    constructor(SLAReportJobJson: any = null) {
        if (SLAReportJobJson !== null) {
            this.id = SLAReportJobJson.id;
            this.type = SLAReportJobJson.type;
            this.environmentName = SLAReportJobJson.environmentName;
            this.environmentSubscriptionId = SLAReportJobJson.environmentSubscriptionId;
            this.userName = SLAReportJobJson.userName;
            this.state = SLAReportJobJson.state;
            this.stateInformation = SLAReportJobJson.stateInformation;
            this.startDate = SLAReportJobJson.startDate;
            this.endDate = SLAReportJobJson.endDate;
            this.queueDate = SLAReportJobJson.queueDate;
            this.fileName = SLAReportJobJson.fileName;
        }
    }
}