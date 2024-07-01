import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getBaseUrl } from 'src/app/shared/utils/host';
import { CustomInputQuestionDto, CustomInputsDto } from '../dtos/job/custom-inputs.dto';
import { MRJHitLogDto } from '../dtos/job/hit-log.dto';
import { JobOverviewDto } from '../dtos/job/job.dto';
import { MultiRunJobOptionsDto } from '../dtos/job/multi-run-job-options.dto';
import { MultiRunJobOverviewDto } from '../dtos/job/multi-run-job-overview.dto';
import { MultiRunJobDto } from '../dtos/job/multi-run-job.dto';
import { ProxyCheckJobOptionsDto } from '../dtos/job/proxy-check-job-options.dto';
import { ProxyCheckJobOverviewDto } from '../dtos/job/proxy-check-job-overview.dto';
import { ProxyCheckJobDto } from '../dtos/job/proxy-check-job.dto';
import { BotDetailsDto } from '../dtos/job/multi-run-job-bot-details.dto';
import { RecordDto } from '../dtos/job/record.dto';

@Injectable({
  providedIn: 'root',
})
export class JobService {
  constructor(private http: HttpClient) { }

  getAllJobs() {
    return this.http.get<JobOverviewDto[]>(`${getBaseUrl()}/job/all`);
  }

  getAllMultiRunJobs() {
    return this.http.get<MultiRunJobOverviewDto[]>(`${getBaseUrl()}/job/multi-run/all`);
  }

  getAllProxyCheckJobs() {
    return this.http.get<ProxyCheckJobOverviewDto[]>(`${getBaseUrl()}/job/proxy-check/all`);
  }

  getMultiRunJob(id: number) {
    return this.http.get<MultiRunJobDto>(`${getBaseUrl()}/job/multi-run`, {
      params: {
        id,
      },
    });
  }

  getProxyCheckJob(id: number) {
    return this.http.get<ProxyCheckJobDto>(`${getBaseUrl()}/job/proxy-check`, {
      params: {
        id,
      },
    });
  }

  getMultiRunJobOptions(id: number) {
    // use id = -1 for default options
    return this.http.get<MultiRunJobOptionsDto>(`${getBaseUrl()}/job/multi-run/options`, {
      params: {
        id,
      },
    });
  }

  getProxyCheckJobOptions(id: number) {
    // use id = -1 for default options
    return this.http.get<ProxyCheckJobOptionsDto>(`${getBaseUrl()}/job/proxy-check/options`, {
      params: {
        id,
      },
    });
  }

  createMultiRunJob(options: MultiRunJobOptionsDto) {
    return this.http.post<MultiRunJobDto>(`${getBaseUrl()}/job/multi-run`, options);
  }

  createProxyCheckJob(options: ProxyCheckJobOptionsDto) {
    return this.http.post<ProxyCheckJobDto>(`${getBaseUrl()}/job/proxy-check`, options);
  }

  updateMultiRunJob(id: number, options: MultiRunJobOptionsDto) {
    return this.http.put<MultiRunJobDto>(`${getBaseUrl()}/job/multi-run`, {
      id,
      ...options,
    });
  }

  updateProxyCheckJob(id: number, options: ProxyCheckJobOptionsDto) {
    return this.http.put<ProxyCheckJobDto>(`${getBaseUrl()}/job/proxy-check`, {
      id,
      ...options,
    });
  }

  getCustomInputs(jobId: number) {
    return this.http.get<CustomInputQuestionDto[]>(`${getBaseUrl()}/job/multi-run/custom-inputs`, {
      params: {
        id: jobId,
      },
    });
  }

  setCustomInputs(inputs: CustomInputsDto) {
    return this.http.patch<MultiRunJobDto>(`${getBaseUrl()}/job/multi-run/custom-inputs`, inputs);
  }

  deleteJob(id: number) {
    return this.http.delete(`${getBaseUrl()}/job`, {
      params: {
        id,
      },
    });
  }

  deleteAllJobs() {
    return this.http.delete(`${getBaseUrl()}/job/all`);
  }

  getHitLog(jobId: number, hitId: string) {
    return this.http.get<MRJHitLogDto>(`${getBaseUrl()}/job/multi-run/hit-log`, {
      params: {
        jobId,
        hitId,
      },
    });
  }

  start(jobId: number, wait = false) {
    return this.http.post(`${getBaseUrl()}/job/start`, {
      jobId,
      wait,
    });
  }

  stop(jobId: number, wait = false) {
    return this.http.post(`${getBaseUrl()}/job/stop`, {
      jobId,
      wait,
    });
  }

  pause(jobId: number, wait = false) {
    return this.http.post(`${getBaseUrl()}/job/pause`, {
      jobId,
      wait,
    });
  }

  resume(jobId: number, wait = false) {
    return this.http.post(`${getBaseUrl()}/job/resume`, {
      jobId,
      wait,
    });
  }

  abort(jobId: number, wait = false) {
    return this.http.post(`${getBaseUrl()}/job/abort`, {
      jobId,
      wait,
    });
  }

  skipWait(jobId: number, wait = false) {
    return this.http.post(`${getBaseUrl()}/job/skip-wait`, {
      jobId,
      wait,
    });
  }

  changeBots(jobId: number, bots: number) {
    return this.http.post(`${getBaseUrl()}/job/change-bots`, {
      jobId,
      bots,
    });
  }

  getBotDetails(jobId: number) {
    return this.http.get<BotDetailsDto[]>(`${getBaseUrl()}/job/multi-run/bot-details`, {
      params: {
        jobId,
      },
    });
  }

  getRecord(configId: string, wordlistId: number) {
    return this.http.get<RecordDto>(`${getBaseUrl()}/job/multi-run/record`, {
      params: {
        configId,
        wordlistId,
      },
    });
  }
}
