import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoggedInGuard } from './logged-in.guard';
import { AuthHttp } from './services/authHttp.service';
import { HttpClientModule } from '@angular/common/http';
import { LoginComponent } from './login/login.component';
import { LogoutComponent } from './logout/logout.component';
import { RouterModule } from '@angular/router';

@NgModule({
  imports: [
    CommonModule,
    HttpClientModule,
    RouterModule
  ],
  entryComponents: [ LoginComponent, LogoutComponent ],
  declarations: [ LoginComponent, LogoutComponent ],
  providers: [ LoggedInGuard, AuthHttp, LoginComponent, LogoutComponent ],
  exports: [ LoginComponent, LogoutComponent ]
})
export class SharedModule { }
