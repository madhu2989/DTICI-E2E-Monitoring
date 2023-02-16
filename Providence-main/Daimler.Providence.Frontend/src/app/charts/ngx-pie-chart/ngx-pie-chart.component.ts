import { ChangeDetectorRef, Component, ElementRef, Input, NgZone, OnChanges, OnInit } from '@angular/core';
import { BaseChartComponent } from '@swimlane/ngx-charts';
import { VanEnvironmentService } from '../../dashboard/services/van-environment.service';
import { VanNodeService } from '../../shared/services/van-node.service';

@Component({
  selector: 'app-ngx-pie-chart',
  templateUrl: './ngx-pie-chart.component.html',
  styleUrls: ['./ngx-pie-chart.component.scss']
})
export class NgxPieChartComponent extends BaseChartComponent implements OnChanges, OnInit {


  protected chartElement: ElementRef;
  protected zone: NgZone;
  protected cd: ChangeDetectorRef;

  legend = false;
  legendFlag = false;
  doughnut = true;
  arcWidth = 0.25;
  labels = false;
  //  view = [200, 200];
  colorScheme: object = {
    domain: ['#1b5e20', '#f57f17', '#b71c1c']
  };
  animations = true;

  @Input() envName;
  @Input() nodeElementId;
  @Input() isNodeChildElementNeeded;

  constructor(chartElement: ElementRef, zone: NgZone, cd: ChangeDetectorRef,
    private vanEnvironmentService: VanEnvironmentService,
    private vanNodeService: VanNodeService
  ) {
    super(chartElement, zone, cd, null);
  }

  ngOnInit() {
    const me = this;
    me.results = [];

    if (me.envName) {

      if (me.nodeElementId) {
        me.view = [65, 65];
        me.legendFlag = false;
        me.arcWidth = 0.4;
        me.animations = false;

        const nodeChildStatesSummaryPromise = me.vanNodeService.getNodeChildStatesSummary(me.nodeElementId, me.envName);
        nodeChildStatesSummaryPromise.then((result) => {
          if (result) {
            if (!me.isNodeChildElementNeeded) {
              me.results = result[0]["series"];
            } else {
              me.results = result[1]["series"];
            }

          }
        });
      } else {
        me.legendFlag = true;
        me.view = [200, 200];
        me.doughnut = true;
        me.animations = true;

        const envServiceStatesPromise = me.vanEnvironmentService.getVanEnvironmentPieChartData(me.envName, false);
        envServiceStatesPromise.then(response => {
          if (response) {
            me.results = response.serviceStates;
          }
        });
      }
    }

  }

  ngOnChanges() {
    this.update();
  }

  update() {
    super.update();
  }


}
