import { ProxyType } from "src/app/main/enums/proxy-type";

export interface MRJProxy {
  type: ProxyType;
  host: string;
  port: number;
  username: string | null;
  password: string | null;
}
