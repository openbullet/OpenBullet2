import { TimeSpanPipe } from 'src/app/shared/pipes/timespan.pipe';
import { parseTimeSpan } from 'src/app/shared/utils/dates';
import { JobStatus } from '../job/job-status';

export enum NumComparison {
  EqualTo = 'equalTo',
  NotEqualTo = 'notEqualTo',
  LessThan = 'lessThan',
  LessThanOrEqualTo = 'lessThanOrEqualTo',
  GreaterThan = 'greaterThan',
  GreaterThanOrEqualTo = 'greaterThanOrEqualTo',
}

interface NumComparisonTriggerDto {
  comparison: NumComparison;
  amount: number;
}

interface TimeComparisonTriggerDto {
  comparison: NumComparison;
  timeSpan: string;
}

export enum TriggerType {
  JobStatus = 'jobStatusTrigger',
  JobFinished = 'jobFinishedTrigger',
  TestedCount = 'testedCountTrigger',
  HitCount = 'hitCountTrigger',
  CustomCount = 'customCountTrigger',
  ToCheckCount = 'toCheckCountTrigger',
  FailCount = 'failCountTrigger',
  RetryCount = 'retryCountTrigger',
  BanCount = 'banCountTrigger',
  ErrorCount = 'errorCountTrigger',
  AliveProxiesCount = 'aliveProxiesCountTrigger',
  BannedProxiesCount = 'bannedProxiesCountTrigger',
  CpmCount = 'cpmCountTrigger',
  CaptchaCredit = 'captchaCreditTrigger',
  Progress = 'progressTrigger',
  TimeElapsed = 'timeElapsedTrigger',
  TimeRemaining = 'timeRemainingTrigger',
}

export interface JobStatusTriggerDto {
  _polyTypeName: TriggerType.JobStatus;
  status: JobStatus;
}

export interface JobFinishedTriggerDto {
  _polyTypeName: TriggerType.JobFinished;
}

export interface TestedCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.TestedCount;
}

export interface HitCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.HitCount;
}

export interface CustomCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.CustomCount;
}

export interface ToCheckCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.ToCheckCount;
}

export interface FailCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.FailCount;
}

export interface RetryCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.RetryCount;
}

export interface BanCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.BanCount;
}

export interface ErrorCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.ErrorCount;
}

export interface AliveProxiesCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.AliveProxiesCount;
}

export interface BannedProxiesCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.BannedProxiesCount;
}

export interface CpmCountTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.CpmCount;
}

export interface CaptchaCreditTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.CaptchaCredit;
}

export interface ProgressTriggerDto extends NumComparisonTriggerDto {
  _polyTypeName: TriggerType.Progress;
}

export interface TimeElapsedTriggerDto extends TimeComparisonTriggerDto {
  _polyTypeName: TriggerType.TimeElapsed;
}

export interface TimeRemainingTriggerDto extends TimeComparisonTriggerDto {
  _polyTypeName: TriggerType.TimeRemaining;
}

export type TriggerDto =
  | JobStatusTriggerDto
  | JobFinishedTriggerDto
  | TestedCountTriggerDto
  | HitCountTriggerDto
  | CustomCountTriggerDto
  | ToCheckCountTriggerDto
  | FailCountTriggerDto
  | RetryCountTriggerDto
  | BanCountTriggerDto
  | ErrorCountTriggerDto
  | AliveProxiesCountTriggerDto
  | BannedProxiesCountTriggerDto
  | CpmCountTriggerDto
  | CaptchaCreditTriggerDto
  | ProgressTriggerDto
  | TimeElapsedTriggerDto
  | TimeRemainingTriggerDto;

function getComparisonText(comparison: NumComparison): string {
  switch (comparison) {
    case NumComparison.EqualTo:
      return '=';
    case NumComparison.NotEqualTo:
      return '!=';
    case NumComparison.LessThan:
      return '<';
    case NumComparison.LessThanOrEqualTo:
      return '<=';
    case NumComparison.GreaterThan:
      return '>';
    case NumComparison.GreaterThanOrEqualTo:
      return '>=';
  }
}

export function getTriggerText(trigger: TriggerDto): string {
  const timeSpanPipe = new TimeSpanPipe();

  switch (trigger._polyTypeName) {
    case TriggerType.JobStatus:
      return `Job status is ${trigger.status}`;
    case TriggerType.JobFinished:
      return 'Job finished';
    case TriggerType.TestedCount:
      return `Tested count ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.HitCount:
      return `Hit count ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.CustomCount:
      return `Custom count ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.ToCheckCount:
      return `To check count ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.FailCount:
      return `Fail count ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.RetryCount:
      return `Retry count ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.BanCount:
      return `Ban count ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.ErrorCount:
      return `Error count ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.AliveProxiesCount:
      return `Alive proxies count ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.BannedProxiesCount:
      return `Banned proxies count ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.CpmCount:
      return `CPM ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.CaptchaCredit:
      return `Captcha credit ${getComparisonText(trigger.comparison)} ${trigger.amount}`;
    case TriggerType.Progress:
      return `Progress ${getComparisonText(trigger.comparison)} ${trigger.amount}%`;
    case TriggerType.TimeElapsed:
      return `Time elapsed ${getComparisonText(trigger.comparison)} ${timeSpanPipe.transform(
        parseTimeSpan(trigger.timeSpan),
      )}`;
    case TriggerType.TimeRemaining:
      return `Time remaining ${getComparisonText(trigger.comparison)} ${timeSpanPipe.transform(
        parseTimeSpan(trigger.timeSpan),
      )}`;
    default:
      return 'Unknown trigger';
  }
}

export function getComparisonSubject(type: TriggerType): string {
  switch (type) {
    case TriggerType.TestedCount:
      return 'number of tested data lines';
    case TriggerType.HitCount:
      return 'number of hits';
    case TriggerType.CustomCount:
      return 'number of custom results';
    case TriggerType.ToCheckCount:
      return 'number of results to check';
    case TriggerType.FailCount:
      return 'number of fails';
    case TriggerType.RetryCount:
      return 'number of retries';
    case TriggerType.BanCount:
      return 'number of bans';
    case TriggerType.ErrorCount:
      return 'number of errors';
    case TriggerType.AliveProxiesCount:
      return 'number of alive proxies';
    case TriggerType.BannedProxiesCount:
      return 'number of banned proxies';
    case TriggerType.CpmCount:
      return 'number of Checks Per Minute';
    case TriggerType.CaptchaCredit:
      return 'captcha credit';
    case TriggerType.Progress:
      return 'progress';
    case TriggerType.TimeElapsed:
      return 'elapsed time';
    case TriggerType.TimeRemaining:
      return 'remaining time';
    default:
      return '???';
  }
}
