export interface CustomInputQuestionDto {
  description: string;
  variableName: string;
  defaultAnswer: string;
  currentAnswer: string | null;
}

export interface CustomInputsDto {
  jobId: number;
  answers: CustomInputAnswerDto[];
}

export interface CustomInputAnswerDto {
  variableName: string;
  answer: string;
}
