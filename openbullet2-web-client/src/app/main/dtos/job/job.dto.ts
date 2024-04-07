import { JobStatus } from './job-status';

export enum JobType {
  MultiRun = 'multiRun',
  ProxyCheck = 'proxyCheck',
}

export interface JobDto {
  id: number;
  ownerId: number;
  type: JobType;
  status: JobStatus;
  name: string;
  startTime: string | null;
}

export interface JobOverviewDto {
  id: number;
  ownerId: number;
  type: JobType;
  status: JobStatus;
  name: string;
}
