import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { AuthHttp } from '../../app/shared/services/authHttp.service';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { environment } from '../../environments/environment';


@Component({
  selector: 'app-help-center-content',
  templateUrl: './help-center-content.component.html',
  styleUrls: ['./help-center-content.component.scss']
})
export class HelpCenterContentComponent implements OnInit, AfterViewInit {
  public theBoundCallback: Function;
  private fragment: string;
  private pathData: string;

  @ViewChild('dataContainer') dataContainer: ElementRef;

  constructor(private http: AuthHttp, private route: ActivatedRoute, private router: Router) { }

  ngOnInit() {
    this.fragment = '';
    this.pathData = '';
    this.theBoundCallback = this.getContentTemplate.bind(this);
    this.route.fragment.subscribe(fragment => { this.fragment = fragment; });
    this.pathData = this.route.routeConfig.data['path'];
    this.router.events
      .subscribe((event) => {
        if (event instanceof NavigationEnd) {
          if (this.pathData && this.pathData.length > 0) {
            this.getContentTemplate(this.pathData);
          }
        }
      });
  }

  ngAfterViewInit(): void {
    if (this.pathData) {
      this.getContentTemplate(this.pathData);
    }
  }

  public getContentTemplate(templateName) {
    const path = environment.templatePath + templateName + '.html';
    this.http.get(path, { responseType: 'text' }).toPromise()
      .then((data) => {
        this.loadData(data);
      });
  }

  loadData(data) {
    this.dataContainer.nativeElement.innerHTML = data;
    if (this.fragment && this.fragment.length > 0) {
      setTimeout(() => {
        try {
          document.querySelector('#' + this.fragment).scrollIntoView();
        } catch (e) { }
      }, 100);
    }
  }
}
