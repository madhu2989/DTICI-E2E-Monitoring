import { Component, OnInit, Output, Input, EventEmitter } from '@angular/core';
import { SettingsService } from '../services/settings.service';

@Component({
  selector: 'app-crud-toolbar',
  templateUrl: './crud-toolbar.component.html',
  styleUrls: ['./crud-toolbar.component.scss']
})
export class CrudToolbarComponent implements OnInit {

  @Output() add: EventEmitter<any> = new EventEmitter();
  @Output() delete: EventEmitter<any> = new EventEmitter();
  @Output() edit: EventEmitter<any> = new EventEmitter();
  @Output() settings: EventEmitter<any> = new EventEmitter();

  @Input() deleteButtonActive = false;
  @Input() deleteButtonVisible = true;
  @Input() addButtonActive = false;
  @Input() addButtonVisible = true;
  @Input() editButtonActive = false;
  @Input() editButtonVisible = true;
  @Input() settingsButtonActive = true;
  @Input() settingsButtonVisible = false;
  @Input() toolBarTitle: string;
  @Input() toolBarSubTitle: string;

  constructor(public settingsService: SettingsService) { }

  ngOnInit() {
  }

  addDialog(): void {
    this.add.emit(null);
  }

  deleteDialog(): void {
    this.delete.emit(null);
  }

  onEditButtonClicked(): void {
    this.edit.emit(null);
  }

  onSettingsButtonClicked(): void {
    this.settings.emit(null);
  }
}