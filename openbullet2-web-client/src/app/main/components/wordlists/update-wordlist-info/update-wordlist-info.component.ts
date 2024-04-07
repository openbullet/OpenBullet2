import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { UpdateWordlistInfoDto } from 'src/app/main/dtos/wordlist/update-wordlist-info.dto';
import { WordlistDto } from 'src/app/main/dtos/wordlist/wordlist.dto';

@Component({
  selector: 'app-update-wordlist-info',
  templateUrl: './update-wordlist-info.component.html',
  styleUrls: ['./update-wordlist-info.component.scss'],
})
export class UpdateWordlistInfoComponent implements OnChanges {
  @Input() wordlist: WordlistDto | null = null;
  @Input() wordlistTypes: string[] = ['Default'];
  @Output() confirm = new EventEmitter<UpdateWordlistInfoDto>();
  name = '';
  purpose = '';
  wordlistType = 'Default';

  faCircleQuestion = faCircleQuestion;

  ngOnChanges(changes: SimpleChanges) {
    if (this.wordlist === null) return;
    this.name = this.wordlist.name;
    this.purpose = this.wordlist.purpose;
    this.wordlistType = this.wordlist.wordlistType;
  }

  submitForm() {
    if (this.wordlist === null) {
      console.log('Wordlist is null, this should not happen!');
      return;
    }

    this.confirm.emit({
      id: this.wordlist.id,
      name: this.name,
      purpose: this.purpose,
      wordlistType: this.wordlistType,
    });
  }

  isFormValid() {
    return this.name.length > 0;
  }
}
