import { Component, OnInit } from '@angular/core';
import { VanEnvironment } from '../../shared/model/van-environment';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { DataService, Element } from '../../shared/services/data.service';
import { BaseComponent } from '../../shared/base-component/base.component';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { UntypedFormBuilder, UntypedFormGroup, UntypedFormControl } from '@angular/forms';
import { SLAReportJob } from '../../shared/model/sla-report-job';

function dateValidator(startDateCtrl: string, endDateCtrl: string) {
  return (formGroup: UntypedFormGroup) => {
    const startDateControl = formGroup.controls[startDateCtrl];
    const endDateControl = formGroup.controls[endDateCtrl];

    const startDate = appendTimeToDate(startDateControl.value, '12:00:00');
    const endDate = appendTimeToDate(endDateControl.value, '12:00:00');
    if (startDate && !isNaN(startDate.getTime()) && endDate && !isNaN(endDate.getTime())) {
      if (endDateControl.errors && !endDateControl.errors.startBiggerEnd) {
        // return if another validator has already found an error
        return;
      }
      // set error on end Date control if validation fails
      if (startDate > endDate) {
        endDateControl.setErrors({startBiggerEnd: true});
      } else {
        endDateControl.setErrors(null);
      }
    }
  };
}

function appendTimeToDate(date: Date, time: string): Date {
  if (date && !isNaN(date.getTime())) {
    if (time && time !== "") {
      const dateTime = time.split(':');
      //if (dateTime.length >= 0) {
        date.setHours(parseInt(dateTime[0], 10));
      // } else {
      //   date.setHours(0);
      // }
      if (dateTime.length >= 1) {
        date.setMinutes(parseInt(dateTime[1], 10));
      } else {
        date.setMinutes(0);
      }
      if (dateTime.length >= 2) {
        date.setSeconds(parseInt(dateTime[2], 10));
      } else {
        date.setSeconds(0);
      }
      date.setMilliseconds(0);
    }
  }
  return date;
}

@Component({
  selector: 'app-sla-reports-jobs-edit',
  templateUrl: './sla-reports-jobs-edit.component.html',
  styleUrls: ['./sla-reports-jobs-edit.component.scss']
})
export class SlaReportJobsEditComponent extends BaseComponent implements OnInit {
  environments: VanEnvironment[] = [];
  environmentSubscriptionId: string;

  public startDate: Date;
  public endDate: Date;
  timeFormGroup: UntypedFormGroup;
  formControlStartDate: UntypedFormControl;
  formControlEndDate: UntypedFormControl;
  showloadingIndicator = false;

  constructor(
    public dialogRef: MatDialogRef<SlaReportJobsEditComponent>,
    private dataService: DataService,
    public dialog: MatDialog,
    private fb: UntypedFormBuilder,
  ) {
    super();  
    this.formControlStartDate = new UntypedFormControl('', []);
    this.formControlEndDate = new UntypedFormControl('', []);

    this.timeFormGroup = this.fb.group({
      startDateCtrl: this.formControlStartDate,
      endDateCtrl: this.formControlEndDate
    }, {validator: dateValidator('startDateCtrl', 'endDateCtrl')});
    const dateEndInit = new Date();
    const daysOffsetInMs = 3 * 24 * 60 * 60 * 1000;
    const dateStartInit = new Date(dateEndInit.getTime() - daysOffsetInMs);
    this.timeFormGroup.get('startDateCtrl').setValue(dateStartInit);
    this.timeFormGroup.get('endDateCtrl').setValue(dateEndInit);
  }

  ngOnInit() {
    const me = this;
    me.dataService.getAllEnvironments(false).then(function (result) {
      if (result && result.length > 0) {
        for (let i = 0; i < result.length; i++) {
          me.environments.push(result[i]);
        }
      }
    });
  }

  displayFn(element: Element) {
    if (element) { return element.name; }
  }
  
  getDateString(date: Date): string {
    if (date && !isNaN(date.getTime())) {
      return date.toISOString();
    }
    return '';
  }
  
  validate(): boolean {
    let valid = true;
    if (!this.environmentSubscriptionId) {
      valid = false;
    }
    if (this.timeFormGroup.errors) {
      valid = false;
    }
    return valid;
  }

  onCreateReportJobClick() {   
    this.showloadingIndicator = true;        
    const start = this.timeFormGroup.get('startDateCtrl').value;
    const end = this.timeFormGroup.get('endDateCtrl').value;   
    let  job = new SLAReportJob();
    job.startDate = this.getDateString(start);
    job.endDate = this.getDateString(end);
    job.type = 'SLA';
    job.environmentSubscriptionId = this.environmentSubscriptionId;

    this.dataService.createSLAReportJob(job).then(
        response => {
          this.showloadingIndicator = false;
          this.dialogRef.close("refresh");
        }
    ).catch(error => {
      this.showloadingIndicator = false;
      this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error creating SLA Job.");
    });
  }

  showErrorDialog(message: string, title: string) {
    const errorDialog = this.dialog.open(ErrorDialogComponent, {
      width: '400px',
      data: { message, title },
      disableClose: true
    });
  }

  onCancelClick() {
    this.dialogRef.close();
  }
}
