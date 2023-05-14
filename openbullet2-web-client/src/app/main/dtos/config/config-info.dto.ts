export interface ConfigInfoDto {
    id: string,
    name: string,
    base64Image: string,
    author: string,
    category: string,
    isRemote: boolean,
    needsProxies: boolean,
    allowedWordlistTypes: string[],
    creationDate: string,
    lastModified: string,
    mode: string
}
