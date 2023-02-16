import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { VanEnvironment } from '../../shared/model/van-environment';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { VanChecks } from '../../shared/model/van-checks';
import { MasterDataService, ElementType } from '../../shared/services/masterdata.service';
import { DataService } from '../../shared/services/data.service';

@Component({
  selector: 'app-check-edit',
  templateUrl: './check-edit.component.html',
  styleUrls: ['./check-edit.component.scss']
})
export class CheckEditComponent implements OnInit {

  showloadingIndicator = false;
  environments: VanEnvironment[] = [];
  currentVanCheck: VanChecks;
  title = "";
  environmentSubscriptionId = "";
  public frequencyIsChecked;
  originalVanCheck: VanChecks;
  chosenfrequency: number;

  constructor(
    public dialogRef: MatDialogRef<CheckEditComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private masterDataService: MasterDataService,
    private dataService: DataService,
    public dialog: MatDialog
  ) {
    this.currentVanCheck = data.vanCheck as VanChecks;
    this.title = data.title;
  }

  ngOnInit() {
    const me = this;
    this.originalVanCheck = new VanChecks(this.data.vanCheck);
    me.dataService.getAllEnvironments(false).then(function (result) {
      if (result && result.length > 0) {
        for (let i = 0; i < result.length; i++) {
          me.environments.push(result[i]);
        }
      }
    });

    if (this.data.mode === "edit") {
      this.currentVanCheck.frequency = this.currentVanCheck.frequency / 60;
      if (this.data.vanCheck.frequency < 0) {
        this.frequencyIsChecked = false;
        this.currentVanCheck.frequency = 10;
      } else {
        this.frequencyIsChecked = true;
        this.currentVanCheck.frequency = this.data.vanCheck.frequency;
      }
    } else {
      this.currentVanCheck.frequency = 10;
    }
    this.chosenfrequency = this.currentVanCheck.frequency;
  }



  onSaveClick() {
    this.showloadingIndicator = true;
    let promise;
    let closeMessage;
    console.log(this.currentVanCheck);
    this.currentVanCheck.frequency = this.chosenfrequency * 60;

    if (!this.frequencyIsChecked) {
      this.currentVanCheck.frequency = -1;
      this.chosenfrequency = -1;
    } else if (this.frequencyIsChecked && !this.currentVanCheck.frequency) {
      this.currentVanCheck.frequency = 10;
      this.chosenfrequency = 10;
    }


    if (this.data.mode === "create") {
      promise = this.masterDataService.create(ElementType.CHECK, this.currentVanCheck, this.currentVanCheck.environmentSubscriptionId);
      closeMessage = "created";
    } else {
      promise = this.masterDataService.update(ElementType.CHECK, this.currentVanCheck, this.currentVanCheck.environmentSubscriptionId);
      closeMessage = "edited";
    }

    promise.then(vanCheck => {
      this.showloadingIndicator = false;
      this.dialogRef.close(closeMessage);
    }).catch(error => {
      this.showloadingIndicator = false;
      this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error creating/updating data.");
    });
  }

  onCancelClick() {

    this.currentVanCheck.name = this.originalVanCheck.name;
    this.currentVanCheck.description = this.originalVanCheck.description;
    this.currentVanCheck.vstsLink = this.originalVanCheck.vstsLink;
    this.currentVanCheck.frequency = this.originalVanCheck.frequency;
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
