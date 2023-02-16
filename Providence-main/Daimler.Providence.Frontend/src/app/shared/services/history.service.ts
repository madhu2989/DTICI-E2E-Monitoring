import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { environment } from '../../../environments/environment';
import { VanStateTransition } from '../model/van-statetransition';
import { VanNodeService } from './van-node.service';
import { NodeBase } from '../model/node-base';
import { SettingsService } from './settings.service';


@Injectable({
  providedIn: "root",
})
export class HistoryService {
  offset = this.settingsService.timerange;

  constructor(protected dataService: DataService,
    protected vanNodeService: VanNodeService,
    public settingsService: SettingsService) { }


  public getHistoryOfElement(forceRefresh: boolean, environmentName: string, elementId: string, checkId: string, alertName: string, startTime?: Date, endTime?: Date): Promise<object[]> {
    const me = this;
    return me.dataService.getHistoryOfElementId(forceRefresh, environmentName).then((result) => {

      return me.vanNodeService.getNodeByElementId(environmentName, elementId).then((node) => {

        return me.processHistoryPerEnvironment(result[elementId], node, startTime, endTime);

      });

    });

  }

  private processHistoryPerEnvironment(stateHistory: VanStateTransition[], node: NodeBase, startTime?: Date, endTime?: Date): object[] {
    const me = this;
    const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    const endDate: Date = endTime ? endTime : new Date(Date.now() + parseInt("" + (me.settingsService.timerange * 0.01), 10));
    const startDate: Date = startTime ? startTime : new Date(endDate.valueOf() - me.offset);
    const chartResult: object[] = [];
    if (stateHistory && stateHistory.length > 1) {
      for (let i = 0; i < stateHistory.length; i++) {
        const eventStartDate = new Date(stateHistory[i]['sourceTimestamp']);
        let eventEndDate: Date;
        if (i + 1 < stateHistory.length) {
          eventEndDate = new Date(stateHistory[i + 1]['sourceTimestamp']);
        } else {
          eventEndDate = new Date(endDate);
        }

        if (i === 0) {
          chartResult.push(
            [
              startDate.valueOf(),
              'OK',
              eventStartDate.valueOf(),
              'no data points'
            ]
          );
        }

        chartResult.push(
          [
            eventStartDate.valueOf(),
            stateHistory[i]['state'],
            eventEndDate.valueOf(),
            // tslint:disable-next-line:max-line-length
            'Check \x27' + stateHistory[i].triggeredByCheckId + '\x27 was triggered for component \x27' + (stateHistory[i].triggerName ? stateHistory[i].triggerName : stateHistory[i].triggeredByElementId) + '\x27 ',
            stateHistory[i]
          ]
        );
      }
    } else if (stateHistory && stateHistory.length === 1) {
      const eventStartDate = new Date(stateHistory[0]['sourceTimestamp']);

      chartResult.push(
        [
          startDate.valueOf(),
          'OK',
          eventStartDate.valueOf(),
          'no data points'
        ]
      );

      chartResult.push(
        [
          eventStartDate.valueOf(),
          stateHistory[0]['state'],
          endDate.valueOf(),
          // tslint:disable-next-line:max-line-length
          'Check \x27' + stateHistory[0].triggeredByCheckId + '\x27 was triggered for component \x27' + (stateHistory[0].triggerName ? stateHistory[0].triggerName : stateHistory[0].triggeredByElementId) + '\x27 ',
          stateHistory[0]
        ]
      );
    } else {
      chartResult.push(
        [
          startDate.valueOf(),
          'OK',
          endDate.valueOf(),
          'no data points'
        ]
      );
    }

    return chartResult;
  }

}