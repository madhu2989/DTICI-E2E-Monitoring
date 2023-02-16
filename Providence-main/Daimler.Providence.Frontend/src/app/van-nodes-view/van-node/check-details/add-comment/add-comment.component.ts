import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialog } from "@angular/material/dialog";
import { AlertComment } from '../../../../shared/model/alert-comment';
import { DataService } from '../../../../shared/services/data.service';
import { AdalService } from "adal-angular4";
import { SecretService } from '../../../../shared/services/secret.service';
import { ErrorDialogComponent } from '../../../../shared/dialogs/error-dialog/error-dialog.component';

@Component({
  selector: 'app-add-comment',
  templateUrl: './add-comment.component.html',
  styleUrls: ['./add-comment.component.scss']
})
export class AddCommentComponent implements OnInit {

  public currentAlertComment: AlertComment;
  public currentUser;
  showloadingIndicator = false;

  constructor(@Inject(MAT_DIALOG_DATA) public data: any, public dialogRef: MatDialogRef<any>,
  public dialog: MatDialog,
  private dataService: DataService,
  private secretService: SecretService,
  private adalService: AdalService) {
    this.currentAlertComment = data.alertComment as AlertComment;
    this.adalService.init(this.secretService.adalConfig);
  }

  ngOnInit() {
    this.adalService.getUser();
    this.currentUser = this.adalService.userInfo;
  }

  onSaveClick() {
    this.showloadingIndicator = true;
    const timestampOfLastChange = new Date(Date.now());

    this.currentAlertComment.timestamp = timestampOfLastChange.toISOString();
    this.currentAlertComment.user = this.currentUser.profile.email ? this.currentUser.profile.email : this.currentUser.profile.unique_name;
    this.currentAlertComment.recordId = this.data.recordId;

    let promise;

    if (this.data.mode === "create") {
      promise = this.dataService.createAlertComment(this.currentAlertComment);
    } else {
      promise = this.dataService.updateAlertComment(this.currentAlertComment, this.currentAlertComment.id);
    }

    promise.then(deployment => {
      this.showloadingIndicator = false;
      this.dialogRef.close("refresh");
    }).catch(error => {
      this.showloadingIndicator = false;
      this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error creating/updating data.");
    });

  }

  onCloseClick() {
    this.dialogRef.close();
  }

  showErrorDialog(message: string, title: string) {
    const errorDialog = this.dialog.open(ErrorDialogComponent, {
      width: '400px',
      data: { message, title },
      disableClose: true
    });
  }

}
