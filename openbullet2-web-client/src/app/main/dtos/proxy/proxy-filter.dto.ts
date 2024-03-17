import { ProxyType } from "../../enums/proxy-type";
import { ProxyWorkingStatus } from "../../enums/proxy-working-status";

export interface ProxyFiltersDto {
    pageNumber: number | null;
    pageSize: number | null;
    proxyGroupId: number;
    searchTerm: string | null;
    type: ProxyType | null;
    status: ProxyWorkingStatus | null
}
