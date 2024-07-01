import { Component, OnInit } from '@angular/core';
import { faFilterCircleXmark, faKey, faPlus, faPuzzlePiece, faX } from '@fortawesome/free-solid-svg-icons';
import { ConfirmationService, MessageService } from 'primeng/api';
import { PluginDto } from '../../dtos/plugin/plugin.dto';
import { PluginService } from '../../services/plugin.service';

@Component({
  selector: 'app-plugins',
  templateUrl: './plugins.component.html',
  styleUrls: ['./plugins.component.scss'],
})
export class PluginsComponent implements OnInit {
  plugins: PluginDto[] | null = null;
  faFilterCircleXmark = faFilterCircleXmark;
  faKey = faKey;
  faX = faX;
  faPlus = faPlus;
  faPuzzlePiece = faPuzzlePiece;

  addPluginModalVisible = false;

  constructor(
    private pluginService: PluginService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
  ) {}

  ngOnInit(): void {
    this.refreshPlugins();
  }

  refreshPlugins() {
    this.pluginService.getAllPlugins().subscribe((plugins) => {
      this.plugins = plugins;
    });
  }

  openAddPluginModal() {
    this.addPluginModalVisible = true;
  }

  addPlugin(file: File) {
    this.pluginService.addPlugin(file).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Added',
        detail: `Plugin ${file.name} was added`,
      });
      this.addPluginModalVisible = false;
      this.refreshPlugins();
    });
  }

  deletePlugin(plugin: PluginDto) {
    this.pluginService.deletePlugin(plugin.name).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `Plugin ${plugin.name} was deleted`,
      });
      this.refreshPlugins();
    });
  }

  confirmDeletePlugin(plugin: PluginDto) {
    this.confirmationService.confirm({
      message: `You are about to delete the plugin ${plugin.name}. Are you sure that you want to proceed?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deletePlugin(plugin),
    });
  }
}
