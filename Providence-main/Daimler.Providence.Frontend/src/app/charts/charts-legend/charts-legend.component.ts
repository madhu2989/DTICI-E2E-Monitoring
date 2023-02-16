import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-charts-legend',
  templateUrl: './charts-legend.component.html',
  styleUrls: ['./charts-legend.component.scss']
})
export class ChartsLegendComponent implements OnInit {
  @Input() results;
  constructor() { }

  ngOnInit() {
  }

}
