import { SettingInputMode, VariableType } from "./block-descriptor.dto";

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

export interface OutputVariable {
    type: VariableType;
    name: string;
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

export type BlockInstanceTypes =
    AutoBlockInstanceDto |
    ParseBlockInstanceDto |
    LoliCodeBlockInstanceDto |
    ScriptBlockInstanceDto;
