import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DataService } from '../../services/data.service';

@Component({
  selector: 'cancel-request-dialog',
  templateUrl: './cancel-request-dialog.component.html',
  styleUrls: ['./cancel-request-dialog.component.scss']
})
export class CancelRequestDialogComponent implements OnInit {
  
  constructor(@Inject(MAT_DIALOG_DATA) public data: any, public dialogRef: MatDialogRef<CancelRequestDialogComponent>) { }

  ngOnInit() {
  }

  onCancelClick() {
    this.data.dataService.cancelOpenRequests();
    this.dialogRef.close("cancelled");
  }
}
