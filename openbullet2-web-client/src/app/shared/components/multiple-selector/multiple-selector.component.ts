import { Component, ContentChild, ElementRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, TemplateRef, ViewChild } from '@angular/core';
import { faArrowLeft, faArrowRight } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-multiple-selector',
  templateUrl: './multiple-selector.component.html',
  styleUrls: ['./multiple-selector.component.scss']
})
export class MultipleSelectorComponent implements OnChanges {
  @Input() selectedItems: string[] = [];
  @Input() allItems: string[] = [];
  deselectedItems: string[] = [];
  @Output() selectedItemsChange = new EventEmitter<string[]>();
  @Output() onChange = new EventEmitter<string[]>();
  
  @ContentChild(TemplateRef) itemTemplate
  : TemplateRef<any> | null = null;

  @ViewChild('defaultItemTemplate', { static: true })
  defaultItemTemplate: TemplateRef<any> | null = null;

  faArrowRight = faArrowRight;
  faArrowLeft = faArrowLeft;

  ngOnChanges(changes: SimpleChanges): void {
    this.deselectedItems = this.allItems.filter(
      i => this.selectedItems.indexOf(i) === -1);
  }

  selectItem(item: string) {
    const index = this.deselectedItems.indexOf(item);

    if (index !== -1) {
      const selected = this.deselectedItems.splice(index, 1)[0];
      this.deselectedItems = [...this.deselectedItems];
      this.selectedItems = [...this.selectedItems, selected];
      this.notifyUpdate();
    }
  }

  deselectItem(item: string) {
    const index = this.selectedItems.indexOf(item);

    if (index !== -1) {
      const deselected = this.selectedItems.splice(index, 1)[0];
      this.selectedItems = [...this.selectedItems];
      this.deselectedItems = [...this.deselectedItems, deselected];
      this.notifyUpdate();
    }
  }

  selectAll() {
    this.selectedItems = [...this.selectedItems, ...this.deselectedItems];
    this.deselectedItems = [];
    this.notifyUpdate();
  }

  deselectAll() {
    this.deselectedItems = [...this.deselectedItems, ...this.selectedItems];
    this.selectedItems = [];
    this.notifyUpdate();
  }

  notifyUpdate() {
    this.selectedItemsChange.emit(this.selectedItems);
    this.onChange.emit(this.selectedItems);
  }
}
