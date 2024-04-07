export interface CreateGuestDto {
  username: string;
  password: string;
  accessExpiration: string;
  allowedAddresses: string[];
}
