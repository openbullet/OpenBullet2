export interface ServerInfoDto {
  localUtcOffset: string;
  startTime: string;
  operatingSystem: string;
  currentWorkingDirectory: string;
  buildNumber: string;
  buildDate: Date;
  clientIpAddress: string;
}
