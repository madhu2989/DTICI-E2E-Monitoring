import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { VanNodeComponent } from "./van-node.component";
import { FlexLayoutModule } from '@angular/flex-layout';
import { RouterModule } from "@angular/router";
import { CheckDetailsComponent } from './check-details/check-details.component';
import { MaterialModule } from "../../material.module";
import { SharedModule } from "../../shared/shared.module";
import { ChartsModule } from "../../charts/charts.module";
import { NgxLoadingModule } from "ngx-loading";
import { AgGridModule } from 'ag-grid-angular';
import { AddCommentComponent } from './check-details/add-comment/add-comment.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MaterialModule,
        SharedModule,
        ChartsModule,
        NgxLoadingModule.forRoot({}),
        FlexLayoutModule,
        RouterModule,
        AgGridModule.withComponents([])
    ],
    exports: [
        VanNodeComponent
    ],
    declarations: [
        VanNodeComponent,
        CheckDetailsComponent,
        AddCommentComponent
    ]
})
export class VanNodeModule { }