

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { DataService } from "../services/data.service";
import { DeploymentWindow } from '../model/deployment-window';
import { HttpErrorResponse } from '@angular/common/http';


@Injectable({
  providedIn: "root",
})
export class HistoryResolver implements Resolve<object[]> {

  constructor(private dataService: DataService, private router: Router) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<object[]> {
    const environmentId = route.paramMap.get('environmentId');
    const forceReload = false;
    const elementId = undefined;
    const loadExtendedModel = false;

    const historyOfElementIdPromise = this.dataService.getHistoryOfElementId(forceReload, environmentId, elementId, loadExtendedModel);
    const deploymentWindowsPromise = this.dataService.getDeploymentWindows(environmentId);

    return Promise.all([historyOfElementIdPromise, deploymentWindowsPromise]).then((result) => {

      if (result.length === 2) {
        const historyOfElementIdObject = result[0];

        if (historyOfElementIdObject) {
          return historyOfElementIdObject;
        } else { // id not found
          this.router.navigate(['/dashboard'], { queryParamsHandling: 'preserve' });
          return null;
        }

      }

    }, (error: HttpErrorResponse) => {
      return Promise.reject(error.message || error);
    });

  }
}