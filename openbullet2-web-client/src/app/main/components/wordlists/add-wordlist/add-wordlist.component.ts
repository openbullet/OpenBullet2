import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CreateWordlistDto } from 'src/app/main/dtos/wordlist/create-wordlist.dto';
import { UserService } from 'src/app/main/services/user.service';

@Component({
  selector: 'app-add-wordlist',
  templateUrl: './add-wordlist.component.html',
  styleUrls: ['./add-wordlist.component.scss'],
})
export class AddWordlistComponent {
  @Input() wordlistTypes: string[] = ['Default'];
  @Output() confirm = new EventEmitter<CreateWordlistDto>();
  name = '';
  purpose = '';
  wordlistType = 'Default';
  filePath = '';
  isCreating = false;
  isAdmin: boolean;

  constructor(private userService: UserService) {
    this.isAdmin = this.userService.isAdmin();
  }

  public reset() {
    this.name = '';
    this.purpose = '';
    this.wordlistType = 'Default';
    this.filePath = '';
    this.isCreating = false;
  }

  submitForm() {
    this.isCreating = true;
    this.confirm.emit({
      name: this.name,
      purpose: this.purpose,
      wordlistType: this.wordlistType,
      // Replace starting and ending double quotes since when copying the
      // path from windows explorer it will copy the quotes as well
      filePath: this.filePath.replace(/^"/, '').replace(/"$/, ''),
    });
  }

  isFormValid() {
    return this.name.length > 0 && this.filePath.length > 0;
  }

  formatFilePath() {
    this.filePath = this.filePath
      .replace(/\\/g, '/')
      .replace(/^"/, '')
      .replace(/"$/, '');
  }
}
