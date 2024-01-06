import { SettingInputMode } from "./block-descriptor.dto";

export enum BlockSettingType {
    None = 'none',
    Bool = 'bool',
    Int = 'int',
    Float = 'float',
    String = 'string',
    ListOfString = 'listOfString',
    DictionaryOfString = 'dictionaryOfString',
    ByteArray = 'byteArray',
    Enum = 'enum',
}

export enum BlockInstanceType {
    Auto = 'auto',
    HttpRequest = 'httpRequest',
    Keycheck = 'keycheck',
    Script = 'script',
    Parse = 'parse',
    LoliCode = 'loliCode',
}

export interface BlockSettingDto {
    name: string;
    inputVariableName: string | null;
    value: any;
    inputMode: SettingInputMode;
    type: BlockSettingType;
}

export interface BlockInstanceDto {
    id: string;
    disabled: boolean;
    label: string;
    settings: { [key: string]: BlockSettingDto };
    type: BlockInstanceType;
}

export interface AutoBlockInstanceDto extends BlockInstanceDto {
    outputVariable: string;
    isCapture: boolean;
    safe: boolean;
}

export type BlockInstanceTypes =
    AutoBlockInstanceDto;
