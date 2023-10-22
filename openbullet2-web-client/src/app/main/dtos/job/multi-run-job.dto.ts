import { AbsoluteTimeStartConditionDto, RelativeTimeStartConditionDto } from "./start-condition.dto"

export interface MultiRunJobDto {
    startCondition: RelativeTimeStartConditionDto | AbsoluteTimeStartConditionDto,
    config: JobConfigDto,
    dataPoolInfo: string,
    bots: number,
    skip: number,
    proxyMode: number,
    proxySources: string[],
    hitOutputs: string[],
    dataStats: MRJDataStatsDto,
    proxyStats: MRJProxyStatsDto,
    cpm: number,
    captchaCredit: number,
    elapsed: string,
    remaining: string,
    progress: number,
    hits: MRJHitDto[]
}

export interface JobConfigDto {
    id: string,
    base64Image: string,
    name: string,
    author: string
}

export interface MRJDataStatsDto {
    hits: number,
    custom: number,
    fails: number,
    invalid: number,
    retried: number,
    banned: number,
    errors: number,
    toCheck: number,
    total: number,
    tested: number
}

export interface MRJProxyStatsDto {
    total: number,
    alive: number,
    bad: number,
    banned: number
}

export interface MRJHitDto {
    id: string, // temp id of the hit in memory
    date: string,
    type: string,
    data: string,
    capturedData: string
}
