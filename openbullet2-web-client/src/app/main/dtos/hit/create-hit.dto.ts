export interface CreateHitDto {
  data: string;
  capturedData: string;
  proxy: string | null;
  date: string;
  type: string;
  configId: string | null;
  configName: string | null;
  configCategory: string | null;
  wordlistId: number; // -1 if no wordlist
  wordlistName: string | null;
}
