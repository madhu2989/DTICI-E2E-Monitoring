import { Component, OnInit, Inject, ElementRef, ViewChild } from '@angular/core';
import { DatePipe, formatDate } from '@angular/common';
import { UntypedFormControl, Validators, UntypedFormGroup, UntypedFormBuilder } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatAutocomplete, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatChipInputEvent } from '@angular/material/chips'
import { DeploymentWindow } from '../../shared/model/deployment-window';
import { DataService } from '../../shared/services/data.service';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { DeploymentWindowRepeatInformation } from '../../shared/model/deploymentRepeatInformation';
import { VanEnvironment } from '../../shared/model/van-environment';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { Observable } from 'rxjs';
import { startWith, map } from 'rxjs/operators';

@Component({
  selector: 'app-deployments-edit',
  templateUrl: './deployments-edit.component.html',
  styleUrls: ['./deployments-edit.component.scss']
})
export class DeploymentsEditComponent implements OnInit {

  public startDate: Date;
  public startTime: string;
  public endDate: Date;
  public endTime: string;
  public timeRegEx = '([0-1]?[0-9]|2[0-3]):[0-5][0-9]';
  public elementIds: string;
  chipSelectable = true;
  chipRemovable = true;
  separatorKeysCodes: number[] = [ENTER, COMMA];
  chipElementIdCtrl = new UntypedFormControl();
  chipFilteredElementIds: Observable<string[]>;
  chipElementIds: string[] = [];
  chipAllElementIds: string[] = [];

  @ViewChild('chipElementIdInput') chipElementIdInput: ElementRef<HTMLInputElement>;
  @ViewChild('auto') matAutocomplete: MatAutocomplete;

  currentDeployment: DeploymentWindow;
  currentDeploymentRepeatInformation: DeploymentWindowRepeatInformation;
  showloadingIndicator = false;
  environments: VanEnvironment[] = [];
  startTimeFormGroup: UntypedFormGroup;
  endTimeFormGroup: UntypedFormGroup;
  formControlStartTime: UntypedFormControl;
  formControlEndTime: UntypedFormControl;
  singleDeployment = false;
  repeatInterval: number;
  weekDayOfMonth: string;
  schedulePattern: string;
  monday = {name: 'Monday', value: 1, checked: false};
  tuesday = {name: 'Tuesday', value: 2, checked: false};
  wednesday = {name: 'Wednesday', value: 3, checked: false};
  thursday = {name: 'Thursday', value: 4, checked: false};
  friday = {name: 'Friday', value: 5, checked: false};
  saturday = {name: 'Saturday', value: 6, checked: false};
  sunday = {name: 'Sunday', value: 0, checked: false};
  weekdays = [this.monday, this.tuesday, this.wednesday, this.thursday, this.friday, this.saturday, this.sunday];
  repeatOnMonthDay: number;
  endOfSeries: string;
  maxDate = new Date(new Date().setFullYear(new Date(Date.now()).getFullYear() + 1));
  createNew = false;
  deploymentSetStartFilter = null;
  
  constructor(
    public dialogRef: MatDialogRef<DeploymentsEditComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dataService: DataService,
    public dialog: MatDialog,
    private datePipe: DatePipe,
    private _formBuilder: UntypedFormBuilder
  ) {
    this.currentDeployment = data.deployment as DeploymentWindow;
    this.currentDeploymentRepeatInformation = data.deployment.repeatInformation as DeploymentWindowRepeatInformation;

    if (this.currentDeployment.elementIds != null) {
      // remove environment elementId from the list if it exists
      this.currentDeployment.elementIds = this.currentDeployment.elementIds.filter(eId => eId != this.currentDeployment.environmentSubscriptionId);
      this.chipElementIds = this.currentDeployment.elementIds;
    }
     

    if (this.currentDeployment.environmentSubscriptionId != null) {
      this.chipAllElementIds = this.dataService.getComponentElementIdsPerEnvironment(this.currentDeployment.environmentSubscriptionId);
      this.chipElementIdCtrl.setValue(null);
    }
    
    this.chipFilteredElementIds = this.chipElementIdCtrl.valueChanges.pipe(
      startWith(null),
      map((chipElementId: string | null) => chipElementId ? this.filterChipElementIds(chipElementId) : this.chipAllElementIds.slice()));

    this.formControlStartTime = new UntypedFormControl({disabled: false}, [
      Validators.pattern(this.timeRegEx), Validators.minLength(3), Validators.maxLength(5), Validators.required]);

    this.formControlEndTime = new UntypedFormControl({disabled: false}, [
      Validators.pattern(this.timeRegEx), Validators.minLength(3), Validators.maxLength(5), Validators.required]);
  }

  ngOnInit() {
    const me = this;
    this.startTimeFormGroup = this._formBuilder.group({
      startTimeCtrl: this.formControlStartTime
    });
    me.endTimeFormGroup = this._formBuilder.group({
      endTimeCtrl: me.formControlEndTime
    });

    me.dataService.getAllEnvironments(false).then(function (result) {
      if (result && result.length > 0) {
        for (let i = 0; i < result.length; i++) {
          me.environments.push(result[i]);
          // fill autocomplete options, when the environments are already there
          if (me.currentDeployment.environmentSubscriptionId != null) {
            me.chipAllElementIds = me.dataService.getComponentElementIdsPerEnvironment(me.currentDeployment.environmentSubscriptionId);
            me.chipElementIdCtrl.setValue(null);
          }
        }
      }
    }
    );
   
    this.singleDeployment = this.currentDeployment.repeatInformation ? false : true;

    if (me.currentDeployment.startDate && me.currentDeployment.startDate !== "") {
      me.startTime = formatDate(new Date(me.currentDeployment.startDate), "HH:mm", "en-US");
      me.startDate = new Date(me.currentDeployment.startDate);
      this.startTimeFormGroup.get('startTimeCtrl').setValue(me.startTime);
    } else {
      this.startTimeFormGroup.get('startTimeCtrl').setValue("");
    }

    if (me.currentDeployment.endDate !== "" && me.currentDeployment.endDate) {
      me.endTime = formatDate(new Date(me.currentDeployment.endDate), "HH:mm", "en-US");
      me.endDate = new Date(me.currentDeployment.endDate);
      this.endTimeFormGroup.get('endTimeCtrl').setValue(me.endTime);
    } else {
      this.endTimeFormGroup.get('endTimeCtrl').setValue("");
    }

    if (this.data.mode === 'create') {
      this.currentDeploymentRepeatInformation = new DeploymentWindowRepeatInformation({});
      this.schedulePattern = 'daily';
      this.endOfSeries = 'endless';
      this.repeatInterval = 1;

    } else {

      /*
      if (this.currentDeployment.notificationDelay) {
        this.notificationDelayMins = Math.round(this.currentDeployment.notificationDelay / 60);
      }
      this.setDelay = this.notificationDelayMins ? true : false;
      */
      if (this.currentDeployment.repeatInformation) {
        this.repeatInterval = this.currentDeployment.repeatInformation.repeatInterval;

        switch (this.currentDeployment.repeatInformation.repeatType) {
          case 0:
            this.schedulePattern = 'daily';
            break;
          case 1:
            this.schedulePattern = 'weekly';
            break;
          case 2:
            this.schedulePattern = 'monthly';
            break;
        }
        this.endOfSeries = 'endDate';
        this.endDate = new Date(this.currentDeploymentRepeatInformation.repeatUntil);

        if (this.currentDeployment.repeatInformation.repeatOnSameWeekDayCount || this.currentDeployment.repeatInformation.repeatType === 1) {

          const daysOfWeek = this.weekdays.filter(function(weekday) {
            return weekday.value === me.startDate.getDay();
          });

          for (const weekday of this.weekdays) {
            if (weekday.value === daysOfWeek[0].value) {
              weekday.checked = true;
            }
          }

          const month = this.startDate.getMonth();
          const calculationDate  = new Date(this.startDate);
          calculationDate.setDate(1);
          // search first day of month with same weekday as the start date
          while (calculationDate.getDay() !== this.startDate.getDay()) {
            calculationDate.setDate(calculationDate.getDate() + 1);
          }
          const allWeekDays = [];
          // get all days of the month with the same weekday
          while (calculationDate.getMonth() === month) {
            allWeekDays.push(new Date(calculationDate.getTime()));
            calculationDate.setDate(calculationDate.getDate() + 7);
          }

          for (let i = 0; i < allWeekDays.length; i++) {
            if (allWeekDays[i].getDate() === this.startDate.getDate()) {
              this.weekDayOfMonth = (i + 1).toString();
            }
          }

        } else {
          this.weekDayOfMonth = '0';
          this.repeatOnMonthDay = me.startDate.getDate();
        }
      } else {
        this.currentDeploymentRepeatInformation = new DeploymentWindowRepeatInformation({});
        this.schedulePattern = 'daily';
        this.endOfSeries = 'endless';
        this.repeatInterval = 1;
      }
    }

  }

  filterstartDatePicker() {
    this.deploymentSetStartFilter = (d: Date): boolean => {

      const day = d.getDay();
      const dayOfMonth = d.getDate();
      if (this.schedulePattern === 'daily' || (this.schedulePattern === 'monthly' && (!this.weekDayOfMonth || this.weekDayOfMonth === '0'))) {
        return true;
      } else {
        return (!this.monday.checked ? day !== this.monday.value : true) &&
      (!this.tuesday.checked ? day !== this.tuesday.value : true) &&
      (!this.wednesday.checked ? day !== this.wednesday.value : true) &&
      (!this.thursday.checked ? day !== this.thursday.value : true) &&
      (!this.friday.checked ? day !== this.friday.value : true) &&
      (!this.saturday.checked ? day !== this.saturday.value : true) &&
      (!this.sunday.checked ? day !== this.sunday.value : true) ;
      }

    };
  }

  getSetEndDate(startDate: Date) {
    let endOfRepetition;

    if (this.endOfSeries === 'endless') {
      // get the chosen startDate and set the end date to one year from that
      if (startDate && !isNaN(startDate.getTime())) {
        endOfRepetition = new Date(new Date().setFullYear(startDate.getFullYear() + 1)).toISOString();
      }

    }  else if (this.endOfSeries === 'endDate') {

      if (this.endTimeFormGroup.get('endTimeCtrl').value !== null) {
        const chosenEndTime = this.endTimeFormGroup.get('endTimeCtrl').value;
        if (this.endDate && !isNaN(this.endDate.getTime())) {
          if (chosenEndTime && chosenEndTime !== "") {
            const dateTime = chosenEndTime.split(':');
            this.endDate.setHours(parseInt(dateTime[0], 10));
            this.endDate.setMinutes(parseInt(dateTime[1], 10));
          } else {
            this.endDate.setHours(0, 0);
          }
          endOfRepetition = new Date(this.endDate).toISOString();
        }
      }
    }

    return endOfRepetition;

  }

  getStartDate(startDate: Date, startTime: string): Date {

    if (startDate && !isNaN(startDate.getTime())) {
      if (startTime && startTime !== "") {
        const dateTime = startTime.split(':');
        startDate.setHours(parseInt(dateTime[0], 10));
        startDate.setMinutes(parseInt(dateTime[1], 10));
      } else {
        startDate.setHours(0, 0);
      }
      return startDate;
    }
  }

  getEndDate(startDate: Date, endDate: Date, endTime: string): Date {
    const newEndDate: Date = new Date(endDate);

    if (newEndDate && !isNaN(newEndDate.getTime())) {
      if (endTime && endTime !== "") {
        const dateTime = endTime.split(':');
        newEndDate.setHours(parseInt(dateTime[0], 10));
        newEndDate.setMinutes(parseInt(dateTime[1], 10));
      } else {
        newEndDate.setHours(0, 0);
      }

      // check if startDate is bigger (e.g. if deployment is set from 23:00 until 02:00)
      if (startDate >= newEndDate) {
        newEndDate.setDate(newEndDate.getDate() + 1);
      }
      return newEndDate;
    }

  }

  findDayOfWeek(currentDate: Date, desiredDateValue: number): Date {
    // increment the given date to find for example the next tuesday
    while (currentDate.getDay() !== desiredDateValue) {
      currentDate.setDate(currentDate.getDate() + 1);
    }

    return currentDate;
  }

  findDayOfWeekPerMonth(startDate: Date, desiredDateValue: number, desiredNrOfDay: number): Date {
    // method to find a the specific occurance (e.g. second) of a specific day of week(e.g. Tuesday)

    startDate.setDate(1);
    const month = startDate.getMonth();
    const desiredDays = [];
    let foundDay: Date;
    while (startDate.getDay() !== desiredDateValue) {
      startDate.setDate(startDate.getDate() + 1);
    }

    while (startDate.getMonth() === month) {
      desiredDays.push(new Date(startDate.getTime()));
      startDate.setDate(startDate.getDate() + 7);
    }

    foundDay = desiredDays[desiredNrOfDay - 1];
    if (foundDay < this.startDate) {
      foundDay.setMonth(foundDay.getMonth() + 1);
      return this.findDayOfWeekPerMonth(foundDay, desiredDateValue, desiredNrOfDay);
    } else {
      return foundDay;
    }

  }

  validate(): boolean {
    let valid = true;

    if (!this.currentDeployment.environmentSubscriptionId || !this.currentDeployment.description ||
      !this.currentDeployment.shortDescription || !this.startDate ) {
      valid = false;
    }

    if (!this.singleDeployment) {
      if (!this.startTimeFormGroup.valid || !this.endTimeFormGroup.valid) {
        valid = false;
      }
    }

    return valid;
  }

  onSaveClick() {
    this.showloadingIndicator = true;
    const chosenStartTime = this.startTimeFormGroup.get('startTimeCtrl').value;

    this.weekdays = this.weekdays.filter(function(weekday) {
      return weekday.checked;
    });

    if (!this.endDate && this.singleDeployment) {
      this.currentDeployment.closeReason = '';
    }


    // for single Deployments
    if (this.singleDeployment) {
      this.currentDeploymentRepeatInformation = null;
      const chosenStartDate = this.getStartDate(this.startDate, chosenStartTime);
      this.currentDeployment.startDate = chosenStartDate.toISOString();

      if (this.endDate) {
        let chosenEndTime;
        if (this.endTimeFormGroup.get('endTimeCtrl').value !== null) {
        chosenEndTime = this.endTimeFormGroup.get('endTimeCtrl').value;
        } else {
          chosenEndTime = '';
        }
        this.currentDeployment.endDate = this.getEndDate(chosenStartDate, this.endDate, chosenEndTime).toISOString();

      } else {
        this.currentDeployment.endDate = null;
      }

      this.saveDeployment(this.currentDeployment);

    } else {
      // deployment sets

      let foundStartDate;
      switch (this.schedulePattern) {
        case 'daily':
          this.currentDeploymentRepeatInformation.repeatType = 0;
          break;
        case 'weekly':
          this.currentDeploymentRepeatInformation.repeatType = 1;
          break;
        case 'monthly':
          this.currentDeploymentRepeatInformation.repeatType = 2;
          break;
      }
      const chosenEndTime = this.endTimeFormGroup.get('endTimeCtrl').value;
      this.currentDeploymentRepeatInformation.repeatInterval = this.repeatInterval ? this.repeatInterval : 1;
      this.currentDeploymentRepeatInformation.repeatOnSameWeekDayCount = false;

      if (this.schedulePattern === 'daily' || (this.schedulePattern === 'weekly' && this.weekdays.length === 0) || (this.schedulePattern === 'monthly' && !this.weekDayOfMonth)) {
        // Daily Deployments or weekly/monthly deployments on one single day
        const startDateOfSet = this.getStartDate(this.startDate, chosenStartTime);
        this.buildDeploymentToSave(startDateOfSet);

      } else if ( this.schedulePattern === 'weekly') {
        // weekly Deployments on one ore more specific day(s)

        if (this.weekdays.length > 0) {
          // multiple Deployments has to be sent to BE
          for (let i = 0; i < this.weekdays.length; i++) {
            this.createNew = i > 0 ? true : false;
            if (this.startDate.getDay() === this.weekdays[i].value) {
              foundStartDate = this.getStartDate(this.startDate, chosenStartTime);
            } else {
              const calculatedStartDate = this.findDayOfWeek(this.startDate, this.weekdays[i].value);
              foundStartDate = this.getStartDate(calculatedStartDate, chosenStartTime);
            }

            this.buildDeploymentToSave(foundStartDate);
          }
        }

      } else if (this.schedulePattern === 'monthly') {
        // monthly deployments

        if (this.weekDayOfMonth === '0') {
          // deployment on a specific date of the month, e.g. the 13th
            if (this.repeatOnMonthDay) {
              this.startDate.setDate(this.repeatOnMonthDay);
              if (this.startDate < new Date(Date.now())) {
                this.startDate.setMonth(this.startDate.getMonth() + 1);
              }
              foundStartDate = this.getStartDate(this.startDate, chosenStartTime);
              this.buildDeploymentToSave(foundStartDate);
            }

        } else {
          // deployments that will be repeated on e.g. the first tuesday of every month
          if (this.weekdays.length > 0) {
            for (let i = 0; i < this.weekdays.length; i++) {

              this.createNew = i > 0 ? true : false;
              let firstPossibleStartDate;
              if (this.startDate.getDay() === this.weekdays[i].value) {
                firstPossibleStartDate = this.findDayOfWeekPerMonth(this.startDate, this.weekdays[i].value, parseInt(this.weekDayOfMonth, 10));
              } else {
                const calculatedStartDate  = this.findDayOfWeek(this.startDate, this.weekdays[i].value);
                firstPossibleStartDate = this.findDayOfWeekPerMonth(calculatedStartDate, this.weekdays[i].value, parseInt(this.weekDayOfMonth, 10));
              }
              foundStartDate = this.getStartDate(firstPossibleStartDate, chosenStartTime);
              this.currentDeploymentRepeatInformation.repeatOnSameWeekDayCount = true;
              this.buildDeploymentToSave(foundStartDate);

            }
          }
        }

      }
    }

  }

  buildDeploymentToSave(foundStartDate: Date) {
    const chosenStartTime = this.startTimeFormGroup.get('startTimeCtrl').value;
    const chosenEndTime = this.endTimeFormGroup.get('endTimeCtrl').value;

    this.currentDeployment.startDate = foundStartDate.toISOString();
    this.currentDeployment.endDate = this.getEndDate(foundStartDate, foundStartDate, chosenEndTime).toISOString();
    this.currentDeploymentRepeatInformation.repeatUntil = this.getSetEndDate(foundStartDate);    

    this.saveDeployment(this.currentDeployment);

  }

  saveDeployment(deployment: DeploymentWindow) {
    deployment.repeatInformation = this.currentDeploymentRepeatInformation;

    if (this.chipElementIds != null && this.chipElementIds.length > 0) {
      deployment.elementIds = [];
      deployment.elementIds = this.chipElementIds;
    } else if (this.chipElementIds == null || this.chipElementIds.length == 0) {
      deployment.elementIds = [];
      deployment.elementIds.push(this.currentDeployment.environmentSubscriptionId);
    };

    console.log("Deployment:" + deployment);

    let promise;

    if (this.data.mode === "create" || this.createNew) {
      promise = this.dataService.createDeploymentWindow(deployment);
    } else {
      promise = this.dataService.updateDeploymentWindow(deployment, deployment.id);
    }

    promise.then(deploymentResult => {
      this.showloadingIndicator = false;
      this.dialogRef.close("refresh");
    }).catch(error => {
      this.showloadingIndicator = false;
      this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error creating/updating data.");
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

  chipElementIdSelected(event: MatAutocompleteSelectedEvent): void {
    // add to the list only once and make it case insensitive
    if (!this.chipElementIds.map((a) => { return a.toLowerCase() }).includes(event.option.viewValue.toLocaleLowerCase())) { 
      this.chipElementIds.push(event.option.viewValue);
    };
    this.chipElementIdInput.nativeElement.value = '';
    this.chipElementIdCtrl.setValue(null);
  }

  addChipElementId(event: MatChipInputEvent): void {
    const input = event.input;
    const value = event.value;

    // Add elementId
    if ((value || '').trim()) {
      // add to the list only once and make it case insensitive
      if (!this.chipElementIds.map((a) => { return a.toLowerCase() }).includes(value.trim().toLocaleLowerCase()) && 
      this.chipAllElementIds.map((a) => { return a.toLowerCase() }).includes(value.trim().toLocaleLowerCase())) {
        this.chipElementIds.push(value.trim());
      }
    }

    // Reset the input value
    if (input) {
      input.value = '';
    }

    this.chipElementIdCtrl.setValue(null);
  }

  removeChipElementId(eId: string): void {
    const index = this.chipElementIds.indexOf(eId);
    if (index >= 0) {
      this.chipElementIds.splice(index, 1);
    }
  }

  private filterChipElementIds(value: string): string[] {
    const filterValue = value.toLowerCase();

    return this.chipAllElementIds.filter(chipElementId => chipElementId.toLowerCase().indexOf(filterValue) === 0);
  }

  environmentSelectionChanged(event: any) {
    this.chipElementIds = [];
    this.chipAllElementIds = this.dataService.getComponentElementIdsPerEnvironment(event);
    this.chipElementIdCtrl.setValue(null);
  }

}
