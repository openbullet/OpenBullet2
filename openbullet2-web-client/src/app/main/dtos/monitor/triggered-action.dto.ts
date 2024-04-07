import { JobType } from '../job/job.dto';
import { ActionDto } from './action.dto';
import { TriggerDto } from './trigger.dto';

export interface TriggeredActionDto {
  id: string;
  name: string;
  isActive: boolean;
  isRepeatable: boolean;
  executions: number;
  jobId: number;
  jobName: string;
  jobType: JobType;
  triggers: TriggerDto[];
  actions: ActionDto[];
}

export interface CreateTriggeredActionDto {
  name: string;
  isActive: boolean;
  isRepeatable: boolean;
  jobId: number;
  triggers: TriggerDto[];
  actions: ActionDto[];
}

export interface UpdateTriggeredActionDto {
  id: string;
  name: string;
  isActive: boolean;
  isRepeatable: boolean;
  jobId: number;
  triggers: TriggerDto[];
  actions: ActionDto[];
}
