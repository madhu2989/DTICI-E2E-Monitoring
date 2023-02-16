import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxPieChartComponent } from './ngx-pie-chart/ngx-pie-chart.component';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { ChartsLegendComponent } from './charts-legend/charts-legend.component';
import { SharedModule } from '../shared/shared.module';
import { TimelineComponent } from './timeline/timeline.component';
import { DepoymentWindowComponent } from './depoyment-window/depoyment-window.component';

@NgModule({
  imports: [
    CommonModule,
    NgxChartsModule,
    SharedModule
  ],
  exports: [
    NgxPieChartComponent,
    TimelineComponent,
    DepoymentWindowComponent
  ],
  declarations: [NgxPieChartComponent, ChartsLegendComponent, TimelineComponent, DepoymentWindowComponent]
})
export class ChartsModule { }
