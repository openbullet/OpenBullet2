export interface EnvironmentSettingsDto {
  wordlistTypes: WordlistType[];
  customStatuses: CustomStatus[];
  exportFormats: ExportFormat[];
}

export interface WordlistType {
  name: string;
  regex: string;
  verify: boolean;
  separator: string;
  slices: string[];
  slicesAlias: string[];
}

export interface CustomStatus {
  name: string;
  color: string;
}

export interface ExportFormat {
  format: string;
}
