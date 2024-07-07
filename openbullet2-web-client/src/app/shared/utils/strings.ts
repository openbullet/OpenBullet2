export function randomString(length: number) {
  let result = '';
  const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  const charactersLength = characters.length;
  let counter = 0;
  while (counter < length) {
    result += characters.charAt(Math.floor(Math.random() * charactersLength));
    counter += 1;
  }
  return result;
}

export function generateUuidv4() {
  const str = () => (`00000000000000000${(Math.random() * 0xffffffffffffffff).toString(16)}`).slice(-16);
  const a = str();
  const b = str();
  return `${a.slice(0, 8)}-${a.slice(8, 12)}-4${a.slice(13)}-${b.slice(0, 4)}-${b.slice(4)}`;
}
