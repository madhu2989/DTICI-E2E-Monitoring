import { Component, OnInit, Input } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { environment } from '../../environments/environment';


@Component({
  selector: 'app-navigation',
  templateUrl: '../../assets/help/navigation.component.html',
  styleUrls: ['./navigation.component.scss']
})
export class NavigationComponent implements OnInit {

  @Input()
  public myCallback: Function;

    getContentTemplate(templateName) {
      this.myCallback(templateName);
    }

  constructor(private route: ActivatedRoute, private router: Router) { }

  ngOnInit() {  }

}


