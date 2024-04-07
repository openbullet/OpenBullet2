export interface PagedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
}
