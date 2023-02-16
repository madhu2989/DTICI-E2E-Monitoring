import { VanNodesViewComponent } from "./van-nodes-view.component";
import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MaterialModule } from "../material.module";
import { ChartsModule } from "../charts/charts.module";
import { SharedModule } from "../shared/shared.module";
import { FlexLayoutModule } from "@angular/flex-layout";
import { VanNodeModule } from "./van-node/van-node.module";
import { VanCheckDetailGridViewComponent } from './van-check-detail-grid-view/van-check-detail-grid-view.component';
import { AgGridModule } from "ag-grid-angular";
import { NgxLoadingModule } from "ngx-loading";
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
    imports: [
        CommonModule,
        MaterialModule,
        ChartsModule,
        SharedModule,
        NgxLoadingModule.forRoot({}),
        VanNodeModule,
        FlexLayoutModule,
        AgGridModule.withComponents([]),
        FormsModule,
        ReactiveFormsModule
    ],
    exports: [
        VanNodesViewComponent
    ],
    declarations: [
        VanNodesViewComponent,
        VanCheckDetailGridViewComponent
    ]
})
export class VanNodesViewModule { }