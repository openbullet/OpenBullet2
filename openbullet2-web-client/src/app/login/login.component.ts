import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../main/services/user.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  username = '';
  password = '';
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
