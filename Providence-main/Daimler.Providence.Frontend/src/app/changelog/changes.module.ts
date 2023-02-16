import { NgModule } from '@angular/core';
import { CommonModule, DatePipe, JsonPipe } from '@angular/common';
import { ChangeOverviewComponent } from './change-overview/change-overview.component';
import { MaterialModule } from '../material.module';
import { SharedModule } from '../shared/shared.module';
import { NgxLoadingModule } from 'ngx-loading';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AgGridModule } from 'ag-grid-angular';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

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
  declarations: [ChangeOverviewComponent],
  providers: [DatePipe, JsonPipe]
})
export class ChangesModule { }
