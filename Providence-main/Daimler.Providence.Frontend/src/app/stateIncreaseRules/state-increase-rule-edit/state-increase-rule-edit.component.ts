import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { VanEnvironment } from '../../shared/model/van-environment';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { StateIncreaseRule } from '../../shared/model/stateIncreaseRule';
import { DataService, ICheckIdSearchResult, IComponentIdSearchResult } from '../../shared/services/data.service';
import { VanChecks } from '../../shared/model/van-checks';
import { VanComponent } from '../../shared/model/van-component';
import { Observable } from 'rxjs';
import { UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';
import { debounceTime } from 'rxjs/internal/operators/debounceTime';
import { map } from 'rxjs/internal/operators';
import { startWith } from 'rxjs/internal/operators/startWith';

@Component({
  selector: 'app-state-increase-rule-edit',
  templateUrl: './state-increase-rule-edit.component.html',
  styleUrls: ['./state-increase-rule-edit.component.scss']
})
export class StateIncreaseRuleEditComponent implements OnInit {

  showloadingIndicator = false;
  environments: VanEnvironment[] = [];
  components: VanComponent[];
  componentIds: string[] = [];
  checks: VanChecks[];
  checkIds: string[] = [];
  currentStateIncreaseRule: StateIncreaseRule;
  originalStateIncreaseRule: StateIncreaseRule;
  title = "";
  environmentSubscriptionId = "";
  public frequencyIsChecked;

  componentIdForm: UntypedFormGroup;
  checkIdForm: UntypedFormGroup;
  filteredComponentIds: Observable<string[]>;
  filteredCheckIds: Observable<string[]>;

  constructor(
    public dialogRef: MatDialogRef<StateIncreaseRuleEditComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dataService: DataService,
    private fb: UntypedFormBuilder,
    public dialog: MatDialog
  ) {
    this.currentStateIncreaseRule = data.stateIncreaseRule as StateIncreaseRule;
    this.title = data.title;
    this.components = data.components;
    this.checks = data.checks;
    for (const check of this.checks) {
      this.checkIds.push(check.elementId);
    }

    for (const comp of this.components) {
      this.componentIds.push(comp.elementId);
    }
  }

  ngOnInit() {
    const me = this;
    this.originalStateIncreaseRule = new StateIncreaseRule(this.data.stateIncreaseRule);
    me.dataService.getAllEnvironments(false).then(function (result) {
      if (result && result.length > 0) {
        for (let i = 0; i < result.length; i++) {
          me.environments.push(result[i]);
        }
      }
    });

    this.componentIdForm = this.fb.group({
      userInputComponentId: null
    });

    this.checkIdForm = this.fb.group({
      userInputCheckId: null
    });

    if (this.data.mode === 'create') {
      this.componentIdForm.get('userInputComponentId').disable();
      this.checkIdForm.get('userInputCheckId').disable();
    }

    if (this.currentStateIncreaseRule.checkId) {
      this.checkIdForm.get('userInputCheckId').setValue(this.currentStateIncreaseRule.checkId);
    } else {
      this.checkIdForm.get('userInputCheckId').setValue('');
    }

    if (this.currentStateIncreaseRule.componentId) {
      this.componentIdForm.get('userInputComponentId').setValue(this.currentStateIncreaseRule.componentId);
    } else {
      this.componentIdForm.get('userInputComponentId').setValue('');
    }

    this.filteredComponentIds = this.componentIdForm.get('userInputComponentId').valueChanges
      .pipe(
        map(value => this._componentFilter(value))
      );

    this.filteredCheckIds = this.checkIdForm.get('userInputCheckId').valueChanges
      .pipe(
        map(value => this._checkFilter(value))
      );

    this.componentIdForm.get('userInputComponentId').valueChanges.subscribe(result => {
      if (result) {
        this.checkIdForm.get('userInputCheckId').enable();
      } else {
        this.checkIdForm.get('userInputCheckId').disable();
      }
      this.checkIdForm.get('userInputCheckId').setValue('');
    });
  }

  public _componentFilter(value: string): string[] {
    const filterValue = value.toLowerCase();
    let currentEnvironment;
    let services;
    let actions;
    let components;
    const componentsInEnv = [];


    if (this.currentStateIncreaseRule.environmentSubscriptionId) {
      for (const env of this.environments) {
        if (env.subscriptionId === this.currentStateIncreaseRule.environmentSubscriptionId) {
          currentEnvironment = env;
          services = currentEnvironment.getChildNodes();
        }
      }

      for (const service of services) {
        actions = service.getChildNodes();
        for (const action of actions) {
          components = action.getChildNodes();
          for (const component of components) {
            componentsInEnv.push(component.elementId);
          }
        }
      }

      return componentsInEnv.filter(component => component.toLowerCase().includes(filterValue));
    } else {
      return this.componentIds.filter(component => component.toLowerCase().includes(filterValue));
    }
  }

  public _checkFilter(value: string): string[] {
    const filterValue = value.toLowerCase();
    const compChecks = [];

    if (this.currentStateIncreaseRule.environmentSubscriptionId) {

      for (const check of this.checks) {
        if (check.environmentSubscriptionId === this.currentStateIncreaseRule.environmentSubscriptionId) {
          compChecks.push(check.elementId);
        }
      }
      return compChecks.filter(component => component.toLowerCase().includes(filterValue));

    } else {

      return this.checkIds.filter(check => check.toLowerCase().includes(filterValue));
    }
  }

  enableComponents() {
    this.componentIdForm.get('userInputComponentId').enable();
    this.componentIdForm.get('userInputComponentId').setValue('');
    this.checkIdForm.get('userInputCheckId').setValue('');
  }

  formatLabel(timerangeValue: number | null) {
    if (!timerangeValue) {
      return 0;
    } else {
      return timerangeValue;
    }
  }

  displayFnCheckId(element: String) {
    if (element) { return element; }
  }

  displayFnCompId(compId: String) {
    if (compId) { return compId; }
  }


  onSaveClick() {
    this.showloadingIndicator = true;
    let promise;
    let closeMessage;

    if (!this.currentStateIncreaseRule.triggerTime) {
      this.currentStateIncreaseRule.triggerTime = 1;
    }
    if (!this.currentStateIncreaseRule.isActive) {
      this.currentStateIncreaseRule.isActive = false;
    }

    this.currentStateIncreaseRule.checkId = this.checkIdForm.get('userInputCheckId').value;
    this.currentStateIncreaseRule.componentId = this.componentIdForm.get('userInputComponentId').value;

    if (this.data.mode === "create") {
      promise = this.dataService.createStateIncreaseRule(this.currentStateIncreaseRule);
      closeMessage = "created";
    } else {
      promise = this.dataService.updateStateIncreaseRule(this.currentStateIncreaseRule, this.currentStateIncreaseRule.id);
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
    this.currentStateIncreaseRule.name = this.originalStateIncreaseRule.name;
    this.currentStateIncreaseRule.description = this.originalStateIncreaseRule.description;
    this.currentStateIncreaseRule.environmentSubscriptionId = this.originalStateIncreaseRule.environmentSubscriptionId;
    this.currentStateIncreaseRule.componentId = this.originalStateIncreaseRule.componentId;
    this.currentStateIncreaseRule.checkId = this.originalStateIncreaseRule.checkId;
    this.currentStateIncreaseRule.alertName = this.originalStateIncreaseRule.alertName;
    this.currentStateIncreaseRule.isActive = this.originalStateIncreaseRule.isActive;
    this.currentStateIncreaseRule.triggerTime = this.originalStateIncreaseRule.triggerTime;
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
