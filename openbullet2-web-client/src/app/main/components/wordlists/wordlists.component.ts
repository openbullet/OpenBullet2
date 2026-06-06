import { Component, OnInit, ViewChild } from '@angular/core';
import { faFileLines, faFilterCircleXmark, faPen, faX } from '@fortawesome/free-solid-svg-icons';
import { ConfirmationService, MenuItem, MessageService, TableState } from 'primeng/api';
import { Table, TablePageEvent } from 'primeng/table';
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

  @ViewChild('wordlistsDt')
  wordlistsDt: Table | undefined = undefined;

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
  tableFirst = 0;
  tableRows = 10;
  searchTerm = '';

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
      this.reapplyTableState();
    });
  }

  onTablePageChange(event: TablePageEvent) {
    this.tableFirst = event.first;
    this.tableRows = event.rows;
  }

  onTableStateRestore(state: TableState) {
    this.tableFirst = state.first ?? 0;
    this.tableRows = state.rows ?? 10;

    const globalFilter = state.filters?.['global'];
    if (globalFilter && !Array.isArray(globalFilter) && globalFilter.value) {
      this.searchTerm = String(globalFilter.value);
    }
  }

  applyGlobalFilter() {
    this.tableFirst = 0;
    this.wordlistsDt?.filterGlobal(this.searchTerm, 'contains');
  }

  clearTable() {
    this.searchTerm = '';
    this.tableFirst = 0;
    this.wordlistsDt?.clear();
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

      if (this.wordlists !== null) {
        this.wordlists = this.wordlists.filter((w) => w.id !== wordlist.id);
      }

      this.reapplyTableState();
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

  private reapplyTableState() {
    setTimeout(() => {
      if (!this.wordlistsDt) {
        return;
      }

      if (this.searchTerm) {
        this.wordlistsDt.filterGlobal(this.searchTerm, 'contains');
      }

      const filteredCount = this.wordlistsDt.filteredValue?.length ?? this.wordlists?.length ?? 0;

      if (filteredCount === 0) {
        this.tableFirst = 0;
        return;
      }

      const maxFirst = Math.floor((filteredCount - 1) / this.tableRows) * this.tableRows;
      this.tableFirst = Math.min(this.tableFirst, maxFirst);
    });
  }
}
