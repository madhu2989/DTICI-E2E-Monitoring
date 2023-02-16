import { Component } from "@angular/core";
import { ICellRendererAngularComp } from "ag-grid-angular/dist/interfaces";

@Component({
  selector: 'app-button-renderer',
  template: '<button type="button" (click)="onClick($event)" [disabled]="disable">{{label}}</button>'
})
export class ButtonRendererComponent implements ICellRendererAngularComp {
  params;
  label: string;
  disable: boolean; 

  agInit(params): void {
    this.disable =  true;
    this.params = params;
    this.label = this.params.label || null;
    const level = params.data ? params.value : params;
    if(level === 3)
    {
      this.disable =  false;
    }
  }

  refresh(params?: any): boolean {
    return true;
  }

  onClick($event) {
    if (this.params.onClick instanceof Function) {
      const params = {
        event: $event,
        rowData: this.params.node.data
      }
      this.params.onClick(params);
    }
  }

  ngOnDestroy() {
    // no need to remove the button click handler 
    // https://stackoverflow.com/questions/49083993/does-angular-automatically-remove-template-event-listeners
  }
}
