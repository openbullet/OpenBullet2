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

export interface BlockSettingDto {
    name: string;
    value: any;
    inputMode: SettingInputMode;
    type: BlockSettingType;
}

export interface BlockInstanceDto {
    id: string;
    disabled: boolean;
    label: string;
    settings: { [key: string]: BlockSettingDto };
}

export interface AutoBlockInstanceDto extends BlockInstanceDto {
    outputVariable: string;
    isCapture: boolean;
    safe: boolean;
}

export type BlockInstanceTypes =
    AutoBlockInstanceDto;
