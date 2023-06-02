export interface AddProxiesFromListDto {
    defaultType: string,
    defaultUsername: string,
    defaultPassword: string,
    proxyGroupId: number,
    proxies: string[]
}
