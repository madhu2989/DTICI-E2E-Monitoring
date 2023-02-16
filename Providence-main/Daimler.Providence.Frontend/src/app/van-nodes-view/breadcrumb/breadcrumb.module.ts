import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { BreadcrumbComponent } from './breadcrumb.component';
import { MaterialModule } from "../../material.module";
import { FlexLayoutModule } from "@angular/flex-layout";

@NgModule({
    imports: [
        CommonModule,
        MaterialModule,
        RouterModule,
        FlexLayoutModule
    ],
    exports: [
        BreadcrumbComponent
    ],
    declarations: [
        BreadcrumbComponent
    ]
})
export class BreadcrumbModule { }