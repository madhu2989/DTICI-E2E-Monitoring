# HelpCenter

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 1.6.0-rc.2.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory. Use the `-prod` flag for a production build.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via [Protractor](http://www.protractortest.org/).

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI README](https://github.com/angular/angular-cli/blob/master/README.md).


## to create the Help-Center 

### the structure of Wiki which is needed
directory: ScaledPilotTestframework.wiki
this includes 
* folder .attachments which includes all images, jsons,...
* file Help-Center.md which is the file for starting point 
* folder Help-Center
  * .order file which contains the navigation order
  * several md files 
  * from here any deeply nested folder structures can be built, with every folder contains a .order file and md files


The ScaledPilotTestframework.wiki directory lies on the first level of Daimler.Providence.Help-Center, but input path can also be configured in package.json


first run 'npm run create-help-center-templates'
then run 'ng serve'


### how to link into view and scroll to headline
/Help-Center/Notification-Panel#id-of-headline

Out of the application
<a [routerLink]="['/Help-Center/Notification-Panel']" fragment="id-of-headline">Route with Fragment </a>
