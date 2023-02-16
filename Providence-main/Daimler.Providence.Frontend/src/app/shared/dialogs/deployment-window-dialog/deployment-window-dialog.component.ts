import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialog } from "@angular/material/dialog";
import { MatSnackBar } from "@angular/material/snack-bar";
import { DatePipe } from "@angular/common";

@Component({
  selector: 'app-deployment-window-dialog',
  templateUrl: './deployment-window-dialog.component.html',
  styleUrls: ['./deployment-window-dialog.component.scss']
})
export class DeploymentWindowComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: any, public dialogRef: MatDialogRef<any>,
  private datepipe: DatePipe,
  public dialog: MatDialog,
  private snackbar: MatSnackBar) { }

  ngOnInit() {
  }
  onCloseClick() {
    this.dialogRef.close();
  }

  // Source: https://stackoverflow.com/questions/49102724/angular-5-copy-to-clipboard/49121680#49121680
  copyText(val: string) {
    const selBox = document.createElement('textarea');
    selBox.style.position = 'fixed';
    selBox.style.left = '0';
    selBox.style.top = '0';
    selBox.style.opacity = '0';
    selBox.value = val;
    document.body.appendChild(selBox);
    selBox.focus();
    selBox.select();
    document.execCommand('copy');
    document.body.removeChild(selBox);
    this.snackbar.open(`Text has been copied to clipboard`, '', {
      panelClass: "mysnackbar",
      horizontalPosition: "center",
      duration: 2000
    });
  }

}
