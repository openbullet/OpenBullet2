import { Component, EventEmitter, Output } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { EndpointDto } from 'src/app/main/dtos/sharing/endpoint.dto';

@Component({
  selector: 'app-create-endpoint',
  templateUrl: './create-endpoint.component.html',
  styleUrls: ['./create-endpoint.component.scss'],
})
export class CreateEndpointComponent {
  @Output() confirm = new EventEmitter<EndpointDto>();
  faCircleQuestion = faCircleQuestion;

  route = '';

  public reset() {
    this.route = 'my-route';
  }

  submitForm() {
    this.confirm.emit({
      route: this.route,
      apiKeys: [],
      configIds: [],
    });
  }

  isFormValid() {
    return this.route.length >= 1 && this.route.match(/^[\w-]+$/) !== null;
  }
}
