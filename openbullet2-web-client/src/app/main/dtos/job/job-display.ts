import { JobLastRunOutcome } from './job-last-run-outcome';
import { JobStatus } from './job-status';

type JobDisplayable = {
  status: JobStatus;
  lastRunOutcome: JobLastRunOutcome;
};

const statusColor: Record<JobStatus, string> = {
  idle: 'secondary',
  waiting: 'accent',
  starting: 'good',
  running: 'good',
  pausing: 'custom',
  paused: 'custom',
  stopping: 'bad',
  resuming: 'good',
};

const outcomeColor: Record<JobLastRunOutcome, string> = {
  none: 'secondary',
  completed: 'good',
  stopped: 'custom',
  aborted: 'bad',
  failed: 'bad',
};

export function getJobDisplayLabel(job: JobDisplayable): string {
  if (job.status === JobStatus.IDLE && job.lastRunOutcome !== JobLastRunOutcome.NONE) {
    return job.lastRunOutcome;
  }

  return job.status;
}

export function getJobDisplayColor(job: JobDisplayable): string {
  if (job.status === JobStatus.IDLE && job.lastRunOutcome !== JobLastRunOutcome.NONE) {
    return outcomeColor[job.lastRunOutcome];
  }

  return statusColor[job.status];
}
