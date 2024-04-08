import { Component, OnInit, ViewChild } from '@angular/core';
import { faFileLines, faFilterCircleXmark, faPen, faX } from '@fortawesome/free-solid-svg-icons';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { EnvironmentSettingsDto } from '../../dtos/settings/environment-settings.dto';
import { CreateWordlistDto } from '../../dtos/wordlist/create-wordlist.dto';
import { UpdateWordlistInfoDto } from '../../dtos/wordlist/update-wordlist-info.dto';
import { WordlistDto } from '../../dtos/wordlist/wordlist.dto';
import { SettingsService } from '../../services/settings.service';
import { WordlistService } from '../../services/wordlist.service';
import { AddWordlistComponent } from './add-wordlist/add-wordlist.component';
import { UploadWordlistComponent } from './upload-wordlist/upload-wordlist.component';

@Component({
  selector: 'app-wordlists',
  templateUrl: './wordlists.component.html',
  styleUrls: ['./wordlists.component.scss'],
})
export class WordlistsComponent implements OnInit {
  envSettings: EnvironmentSettingsDto | null = null;
  wordlists: WordlistDto[] | null = null;

  @ViewChild('addWordlistComponent')
  addWordlistComponent: AddWordlistComponent | undefined = undefined;

  @ViewChild('uploadWordlistComponent')
  uploadWordlistComponent: UploadWordlistComponent | undefined = undefined;

  faPen = faPen;
  faX = faX;
  faFilterCircleXmark = faFilterCircleXmark;
  faFileLines = faFileLines;

  selectedWordlist: WordlistDto | null = null;
  addWordlistModalVisible = false;
  uploadWordlistModalVisible = false;
  updateWordlistInfoModalVisible = false;

  wordlistTypes: string[] = [];

  wordlistMenuItems: MenuItem[] = [
    {
      id: 'add',
      label: 'Add',
      icon: 'pi pi-fw pi-plus',
      items: [
        {
          id: 'add-from-local-file',
          label: 'From local file',
          icon: 'pi pi-fw pi-file color-good',
          command: (e) => this.openAddWordlistModal(),
        },
        {
          id: 'upload-file',
          label: 'Upload (remote)',
          icon: 'pi pi-fw pi-upload color-good',
          command: (e) => this.openUploadWordlistModal(),
        },
      ],
    },
    {
      id: 'delete',
      label: 'Delete',
      icon: 'pi pi-fw pi-trash',
      items: [
        {
          id: 'delete-not-found',
          label: 'With missing files',
          icon: 'pi pi-fw pi-trash color-bad',
          command: (e) => this.confirmDeleteNotFound(),
        },
      ],
    },
  ];

  constructor(
    private wordlistService: WordlistService,
    private settingsService: SettingsService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
  ) {}

  ngOnInit(): void {
    this.settingsService.getEnvironmentSettings().subscribe((envSettings) => {
      this.envSettings = envSettings;
      this.wordlistTypes = envSettings.wordlistTypes.map((t) => t.name);
      this.refreshWordlists();
    });
  }

  refreshWordlists() {
    if (this.envSettings === null) return;

    this.wordlistService.getAllWordlists().subscribe((wordlists) => {
      this.wordlists = wordlists;
    });
  }

  openAddWordlistModal() {
    this.addWordlistComponent?.reset();
    this.addWordlistModalVisible = true;
  }

  openUploadWordlistModal() {
    this.uploadWordlistComponent?.reset();
    this.uploadWordlistModalVisible = true;
  }

  openUpdateWordlistInfoModal(wordlist: WordlistDto) {
    this.selectedWordlist = wordlist;
    this.updateWordlistInfoModalVisible = true;
  }

  createWordlist(wordlist: CreateWordlistDto) {
    this.wordlistService.createWordlist(wordlist).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Added',
        detail: `Wordlist ${resp.name} was added`,
      });
      this.uploadWordlistModalVisible = false;
      this.addWordlistModalVisible = false;
      this.refreshWordlists();
    });
  }

  updateWordlistInfo(updated: UpdateWordlistInfoDto) {
    this.wordlistService.updateWordlistInfo(updated).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Updated',
        detail: `Wordlist ${resp.name} was updated`,
      });
      this.updateWordlistInfoModalVisible = false;
      this.refreshWordlists();
    });
  }

  confirmDeleteWordlist(wordlist: WordlistDto) {
    this.confirmationService.confirm({
      message: `In addition to deleting the wordlist, do you also
      want to delete the file that it references from disk? The original
      file will be permanently deleted from disk.`,
      header: 'Also delete the file?',
      acceptLabel: 'Yes, delete the file',
      rejectLabel: 'No, just the wordlist',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteWordlist(wordlist, true),
      reject: () => this.deleteWordlist(wordlist, false),
    });
  }

  deleteWordlist(wordlist: WordlistDto, alsoDeleteFile: boolean) {
    this.wordlistService.deleteWordlist(wordlist.id, alsoDeleteFile).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `Wordlist ${wordlist.name} was deleted${alsoDeleteFile ? ', along with its file' : ''}`,
      });
      this.refreshWordlists();
    });
  }

  confirmDeleteNotFound() {
    this.confirmationService.confirm({
      message: `You are about to delete all wordlists that reference 
      a file that can no longer be found (e.g. it was deleted).
      Are you sure that you want to proceed?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteNotFound(),
    });
  }

  deleteNotFound() {
    this.wordlistService.deleteNotFoundWordlists().subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `${resp.count} wordlists were deleted from the database`,
      });
      this.refreshWordlists();
    });
  }
}
