import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, HostListener } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import * as d3 from 'd3';
import { HistoryService } from "../../shared/services/history.service";
import * as visavail from "./d3-timeline/d3-timeline";
import { CheckDetailsComponent } from "../../van-nodes-view/van-node/check-details/check-details.component";
import { MatDialog } from "@angular/material/dialog";
import { VanNodeService } from "../../shared/services/van-node.service";
import { NodeBase } from "../../shared/model/node-base";

@Component({
  selector: 'app-timeline',
  templateUrl: './timeline.component.html',
  styleUrls: ['./timeline.component.scss']
})
export class TimelineComponent implements OnInit, OnDestroy {
  private static categories = {
    "OK": { "color": "#1b5e20" },
    "WARNING": { "color": "#f57f17" },
    "ERROR": { "color": "#b71c1c" }
  };

  @Input() elementId;
  @Input() view;
  @Input() simpleVersion;
  @Input() checkId;
  @Input() alertName;
  @Input() resize;
  @Input() resizeFactor;
  @Input() showTooltip;

  @Output() selectedEvent = new EventEmitter<Event>();

  results: object;
  className: string;
  timeline: any;
  nodeData: NodeBase;
  nodeCheck: NodeBase;
  private dataSet: any;
  private resizeMinTime: Number = -1;

  constructor(
    private route: ActivatedRoute,
    private historyService: HistoryService,
    private vanNodeService: VanNodeService,
    public dialog: MatDialog
  ) { }

  ngOnInit() {
    const me = this;

    const environmentId = me.route.snapshot.params['environmentId'];
    if (environmentId && me.elementId) {

      me.className = me.getClass();
      me.historyService.getHistoryOfElement(false, environmentId, me.elementId, me.checkId, me.alertName).then((result) => {

        me.dataSet = [{
          "categories": TimelineComponent.categories,
          'simpleVersion': me.simpleVersion,
          'data': result,
          'showTooltip': me.showTooltip
        }
        ];

        me.redrawTimeline();

        const d3RootELement = d3.select("." + this.className);

        d3RootELement.selectAll('.tooltip')
          .transition()
          .duration(500)
          .style('opacity', 0);

        d3RootELement.selectAll('rect')
          .on('click', (d, i) => {
            d3.selectAll('.tooltip')
              .transition()
              .duration(500)
              .style('opacity', 0);

            if (d[4]) {
              this.vanNodeService.getNodeByElementId(environmentId, d[4].elementId).then((node) => {
                me.nodeData = node;

                if (me.nodeData.checks.length === 0) {
                  this.vanNodeService.getNodeByElementId(environmentId, d[4].triggeredByCheckId).then((nodeChk) => {
                    me.nodeCheck = nodeChk;
                    if (!me.route.snapshot.params['componentId']) {
                      this.showDetailsDialog(d[4], me.nodeData, me.nodeCheck);
                    }
                  });
                } else {
                  me.nodeCheck = this.nodeData.checks.find(function (nodeChk: NodeBase) {
                    return nodeChk.elementId === d[4].triggeredByCheckId;
                  });

                  if (!me.route.snapshot.params['componentId']) {
                    this.showDetailsDialog(d[4], me.nodeData, me.nodeCheck);
                  }
                }
                me.selectedEvent.emit(d);
              });
            }
          });
      });
    }
  }

  private redrawTimeline(): void {
    if (this.resize === "true") {
      if (this.resizeFactor) {
        const factor = parseInt(this.resizeFactor, 10);
        this.view[0] = (Math.floor((window.innerWidth - 36) / factor) * factor) - 13;
      } else {
        this.view[0] = window.innerWidth - 46 - 50;
      }
    }

    const d3RootELement = d3.select("." + this.className);
    d3RootELement.selectAll("*").remove();
    const chart = visavail.visavailChart().view(this.view); // define width of chart in px

    d3RootELement
      .datum(this.dataSet)
      .call(chart);
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    if (this.resize === "true") {
      this.handleResizeEventDebounced();

    }
  }

  private handleResizeEventDebounced(): void {
    const currentTicks = new Date().getTime();
    this.resizeMinTime = currentTicks + 125;

    console.log("handleResizeEventDebounced");
    setTimeout(() => {
      if (new Date().getTime() >= this.resizeMinTime) {
        this.redrawTimeline();
      }
    }, 125);
  }

  // getClass(): string {
  //   /*const className = this.elementId + (this.checkId && this.checkId !== this.elementId ? "_" + this.checkId : "") + (this.alertName ? "_" + this.alertName : "");
  //   return 'tl-' + className.replace(/[^_a-zA-Z0-9-]* /g, '');
  //   */

  //   return ("tl" + this.elementId + Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15)).replace(/[^_a-zA-Z0-9-]*/g, '');
  // }

  getClass(): string {
    /*const className = this.elementId + (this.checkId && this.checkId !== this.elementId ? "_" + this.checkId : "") + (this.alertName ? "_" + this.alertName : "");
    return 'tl-' + className.replace(/[^_a-zA-Z0-9-]* /g, '');
    */

    return ("tl" + this.elementId + this.cryprtoRandomGenerator() + this.cryprtoRandomGenerator()).replace(/[^_a-zA-Z0-9-]*/g, '');
  }

  cryprtoRandomGenerator(): any {
    const crypto = window.crypto;
    var array = new Uint32Array(1);
    crypto.getRandomValues(array); 
    return array[0].toString(36).substring(0, 15);
    } 

  showDetailsDialog(historyData: any, nodeData: NodeBase, nodeCheck: NodeBase) {
    const dialogRef = this.dialog.open(CheckDetailsComponent, {
      disableClose: false,
      width: '900px',
      maxWidth: '100vw',
      data: { historyData, nodeData, nodeCheck }
    });
  }

  ngOnDestroy() {
    const d3RootELement = d3.select("." + this.className);
    d3RootELement.datum(null);
    d3.selectAll('.tooltip').remove();
    d3RootELement.selectAll('rect').on('click', null);
    d3RootELement.remove();
  }

}
