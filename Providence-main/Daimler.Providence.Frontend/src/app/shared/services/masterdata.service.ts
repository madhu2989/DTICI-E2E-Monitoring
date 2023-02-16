import { timeout } from 'rxjs/operators';
import { HttpHeaders, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthHttp } from './authHttp.service';
import { NodeBase } from '../model/node-base';
import { EnvironmentService } from '../services/config.service'
import { Environment } from "../model/environment-config";

export enum ElementType {
    ENVIRONMENT = 'environments',
    SERVICE = 'services',
    ACTION = 'actions',
    COMPONENT = 'components',
    CHECK = 'checks'
}

@Injectable({
    providedIn: "root",
  })
export class MasterDataService {

    protected headers = new HttpHeaders().append(
        'Content-Type', 'application/json'
    );
    protected baseUrl: string = "";//environment.dataEndpoint + '/masterdata';
    protected serviceName = "MasterDataService";
    protected defaultTimeout = 60000;
    private environmentConfig: Environment = null;
    constructor(protected http: AuthHttp,envService:EnvironmentService) {
        this.environmentConfig= envService.getConfiguration(); 
        if(this.environmentConfig !=null)
            this.baseUrl = this.environmentConfig.dataEndpoint + '/masterdata';    
    }


    public getAll(elementType: ElementType, subscriptionId: string = null, requestTimeout: number = this.defaultTimeout): Promise<NodeBase[]> {
        console.log(`${this.serviceName}: getAll ${elementType}`);

        const url = `${this.baseUrl}/${elementType}`;

        let queryParams = new HttpParams();
        if (subscriptionId !== null) {
            queryParams = queryParams.append('environmentSubscriptionId', subscriptionId);
        }

        return this.http.get(url, {
            headers: this.headers,
            params: queryParams,
            observe: 'response'
        })
            .pipe(timeout(requestTimeout))
            .toPromise()
            .then(
                response => {
                    return response.body as NodeBase[];
                }, (error: HttpErrorResponse) => {

                    return Promise.reject(error || error.message);
                });

    }

    public delete(elementType: ElementType, subscriptionId: string, elementId: string, requestTimeout: number = this.defaultTimeout): Promise<void> {
        console.log(`Deleting ${elementType} '${elementId}'`);
        const url = `${this.baseUrl}/${elementType}`;

        let queryParams = new HttpParams();
        queryParams = queryParams.append('environmentSubscriptionId', subscriptionId);
        queryParams = queryParams.append('elementId', elementId);

        return this.http.delete(url, { headers: this.headers, params: queryParams, observe: 'response' })
            .pipe(timeout(requestTimeout))
            .toPromise()
            .then(res => {

                return res;
            },
                (error: HttpErrorResponse) => {

                    return Promise.reject(error.error || error);
                }
            )
            .catch(this.handleError.bind(this));
    }

    public create(elementType: ElementType, data: any, subscriptionId: string, requestTimeout: number = this.defaultTimeout): Promise<NodeBase> {
        const url = `${this.baseUrl}/${elementType}`;

        return this.http
            .post(url, JSON.stringify(data), { headers: this.headers, observe: 'response' })
            .pipe(timeout(requestTimeout))
            .toPromise()
            .then(res => {
                return res;
            },
                (error: HttpErrorResponse) => {
                    return Promise.reject(error.error || error);
                }
            )
            .catch(this.handleError.bind(this));
    }

    public update(elementType: ElementType, data: any, subscriptionId: string, requestTimeout: number = this.defaultTimeout): Promise<NodeBase> {
        const url = `${this.baseUrl}/${elementType}`;

        let queryParams = new HttpParams();
        queryParams = queryParams.append('environmentSubscriptionId', subscriptionId);
        queryParams = queryParams.append('elementId', data.elementId);

        return this.http
            .put(url, JSON.stringify(data), { headers: this.headers, params: queryParams, observe: 'response' })
            .pipe(timeout(requestTimeout))
            .toPromise()
            .then(() => {
                return data;
            }
                , (error: HttpErrorResponse) => {
                    return Promise.reject(error.error || error);
                }
            )
            .catch(this.handleError.bind(this));
    }

    protected handleError(error: HttpErrorResponse) {
        if (error instanceof ErrorEvent) {
            let err:ErrorEvent = error;
            return Promise.reject(err.message || err);
        } else {
            return Promise.reject(error.error || error);
        }
    }
}
