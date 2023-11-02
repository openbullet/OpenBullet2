import { JobStatus } from "./job-status"
import { AbsoluteTimeStartConditionDto, RelativeTimeStartConditionDto } from "./start-condition.dto"

export interface ProxyCheckJobDto {
    id: number;
    ownerId: number;
    status: JobStatus;
    name: string;
    bots: number;
    startCondition: RelativeTimeStartConditionDto | AbsoluteTimeStartConditionDto;
    groupId: number;
    groupName: string;
    checkOnlyUntested: boolean;
    target: ProxyCheckTargetDto | null;
    timeoutMilliseconds: number;
    checkOutput: any; // TODO: Polymorphic
    total: number;
    tested: number;
    working: number;
    notWorking: number;
    cpm: number;
    elapsed: string;
    remaining: string;
    progress: number
}

export interface ProxyCheckTargetDto {
    url: string;
    successKey: string
}
