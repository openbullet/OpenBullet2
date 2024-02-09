export interface HitFiltersDto {
    pageNumber: number | null;
    pageSize: number | null;
    searchTerm: string | null;
    type: string | null;
    minDate: string | null;
    maxDate: string | null,
}

export interface ListHitFiltersDto {
    pageNumber: number | null;
    pageSize: number | null;
    searchTerm: string | null;
    type: string | null;
    minDate: string | null;
    maxDate: string | null,
    sortBy: string | null;
    sortDescending: boolean;
}
