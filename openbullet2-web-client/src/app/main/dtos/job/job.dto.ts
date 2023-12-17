import { JobStatus } from "./job-status";

export interface JobDto {
    id: number;
    ownerId: number;
    status: JobStatus;
    name: string;
    startTime: string | null;
}

export interface JobOverviewDto {
    id: number;
    ownerId: number;
    status: JobStatus;
    name: string;
}
