import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { StateIncreaseRuleOverviewComponent } from './state-increase-rule-overview/state-increase-rule-overview.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '../material.module';
import { SharedModule } from '../shared/shared.module';
import { NgxLoadingModule } from 'ngx-loading';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AgGridModule } from 'ag-grid-angular';
import { StateIncreaseRuleEditComponent } from './state-increase-rule-edit/state-increase-rule-edit.component';

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
  declarations: [StateIncreaseRuleOverviewComponent, StateIncreaseRuleEditComponent]
})
export class StateIncreaseRulesModule { }
