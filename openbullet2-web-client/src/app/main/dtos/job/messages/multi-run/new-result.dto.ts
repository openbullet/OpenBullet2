import { MRJProxy } from './proxy.dto';

export interface MRJNewResultMessage {
  dataLine: string;
  proxy: MRJProxy | null;
  status: string;
}
