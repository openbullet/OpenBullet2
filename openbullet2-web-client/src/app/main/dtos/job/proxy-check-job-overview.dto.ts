export interface ProxyCheckJobOverviewDto {
    id: number,
    ownerId: number,
    status: string,
    bots: number,
    total: number,
    tested: number,
    working: number,
    notWorking: number,
    cpm: number,
    progress: number
}
