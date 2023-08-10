export interface MultiRunJobOverviewDto {
    id: number,
    ownerId: number,
    status: string,
    configName: string,
    dataPoolInfo: string,
    bots: number,
    proxyMode: string,
    dataHits: number,
    dataCustom: number,
    dataToCheck: number,
    dataTotal: number,
    dataTested: number,
    cpm: number,
    progress: number
}
