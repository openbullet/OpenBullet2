export interface RecentHitsDto {
  dates: string[];
  hits: { [id: string]: number[] };
}
