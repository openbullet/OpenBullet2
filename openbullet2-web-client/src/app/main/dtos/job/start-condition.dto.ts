export enum StartConditionType {
  Relative = 'relativeTimeStartCondition',
  Absolute = 'absoluteTimeStartCondition',
}

export interface RelativeTimeStartConditionDto {
  _polyTypeName: StartConditionType.Relative;
  startAfter: string;
}

export interface AbsoluteTimeStartConditionDto {
  _polyTypeName: StartConditionType.Absolute;
  startAt: string;
}
