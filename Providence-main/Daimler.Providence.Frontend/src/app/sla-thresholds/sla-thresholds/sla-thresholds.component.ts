import { Component, OnInit } from '@angular/core';
import { MatDialog } from "@angular/material/dialog";
import { MatSnackBar } from "@angular/material/snack-bar";
import { UntypedFormBuilder, UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import { VanEnvironment } from '../../shared/model/van-environment';
import { DataService } from '../../shared/services/data.service';
import { SLAConfig } from '../../shared/model/sla-config';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';

function percentageValidator(warningPercentage: string, errorPercentage: string) {

  return (formGroup: UntypedFormGroup) => {
    const warning = formGroup.controls[warningPercentage];
    const error = formGroup.controls[errorPercentage];

    if (warning.errors && !warning.errors.warningBiggerError) {
        // return if another validator has already found an error on the matchingControl
        return;
    }

    // set error on matchingControl if validation fails
    if (warning.value < error.value) {
        warning.setErrors({ warningBiggerError: true });
    } else {
        warning.setErrors(null);
    }
};
}
@Component({
  selector: 'app-sla-thresholds',
  templateUrl: './sla-thresholds.component.html',
  styleUrls: ['./sla-thresholds.component.scss']
})
export class SlaThresholdsComponent implements OnInit {

  environments: VanEnvironment[] = [];
  environmentSubscriptionId: string;
  thresholdSource: string;
  showloadingIndicator = false;
  existingSLAConfig = false;
  errorConfig: SLAConfig = new SLAConfig();
  warningConfig: SLAConfig = new SLAConfig();
  includeWarningConfig: SLAConfig = new SLAConfig();
  warningFormGroup: UntypedFormGroup;
  dialogFormGroup: UntypedFormGroup;
  warningFormControl: UntypedFormControl;
  errorFormControl: UntypedFormControl;
  sourceFormControl: UntypedFormControl;


  constructor(
    private dataService: DataService,
    private snackbar: MatSnackBar,
    private fb: UntypedFormBuilder,
    private dialog: MatDialog) {
      this.warningFormControl = new UntypedFormControl({disabled: false}, [Validators.min(0), Validators.max(100)]);
      this.errorFormControl = new UntypedFormControl({disabled: false}, [Validators.min(0), Validators.max(100)]);
      this.sourceFormControl = new UntypedFormControl();
      this.dialogFormGroup = this.fb.group({
        sourceFormControl: this.sourceFormControl
      });  
      this.warningFormGroup = this.fb.group({
        
        warningFormControl: this.warningFormControl,
        errorFormControl: this.errorFormControl
        
      }, {validator: percentageValidator('warningFormControl', 'errorFormControl')});
      this.warningFormGroup.get('warningFormControl').setValue('');
      this.warningFormGroup.get('errorFormControl').setValue('');

      // disable till a environment is selected
      this.dialogFormGroup.get('sourceFormControl').disable();
      this.warningFormGroup.get('warningFormControl').disable();
      this.warningFormGroup.get('errorFormControl').disable();

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
    me.includeWarningConfig.key = 'SLA_Include_Warnings';
    me.warningConfig.key = 'SLA_Warning_Threshold';
    me.errorConfig.key = 'SLA_Error_Threshold';

  }

  getSLAThreshold() {
    const me = this;
    this.showloadingIndicator = true;
    me.existingSLAConfig = false;

    me.dataService.getSLAConfig(this.environmentSubscriptionId).then(function (slaConfigResponse: SLAConfig[]) {
      if (slaConfigResponse && slaConfigResponse.length > 0) {
        me.showloadingIndicator = false;
        me.existingSLAConfig = true;
        me.errorFormControl.setValue(Number((Number(slaConfigResponse.find(config => config.key === 'SLA_Error_Threshold').value) * 100).toFixed(2)));
        me.warningFormControl.setValue(Number((Number(slaConfigResponse.find(config => config.key === 'SLA_Warning_Threshold').value) * 100).toFixed(2)));

        me.errorConfig.id = slaConfigResponse.find(config => config.key === 'SLA_Error_Threshold').id;
        me.warningConfig.id = slaConfigResponse.find(config => config.key === 'SLA_Warning_Threshold').id;

        me.includeWarningConfig.value = slaConfigResponse.find(config => config.key === 'SLA_Include_Warnings').value === 'true' ? 'errorWarning' : 'error';
        me.includeWarningConfig.id = slaConfigResponse.find(config => config.key === 'SLA_Include_Warnings').id;
        me.thresholdSource = slaConfigResponse.find(config => config.key === 'SLA_Include_Warnings').value === 'true' ? 'errorWarning' : 'error';

      } else {
        me.showloadingIndicator = false;
        me.thresholdSource = null;
        me.warningFormControl.setValue(null);
        me.errorFormControl.setValue(null);
        me.snackbar.open(`No SLA Threshold configuration found. Please create a new one`, '', {
          panelClass: "mysnackbar",
          horizontalPosition: "center",
          duration: 8000
        });
        }
    }, (error) => {
      me.showloadingIndicator = false;
      this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error loading data");
      me.existingSLAConfig = false;
    });

  }

  setSLAThreshold() {
    const me = this;
    this.showloadingIndicator = true;
    // const errorThreshold = new SLAConfig;
    let promiseError;
    let promiseWarning;
    let promiseSource;
    me.errorConfig.value = (this.errorFormControl.value / 100).toString();
    me.warningConfig.value = (this.warningFormControl.value / 100).toString();
    me.includeWarningConfig.value = me.thresholdSource === 'error' ? 'false' : 'true';
    me.errorConfig.environmentSubscriptionId = me.environmentSubscriptionId;
    me.warningConfig.environmentSubscriptionId = me.environmentSubscriptionId;
    me.includeWarningConfig.environmentSubscriptionId = me.environmentSubscriptionId;

      if (this.existingSLAConfig) {
        promiseError = this.dataService.updateSLAConfig(this.environmentSubscriptionId, me.errorConfig, me.errorConfig.key);
        promiseWarning = this.dataService.updateSLAConfig(this.environmentSubscriptionId, me.warningConfig, me.warningConfig.key);
        promiseSource = this.dataService.updateSLAConfig(this.environmentSubscriptionId, me.includeWarningConfig, me.includeWarningConfig.key);
      } else {
        promiseError = this.dataService.createSLAConfig(me.errorConfig);
        promiseWarning = this.dataService.createSLAConfig(me.warningConfig);
        promiseSource = this.dataService.createSLAConfig(me.includeWarningConfig);
      }

      promiseError.then(errorConfig => {
        promiseWarning.then(slaWarningConfig => {
          promiseSource.then (slaSourceConfig => {
            this.showloadingIndicator = false;
            this.snackbar.open(`SLA Threshold configuration sent`, '', {
              panelClass: "mysnackbar",
              horizontalPosition: "center",
              duration: 2000
            });
            me.errorConfig = new SLAConfig();
            me.warningConfig = new SLAConfig();
            me.includeWarningConfig = new SLAConfig();
            me.includeWarningConfig.key = 'SLA_Include_Warnings';
            me.warningConfig.key = 'SLA_Warning_Threshold';
            me.errorConfig.key = 'SLA_Error_Threshold';

          }).catch(reason => {
            this.showloadingIndicator = false;
            this.showErrorDialog(reason, 'Error');
          });
        }).catch(reason => {
          this.showloadingIndicator = false;
          this.showErrorDialog(reason, 'Error');
        });
      }).catch(reason => {
        this.showloadingIndicator = false;
        this.showErrorDialog(reason, 'Error');
      });
      me.existingSLAConfig = true;
  }

  deleteSLAThreshold() {
    const me = this;
    me.showloadingIndicator = true;
    const errorPromise = me.dataService.deleteSLAConfig(me.environmentSubscriptionId, me.errorConfig.key);
    const warningPromise = me.dataService.deleteSLAConfig(me.environmentSubscriptionId, me.warningConfig.key);
    const includeWarningPromise = me.dataService.deleteSLAConfig(me.environmentSubscriptionId, me.includeWarningConfig.key);

    errorPromise.then(slaErrorConfig => {
      warningPromise.then(slaWarningConfig => {
        includeWarningPromise.then(includeWarningConfig => {
          me.showloadingIndicator = false;
          me.existingSLAConfig = false;
          me.snackbar.open(`SLA Threshold configuration deleted`, '', {
            panelClass: "mysnackbar",
            horizontalPosition: "center",
            duration: 2000
          });
          me.thresholdSource = null;
          me.warningFormControl.setValue(null);
          me.errorFormControl.setValue(null);
          me.errorConfig = new SLAConfig();
          me.warningConfig = new SLAConfig();
          me.includeWarningConfig = new SLAConfig();
          me.includeWarningConfig.key = 'SLA_Include_Warnings';
          me.warningConfig.key = 'SLA_Warning_Threshold';
          me.errorConfig.key = 'SLA_Error_Threshold';
        }).catch(reason => {
          this.showErrorDialog(reason, 'Error');
        });
      }).catch(reason => {
        this.showErrorDialog(reason, 'Error');
      });
    }).catch(reason => {
      this.showErrorDialog(reason, 'Error');
    });
  }

  validateSave() {
    let valid = true;
     if (!this.environmentSubscriptionId || !this.thresholdSource) {
      valid = false;
    }
    if (this.warningFormControl.errors || !this.warningFormControl.value || this.errorFormControl.errors || !this.errorFormControl.value) {
      valid = false;
    }
    return valid;
  }

  validateDelete() {
    let valid = true;
     if (!this.environmentSubscriptionId || !this.thresholdSource) {
      valid = false;
    }
    if (this.warningFormControl.errors || !this.warningFormControl.value || this.errorFormControl.errors || !this.errorFormControl.value) {
      valid = false;
    }
    if (!this.existingSLAConfig) {
      valid = false;
    }
    return valid;
  }

  disabled() {       
    if (this.environmentSubscriptionId !== null && this.environmentSubscriptionId !== undefined) {
      
      this.dialogFormGroup.get('sourceFormControl').enable();
      this.warningFormGroup.get('warningFormControl').enable()
      this.warningFormGroup.get('errorFormControl').enable();
      return false;
    } else {
      
      this.dialogFormGroup.get('sourceFormControl').disable();
      this.warningFormGroup.get('warningFormControl').disable();
      this.warningFormGroup.get('errorFormControl').disable()
      
      return true;
    }
        
  }


  showErrorDialog(message: string, title: string) {
    const errorDialog = this.dialog.open(ErrorDialogComponent, {
      width: '400px',
      data: { message, title },
      disableClose: true
    });
  }

}
