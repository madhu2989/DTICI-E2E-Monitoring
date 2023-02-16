import { Component, OnInit} from '@angular/core';
import { Router } from '@angular/router';
import { AdalService } from 'adal-angular4';
import { SecretService } from './shared/services/secret.service';
import { MatDialog } from '@angular/material/dialog';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  title = 'Providence Monitoring Help Center ';
  currentUser = null;
  currentUserName = 'Not logged in.';
  environmentName = 'Loading...';
  notificationsButtonActive = false;
  itemCount: number;

  ngOnInit(): void {
    this.adalService5.handleWindowCallback();
    this.adalService5.getUser();
    this.currentUser = this.adalService5.userInfo;

    if (this.currentUser !== null && this.currentUser.profile !== null) {
      this.currentUserName = this.currentUser.profile.name;
    }

  }

  constructor(
    private adalService5: AdalService,
    private secretService: SecretService,
    private router: Router,
    public dialog: MatDialog
  ) {
    this.adalService5.init(this.secretService.adalConfig);
  }
}

