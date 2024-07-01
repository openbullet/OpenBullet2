import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { HitDto } from 'src/app/main/dtos/hit/hit.dto';
import { UpdateHitDto } from 'src/app/main/dtos/hit/update-hit.dto';

@Component({
  selector: 'app-update-hit',
  templateUrl: './update-hit.component.html',
  styleUrls: ['./update-hit.component.scss'],
})
export class UpdateHitComponent implements OnChanges {
  @Input() hit: HitDto | null = null;
  @Input() hitTypes: string[] = ['SUCCESS', 'NONE'];
  @Output() confirm = new EventEmitter<UpdateHitDto>();
  data = '';
  capturedData = '';
  hitType = 'SUCCESS';

  faCircleQuestion = faCircleQuestion;

  ngOnChanges(changes: SimpleChanges) {
    this.hitTypes = this.hitTypes.filter((t) => t !== 'Any Type');

    if (this.hit === null) return;
    this.data = this.hit.data;
    this.capturedData = this.hit.capturedData;
    this.hitType = this.hit.type;
  }

  submitForm() {
    if (this.hit === null) {
      console.log('Hit is null, this should not happen!');
      return;
    }

    this.confirm.emit({
      id: this.hit.id,
      data: this.data,
      capturedData: this.capturedData,
      type: this.hitType,
    });
  }
}
