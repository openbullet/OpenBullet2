import { JobStatus } from "../job/job-status";

export enum NumComparison {
    EqualTo = 'equalTo',
    NotEqualTo = 'notEqualTo',
    LessThan = 'lessThan',
    LessThanOrEqualTo = 'lessThanOrEqualTo',
    GreaterThan = 'greaterThan',
    GreaterThanOrEqualTo = 'greaterThanOrEqualTo'
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
    jobStatus: JobStatus;
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
    JobStatusTriggerDto |
    JobFinishedTriggerDto |
    TestedCountTriggerDto |
    HitCountTriggerDto |
    CustomCountTriggerDto |
    ToCheckCountTriggerDto |
    FailCountTriggerDto |
    RetryCountTriggerDto |
    BanCountTriggerDto |
    ErrorCountTriggerDto |
    AliveProxiesCountTriggerDto |
    BannedProxiesCountTriggerDto |
    CpmCountTriggerDto |
    CaptchaCreditTriggerDto |
    ProgressTriggerDto |
    TimeElapsedTriggerDto |
    TimeRemainingTriggerDto;
