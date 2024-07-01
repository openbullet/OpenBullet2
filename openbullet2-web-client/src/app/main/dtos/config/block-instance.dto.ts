import { SettingInputMode, VariableType } from './block-descriptor.dto';

export enum BlockSettingType {
  None = 'none',
  Bool = 'bool',
  Int = 'int',
  Float = 'float',
  String = 'string',
  ListOfStrings = 'listOfStrings',
  DictionaryOfStrings = 'dictionaryOfStrings',
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

export enum ParseMode {
  LR = 'lr',
  CSS = 'css',
  XPath = 'xPath',
  Json = 'json',
  Regex = 'regex',
}

export enum Interpreter {
  Jint = 'jint',
  NodeJS = 'nodeJS',
  IronPython = 'ironPython',
}

export enum KeychainMode {
  Or = 'or',
  And = 'and',
}

export enum KeyType {
  Bool = 'boolKey',
  Dictionary = 'dictionaryKey',
  List = 'listKey',
  Int = 'intKey',
  Float = 'floatKey',
  String = 'stringKey',
}

export enum BoolComparison {
  Is = 'is',
  IsNot = 'isNot',
}

export enum DictComparison {
  HasKey = 'hasKey',
  DoesNotHaveKey = 'doesNotHaveKey',
  HasValue = 'hasValue',
  DoesNotHaveValue = 'doesNotHaveValue',
  Exists = 'exists',
  DoesNotExist = 'doesNotExist',
}

export enum ListComparison {
  Contains = 'contains',
  DoesNotContain = 'doesNotContain',
  Exists = 'exists',
  DoesNotExist = 'doesNotExist',
}

export enum NumComparison {
  EqualTo = 'equalTo',
  NotEqualTo = 'notEqualTo',
  LessThan = 'lessThan',
  LessThanOrEqualTo = 'lessThanOrEqualTo',
  GreaterThan = 'greaterThan',
  GreaterThanOrEqualTo = 'greaterThanOrEqualTo',
}

export enum StrComparison {
  EqualTo = 'equalTo',
  NotEqualTo = 'notEqualTo',
  Contains = 'contains',
  DoesNotContain = 'doesNotContain',
  Exists = 'exists',
  DoesNotExist = 'doesNotExist',
  MatchesRegex = 'matchesRegex',
  DoesNotMatchRegex = 'doesNotMatchRegex',
}

export enum RequestParamsType {
  Standard = 'standardRequestParams',
  Raw = 'rawRequestParams',
  BasicAuth = 'basicAuthRequestParams',
  Multipart = 'multipartRequestParams',
}

export enum MultipartContentType {
  String = 'multipartString',
  Raw = 'multipartRaw',
  File = 'multipartFile',
}

export interface OutputVariable {
  type: VariableType;
  name: string;
}

export interface BlockSettingDto {
  name: string;
  inputVariableName: string | null;
  // biome-ignore lint/suspicious/noExplicitAny: any is needed here because the value can be of any type
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
  type: BlockInstanceType.Auto;
}

export interface ParseBlockInstanceDto extends BlockInstanceDto {
  outputVariable: string;
  recursive: boolean;
  isCapture: boolean;
  safe: boolean;
  mode: ParseMode;
  type: BlockInstanceType.Parse;
}

export interface LoliCodeBlockInstanceDto extends BlockInstanceDto {
  script: string;
  type: BlockInstanceType.LoliCode;
}

export interface ScriptBlockInstanceDto extends BlockInstanceDto {
  script: string;
  inputVariables: string; // Comma separated list of input variables
  interpreter: Interpreter;
  outputVariables: OutputVariable[];
  type: BlockInstanceType.Script;
}

export interface KeyDto {
  left: BlockSettingDto;
  right?: BlockSettingDto;
}

export interface BoolKeyDto extends KeyDto {
  _polyTypeName: KeyType.Bool;
  comparison: BoolComparison;
}

export interface DictionaryKeyDto extends KeyDto {
  _polyTypeName: KeyType.Dictionary;
  comparison: DictComparison;
}

export interface ListKeyDto extends KeyDto {
  _polyTypeName: KeyType.List;
  comparison: ListComparison;
}

export interface IntKeyDto extends KeyDto {
  _polyTypeName: KeyType.Int;
  comparison: NumComparison;
}

export interface FloatKeyDto extends KeyDto {
  _polyTypeName: KeyType.Float;
  comparison: NumComparison;
}

export interface StringKeyDto extends KeyDto {
  _polyTypeName: KeyType.String;
  comparison: StrComparison;
}

export type KeyTypes = BoolKeyDto | DictionaryKeyDto | ListKeyDto | IntKeyDto | FloatKeyDto | StringKeyDto;

export interface KeychainDto {
  keys: KeyTypes[];
  mode: KeychainMode;
  resultStatus: string;
}

export interface KeycheckBlockInstanceDto extends BlockInstanceDto {
  keychains: KeychainDto[];
  type: BlockInstanceType.Keycheck;
}

export interface StandardRequestParamsDto {
  _polyTypeName: RequestParamsType.Standard;
  content: BlockSettingDto;
  contentType: BlockSettingDto;
}

export interface RawRequestParamsDto {
  _polyTypeName: RequestParamsType.Raw;
  content: BlockSettingDto;
  contentType: BlockSettingDto;
}

export interface BasicAuthRequestParamsDto {
  _polyTypeName: RequestParamsType.BasicAuth;
  username: BlockSettingDto;
  password: BlockSettingDto;
}

export interface HttpContentSettingsGroupDto {
  name: BlockSettingDto;
  contentType: BlockSettingDto;
}

export interface StringHttpContentSettingsGroupDto extends HttpContentSettingsGroupDto {
  _polyTypeName: MultipartContentType.String;
  data: BlockSettingDto;
}

export interface RawHttpContentSettingsGroupDto extends HttpContentSettingsGroupDto {
  _polyTypeName: MultipartContentType.Raw;
  data: BlockSettingDto;
}

export interface FileHttpContentSettingsGroupDto extends HttpContentSettingsGroupDto {
  _polyTypeName: MultipartContentType.File;
  fileName: BlockSettingDto;
}

export type MultipartContentSettingsGroupTypes =
  | StringHttpContentSettingsGroupDto
  | RawHttpContentSettingsGroupDto
  | FileHttpContentSettingsGroupDto;

export interface MultipartRequestParamsDto {
  _polyTypeName: RequestParamsType.Multipart;
  contents: MultipartContentSettingsGroupTypes[];
  boundary: BlockSettingDto;
}

export type RequestParamsTypes =
  | StandardRequestParamsDto
  | RawRequestParamsDto
  | BasicAuthRequestParamsDto
  | MultipartRequestParamsDto;

export interface HttpRequestBlockInstanceDto extends BlockInstanceDto {
  type: BlockInstanceType.HttpRequest;
  safe: boolean;
  requestParams: RequestParamsTypes;
}

export type BlockInstanceTypes =
  | AutoBlockInstanceDto
  | ParseBlockInstanceDto
  | LoliCodeBlockInstanceDto
  | ScriptBlockInstanceDto
  | KeycheckBlockInstanceDto
  | HttpRequestBlockInstanceDto;
