import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ProxyGroupDto } from 'src/app/main/dtos/proxy-group/proxy-group.dto';

export interface DeleteLowQualityProxiesParams {
  proxyGroupId: number;
  deleteUnknown: boolean;
  deleteTransparent: boolean;
  deleteAnonymous: boolean;
}

@Component({
  selector: 'app-delete-low-quality-proxies',
  templateUrl: './delete-low-quality-proxies.component.html',
  styleUrls: ['./delete-low-quality-proxies.component.scss'],
})
export class DeleteLowQualityProxiesComponent {
  @Input() proxyGroup: ProxyGroupDto | null = null;
  @Output() confirm = new EventEmitter<DeleteLowQualityProxiesParams>();

  deleteUnknown = true;
  deleteTransparent = true;
  deleteAnonymous = true;

  submitForm() {
    if (this.proxyGroup === null) {
      console.log('Proxy group is null, this should not happen!');
      return;
    }

    this.confirm.emit({
      proxyGroupId: this.proxyGroup.id,
      deleteUnknown: this.deleteUnknown,
      deleteTransparent: this.deleteTransparent,
      deleteAnonymous: this.deleteAnonymous,
    });
  }

  isFormValid() {
    return this.deleteUnknown || this.deleteTransparent || this.deleteAnonymous;
  }
}
