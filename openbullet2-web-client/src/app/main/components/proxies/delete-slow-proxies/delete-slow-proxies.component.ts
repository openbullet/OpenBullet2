import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ProxyGroupDto } from 'src/app/main/dtos/proxy-group/proxy-group.dto';

export interface DeleteSlowProxiesParams {
  proxyGroupId: number;
  maxPing: number;
}

@Component({
  selector: 'app-delete-slow-proxies',
  templateUrl: './delete-slow-proxies.component.html',
  styleUrls: ['./delete-slow-proxies.component.scss'],
})
export class DeleteSlowProxiesComponent {
  @Input() proxyGroup: ProxyGroupDto | null = null;
  @Output() confirm = new EventEmitter<DeleteSlowProxiesParams>();

  maxPing = 10000;

  submitForm() {
    if (this.proxyGroup === null) {
      console.log('Proxy group is null, this should not happen!');
      return;
    }

    this.confirm.emit({
      proxyGroupId: this.proxyGroup.id,
      maxPing: this.maxPing,
    });
  }

  isFormValid() {
    return Number.isInteger(this.maxPing) && this.maxPing >= 0;
  }
}
