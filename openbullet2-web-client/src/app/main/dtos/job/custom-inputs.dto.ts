export interface CustomInputQuestionDto {
  description: string;
  variableName: string;
  defaultAnswer: string;
}

export interface CustomInputsDto {
  id: number;
  inputs: CustomInputAnswerDto[];
}

export interface CustomInputAnswerDto {
  variableName: string;
  answer: string;
}
