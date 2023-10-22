import { ProxyCheckTargetDto } from "./proxy-check-job.dto";

export interface ProxyCheckJobOptionsDto {
    name: string,
    bots: number,
    groupId: number,
    checkOnlyUntested: boolean,
    target: ProxyCheckTargetDto | null,
    timeoutMilliseconds: number,
    checkOutput: DatabaseProxyCheckOutput
}

export interface DatabaseProxyCheckOutput {
    _polyTypeName: 'databaseProxyCheckOutput',
}
