import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { GuestDto } from 'src/app/main/dtos/guest/guest.dto';
import { UpdateGuestInfoDto } from 'src/app/main/dtos/guest/update-guest-info.dto';

@Component({
  selector: 'app-update-guest-info',
  templateUrl: './update-guest-info.component.html',
  styleUrls: ['./update-guest-info.component.scss'],
})
export class UpdateGuestInfoComponent implements OnChanges {
  @Input() guest: GuestDto | null = null;
  @Output() confirm = new EventEmitter<UpdateGuestInfoDto>();
  username = '';
  accessExpiration: Date = new Date();
  allowedAddresses = '';
  faCircleQuestion = faCircleQuestion;

  ngOnChanges(changes: SimpleChanges) {
    if (this.guest === null) return;
    this.username = this.guest.username;
    this.accessExpiration = new Date(this.guest.accessExpiration);
    this.allowedAddresses = this.guest.allowedAddresses.join('\n');
  }

  submitForm() {
    if (this.guest === null) {
      console.log('Guest is null, this should not happen!');
      return;
    }

    this.confirm.emit({
      id: this.guest.id,
      username: this.username,
      accessExpiration: this.accessExpiration.toISOString(),
      allowedAddresses: this.allowedAddresses.split('\n'),
    });
  }

  isFormValid() {
    return this.username.length >= 3 && this.username.length <= 32;
  }
}
