import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getBaseUrl } from 'src/app/shared/utils/host';
import { AffectedEntriesDto } from '../dtos/common/affected-entries.dto';
import { CreateWordlistDto } from '../dtos/wordlist/create-wordlist.dto';
import { UpdateWordlistInfoDto } from '../dtos/wordlist/update-wordlist-info.dto';
import { WordlistFileDto } from '../dtos/wordlist/wordlist-file.dto';
import { WordlistPreviewDto } from '../dtos/wordlist/wordlist-preview.dto';
import { WordlistDto } from '../dtos/wordlist/wordlist.dto';

@Injectable({
  providedIn: 'root',
})
export class WordlistService {
  constructor(private http: HttpClient) {}

  getWordlist(id: number) {
    return this.http.get<WordlistDto>(`${getBaseUrl()}/wordlist`, {
      params: {
        id,
      },
    });
  }

  getAllWordlists() {
    return this.http.get<WordlistDto[]>(`${getBaseUrl()}/wordlist/all`);
  }

  getWordlistPreview(id: number, lineCount: number) {
    return this.http.get<WordlistPreviewDto>(`${getBaseUrl()}/wordlist/preview`, {
      params: {
        id,
        lineCount,
      },
    });
  }

  uploadWordlistFile(file: File) {
    const formData: FormData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<WordlistFileDto>(`${getBaseUrl()}/wordlist/upload`, formData, {
      reportProgress: true,
      observe: 'events',
    });
  }

  createWordlist(wordlist: CreateWordlistDto) {
    return this.http.post<WordlistDto>(`${getBaseUrl()}/wordlist`, wordlist);
  }

  updateWordlistInfo(updated: UpdateWordlistInfoDto) {
    return this.http.patch<WordlistDto>(`${getBaseUrl()}/wordlist/info`, updated);
  }

  deleteWordlist(id: number, alsoDeleteFile: boolean) {
    return this.http.delete(`${getBaseUrl()}/wordlist`, {
      params: {
        id,
        alsoDeleteFile,
      },
    });
  }

  deleteNotFoundWordlists() {
    return this.http.delete<AffectedEntriesDto>(`${getBaseUrl()}/wordlist/not-found`);
  }
}
