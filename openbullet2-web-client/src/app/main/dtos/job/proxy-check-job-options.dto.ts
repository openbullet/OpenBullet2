import { ProxyCheckTargetDto } from './proxy-check-job.dto';
import { AbsoluteTimeStartConditionDto, RelativeTimeStartConditionDto } from './start-condition.dto';

export interface ProxyCheckJobOptionsDto {
  name: string;
  startCondition: RelativeTimeStartConditionDto | AbsoluteTimeStartConditionDto;
  bots: number;
  groupId: number;
  checkOnlyUntested: boolean;
  target: ProxyCheckTargetDto | null;
  timeoutMilliseconds: number;
  checkOutput: DatabaseProxyCheckOutput;
}

export enum ProxyCheckOutputType {
  Database = 'databaseProxyCheckOutput',
}

export interface DatabaseProxyCheckOutput {
  _polyTypeName: ProxyCheckOutputType.Database;
}
