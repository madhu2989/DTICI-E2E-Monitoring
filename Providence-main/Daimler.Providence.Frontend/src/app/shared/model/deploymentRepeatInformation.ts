export class DeploymentWindowRepeatInformation {
    repeatId: number; // id set from BE
    repeatUntil: string; // enddate (max 1 year)
    repeatInterval: number; // 1,2,3,4 -> everey 4 weeks
    repeatType: number; // daily=0, weekly=1, monthly=2
    repeatOnSameWeekDayCount: boolean; // for special cases like every third Monday in every second month

    public constructor(init?: Partial<DeploymentWindowRepeatInformation>) {
        Object.assign(this, init);
    }
}