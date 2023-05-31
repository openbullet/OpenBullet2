export interface MoveProxiesDto {
    pageNumber: number | null,
    pageSize: number | null,
    proxyGroupId: number,
    searchTerm: string | null,
    type: string | null,
    status: string | null,
    destinationGroupId: number
}
