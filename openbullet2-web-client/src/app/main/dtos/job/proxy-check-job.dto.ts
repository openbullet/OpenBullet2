export interface ProxyCheckJobDto {
    bots: number,
    groupId: number,
    checkOnlyUntested: boolean,
    target: ProxyCheckTargetDto | null,
    timeoutMilliseconds: number,
    checkOutput: any, // TODO: Polymorphic
    total: number,
    tested: number,
    working: number,
    notWorking: number,
    cpm: number,
    elapsed: string,
    remaining: string,
    progress: number
}

export interface ProxyCheckTargetDto {
    url: string,
    successKey: string
}
