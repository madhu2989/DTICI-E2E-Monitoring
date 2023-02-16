import { Component, OnInit, Inject, EventEmitter, ViewChild, ElementRef } from "@angular/core";
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { ActivatedRoute, Router } from "@angular/router";
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-sla-chart-dialog',
  templateUrl: './sla-chart-dialog.component.html',
  styleUrls: ['./sla-chart-dialog.component.scss']
})
export class SlaChartDialogComponent implements OnInit {
  @ViewChild('pieChart') pieChart: ElementRef;
  public okValue;
  public nokValue;
  public isError: boolean;
  public isWarning: boolean;
  public pieChartStrokeDashArray;
  public pieChartBackgroundColor;
  public warningThresholdCoordinateY: number;
  public errorThresholdCoordinateY: number;

  constructor(@Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<SlaChartDialogComponent>,
    private datePipe: DatePipe) { }

  ngOnInit() {
    this.pieChartStrokeDashArray = this.data.data.value;
    this.pieChartBackgroundColor = this.data.data.level === 2 ? '#b71c1c' : '#f57f17';
    this.pieChartBackgroundColor = this.data.data.value === 1 ? '#1b5e20' : this.pieChartBackgroundColor;
    this.okValue = this.data.data.value;
    this.nokValue = (100 - this.okValue).toFixed(2);
    this.isError = this.data.data.level === 2;
    this.isWarning = this.data.data.level === 0 || this.data.data.level === 1;
    this.warningThresholdCoordinateY = 175 - (this.data.data.warningThreshold * 1.75) + 50;
    this.errorThresholdCoordinateY   = 175 - (this.data.data.errorThreshold * 1.75) + 50;
  }

  onCloseClick() {
  }

  getpieChartStrokeDashArray() {
    
    // workaround for pie chart
    if (this.pieChartStrokeDashArray == 100) {
      this.pieChartStrokeDashArray = 101;
    }
    
    const value = this.pieChartStrokeDashArray + ' 100';
    
    return value;
  }

  getPieChartBackgroundColor() {
    return this.pieChartBackgroundColor;
  }

  setLineChartPoints() {
    // function to map values and percentages to pixels for the line chart
    const points = this.data.data.lineChartPoints;

    // map the xAxis to the maxWidth of the chart (350px)
    const lineChartxAxisKey = 350 / points.length;
    const lineChartCoordinates: string[] = [];
    let coordinateString;
    for (let i  = 0; i < points.length; i++) {
      let xCoordinate = parseInt(points[i].split(", ")[0], 10);
      let yCoordinate = parseInt(points[i].split(", ")[1], 10);
      // move the axes to make place for the descriptions and map the yAxis to the maxHeight of the chart (175px)
      xCoordinate = xCoordinate * lineChartxAxisKey + 40;
      yCoordinate = 175 - (yCoordinate * 1.75) + 50;
      coordinateString = xCoordinate + ', ' + yCoordinate;
      lineChartCoordinates.push(coordinateString);
    }
    return lineChartCoordinates;
  }

  getLineChartDataPoint(chartCoordinateString: string, coordinateValue: number): number {
    const coordinatePoint = parseInt(chartCoordinateString.split(', ')[coordinateValue], 10);
    return coordinatePoint;
  }

  getTooltipForDataPoint(index: number) {
    const dates = this.data.data.lineChartDates;
    const values = this.data.data.lineChartPoints;
    const percentage = parseInt(values[index].split(', ')[1], 10);
    let formattedDate;
    
    if (dates[index] && dates[index] !== 'No SLA data available') {
      formattedDate = this.datePipe.transform(dates[index], 'yyyy-MM-dd HH:mm:ss');
    } else {
      formattedDate = 'No SLA data available';
    }
    const tooltipString = formattedDate + ' - ' + percentage + '%';
    return tooltipString;
  }

  getFormattedDate(date: string): string {
    let formattedDate;

    if (date && date !== 'No SLA data available') {
      formattedDate = this.datePipe.transform(date, 'yyyy-MM-dd');
    } else {
      formattedDate = 'No SLA data available';
    }
    return formattedDate;
  }

}
