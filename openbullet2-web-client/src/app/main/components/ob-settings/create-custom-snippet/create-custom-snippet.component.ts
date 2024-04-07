import { Component, EventEmitter, Output } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { CustomSnippet } from 'src/app/main/dtos/settings/ob-settings.dto';

@Component({
  selector: 'app-create-custom-snippet',
  templateUrl: './create-custom-snippet.component.html',
  styleUrls: ['./create-custom-snippet.component.scss'],
})
export class CreateCustomSnippetComponent {
  @Output() confirm = new EventEmitter<CustomSnippet>();

  faCircleQuestion = faCircleQuestion;
  name = '';
  description = '';
  body = '';

  public reset() {
    this.name = '';
    this.description = '';
    this.body = '';
  }

  submitForm() {
    this.confirm.emit({
      name: this.name,
      description: this.description,
      body: this.body,
    });
  }

  isFormValid() {
    return this.name.length > 0;
  }
}
