import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { GuestDto } from 'src/app/main/dtos/guests/guest.dto';
import { UpdateGuestPasswordDto } from 'src/app/main/dtos/guests/update-guest-password.dto';

@Component({
  selector: 'app-update-guest-password',
  templateUrl: './update-guest-password.component.html',
  styleUrls: ['./update-guest-password.component.scss']
})
export class UpdateGuestPasswordComponent {
  @Input() guest: GuestDto | null = null;
  @Output() confirm = new EventEmitter<UpdateGuestPasswordDto>();
  password: string = '';
  confirmPassword: string = '';
}
