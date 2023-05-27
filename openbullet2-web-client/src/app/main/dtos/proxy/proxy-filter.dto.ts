export interface ProxyFilterDto {
    pageNumber: number,
    pageSize: number,
    proxyGroupId: number,
    searchTerm: string | null,
    type: string | null,
    status: string | null
}
