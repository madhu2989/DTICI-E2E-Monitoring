import { Injectable } from "@angular/core";
import { DataService } from "./data.service";
import { HttpErrorResponse } from "@angular/common/http";

@Injectable({
  providedIn: "root",
})
export class SystemStateService {
    constructor(protected dataService: DataService) { }

  public getEnvironmentLogSystemState(env: string): Promise<string> {
    return this.dataService
      .getAllEnvironments(false)
      .then(
        response => {
            if (response && response.length > 0) {
            for (let i = 0; i < response.length; i++) {
                if (response[i].name === env) {
                return response[i].logSystemState;
                }
            }
            } else {
            return null;
            }
                },
                (error: HttpErrorResponse) => {
                return Promise.reject(error.message || error);
                }
            );
        }
}