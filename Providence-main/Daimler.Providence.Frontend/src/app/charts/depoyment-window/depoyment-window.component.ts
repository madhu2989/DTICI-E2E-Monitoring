import { Component, OnInit, EventEmitter, Output, Input, HostListener, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { DeploymentWindowService } from '../../shared/services/deployment-window.service';
import { ActivatedRoute } from '@angular/router';
import * as d3 from 'd3';
import * as visavail from "../timeline/d3-timeline/d3-timeline";
import { SettingsService } from '../../shared/services/settings.service';
import { DeploymentWindowComponent } from '../../shared/dialogs/deployment-window-dialog/deployment-window-dialog.component';

@Component({
  selector: 'app-depoyment-window',
  templateUrl: './depoyment-window.component.html',
  styleUrls: ['./depoyment-window.component.scss']
})
export class DepoymentWindowComponent implements OnInit, OnDestroy {
  private static categories = {
    "Deploying": { "color": "deepskyblue" }
  };

  @Input() resize;
  @Input() resizeFactor;
  @Input() view;
  @Input() simpleVersion;
  @Input() viewSizeForElements;
  @Input() showTooltip;
  @Input() elementId;
  

  @Output() selectedEvent = new EventEmitter<Event>();

  results: object;
  className: string;
  deploymentData: any;
  private dataSet: any;
  private resizeMinTime: Number = -1;
  private startDate: Date;
  private endDate: Date;
  



  offset = this.settingsService.timerange;

  constructor(
    private route: ActivatedRoute,
    private deploymentWindowService: DeploymentWindowService,
    private settingsService: SettingsService,
    public dialog: MatDialog
  ) { }

  ngOnInit() {
    const me = this;
    me.endDate = new Date(Date.now() + parseInt("" + (me.settingsService.timerange * 0.01), 10)); // general boundarys for timeline
    me.startDate = new Date(me.endDate.valueOf() - me.offset);                                    // general boundarys for timeline

    const environmentId = me.route.snapshot.params['environmentId'];
    
    if (environmentId) {

      me.className = me.getClass();
      // me.historyService.getHistoryOfElement(false, environmentId, me.elementId, me.checkId, me.alertName).then((result) => {
      me.deploymentWindowService.getDeploymentWindowsData(environmentId, this.elementId).then((result) => {
        if (result && result.length > 0) {
          
        

          me.dataSet = [{
            "categories": DepoymentWindowComponent.categories,
            'simpleVersion': me.simpleVersion,
            'data': result,
            'showTooltip': me.showTooltip
          }
          ];

          me.redrawDepoymentWindow();

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
                  this.deploymentWindowService.getDeploymentWindowsData(environmentId, this.elementId).then((node) => {
                    
                    me.deploymentData = node;
                    this.showDeploymentDialog(d[4], me.deploymentData);

                    me.selectedEvent.emit(d);

                  });
                }
            });
        }
      });
    }
  }

  private redrawDepoymentWindow(): void {
    if (this.resize === "true") {
      if (this.resizeFactor) {
        const factor = parseInt(this.resizeFactor, 10);
        this.view[0] = (Math.floor((window.innerWidth - 36) / factor) * factor) - 13;
      } else {
        this.view[0] = window.innerWidth - 46 - 50;
      }
    }

    if (this.dataSet && this.dataSet.length > 0) {
      
      const d3RootELement = d3.select("." + this.className);
      d3RootELement.selectAll("*").remove();
      const chart = visavail.visavailChart().view(this.view).displayDateRange([
        this.startDate,
        this.endDate
      ]); // define width of chart in px

      d3RootELement
        .datum(this.dataSet)
        .call(chart);
    }

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
        this.redrawDepoymentWindow();
      }
    }, 125);
  }

  // getClass(): string {
  //   return ("dw" + Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15)).replace(/[^_a-zA-Z0-9-]*/g, '');
  // }
  getClass(): string {
    return ("dw" + this.cryprtoRandomGenerator() + this.cryprtoRandomGenerator()).replace(/[^_a-zA-Z0-9-]*/g, '');
  }

  cryprtoRandomGenerator(): any {
    const crypto = window.crypto;
    var array = new Uint32Array(1);
    crypto.getRandomValues(array); 
    return array[0].toString(36).substring(0, 15);
  }  

  // TODO find better name for historyData is it the legend?
  showDeploymentDialog(historyData: any, deploymentData: any) {
      const dialogRef = this.dialog.open(DeploymentWindowComponent, {
        disableClose: false,
        width: '1000px',
        data: { historyData, deploymentData }
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
