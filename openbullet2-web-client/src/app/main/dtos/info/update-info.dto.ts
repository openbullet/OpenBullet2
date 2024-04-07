export enum VersionType {
  Alpha = 'alpha',
  Beta = 'beta',
  Release = 'release',
}

export interface UpdateInfoDto {
  currentVersion: string;
  remoteVersion: string;
  isUpdateAvailable: boolean;
  currentVersionType: VersionType;
  remoteVersionType: VersionType;
}
