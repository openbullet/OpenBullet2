import { JobStatus } from "./job-status";

export interface ProxyCheckJobOverviewDto {
    id: number;
    ownerId: number;
    status: JobStatus;
    bots: number;
    total: number;
    tested: number;
    working: number;
    notWorking: number;
    cpm: number;
    progress: number
}
