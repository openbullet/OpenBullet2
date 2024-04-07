import { Injectable } from '@angular/core';
import { UserService } from 'src/app/main/services/user.service';

@Injectable({
  providedIn: 'root',
})
export class AdminGuard {
  constructor(private userService: UserService) {}

  canActivate(): boolean {
    return this.userService.isAdmin();
  }
}
