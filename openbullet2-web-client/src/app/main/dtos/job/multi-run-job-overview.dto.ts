import { JobStatus } from "./job-status";

export interface MultiRunJobOverviewDto {
    id: number;
    ownerId: number;
    status: JobStatus;
    configName: string;
    dataPoolInfo: string;
    bots: number;
    proxyMode: string;
    dataHits: number;
    dataCustom: number;
    dataToCheck: number;
    dataTotal: number;
    dataTested: number;
    cpm: number;
    progress: number
}
