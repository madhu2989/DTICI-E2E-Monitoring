import { Component, OnInit, Inject } from '@angular/core';
import { VanEnvironment } from '../../shared/model/van-environment';
import { SLAReport } from '../../shared/model/sla-report';
import { DataService, IElementSearchResult, Element } from '../../shared/services/data.service';
import { BaseComponent } from '../../shared/base-component/base.component';
import { ErrorDialogComponent } from '../../shared/dialogs/error-dialog/error-dialog.component';
import { SlaChartDialogComponent } from '../../shared/dialogs/sla-chart-dialog/sla-chart-dialog.component';
import { CancelRequestDialogComponent } from '../../shared/dialogs/cancel-request-dialog/cancel-request-dialog.component';
import { ConfirmationDialogComponent } from '../../shared/dialogs/confirmation-dialog/confirmation-dialog.component';
import { GridOptions } from 'ag-grid-community';
import { MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DatePipe } from '@angular/common';
import { UntypedFormBuilder, UntypedFormGroup, UntypedFormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { switchMap, debounceTime } from 'rxjs/operators';
import { SLAConfig } from '../../shared/model/sla-config';
import * as pdfMake from 'pdfmake/build/pdfmake';
import * as pdfFonts from 'pdfmake/build/vfs_fonts';
import * as htmlToPdfmake from 'html-to-pdfmake';

@Component({
  selector: 'app-sla-reports',
  templateUrl: './sla-reports.component.html',
  styleUrls: ['./sla-reports.component.scss']
})
export class SlaReportsComponent extends BaseComponent implements OnInit {

  environments: VanEnvironment[] = [];
  id: number;
  startDate: Date;
  endDate: Date;
  environmentSubscriptionId: string;

  searchFrom: UntypedFormGroup;
  timeFormGroup: UntypedFormGroup;
  formControlStartDate: UntypedFormControl;
  formControlEndDate: UntypedFormControl;
  formControlEnvironmentName: UntypedFormControl;

  private gridApi;
  private gridColumnApi;
  public columnDefs;
  public rowSelection;
  public gridOptions: GridOptions; 
  rowData: SLAReport[] = [];
  showloadingIndicator = false;
  presentationType: string;
  downloadEnabled: boolean;
  chosenElementId: string;
  searchedElementId: string;

  filteredElements: Observable<IElementSearchResult>;

  warningThreshold: number;
  errorThreshold: number;

  constructor(
    private dataService: DataService,
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialog: MatDialog,
    private datePipe: DatePipe,
    private fb: UntypedFormBuilder,
    private snackbar: MatSnackBar
  ) {
    super(); 

    pdfMake.vfs = pdfFonts.pdfMake.vfs;

    this.downloadEnabled = false;
    this.id = data.id;
    this.startDate = data.startDate;
    this.endDate = data.endDate;
    this.environmentSubscriptionId = data.environmentSubscriptionId;   
    this.formControlStartDate = new UntypedFormControl('', []);
    this.formControlEndDate = new UntypedFormControl('', []);
    this.formControlEnvironmentName = new UntypedFormControl('', []);
    this.timeFormGroup = this.fb.group({
        startDateCtrl: this.formControlStartDate,
        endDateCtrl: this.formControlEndDate
    }, {});
    this.timeFormGroup.get('startDateCtrl').disable();
    this.timeFormGroup.get('endDateCtrl').disable();  
    this.presentationType = 'value';
    this.columnDefs = [
      { headerName: 'Element Name', field: 'elementId', width: 400, sort: 'desc', },
      { headerName: 'Element Type', field: 'elementType', width: 350, suppressSizeToFit: true, },
      { headerName: 'SLA State', field: 'level', width: 200, suppressSizeToFit: true, cellRenderer: this.slaStateCellRenderer },
      { headerName: 'SLA Value (uptime)', field: 'value', width: 250, suppressSizeToFit: true, cellRenderer: this.chartCellRenderer }
    ];

    this.gridOptions = <GridOptions>{
      enableColResize: true,
      cacheOverflowSize: 2,
      onGridReady: function (params) {
        this.onGridReady(params);
      }.bind(this),
      onCellDoubleClicked: this.openChart.bind(this),
      suppressRowClickSelection: true
    };
  }

  slaStateCellRenderer(params) {
    let slaStateString;
    let icon;
    const level = params.data ? params.value : params;
    switch (level) {
      case 0:
        icon = 'check_box';
        slaStateString = 'Ok';
        break;
      case 1:
        icon = 'warning';
        slaStateString = 'Warning';
        break;
      case 2:
        icon = 'error';
        slaStateString = 'Error';
        break;
      default:
        icon = '';
        slaStateString = '-';
        break;
    }
    const div = document.createElement('div');
    div.innerHTML = '<div style="display: flex; flex-direction: row;">' +
      '<i class="material-icons" style="margin-top: 12px">' + icon + '</i>' + slaStateString +
      '</div>';
    return div;
  }

  slaStatePdfRenderer(params) {
    let slaStateString;
    let level;
    if (params === null || params === undefined) {
      level = 4;
    } else {
      level = params.data ? params.value : params;
    }
    switch (level) {
      case 0:
        slaStateString = 'Ok';
        break;
      case 1:       
        slaStateString = 'Warning';
        break;
      case 2:       
        slaStateString = 'Error';
        break;
      default:       
        slaStateString = '-';
        break;
    }
    const div = document.createElement('div');
    div.innerHTML = '<div style="display: flex; flex-direction: row;">' + slaStateString + '</div>';
    return div;
  }

  chartCellRenderer(params) {
    const value = params.value;
    const level = params.data ? params.data.level : params.level;
    const presentationType = params.data ? params.data.presentationType : params.presentationType;
    const warningThreshold = params.data ? params.data.warningThreshold : params.warningThreshold;
    const errorThreshold = params.data ? params.data.errorThreshold : params.errorThreshold;
    const lineChartPoints = params.data ? params.data.lineChartPoints : params.lineChartPoints;

    const warningErrorColor = level === 2 ? '#b71c1c' : '#f57f17';
    const div = document.createElement('div');
    if (value !== null) {
      if (presentationType === 'pieChart') {
        div.setAttribute('style', 'width: 100%; height: 100%; text-align: left;');
        const htmlSvg = '<svg viewBox="0 0 64 64" style="width: 40px; margin-top: 0px; background:' + warningErrorColor +
        '; border-radius: 50%; transform: rotate(-90deg);"><circle r="25%" cx="50%" cy="50%" style="fill: none; stroke: #1b5e20; stroke-dasharray:' + (value) +
          ' 100; stroke-width: 33;" shape-rendering="geometricPrecision"/></svg>';
        const htmlValue = value + '%';
        div.innerHTML = htmlSvg + htmlValue;
      } else if (presentationType === 'lineChart') {
        const warningLineCoordinateY = 30 - (warningThreshold * 0.3);
        const errorLineCoordinateY = 30 - (errorThreshold * 0.3);
        const errorLineCoordinates: string[] = ['0,' + errorLineCoordinateY, '150,' + errorLineCoordinateY];
        const warningLineCoordinates: string[] = ['0,' + warningLineCoordinateY, '150,' + warningLineCoordinateY];
        const points = lineChartPoints;
        const lineChartxAxisKey = 120 / points.length;
        const lineChartCoordinates: string[] = [];
        let coordinateString;
        for (let i  = 0; i < points.length; i++) {
          let xCoordinate = parseInt(points[i].split(", ")[0], 10);
          let yCoordinate = parseInt(points[i].split(", ")[1], 10);
          xCoordinate = xCoordinate * lineChartxAxisKey;
          yCoordinate = 30 - (yCoordinate * 0.3);
          coordinateString = xCoordinate + ',' + yCoordinate;
          lineChartCoordinates.push(coordinateString);
        }
        div.setAttribute('style', 'width: 70%; height: 30%; text-align: left;');
        div.innerHTML = '<svg viewBox="0 0 110 30" width="100px" height="30px" style="width: 110px; height: 30px; margin-top: 10px; margin-bottom: 5px; border-left: 1px solid #555; border-bottom: 1px solid #555; padding-top: 3px;">' +
          '<polyline fill="none" stroke="#0074d9" stroke-width="1" points="' + lineChartCoordinates + '"/>' +
          '<polyline fill="none" stroke="#ff0000" stroke-width="1" stroke-dasharray="2,2" points="' + errorLineCoordinates + '"/>' +
          '<polyline fill="none" stroke="#ff6600" stroke-width="1" stroke-dasharray="2,2" points="' + warningLineCoordinates + '"/>' +
          '</svg>';
      } else {
        div.innerHTML = value + '%';
      }
    } else {
      div.innerHTML = '-';
    }
    return div;
  }

  ngOnInit() {
    const me = this;
    me.dataService.getAllEnvironments(false).then(function (result) {
      if (result && result.length > 0) {
        for (let i = 0; i < result.length; i++) {
          me.environments.push(result[i]);
        }
      }
    });
    this.searchFrom = this.fb.group({
      elementIdInput: null
    });
    this.filteredElements = this.searchFrom.get('elementIdInput').valueChanges
      .pipe(
        debounceTime(300),
        switchMap(value => this.dataService.getElementsPerEnvironment(this.environments.find(env => env.subscriptionId === this.environmentSubscriptionId).name, { name: value }, 1))
      );
  }

  displayFn(element: Element) {
    if (element) { return element.name; }
  }

  confirmLoading() {
    const me = this;
    me.dataService.getSLAConfig(me.environmentSubscriptionId).then(function (slaConfigResponse: SLAConfig[]) {
      if (slaConfigResponse && slaConfigResponse.length > 0) {
        if(me.presentationType !== 'lineChart'){
          me.downloadEnabled = true;
        }
        me.showloadingIndicator = false;
        me.errorThreshold = Number((Number(slaConfigResponse.find(config => config.key === 'SLA_Error_Threshold').value)   * 100).toFixed(2));
        me.warningThreshold   = Number((Number(slaConfigResponse.find(config => config.key === 'SLA_Warning_Threshold').value) * 100).toFixed(2));
      } else {
        me.showloadingIndicator = false;
        me.warningThreshold = 0;
        me.errorThreshold = 0;
        me.snackbar.open(`No SLA Threshold configuration found. Please create a new one`, '', {
          panelClass: "mysnackbar",
          horizontalPosition: "center",
          duration: 8000
        });
      }
    }, (error) => {
      me.showloadingIndicator = false;
      this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error loading configuration for SLA reports");
    });

    const start = new Date(Math.round((this.timeFormGroup.get('endDateCtrl').value)));
    const end = new Date(this.timeFormGroup.get('startDateCtrl').value);
    const dateDiff = (start.getTime() - end.getTime()) / (1000 * 60 * 60 * 24);
    if (dateDiff > 30) {
      const message = "You chose a time interval which is bigger than 30 days. Loading the report for such a big time interval can take several minutes. Are you sure you want to load the report?";
      const title = "Load the report?";
      const action = "Continue";
      const confirmationDialog = this.dialog.open(ConfirmationDialogComponent, {
        width: '400px',
        data: { message, title, action}
      });

      confirmationDialog.afterClosed().toPromise().then(result => {
        if (result) {
          this.setRowData();
        }
      });
    } else {
      this.setRowData();
    }
  }

  setRowData(): void {
    const me = this;
    const startDateTime = this.startDate;
    const endDateTime = this.endDate;
    const chosenStartDate = startDateTime;
    const chosenEndDate = endDateTime;
    setTimeout(() => {
      const dialogRef = this.dialog.open(CancelRequestDialogComponent, {
        width: '300px',
        disableClose: true,
        data: { title: "Loading SLA Reports", dataService: this.dataService }
      });

    me.rowData = [];
    let promise;
    let elementId = me.searchFrom.get('elementIdInput').value;
    if (typeof(elementId) === 'object' && elementId) {
      elementId = elementId.elementId;
    } else {
      if (elementId && elementId !== '') {
        this.dataService.getElementsPerEnvironment(this.environments.find(env => env.subscriptionId === this.environmentSubscriptionId).name, { name: elementId }, 1).subscribe(element => {
          elementId = element.results[0].elementId;
        });
      } else {
        elementId = null;
      }
    }
    if (this.presentationType && this.presentationType === 'lineChart') {
      promise = this.dataService.getSLAReportJobData(me.id, elementId, true);      
      promise.then((slaReports: Object) => {
        if (slaReports) {
          dialogRef.close("loaded");
          for (const [slaReportName, slaReportValue] of Object.entries(slaReports)) {
            let currentSlaReport = new SLAReport();
            currentSlaReport.lineChartPoints = [];
            currentSlaReport.lineChartDates = [];
            
            if (slaReportValue != null) {
              currentSlaReport = this.buildSLAReportForLineChart(currentSlaReport, slaReportValue, chosenStartDate, chosenEndDate);
            }
            currentSlaReport.elementId = slaReportName;
            currentSlaReport.errorThreshold = this.errorThreshold;
            currentSlaReport.warningThreshold = this.warningThreshold;
            currentSlaReport.presentationType = this.presentationType;           
            me.rowData.push(currentSlaReport);
          }
          if (me.gridApi) {
            me.gridApi.setRowData(me.rowData);
          }
        }
      }, (error) => {
        dialogRef.close("error");
        if (error.includes('404')) {
          this.showErrorDialog("Either there is no SLA Threshold configuration for the given environment yet, or the specified Element couldn't be found", "Error loading data");
        } else {
          this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error loading data");
        }
      });

    } else {
      promise = this.dataService.getSLAReportJobData(me.id, elementId, false);
      promise.then((slaReports: Object) => {
        if (slaReports) {
          dialogRef.close("loaded");
          for (const [slaReportName, slaReportValue] of Object.entries(slaReports)) {
            const currentSlaReport = new SLAReport();
            if (slaReportValue === null) {
              currentSlaReport.elementType = null;
              currentSlaReport.level = null;
              currentSlaReport.value = null;
            } else {
              currentSlaReport.elementType = slaReportValue.elementType;
              currentSlaReport.level = slaReportValue.level;
              currentSlaReport.value = slaReportValue.value;
            }
            currentSlaReport.elementId = slaReportName;
            currentSlaReport.presentationType = this.presentationType;
            me.rowData.push(currentSlaReport);
          }
          if (me.gridApi) {
            me.gridApi.setRowData(me.rowData);
          }
        }
      }, (error) => {
        dialogRef.close("error");
        if (error.includes('404')) {
          this.showErrorDialog("There is no SLA Threshold configuration for the given environment yet. Please configure the SLA Threshold in the 'SLA Thresholds' tab first.", "Error loading data");
        } else {
          this.showErrorDialog("Reason: " + (error.error || error.message || error), "Error loading data");
        }
      });
    }
  });
  }

  buildSLAReportForLineChart(currentSlaReport: SLAReport, slaReports, chosenStartDate, chosenEndDate): SLAReport {   
    for (const [key, data] of Object.entries(slaReports)) {
      if (!data) {
        // placeholder for points
        currentSlaReport.lineChartPoints.push(key + ', 0');
        currentSlaReport.lineChartDates.push('No SLA data available');
      } else {
        // actual datapoints for linechart
        currentSlaReport.lineChartPoints.push(key + ', ' + (data['value']));
        currentSlaReport.elementType = data['elementType'];
        currentSlaReport.level = data['level'];
        currentSlaReport.startDate = key === '0' ? data['startDate'] : currentSlaReport.startDate;
        currentSlaReport.endDate = key === (Object.keys(slaReports).length - 1).toString() ? data['endDate'] : null;

        const dateStringForChart = new Date((new Date(data['startDate']).getTime() + new Date(data['endDate']).getTime()) / 2).toISOString();
        currentSlaReport.lineChartDates.push(dateStringForChart);
      }
    }
    if (chosenStartDate && chosenEndDate) {
      currentSlaReport.startDate = chosenStartDate;
      currentSlaReport.endDate = chosenEndDate;
    }
    currentSlaReport.value = currentSlaReport.elementType ? 1 : null;
    currentSlaReport.presentationType = this.presentationType;   
    return currentSlaReport;
  }

  onGridReady(params) {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;
    params.api.sizeColumnsToFit();
  }

  validate(): boolean {
    let valid = true;
    if (!this.environmentSubscriptionId || !this.presentationType) {
      valid = false;
    }
    return valid;
  }

  openChart(params) {    
    if (params.data.value == null) {
      // do not show charts for elements without data
      return;
    }
    if (params.data.presentationType === 'pieChart' || params.data.presentationType === 'lineChart') {
      const type = params.data.presentationType;
      const data = params.data;
      const chartDialog = this.dialog.open(SlaChartDialogComponent, {
        width: '500px',
        data: { type, data }
      });
    } else {
      this.snackbar.open(`Please select a presentation type`, '', {
        panelClass: "mysnackbar",
        horizontalPosition: "center",
        duration: 2000
      });
    }
  }

  showErrorDialog(message: string, title: string) {
    const errorDialog = this.dialog.open(ErrorDialogComponent, {
      width: '400px',
      data: { message, title },
      disableClose: true
    });
  }

  disableDownload() {
      this.downloadEnabled = false;
  }

  slaDataLoaded(): boolean {
    return this.rowData && this.rowData.length > 0 && this.downloadEnabled;
  }

  downloadCsv() {
    const environmentName = this.environments.find(
      (env) => env.subscriptionId === this.environmentSubscriptionId
    ).name;
    const startDateTime = new Date(this.timeFormGroup.get("startDateCtrl").value);
    const endDateTime = new Date(this.timeFormGroup.get("endDateCtrl").value);
    const filename = this.buildExportFilename(environmentName, startDateTime, endDateTime) + ".csv";
    const csv = this.generateCsv(environmentName, startDateTime, endDateTime);
    this.downloadStringAsFile(csv, "text/csv", filename);
  }

  downloadPdf() {
    const environmentName = this.environments.find(
      (env) => env.subscriptionId === this.environmentSubscriptionId
    ).name;
    const startDateTime = new Date(this.timeFormGroup.get("startDateCtrl").value);
    const endDateTime = new Date(this.timeFormGroup.get("endDateCtrl").value);
    const filename = this.buildExportFilename(environmentName, startDateTime, endDateTime) + ".pdf";

    const md = this.generateHtml(environmentName, startDateTime, endDateTime);
    
    // generate and download
    const pdf = this.convertHtmlToPdf(md, filename);
  }

  generateCsv(environmentName: string, startDateTime: Date, endDateTime: Date): string {
    const startDate = startDateTime.toLocaleString();
    const endDate = endDateTime.toLocaleString();
    let csv = "Environment;StartDate;EndDate;ElementType;ElementId;State;Value";
    for (const data of this.rowData) {
      let stateString;
      if (data.level === 0) {
        stateString = "Ok";
      } else if (data.level === 1) {
        stateString = "Warning";
      } else if (data.level === 2) {
        stateString = "Error";
      } else {
        stateString = "-";
      }
      const csvLine = "\n" + environmentName + ";" + startDate + ";" + endDate + ";" + data.elementType + ";" + data.elementId + ";" + stateString + ";" + data.value + ";";
      csv = csv.concat(csvLine);
    }
    return csv;
  }

  generateHtml(environmentName: string, startDateTime: Date, endDateTime: Date): string {
    let md = "";
    md = md + '<html><body>';
    md = md + '<h2>Environment: ' + environmentName + '<h2><BR>';
    md = md + '<p>StartDate:' + startDateTime + '<BR>EndDate:' + endDateTime + '</p>'  
    md = md + '<table style="width: 100%">'
    md = md + '<colgroup><col span="1" class="col1"><col span ="1" class="col2"><col span ="1" class="col3"><col span ="1" class="col4"></colgroup>';
    md = md + '<th width="30%" class="col1" style="word-break: break-all">Element Name</th> <th class="col2"> Element Type </th> <th class="col3"> SLA State </th> <th class="col4"> SLA Value </th>';    
    let state = document.createElement('div');
    for (let i = 0; i < this.rowData.length; i++) {
      let value = this.rowData[i].value;
      state = this.slaStatePdfRenderer(this.rowData[i].level);
      
      // split the elementId
      let elementId = this.rowData[i].elementId.replace(new RegExp('/', 'g'),'<BR>/');
      elementId = elementId.replace(new RegExp('_', 'g'),'<BR>_');
      let elementBox = '<p style="word-wrap: break-word" >'+ elementId + '</p>'; 
      md = md + "\n" + ' <tr> <td width="30%" class="col1"> ' + elementBox + ' </td> <td width="20%" class="col2"> ' + this.rowData[i].elementType + ' </td> <td width="20%" class="col3"> ' + state.innerHTML + ' </td> <td width="20%" class="col4" style="min-width:150px"> ' + value + ' </td> </tr>';
    }
    md = md + '</table></body></html>';
    return md;
  }

  convertHtmlToPdf(htmlContent: string, filename: string): Object {
    var html = htmlToPdfmake(htmlContent);
    var docDefinition = {
      content: [
        html
      ],
      layout: 'lightHorizontalLines',
      table: {
        // headers are automatically repeated if the table spans over multiple pages
        // you can declare how many rows should be treated as headers
        headerRows: 1,
        widths: [ '40%', '20%', '20%', '20%' ],
      },
      pageMargins: [ 30, 40, 30, 40 ],
      pageSize: 'A4',
      styles:{
        'html-strong':{
          background:'yellow' 
        },
        'col1': {color: 'black', width: '30%', "max-width": '200px', "word-wrap": 'break-word'},
        'col2': {color: 'black', width: '20%'},
        'col3': {color: 'black', width: '20%'},
        'col4': {color: 'black', width: '20%', "min-width": "150px"},
      },
      defaultStyle: {
        fontSize: 10
      }
    };
    pdfMake.createPdf(docDefinition).download(filename);
    return html;
  }

  downloadStringAsFile(text, filetype, filename) {
    try {
      const linkElement = document.createElement("a");
      const blob = new Blob([text], { type: filetype });
      linkElement.download = filename;
      linkElement.href = URL.createObjectURL(blob);
      linkElement.dataset.downloadurl = [ filetype, linkElement.download, linkElement.href].join(":");
      linkElement.style.display = "none";
      document.body.appendChild(linkElement);
      linkElement.click();
      document.body.removeChild(linkElement);
      setTimeout(function () { URL.revokeObjectURL(linkElement.href); }, 1500);
    } catch (ex) {
      this.snackbar.open("Error exporting data.", "", {
        panelClass: "mysnackbar",
        horizontalPosition: "center",
        duration: 8000,
      });
    }
  }

  buildExportFilename(environmentName: string, startDateTime: Date, endDateTime: Date): string {
    const startDate = this.datePipe.transform(startDateTime, 'yyyyMMdd');
    const endDate = this.datePipe.transform(endDateTime, 'yyyyMMdd');
    return "SLA_" + environmentName + "_" + startDate + "_" + endDate;
    }
}
