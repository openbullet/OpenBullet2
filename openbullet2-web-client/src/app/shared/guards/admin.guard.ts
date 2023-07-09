import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { UserService } from 'src/app/main/services/user.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard {

  constructor(private userService: UserService, private toastr: ToastrService) { }

  canActivate(): boolean {
    const userInfo = this.userService.loadUserInfo();
    return userInfo.role.toLowerCase() === 'admin';
  }
}
