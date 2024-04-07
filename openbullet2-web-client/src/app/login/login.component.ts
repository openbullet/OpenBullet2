import { Component } from '@angular/core';
import { UserService } from '../main/services/user.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  username: string = '';
  password: string = '';
  failedLoginMessage: string | null = null;
  forgotCredentialsModalVisible = false;

  constructor(
    private userService: UserService,
    private router: Router,
  ) {}

  isFormValid(): boolean {
    return this.username.length > 0 && this.password.length > 0;
  }

  submitForm() {
    this.userService
      .login({
        username: this.username,
        password: this.password,
      })
      .subscribe({
        next: (response) => {
          this.userService.saveJwt(response.token);
          this.router.navigate(['/']);
        },
        error: (error) => {
          this.failedLoginMessage = error.error.message;
        },
      });
  }
}
