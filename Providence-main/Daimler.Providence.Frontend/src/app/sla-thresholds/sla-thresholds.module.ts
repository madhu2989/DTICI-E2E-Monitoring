import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '../material.module';
import { SharedModule } from '../shared/shared.module';
import { NgxLoadingModule } from 'ngx-loading';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AgGridModule } from 'ag-grid-angular';
import { SlaThresholdsComponent } from './sla-thresholds/sla-thresholds.component';
import { SlaReportsComponent } from './sla-reports/sla-reports.component';
import { SlaReportJobsOverviewComponent } from './sla-reports-jobs-overview/sla-reports-jobs-overview.component';
import { SlaReportJobsEditComponent } from './sla-reports-jobs-edit/sla-reports-jobs-edit.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    SharedModule,
    NgxLoadingModule.forRoot({}),
    FlexLayoutModule,
    AgGridModule.withComponents([])
  ],
  declarations: [SlaThresholdsComponent, SlaReportsComponent, SlaReportJobsOverviewComponent, SlaReportJobsEditComponent],
  providers: [DatePipe]
})
export class SlaThresholdsModule { }
