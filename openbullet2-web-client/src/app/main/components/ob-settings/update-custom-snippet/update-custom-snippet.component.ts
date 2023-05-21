import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { CustomSnippet } from 'src/app/main/dtos/settings/ob-settings.dto';

@Component({
  selector: 'app-update-custom-snippet',
  templateUrl: './update-custom-snippet.component.html',
  styleUrls: ['./update-custom-snippet.component.scss']
})
export class UpdateCustomSnippetComponent implements OnChanges {
  @Input() snippet: CustomSnippet | null = null;
  @Output() confirm = new EventEmitter<CustomSnippet>();

  faCircleQuestion = faCircleQuestion;
  name: string = '';
  description: string = '';
  body: string = '';

  ngOnChanges(changes: SimpleChanges) {
    if (this.snippet === null) return;
    this.name = this.snippet.name;
    this.description = this.snippet.description;
    this.body = this.snippet.body;
  }

  submitForm() {
    this.confirm.emit({
      name: this.name,
      description: this.description,
      body: this.body
    });
  }

  isFormValid() {
    return this.name.length > 0;
  }
}
