import { Component, OnInit, Inject, EventEmitter } from "@angular/core";
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { ActivatedRoute, Router } from "@angular/router";

@Component({
  selector: 'app-confirmation-dialog',
  templateUrl: './confirmation-dialog.component.html',
  styleUrls: ['./confirmation-dialog.component.scss']
})
export class ConfirmationDialogComponent implements OnInit {
  public isConfirmed = true;
  isUnassignedButtonActive: boolean;
  onDelete = new EventEmitter();
  onUnassigned = new EventEmitter();
  onCancel = new EventEmitter();

  constructor(@Inject(MAT_DIALOG_DATA) public data: any, public dialogRef: MatDialogRef<ConfirmationDialogComponent>) { }

  ngOnInit() {
    if (this.data.mode && this.data.mode === "Unassign" && this.data.nodeType === "component") {
      this.isUnassignedButtonActive = true;
    }
  }

  onActionClick() {
    this.onDelete.emit(null);
    this.dialogRef.close(this.isConfirmed);
  }

  onCancelClick() {
    this.onCancel.emit(null);
    this.dialogRef.close();
  }

  onUnassignedClick() {
    this.onUnassigned.emit(null);
    this.dialogRef.close();
  }

}
