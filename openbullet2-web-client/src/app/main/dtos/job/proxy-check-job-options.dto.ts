import { ProxyCheckTargetDto } from "./proxy-check-job.dto";

export interface ProxyCheckJobOptionsDto {
    bots: number,
    groupId: number,
    checkOnlyUntested: boolean,
    target: ProxyCheckTargetDto | null,
    timeoutMilliseconds: number,
    checkOutput: any, // TODO: Polymorphic
}
