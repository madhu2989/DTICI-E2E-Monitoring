import { Injectable } from "@angular/core";
import { DataService } from "./data.service";
import { NodeBase } from "../model/node-base";
import { HttpErrorResponse } from "@angular/common/http";

@Injectable({
  providedIn: "root",
})
export class VanNodeService {

  constructor(protected dataService: DataService) { }

  public getNodeByPath(nodePath: string[]): Promise<NodeBase> {
    return this.dataService
      .getAllEnvironments(false)
      .then(
        response => {
          let startNode: NodeBase[] = response;
          let found: boolean;
          const lastPathElement = decodeURIComponent(nodePath[nodePath.length - 1]);
          if(nodePath.length>1)
          if(nodePath.length>1 && nodePath[0]==="dashboard")
          nodePath.shift();
          for (let i = 0; i < nodePath.length; i++) {
            found = false;
            for (let j = 0; j < startNode.length; j++) {
              const decodedPath = decodeURIComponent(nodePath[i]);
              if (startNode[j].name === decodedPath) {
                if (decodedPath === lastPathElement) {
                  return startNode[j];
                }
                startNode = startNode[j].getChildNodes();
                found = true;
                break;
              }
            }
            if (!found) {
              return null;
            }
          }

          return null;
        },
        (error: HttpErrorResponse) => {
          return Promise.reject(error.message || error);
        }
      );
  }

  public getNodeByElementId(environmentName: string, elementId: string): Promise<NodeBase> {
    return this.dataService
      .getEnvironment(environmentName, false)
      .then(
        environment => {
          return this.getNodeByElementIdRecursive(environment, elementId);
        },
        (error: HttpErrorResponse) => {
          return Promise.reject(error.message || error);
        }
      );
  }


  private getNodeByElementIdRecursive(startNode: NodeBase, elementId: string): NodeBase {
    if (startNode.elementId.toLowerCase() === elementId.toLowerCase()) {
      return startNode;
    } else {
      if (startNode.getChildNodes) {
        const childNodes = startNode.getChildNodes();
        if (childNodes) {
          for (let childIndex = 0; childIndex < childNodes.length; childIndex++) {
            const result = this.getNodeByElementIdRecursive(childNodes[childIndex], elementId);
            if (result) {
              return result;
            }
          }
        }
      }

      if (startNode.checks) {
        for (let checksIndex = 0; checksIndex < startNode.checks.length; checksIndex++) {
          const result = this.getNodeByElementIdRecursive(startNode.checks[checksIndex], elementId);
          if (result) {
            return result;
          }
        }
      }
    }

    return null;
  }

  public getPathByElementId(startNode: NodeBase, elementId: string, parentNodes: NodeBase[]): NodeBase[] {
    parentNodes.push(startNode);
    if (startNode.elementId.toLowerCase() === elementId.toLowerCase()) {
      return parentNodes;
    } else if (startNode.getChildNodes) {

      const childNodes = startNode.getChildNodes();
      if (childNodes) {
        for (let childIndex = 0; childIndex < childNodes.length; childIndex++) {

          const result = this.getPathByElementId(childNodes[childIndex], elementId, parentNodes);
          if (result) {
            return parentNodes;
          } else {

          }
        }
      }
    }
    parentNodes.pop();
    return null;
  }


  public getNodeChildStatesSummary(elementId: string, envName: string): Promise<Object> {
    let stateOk = 0;
    let stateWarn = 0;
    let stateError = 0;
    let grandChildStateOk = 0;
    let grandChildStateWarn = 0;
    let grandChildStateError = 0;
    let rootNode: NodeBase;
    let childNodes: NodeBase[];
    let grandChildNodes: NodeBase[];

    return this.getNodeByElementId(envName, elementId).then((result) => {
      rootNode = result;

      childNodes = rootNode.getChildNodes();

      const childStatesSummary = new Object([
        {
          "name": "childStates",
          "series": [
            {
              "name": "OK",
              "value": 0
            },
            {
              "name": "WARNING",
              "value": 0
            },
            {
              "name": "ERROR",
              "value": 0
            }
          ]
        },
        {
          "name": "grandChildNodes",
          "series": [
            {
              "name": "OK",
              "value": 0
            },
            {
              "name": "WARNING",
              "value": 0
            },
            {
              "name": "ERROR",
              "value": 0
            }
          ]
        }]
      );

      if (childNodes && childNodes.length > 0) {

        for (let i = 0; i < childNodes.length; i++) {
          if (childNodes[i].state.state === "OK") {
            stateOk++;
          }
          if (childNodes[i].state.state === "WARNING") {
            stateWarn++;
          }
          if (childNodes[i].state.state === "ERROR") {
            stateError++;
          }

          grandChildNodes = childNodes[i].getChildNodes();
          if (grandChildNodes && grandChildNodes.length > 0) {

            for (let j = 0; j < grandChildNodes.length; j++) {
              if (grandChildNodes[j].state.state === "OK") {
                grandChildStateOk++;
              }
              if (grandChildNodes[j].state.state === "WARNING") {
                grandChildStateWarn++;
              }
              if (grandChildNodes[j].state.state === "ERROR") {
                grandChildStateError++;
              }
            }
          }

        }

        childStatesSummary[0]["series"][0].value = stateOk;
        childStatesSummary[0]["series"][1].value = stateWarn;
        childStatesSummary[0]["series"][2].value = stateError;

        childStatesSummary[1]["series"][0].value = grandChildStateOk;
        childStatesSummary[1]["series"][1].value = grandChildStateWarn;
        childStatesSummary[1]["series"][2].value = grandChildStateError;

        return childStatesSummary;
      } else {
        return childStatesSummary;
      }

    }
    );

  }
}