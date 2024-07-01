export interface PCJStatsMessage {
  tested: number;
  working: number;
  notWorking: number;
  cpm: number;
  elapsed: string;
  remaining: string;
  progress: number;
}
