import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ChartsModule } from "../../charts/charts.module";
import { MaterialModule } from "../../material.module";
import { SharedModule } from "../../shared/shared.module";
import { VanEnvironmentComponent } from './van-environment.component';


@NgModule({
  imports: [
    CommonModule,
    MaterialModule,
    ChartsModule,
    SharedModule,
    RouterModule
  ],
  exports: [
    VanEnvironmentComponent
  ],
  declarations: [
    VanEnvironmentComponent
  ]
})
export class VanEnvironmentModule { }
