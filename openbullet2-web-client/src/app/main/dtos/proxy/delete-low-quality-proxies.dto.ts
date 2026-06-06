export interface DeleteLowQualityProxiesDto {
  proxyGroupId: number;
  deleteUnknown: boolean;
  deleteTransparent: boolean;
  deleteAnonymous: boolean;
}
