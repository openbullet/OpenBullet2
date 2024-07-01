import { Component, EventEmitter, Output } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { CreateGuestDto } from 'src/app/main/dtos/guest/create-guest.dto';

@Component({
  selector: 'app-create-guest',
  templateUrl: './create-guest.component.html',
  styleUrls: ['./create-guest.component.scss'],
})
export class CreateGuestComponent {
  @Output() confirm = new EventEmitter<CreateGuestDto>();

  username = '';
  accessExpiration: Date = new Date();
  allowedAddresses = '';
  faCircleQuestion = faCircleQuestion;
  password = '';
  confirmPassword = '';

  public reset() {
    this.username = '';
    this.accessExpiration = moment().add(7, 'days').toDate();
    this.allowedAddresses = '';
    this.password = '';
    this.confirmPassword = '';
  }

  submitForm() {
    this.confirm.emit({
      username: this.username,
      accessExpiration: this.accessExpiration.toISOString(),
      allowedAddresses: this.allowedAddresses.split('\n'),
      password: this.password,
    });
  }

  isFormValid() {
    if (this.username.length < 3 || this.username.length > 32) {
      return false;
    }

    return this.password.length >= 8 && this.password === this.confirmPassword;
  }
}
