export interface UpdateGuestInfoDto {
  id: number;
  username: string;
  accessExpiration: string;
  allowedAddresses: string[];
}
