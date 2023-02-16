import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot, RouterStateSnapshot, ActivatedRoute } from '@angular/router';
import { VanNodeService } from '../services/van-node.service';
import { NodeBase } from '../model/node-base';

@Injectable({
  providedIn: "root",
})
export class NodeDataResolverService implements Resolve<NodeBase>  {

  rootNodePath: string[] = [];
  elementId: string;
  rootNode: NodeBase;

  constructor(private vanNodeService: VanNodeService, private router: Router,
    private route: ActivatedRoute) { }

  resolve(routeSnapshot: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<NodeBase> {
    this.rootNodePath.length = 0;
    for (let i = 0; i < routeSnapshot.url.length; i++) {
      this.rootNodePath.push(routeSnapshot.url[i].path);
    }

    return this.vanNodeService.getNodeByPath(this.rootNodePath).then((result) => {

      if (result !== null) {

        //    return this.elementId = result.elementId;
        return this.rootNode = result;

      } else { // id not found
        this.router.navigate(['/dashboard'], { queryParamsHandling: 'preserve' });
        return null;
      }

    });


  }
}
