import { Component, EventEmitter, Input, Output, ViewEncapsulation } from '@angular/core';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';
import { ConfigStackerComponent } from '../../config-stacker.component';
import { AutoCompleteCompleteEvent } from 'primeng/autocomplete';

@Component({
  selector: 'app-setting-input-variable',
  templateUrl: './setting-input-variable.component.html',
  styleUrls: ['./setting-input-variable.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class SettingInputVariableComponent {
  @Input() setting!: BlockSettingDto;
  @Input() stacker!: ConfigStackerComponent;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  suggestions: string[] = [];

  getSuggestions(event: AutoCompleteCompleteEvent) {
    const trimmedQuery = event.query.trim().toLowerCase();
    const allSuggestions = this.stacker.getSuggestions();

    this.suggestions = trimmedQuery === ''
      ? allSuggestions
      : allSuggestions.filter(s => {
        const lower = s.toLowerCase();
        return lower.startsWith(trimmedQuery) && lower !== trimmedQuery;
      });
  }

  valueChanged() {
    this.onChange.emit();
  }
}
