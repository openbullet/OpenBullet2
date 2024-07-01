import { Component, EventEmitter, Output } from '@angular/core';
import { CreateProxyGroupDto } from 'src/app/main/dtos/proxy-group/create-proxy-group.dto';

@Component({
  selector: 'app-create-proxy-group',
  templateUrl: './create-proxy-group.component.html',
  styleUrls: ['./create-proxy-group.component.scss'],
})
export class CreateProxyGroupComponent {
  @Output() confirm = new EventEmitter<CreateProxyGroupDto>();

  name = '';

  public reset() {
    this.name = '';
  }

  submitForm() {
    this.confirm.emit({
      name: this.name,
    });
  }

  isFormValid() {
    return this.name.length >= 3 && this.name.length <= 32;
  }
}
