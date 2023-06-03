import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { getBaseUrl } from "src/app/shared/utils/host";
import { WordlistDto } from "../dtos/wordlist/wordlist.dto";
import { WordlistPreviewDto } from "../dtos/wordlist/wordlist-preview.dto";
import { CreateWordlistDto } from "../dtos/wordlist/create-wordlist.dto";
import { UpdateWordlistInfoDto } from "../dtos/wordlist/update-wordlist-info.dto";
import { AffectedEntriesDto } from "../dtos/common/affected-entries.dto";

@Injectable({
    providedIn: 'root'
})
export class WordlistService {
    constructor(
        private http: HttpClient
    ) { }

    getAllWordlists() {
        return this.http.get<WordlistDto[]>(
            getBaseUrl() + '/wordlist/all'
        );
    }

    getWordlistPreview(id: number, lineCount: number) {
        return this.http.get<WordlistPreviewDto[]>(
            getBaseUrl() + '/wordlist/all', {
                params: {
                    id,
                    lineCount
                }
            }
        );
    }

    uploadWordlistFile(file: File) {
        const formData: FormData = new FormData();
        formData.append('file', file, file.name);
        return this.http.post<WordlistDto>(
            getBaseUrl() + '/wordlist/upload', formData
        );
    }

    createWordlist(wordlist: CreateWordlistDto) {
        return this.http.post<WordlistDto>(
            getBaseUrl() + '/wordlist', wordlist
        );
    }

    updateWordlistInfo(updated: UpdateWordlistInfoDto) {
        return this.http.patch<WordlistDto>(
            getBaseUrl() + '/wordlist', updated
        );
    }

    deleteWordlist(id: number, alsoDeleteFile: boolean) {
        return this.http.delete(
            getBaseUrl() + '/wordlist',
            {
                params: {
                    id,
                    alsoDeleteFile
                }
            }
        );
    }

    deleteNotFoundWordlists() {
        return this.http.delete<AffectedEntriesDto>(
            getBaseUrl() + '/wordlist/not-found'
        );
    }
}
