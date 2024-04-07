import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { GuestDto } from 'src/app/main/dtos/guest/guest.dto';
import { UpdateGuestPasswordDto } from 'src/app/main/dtos/guest/update-guest-password.dto';

@Component({
  selector: 'app-update-guest-password',
  templateUrl: './update-guest-password.component.html',
  styleUrls: ['./update-guest-password.component.scss'],
})
export class UpdateGuestPasswordComponent implements OnChanges {
  @Input() guest: GuestDto | null = null;
  @Output() confirm = new EventEmitter<UpdateGuestPasswordDto>();
  password = '';
  confirmPassword = '';

  ngOnChanges(changes: SimpleChanges) {
    if (this.guest === null) return;
    this.password = '';
    this.confirmPassword = '';
  }

  submitForm() {
    if (this.guest === null) {
      console.log('Guest is null, this should not happen!');
      return;
    }

    this.confirm.emit({
      id: this.guest.id,
      password: this.password,
    });
  }

  isFormValid() {
    return this.password.length >= 8 && this.password === this.confirmPassword;
  }
}
