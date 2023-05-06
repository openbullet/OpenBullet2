import { Component, EventEmitter, Output } from '@angular/core';
import { faUser } from '@fortawesome/free-solid-svg-icons';
import { CreateGuestDto } from 'src/app/main/dtos/guests/create-guest.dto';

@Component({
  selector: 'app-create-guest',
  templateUrl: './create-guest.component.html',
  styleUrls: ['./create-guest.component.scss']
})
export class CreateGuestComponent {
  @Output() confirm = new EventEmitter<CreateGuestDto>();

  faUser = faUser;
}
