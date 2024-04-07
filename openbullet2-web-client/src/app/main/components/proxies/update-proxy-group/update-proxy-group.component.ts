import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { ProxyGroupDto } from 'src/app/main/dtos/proxy-group/proxy-group.dto';
import { UpdateProxyGroupDto } from 'src/app/main/dtos/proxy-group/update-proxy-group.dto';

@Component({
  selector: 'app-update-proxy-group',
  templateUrl: './update-proxy-group.component.html',
  styleUrls: ['./update-proxy-group.component.scss'],
})
export class UpdateProxyGroupComponent implements OnChanges {
  @Input() proxyGroup: ProxyGroupDto | null = null;
  @Output() confirm = new EventEmitter<UpdateProxyGroupDto>();

  name = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (this.proxyGroup === null) return;
    this.name = this.proxyGroup.name;
  }

  submitForm() {
    if (this.proxyGroup === null) {
      console.log('Proxy group is null, this should not happen!');
      return;
    }

    this.confirm.emit({
      id: this.proxyGroup.id,
      name: this.name,
    });
  }

  isFormValid() {
    return this.name.length >= 3 && this.name.length <= 32;
  }
}
