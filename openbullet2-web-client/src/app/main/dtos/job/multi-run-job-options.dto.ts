import { AbsoluteTimeStartConditionDto, RelativeTimeStartConditionDto } from "./start-condition.dto";

export interface MultiRunJobOptionsDto {
    name: string,
    startCondition: RelativeTimeStartConditionDto | AbsoluteTimeStartConditionDto,
    configId: string,
    bots: number,
    skip: number,
    proxyMode: string,
    shuffleProxies: boolean,
    noValidProxyBehaviour: string,
    proxyBanTimeSeconds: number,
    markAsToCheckOnAbort: boolean,
    neverBanProxies: boolean,
    concurrentProxyMode: boolean,
    periodicReloadIntervalSeconds: number,
    dataPool: any, // TODO: Polymorphic
    proxySources: any[], // TODO: Polymorphic
    hitOutputs: any[], // TODO: Polymorphic
}
