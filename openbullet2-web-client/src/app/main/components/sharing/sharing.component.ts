import { Component, OnInit } from '@angular/core';
import {
  faCircleQuestion,
  faDiceFive,
  faPen,
  faPlus,
  faRetweet,
  faTriangleExclamation,
  faX,
} from '@fortawesome/free-solid-svg-icons';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConfigInfoDto } from '../../dtos/config/config-info.dto';
import { EndpointDto } from '../../dtos/sharing/endpoint.dto';
import { ConfigService } from '../../services/config.service';
import { SharingService } from '../../services/sharing.service';
import { randomString } from 'src/app/shared/utils/strings';

@Component({
  selector: 'app-sharing',
  templateUrl: './sharing.component.html',
  styleUrls: ['./sharing.component.scss'],
})
export class SharingComponent implements OnInit {
  endpoints: EndpointDto[] | null = null;
  configs: ConfigInfoDto[] | null = null;
  faPen = faPen;
  faX = faX;
  faPlus = faPlus;
  faDiceFive = faDiceFive;
  faCircleQuestion = faCircleQuestion;
  faRetweet = faRetweet;
  faTriangleExclamation = faTriangleExclamation;

  selectedEndpoint: EndpointDto | null = null;
  availableConfigs: ConfigInfoDto[] | null = null;
  selectedConfigs: ConfigInfoDto[] | null = null;

  createEndpointModalVisible = false;
  updateEndpointModalVisible = false;

  constructor(
    private sharingService: SharingService,
    private configService: ConfigService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.refreshConfigs();
  }

  refreshEndpoints() {
    this.sharingService.getAllEndpoints().subscribe((endpoints) => {
      this.endpoints = endpoints;

      // Try to re-select the same route if possible
      if (this.selectedEndpoint !== null) {
        const match = endpoints.filter((e) => e.route === this.selectedEndpoint?.route);

        if (match.length > 0) {
          this.selectEndpoint(match[0]);
          return;
        }
      }

      if (endpoints.length > 0) {
        this.selectEndpoint(endpoints[0]);
      }
    });
  }

  refreshConfigs() {
    this.configService.getAllConfigs(false).subscribe((configs) => {
      this.configs = configs;
      this.refreshEndpoints();
    });
  }

  openCreateEndpointModal() {
    this.createEndpointModalVisible = true;
  }

  openUpdateEndpointModal() {
    this.updateEndpointModalVisible = true;
  }

  createEndpoint(endpoint: EndpointDto) {
    this.sharingService.createEndpoint(endpoint).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Created',
        detail: `Endpoint ${resp.route} was created`,
      });
      this.createEndpointModalVisible = false;
      this.refreshEndpoints();
    });
  }

  updateEndpoint(endpoint: EndpointDto) {
    this.sharingService.updateEndpoint(endpoint).subscribe((resp) => {
      console.log('resp', resp);
      this.messageService.add({
        severity: 'success',
        summary: 'Updated',
        detail: `Endpoint ${resp.route} was updated`,
      });
      this.updateEndpointModalVisible = false;
      this.availableConfigs = null;
      this.selectedConfigs = null;
      this.refreshEndpoints();
    });
  }

  deleteEndpoint(endpoint: EndpointDto) {
    this.sharingService.deleteEndpoint(endpoint.route).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `Endpoint ${endpoint.route} was deleted`,
      });
      this.refreshEndpoints();
    });
  }

  confirmDeleteEndpoint(endpoint: EndpointDto) {
    this.confirmationService.confirm({
      message: `You are about to delete the endpoint ${endpoint.route}. 
      Are you sure that you want to proceed?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteEndpoint(endpoint),
    });
  }

  selectEndpoint(endpoint: EndpointDto) {
    if (this.configs === null) return;
    this.selectedEndpoint = endpoint;
    this.availableConfigs = this.configs.filter((c) => !endpoint.configIds.includes(c.id));
    this.selectedConfigs = this.configs.filter((c) => endpoint.configIds.includes(c.id));
  }

  getButtonClass(endpoint: EndpointDto) {
    return endpoint === this.selectedEndpoint ? 'selected-endpoint' : '';
  }

  generateApiKey(endpoint: EndpointDto) {
    endpoint.apiKeys.push(randomString(20));
  }

  removeApiKey(endpoint: EndpointDto, apiKey: string) {
    const index = endpoint.apiKeys.indexOf(apiKey);
    if (index > -1) {
      endpoint.apiKeys.splice(index, 1);
    }
  }

  updateConfigIds(endpoint: EndpointDto) {
    if (this.selectedConfigs === null) return;
    endpoint.configIds = this.selectedConfigs.map((c) => c.id);
  }
}
