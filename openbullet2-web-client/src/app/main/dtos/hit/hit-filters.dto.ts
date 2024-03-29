export enum HitSortField {
    Data = 'data',
    ConfigName = 'configName',
    Date = 'date',
    WordlistName = 'wordlistName',
    Proxy = 'proxy',
    CapturedData = 'capturedData',
}

export interface ListHitFiltersDto {
    pageNumber: number | null;
    pageSize: number | null;
    searchTerm: string | null;
    configName: string | null;
    type: string | null;
    minDate: string | null;
    maxDate: string | null,
    sortBy: HitSortField | null;
    sortDescending: boolean;
}
