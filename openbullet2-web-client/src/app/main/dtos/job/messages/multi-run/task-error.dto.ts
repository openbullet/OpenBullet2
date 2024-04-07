import { MRJProxy } from './proxy.dto';

export interface MRJTaskErrorMessage {
  dataLine: string;
  proxy: MRJProxy | null;
  errorMessage: string;
}
