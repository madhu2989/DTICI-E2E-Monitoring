import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { SettingsService } from './settings.service';
import { DeploymentWindow } from '../model/deployment-window';
import { DeploymentBlock } from '../model/deployment-block';

@Injectable({
  providedIn: 'root'
})
export class DeploymentWindowService {
  offset = this.settingsService.timerange;

  constructor(protected dataService: DataService,
    public settingsService: SettingsService) { }


  public getDeploymentWindowsData(environmentName: string, elementId: string): Promise<object[]> {
    const me = this;
      
    
    return me.dataService.getDeploymentWindows(environmentName).then((result) => {


      let allComponents = me.getComponentElementIdsMapPerEnvironment(environmentName);
      
      let filteredDeployments = me.filterDeployments(elementId, allComponents, result);


      return me.processDeploymentWindowsData(filteredDeployments);

    });
    
  }


  public getComponentElementIdsMapPerEnvironment(environmentSubscriptionId: string): Map<string, string[]> {
    const me = this;
    
   return me.dataService.getComponentElementIdsMapPerEnvironment(environmentSubscriptionId); 
  }

  /* Filters the deployments, returns the relevant deployments to a component
  */
  public filterDeployments(componentId: string, componentMap: Map<string, string[]>, deployments: DeploymentWindow[] ): DeploymentWindow[] {
      
      if (componentMap && componentMap.size > 0) {
  
        let includedComponents = componentMap.get(componentId.toUpperCase());

        if (typeof includedComponents === "string" || includedComponents instanceof String) {
            includedComponents = []; 
            includedComponents.push(componentMap.get(componentId.toUpperCase()).toString());
        }
        

        let includedDeployments : DeploymentWindow[] = []; 

        // watch if the includedComponent is included in the list of deployments
        for (const deployment of deployments) {
          
          // we need to add also the environent if not set in the deployment itself
          if (!includedComponents.includes(deployment["environmentSubscriptionId"])) {
            includedComponents.push(deployment["environmentSubscriptionId"]); 
          }
          
          if (includedComponents) {
            for (const includedComponent of includedComponents) {
              
              let elementIds: string[] = deployment["elementIds"];

              if (typeof elementIds === "string" || elementIds instanceof String) {
                  elementIds = [];
                  elementIds.push(deployment["elementIds"].toString());
              }

              if (elementIds.includes(includedComponent) && !includedDeployments.includes(deployment)) {
               includedDeployments.push(deployment);
              }

          }
          } 
          


        }
        return includedDeployments;
       
      } else {
        return [];
      }

     
  }




  private buildWindowBlocks(deploymentWindows: DeploymentWindow[]): DeploymentBlock[] {
    const me = this;
    
    var blocksList: DeploymentBlock[] = [];
    
    if (deploymentWindows && deploymentWindows.length > 0) {

      for (let i = 0; i < deploymentWindows.length; i++) {

        if (blocksList.length == 0) {
          // Build first block 
          var block = new DeploymentBlock();
          block.startDate = deploymentWindows[i]['startDate'];
          block.endDate = deploymentWindows[i]['endDate'];
          block.environmentName = deploymentWindows[i]['environmentName'];
          block.environmentSubscriptionId = deploymentWindows[i]['environmentSubscriptionId']
  
          block.deployments = [];
          block.deployments.push(deploymentWindows[i]);
  
          blocksList.push(block);
        } else {
          // walk through blocks
          if (blocksList.length > 0) {
            var added = false;
            for (let j = 0; j < blocksList.length; j++) {
          
               // cases where endDate is not null

               // neues Deployment ist mitten in einem Block
               if (deploymentWindows[i]['startDate'] > blocksList[j]['startDate'] && deploymentWindows[i]['startDate'] < blocksList[j]['startDate']) {
                  blocksList[j].deployments.push(deploymentWindows[i]);
                  added = true;
                  break; 
               }

               // neues Deployment Erweitert Block nach vorne *2
               if (deploymentWindows[i]['startDate'] < blocksList[j]['startDate'] && 
                  deploymentWindows[i]['endDate'] >= blocksList[j]['startDate'] && deploymentWindows[i]['endDate'] <= blocksList[j]['startDate']) {
                    blocksList[j]['startDate'] = deploymentWindows[i]['startDate'];
                    blocksList[j].deployments.push(deploymentWindows[i]);
                    added = true;
                    break; 
               }

               // neues Deployment Erweitert Block nach hinten*1, *2
               if ((deploymentWindows[i]['endDate'] > blocksList[j]['endDate'] || deploymentWindows[i]['endDate'] == null  ) 
               && deploymentWindows[i]['startDate'] >= blocksList[j]['startDate'] && deploymentWindows[i]['startDate'] <= blocksList[j]['startDate']) {
                blocksList[j]['endDate'] = deploymentWindows[i]['endDate'];
                blocksList[j].deployments.push(deploymentWindows[i]);
                added = true;
                break; 
               }

               // Neues Deployment includiert Block(Erweitert nach Vorne und Hinten) *1 *2
               if ( (deploymentWindows[i]['endDate'] > blocksList[j]['endDate'] || deploymentWindows[i]['endDate'] == null) && deploymentWindows[i]['startDate'] < blocksList[j]['startDate']) {
                blocksList[j]['endDate'] = deploymentWindows[i]['endDate'];
                blocksList[j]['startDate'] = deploymentWindows[i]['startDate'];
                blocksList[j].deployments.push(deploymentWindows[i]);
                added = true;
                break; 
               }


            } // end of for blockList
          
            // no existing block found add a new one
            if (!added) {
              var block = new DeploymentBlock();
              block.startDate = deploymentWindows[i]['startDate'];
              block.endDate = deploymentWindows[i]['endDate'];
              block.environmentName = deploymentWindows[i]['environmentName'];
              block.environmentSubscriptionId = deploymentWindows[i]['environmentSubscriptionId']
              
              block.deployments = [];
              block.deployments.push(deploymentWindows[i]);
      
              blocksList.push(block);
            }


          }


        } // end of else walk through blocks



      }  // end of for deployments



    }
    
    // sort along the start date
    blocksList.sort(function(a, b){
      return a['startDate'] == b['startDate'] ? 0 : +(a['startDate'] > b['startDate']) || -1;
    });

    // check if 2 blocks overlap
    blocksList = this.merge(blocksList);
        

    return blocksList;
  }



   /* merges deployments to blocks. */
   private merge(blocksList: DeploymentBlock[]): DeploymentBlock[] {
    var newBlocksList = [];
    
    var i = 0;

    while (i < blocksList.length-1) {

       // merge 2 direct neighbour blocks 
       if (blocksList[i]['endDate'] >= blocksList[i+1]['startDate'] || blocksList[i]['endDate'] == null) {
          // overlap -> merge
          var newBlock = new DeploymentBlock();
          newBlock.startDate = blocksList[i]['startDate'];
          newBlock.endDate = blocksList[i+1]['endDate'];
          newBlock.environmentName = blocksList[i]['environmentName'];
          newBlock.environmentSubscriptionId = blocksList[i]['environmentSubscriptionId'];


          newBlock.deployments = [];
          newBlock.deployments = newBlock.deployments.concat(blocksList[i]['deployments']);
          newBlock.deployments = newBlock.deployments.concat(blocksList[i+1]['deployments']);
          
          blocksList[i] = null;
          blocksList[i+1] = newBlock;

        } 
        i++;
    
    }
    
    // Take only Blocks which we did not erase
    for (var j=0; j < blocksList.length; j++) {
      if (blocksList[j] != null) {
        
        // merge shortDescription
        
        if (blocksList[j].deployments.length > 1) {
          blocksList[j]['shortDescription'] = 'Deployments';
        } else {
          blocksList[j]['shortDescription'] = 'Deployment';
        }
        
        
        
        
        
        newBlocksList.push(blocksList[j]);
      }
    }
    
    return newBlocksList;
   }



  
  private processDeploymentWindowsData(deploymentWindows: DeploymentWindow[]): object[] {
    const me = this;
    // const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    const endDate: Date = new Date(Date.now() + parseInt("" + (me.settingsService.timerange * 0.01), 10));
    const startDate: Date = new Date(endDate.valueOf() - me.offset);
    const chartResult: object[] = [];
    if (deploymentWindows && deploymentWindows.length > 0) {
      
      var deploymentBlocks = me.buildWindowBlocks(deploymentWindows);
      
      for (let i = 0; i < deploymentBlocks.length; i++) {
        if (deploymentBlocks[i]['startDate'] && deploymentBlocks[i]['endDate']) {

          const eventStartDate = new Date(deploymentBlocks[i]['startDate']);
          const eventEndDate = new Date(deploymentBlocks[i]['endDate']);

          chartResult.push(
            [
              eventStartDate.valueOf(),
              "Deploying",
              eventEndDate.valueOf(),
              deploymentBlocks[i]['shortDescription'] ? deploymentBlocks[i]['shortDescription'] : "Deployments",
              deploymentBlocks[i]
            ]
          );

        } else if (deploymentBlocks[i]['startDate'] && (deploymentBlocks[i]['endDate'] === null)) {

          const eventStartDate = new Date(deploymentBlocks[i]['startDate']);
          const eventEndDate = endDate;

          chartResult.push(
            [
              eventStartDate.valueOf(),
              "Deploying",
              eventEndDate.valueOf(),
              deploymentBlocks[i]['shortDescription'] ? deploymentBlocks[i]['shortDescription'] : "Deployments",
              deploymentBlocks[i]
            ]
          );

        }



      }
    }

    return chartResult;
  }

}
