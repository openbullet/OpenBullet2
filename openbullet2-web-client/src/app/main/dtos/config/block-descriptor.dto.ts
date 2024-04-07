export enum VariableType {
  String = 'string',
  Int = 'int',
  Float = 'float',
  Bool = 'bool',
  ListOfStrings = 'listOfStrings',
  DictionaryOfStrings = 'dictionaryOfStrings',
  ByteArray = 'byteArray',
}

export enum ParamType {
  Bool = 'boolParam',
  Int = 'intParam',
  Float = 'floatParam',
  String = 'stringParam',
  ListOfStrings = 'listOfStringsParam',
  DictionaryOfStrings = 'dictionaryOfStringsParam',
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

export interface ListOfStringsBlockParameterDto extends BlockParameterDto {
  _polyTypeName: ParamType.ListOfStrings;
  defaultValue: string[];
}

export interface DictionaryOfStringsBlockParameterDto extends BlockParameterDto {
  _polyTypeName: ParamType.DictionaryOfStrings;
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
  | BoolBlockParameterDto
  | IntBlockParameterDto
  | FloatBlockParameterDto
  | StringBlockParameterDto
  | ListOfStringsBlockParameterDto
  | DictionaryOfStringsBlockParameterDto
  | ByteArrayBlockParameterDto
  | EnumBlockParameterDto;

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
