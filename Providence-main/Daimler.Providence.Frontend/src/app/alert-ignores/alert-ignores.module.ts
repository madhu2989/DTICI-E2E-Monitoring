import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { AlertIgnoresOverviewComponent } from './alert-ignores-overview/alert-ignores-overview.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '../material.module';
import { SharedModule } from '../shared/shared.module';
import { NgxLoadingModule } from 'ngx-loading';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AgGridModule } from 'ag-grid-angular';
import { AlertIgnoresEditComponent } from './alert-ignores-edit/alert-ignores-edit.component';

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
  declarations: [AlertIgnoresOverviewComponent, AlertIgnoresEditComponent],
  providers: [DatePipe]
})
export class AlertIgnoresModule { }
