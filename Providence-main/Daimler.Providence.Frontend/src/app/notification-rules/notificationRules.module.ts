import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { NotificationOverviewComponent } from './notification-overview/notification-overview.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '../material.module';
import { SharedModule } from '../shared/shared.module';
import { NgxLoadingModule } from 'ngx-loading';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AgGridModule } from 'ag-grid-angular';
import { NotificationEditComponent } from './notification-edit/notification-edit.component';

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
  declarations: [NotificationOverviewComponent, NotificationEditComponent],
  providers: [DatePipe]
})
export class NotificationRulesModule { }
