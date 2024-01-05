import { Component, OnInit } from '@angular/core';
import { GuestService } from '../../services/guest.service';
import { GuestDto } from '../../dtos/guest/guest.dto';
import { faFilterCircleXmark, faKey, faPen, faPlus, faUsers, faX } from '@fortawesome/free-solid-svg-icons';
import { ConfirmationService, MessageService } from 'primeng/api';
import { UpdateGuestInfoDto } from '../../dtos/guest/update-guest-info.dto';
import { UpdateGuestPasswordDto } from '../../dtos/guest/update-guest-password.dto';
import { CreateGuestDto } from '../../dtos/guest/create-guest.dto';

@Component({
  selector: 'app-guests',
  templateUrl: './guests.component.html',
  styleUrls: ['./guests.component.scss']
})
export class GuestsComponent implements OnInit {
  guests: GuestDto[] | null = null;
  faPen = faPen;
  faKey = faKey;
  faFilterCircleXmark = faFilterCircleXmark;
  faX = faX;
  faPlus = faPlus;
  faUsers = faUsers;

  selectedGuest: GuestDto | null = null;

  createGuestModalVisible = false;
  updateGuestInfoModalVisible = false;
  updateGuestPasswordModalVisible = false;

  constructor(private guestService: GuestService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService) {

  }

  ngOnInit(): void {
    this.refreshGuests();
  }

  refreshGuests() {
    this.guestService.getAllGuests()
      .subscribe(guests => this.guests = guests);
  }

  openUpdateGuestInfoModal(guest: GuestDto) {
    this.selectedGuest = guest;
    this.updateGuestInfoModalVisible = true;
  }

  openUpdateGuestPasswordModal(guest: GuestDto) {
    this.selectedGuest = guest;
    this.updateGuestPasswordModalVisible = true;
  }

  openCreateGuestModal() {
    this.createGuestModalVisible = true;
  }

  createGuest(guest: CreateGuestDto) {
    this.guestService.createGuest(guest)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Created',
          detail: `Guest ${resp.username} was created`
        });
        this.createGuestModalVisible = false;
        this.refreshGuests();
      });
  }

  updateGuestInfo(guest: UpdateGuestInfoDto) {
    this.guestService.updateGuestInfo(guest)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Updated',
          detail: `Guest ${resp.username} was updated`
        });
        this.updateGuestInfoModalVisible = false;
        this.refreshGuests();
      });
  }

  updateGuestPassword(guest: UpdateGuestPasswordDto) {
    this.guestService.updateGuestPassword(guest)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Password updated',
          detail: `Changed the password of ${resp.username}`
        });
        this.updateGuestPasswordModalVisible = false;
        this.refreshGuests();
      });
  }

  deleteGuest(guest: GuestDto) {
    this.guestService.deleteGuest(guest.id)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Deleted',
          detail: `Guest ${guest.username} was deleted`
        });
        this.refreshGuests();
      });
  }

  confirmDeleteGuest(guest: GuestDto) {
    this.confirmationService.confirm({
      message: `You are about to delete the guest user ${guest.username}. 
      Are you sure that you want to proceed?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteGuest(guest)
    });
  }
}
