import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { DeploymentsOverviewComponent } from './deployments-overview/deployments-overview.component';
import { MaterialModule } from '../material.module';
import { SharedModule } from '../shared/shared.module';
import { NgxLoadingModule } from 'ngx-loading';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AgGridModule } from 'ag-grid-angular';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DeploymentsEditComponent } from './deployments-edit/deployments-edit.component';

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
  declarations: [DeploymentsOverviewComponent, DeploymentsEditComponent],
  providers: [DatePipe]
})
export class DeploymentsModule { }
