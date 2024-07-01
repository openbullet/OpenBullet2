import { JobStatus } from '../job-status';

export interface JobStatusChangedMessage {
  newStatus: JobStatus;
}
