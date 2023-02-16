import { Component, OnInit, ViewEncapsulation, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, RoutesRecognized, ActivationEnd } from '@angular/router';
import { BreadCrumb } from './breadcrumb';
import { filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-breadcrumb',
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.scss']
})
export class BreadcrumbComponent implements OnInit {

  public breadcrumbs: BreadCrumb[] = null;
  private subscription: Subscription;

  tooltips: string[] = [
    "Dashboard",
    "Environment",
    "Service",
    "Action",
    "Component"
  ];

  icons: string[] = [
    "",
    "public",
    "category",
    "directions_run",
    "extensions"
  ];

  // Build your breadcrumb starting with the root route of your current activated route
  constructor(private route: ActivatedRoute,
    private router: Router) {
  }

  ngOnInit() {

    this.subscription = this.router.events.subscribe((val) => {
      if (val instanceof ActivationEnd) {
        if (val) {
          const routeParams = val.snapshot.params;
          const routePath = val.snapshot.routeConfig ? ('/' + val.snapshot.routeConfig.path).split("/:") : '';
          this.breadcrumbs = this.buildBreadCrumb(routeParams, routePath);
          
        }
      }
    });

  }


  buildBreadCrumb(params, routePath): Array<BreadCrumb> {
    const breadcrumbs: BreadCrumb[] = [];
    const pathArray = routePath;

    let path = '';

    for (let i = 1; i < pathArray.length; i++) {
      path = path + '/' + params[pathArray[i]];

      const breadcrumb: BreadCrumb = {
        label: decodeURIComponent(params[pathArray[i]]),
        url: this.decode(path),
        icon: this.icons[i],
        tooltip: this.tooltips[i]
      };

      
      breadcrumbs.push(breadcrumb);

    }
    return breadcrumbs;
  }


  decode(url: string) {
    
    let newUrl = decodeURIComponent(url);
    newUrl = newUrl.replace('%2F','/');
    
    return newUrl;
  }


  redirect(url: string) {
    
    if (url === '/') {
      this.router.navigate(['/dashboard'], { queryParamsHandling: 'preserve' });
    } else {
      this.router.navigate([url], { queryParamsHandling: 'preserve' });
    }
  }

  OnDestroy() {
    this.subscription.unsubscribe();
  }

}