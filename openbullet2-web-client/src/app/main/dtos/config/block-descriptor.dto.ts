import { Observable } from "rxjs";

export enum VariableType {
    String = 'string',
    Int = 'int',
    Float = 'float',
    Bool = 'bool',
    ListOfString = 'listOfStrings',
    DictionaryOfString = 'dictionaryOfStrings',
    ByteArray = 'byteArray',
}

export interface BlockCategoryDto {
    name: string;
    path: string;
    namespace: string;
    description: string;
    backgroundColor: string;
    foregroundColor: string;
}

export interface BlockDescriptorDto {
    id: string;
    name: string;
    description: string;
    extraInfo: string;
    returnType: VariableType | null;
    category: BlockCategoryDto | null;
    parameters: { [key: string]: any };
}

export type BlockDescriptors = { [key: string]: BlockDescriptorDto };
