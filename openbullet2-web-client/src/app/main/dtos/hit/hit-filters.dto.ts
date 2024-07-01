export enum HitSortField {
  Data = 'data',
  ConfigName = 'configName',
  Date = 'date',
  WordlistName = 'wordlistName',
  Proxy = 'proxy',
  CapturedData = 'capturedData',
}

export interface PaginatedHitFiltersDto {
  pageNumber: number | null;
  pageSize: number | null;
  searchTerm: string | null;
  configName: string | null;
  types: string | null;
  minDate: string | null;
  maxDate: string | null;
  sortBy: HitSortField | null;
  sortDescending: boolean;
}

export interface HitFiltersDto {
  searchTerm: string | null;
  configName: string | null;
  types: string | null;
  minDate: string | null;
  maxDate: string | null;
  sortBy: HitSortField | null;
  sortDescending: boolean;
}
