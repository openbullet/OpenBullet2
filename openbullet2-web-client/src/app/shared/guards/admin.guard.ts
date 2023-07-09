import { Injectable } from '@angular/core';
import { UserService } from 'src/app/main/services/user.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard {

  constructor(private userService: UserService) { }

  canActivate(): boolean {
    const userInfo = this.userService.loadUserInfo();
    console.log(userInfo);
    return userInfo.role.toLowerCase() === 'admin';
  }
}
