export enum ConfigMode {
  Stack = 'stack',
  LoliCode = 'loliCode',
  CSharp = 'cSharp',
  DLL = 'dll',
  Legacy = 'legacy',
}

export interface ConfigInfoDto {
  id: string;
  name: string;
  base64Image: string;
  author: string;
  category: string;
  isRemote: boolean;
  needsProxies: boolean;
  allowedWordlistTypes: string[];
  creationDate: string;
  lastModified: string;
  mode: ConfigMode;
  dangerous: boolean;
  suggestedBots: number;
}
