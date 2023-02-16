import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AdalService } from "adal-angular4";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private adalService: AdalService
  ) { }

  ngOnInit() {
    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';

    if (this.adalService.userInfo.authenticated) {
      this.router.navigate([returnUrl], { queryParamsHandling: 'preserve' });
    } else {
      this.adalService.config.redirectUri = window.location.origin + returnUrl;
      this.adalService.login();
    }
  }

}
