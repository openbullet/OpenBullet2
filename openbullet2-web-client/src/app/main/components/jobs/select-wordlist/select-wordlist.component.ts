import { Component, EventEmitter, Output } from '@angular/core';
import * as moment from 'moment';
import { WordlistPreviewDto } from 'src/app/main/dtos/wordlist/wordlist-preview.dto';
import { WordlistDto } from 'src/app/main/dtos/wordlist/wordlist.dto';
import { WordlistService } from 'src/app/main/services/wordlist.service';

@Component({
  selector: 'app-select-wordlist',
  templateUrl: './select-wordlist.component.html',
  styleUrls: ['./select-wordlist.component.scss'],
})
export class SelectWordlistComponent {
  @Output() confirm = new EventEmitter<WordlistDto>();

  moment = moment;
  wordlists: WordlistDto[] | null = null;
  filteredWordlists: WordlistDto[] | null = null;
  searchTerm = '';
  selectedWordlist: WordlistDto | null = null;
  preview: WordlistPreviewDto | null = null;

  constructor(private wordlistService: WordlistService) {}

  public refresh() {
    this.wordlists = null;
    this.wordlistService.getAllWordlists().subscribe((wordlists) => {
      this.wordlists = wordlists;
      this.filterWordlists();
    });
  }

  selectWordlist(wordlist: WordlistDto) {
    if (wordlist === this.selectedWordlist) {
      return;
    }

    this.preview = null;
    this.selectedWordlist = wordlist;
    this.wordlistService.getWordlistPreview(wordlist.id, 10).subscribe((preview) => {
      this.preview = preview;
    });
  }

  chooseWordlist(wordlist: WordlistDto) {
    this.confirm.emit(wordlist);
  }

  searchBoxKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      this.filterWordlists();
    }
  }

  filterWordlists() {
    if (this.wordlists === null) {
      return;
    }

    this.filteredWordlists = this.wordlists.filter((wordlist) =>
      wordlist.name.toLowerCase().includes(this.searchTerm.toLowerCase()),
    );
  }
}
