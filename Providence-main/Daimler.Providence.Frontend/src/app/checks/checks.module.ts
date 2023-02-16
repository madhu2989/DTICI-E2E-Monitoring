import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CheckOverviewComponent } from './check-overview/check-overview.component';
import { MaterialModule } from '../material.module';
import { SharedModule } from '../shared/shared.module';
import { NgxLoadingModule } from 'ngx-loading';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AgGridModule } from 'ag-grid-angular';
import { FormsModule } from '@angular/forms';
import { CheckEditComponent } from './check-edit/check-edit.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule,
    SharedModule,
    NgxLoadingModule.forRoot({}),
    FlexLayoutModule,
    AgGridModule.withComponents([])
  ],
  declarations: [CheckOverviewComponent, CheckEditComponent]
})
export class ChecksModule { }
