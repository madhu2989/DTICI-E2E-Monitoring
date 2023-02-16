import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from '../assets/help/app-routing.module';
import { AdalService } from 'adal-angular4/adal.service';
import { SecretService } from './shared/services/secret.service';

import { SharedModule } from './shared/shared.module';
import { MaterialModule } from './material.module';
import { AppComponent } from './app.component';
import { NavigationComponent } from './navigation/navigation.component';
import { HelpCenterContentComponent } from './help-center-content/help-center-content.component';


@NgModule({
  declarations: [
    AppComponent,
    NavigationComponent,
    HelpCenterContentComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    SharedModule,
    MaterialModule
  ],
  providers: [AdalService, SecretService],
  bootstrap: [AppComponent]
})
export class AppModule { }
