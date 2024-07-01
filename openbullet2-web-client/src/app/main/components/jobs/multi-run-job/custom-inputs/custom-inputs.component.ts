import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CustomInputAnswerDto, CustomInputQuestionDto } from 'src/app/main/dtos/job/custom-inputs.dto';

@Component({
  selector: 'app-custom-inputs',
  templateUrl: './custom-inputs.component.html',
  styleUrls: ['./custom-inputs.component.scss']
})
export class CustomInputsComponent implements OnInit {
  @Input() jobId!: number;
  @Input() questions!: CustomInputQuestionDto[];
  @Output() confirm = new EventEmitter<CustomInputAnswerDto[]>();
  answers: CustomInputAnswerDto[] | null = null;

  ngOnInit(): void {
    this.answers = this.questions.map(q => {
      return {
        variableName: q.variableName,
        answer: q.currentAnswer ?? '',
      };
    });
  }

  setCustomInputs() {
    if (this.answers === null) {
      console.log('Answers are null, this should not happen!');
      return;
    }

    this.confirm.emit(this.answers);
  }
}
