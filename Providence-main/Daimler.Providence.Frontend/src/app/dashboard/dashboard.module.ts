import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardComponent } from './dashboard.component';
import { MaterialModule } from '../material.module';
import { SharedModule } from '../shared/shared.module';
import { RouterModule } from '@angular/router';
import { VanEnvironmentModule } from './van-environment/van-environment.module';
import { VanEnvironmentService } from './services/van-environment.service';
import { NgxLoadingModule } from 'ngx-loading';

@NgModule({
  imports: [
    CommonModule,
    MaterialModule,
    SharedModule,
    VanEnvironmentModule,
    NgxLoadingModule.forRoot({}),
    RouterModule
  ],
  declarations: [DashboardComponent],
  providers: [
    VanEnvironmentService
  ],
})
export class DashboardModule { }
