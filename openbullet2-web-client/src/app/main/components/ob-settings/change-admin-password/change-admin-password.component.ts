import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-change-admin-password',
  templateUrl: './change-admin-password.component.html',
  styleUrls: ['./change-admin-password.component.scss'],
})
export class ChangeAdminPasswordComponent {
  @Output() confirm = new EventEmitter<string>();

  password = '';
  confirmPassword = '';

  public reset() {
    this.password = '';
    this.confirmPassword = '';
  }

  submitForm() {
    this.confirm.emit(this.password);
  }

  isFormValid() {
    return this.password.length >= 8 && this.password === this.confirmPassword;
  }
}
