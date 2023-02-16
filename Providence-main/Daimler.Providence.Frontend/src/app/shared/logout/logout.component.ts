import { Component, OnInit } from '@angular/core';
import { AdalService } from "adal-angular4";

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.scss']
})
export class LogoutComponent implements OnInit {

  isAuthenticated = false;

  constructor(
    private adalService: AdalService
  ) {
    this.isAuthenticated = this.adalService.userInfo.authenticated;
  }

  ngOnInit() {
    if (this.isAuthenticated) {
      this.adalService.logOut();
    }
  }

}
