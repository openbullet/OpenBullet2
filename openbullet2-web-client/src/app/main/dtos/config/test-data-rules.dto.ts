import { DataRulesDto } from './config.dto';

export interface TestDataRulesDto {
  testData: string;
  wordlistType: string;
  dataRules: DataRulesDto;
}

export interface TestDataRulesResultDto {
  wordlistType: string;
  regexValidation: TestDataRegexValidationDto;
  slices: TestDataRuleSliceDto[];
  results: TestDataRuleResultDto[];
}

export interface TestDataRegexValidationDto {
  passed: boolean;
}

export interface TestDataRuleSliceDto {
  name: string;
  value: string;
}

export interface TestDataRuleResultDto {
  passed: boolean;
  text: string;
}
