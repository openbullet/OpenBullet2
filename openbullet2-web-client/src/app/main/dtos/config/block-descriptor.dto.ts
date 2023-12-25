export enum VariableType {
    String = 'string',
    Int = 'int',
    Float = 'float',
    Bool = 'bool',
    ListOfString = 'listOfStrings',
    DictionaryOfString = 'dictionaryOfStrings',
    ByteArray = 'byteArray',
}

export enum ParamType {
    Bool = 'boolParam',
    Int = 'intParam',
    Float = 'floatParam',
    String = 'stringParam',
    ListOfString = 'listOfStringParam',
    DictionaryOfString = 'dictionaryOfStringParam',
    ByteArray = 'byteArrayParam',
    Enum = 'enumParam',
}

export enum SettingInputMode {
    Variable = 'variable',
    Fixed = 'fixed',
    Interpolated = 'interpolated',
}

export interface BlockCategoryDto {
    name: string;
    description: string;
    backgroundColor: string;
    foregroundColor: string;
}

export interface BlockParameterDto {
    name: string;
    assignedName: string;
    prettyName: string;
    description: string;
    inputMode: SettingInputMode;
    defaultVariableName: string;
}

export interface BoolBlockParameterDto extends BlockParameterDto {
    _polyTypeName: ParamType.Bool;
    defaultValue: boolean;
}

export interface IntBlockParameterDto extends BlockParameterDto {
    _polyTypeName: ParamType.Int;
    defaultValue: number;
}

export interface FloatBlockParameterDto extends BlockParameterDto {
    _polyTypeName: ParamType.Float;
    defaultValue: number;
}

export interface StringBlockParameterDto extends BlockParameterDto {
    _polyTypeName: ParamType.String;
    defaultValue: string;
}

export interface ListOfStringBlockParameterDto extends BlockParameterDto {
    _polyTypeName: ParamType.ListOfString;
    defaultValue: string[];
}

export interface DictionaryOfStringBlockParameterDto extends BlockParameterDto {
    _polyTypeName: ParamType.DictionaryOfString;
    defaultValue: { [key: string]: string };
}

export interface ByteArrayBlockParameterDto extends BlockParameterDto {
    _polyTypeName: ParamType.ByteArray;
    defaultValue: string;
}

export interface EnumBlockParameterDto extends BlockParameterDto {
    _polyTypeName: ParamType.Enum;
    type: string;
    defaultValue: string;
    options: string[];
}

export type BlockParameterTypes =
    BoolBlockParameterDto |
    IntBlockParameterDto |
    FloatBlockParameterDto |
    StringBlockParameterDto |
    ListOfStringBlockParameterDto |
    DictionaryOfStringBlockParameterDto |
    ByteArrayBlockParameterDto |
    EnumBlockParameterDto;

export interface BlockDescriptorDto {
    id: string;
    name: string;
    description: string;
    extraInfo: string;
    returnType: VariableType | null;
    category: BlockCategoryDto | null;
    parameters: { [key: string]: BlockParameterTypes };
}

export type BlockDescriptors = { [key: string]: BlockDescriptorDto };
