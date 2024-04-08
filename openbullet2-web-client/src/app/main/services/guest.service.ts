import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getBaseUrl } from 'src/app/shared/utils/host';
import { CreateGuestDto } from '../dtos/guest/create-guest.dto';
import { GuestDto } from '../dtos/guest/guest.dto';
import { UpdateGuestInfoDto } from '../dtos/guest/update-guest-info.dto';
import { UpdateGuestPasswordDto } from '../dtos/guest/update-guest-password.dto';

@Injectable({
  providedIn: 'root',
})
export class GuestService {
  constructor(private http: HttpClient) {}

  getAllGuests() {
    return this.http.get<GuestDto[]>(`${getBaseUrl()}/guest/all`);
  }

  createGuest(guest: CreateGuestDto) {
    return this.http.post<GuestDto>(`${getBaseUrl()}/guest`, guest);
  }

  updateGuestInfo(updated: UpdateGuestInfoDto) {
    return this.http.patch<GuestDto>(`${getBaseUrl()}/guest/info`, updated);
  }

  updateGuestPassword(updated: UpdateGuestPasswordDto) {
    return this.http.patch<GuestDto>(`${getBaseUrl()}/guest/password`, updated);
  }

  deleteGuest(id: number) {
    return this.http.delete(`${getBaseUrl()}/guest`, {
      params: {
        id,
      },
    });
  }
}
