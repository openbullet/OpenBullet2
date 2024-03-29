import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { getBaseUrl } from "src/app/shared/utils/host";
import { CreateHitDto } from "../dtos/hit/create-hit.dto";
import { HitDto } from "../dtos/hit/hit.dto";
import { UpdateHitDto } from "../dtos/hit/update-hit.dto";
import { ListHitFiltersDto } from "../dtos/hit/hit-filters.dto";
import { AffectedEntriesDto } from "../dtos/common/affected-entries.dto";
import { RecentHitsDto } from "../dtos/hit/recent-hits.dto";
import { PagedList } from "../dtos/common/paged-list.dto";

@Injectable({
    providedIn: 'root'
})
export class HitService {
    constructor(
        private http: HttpClient
    ) { }

    getHits(filter: ListHitFiltersDto) {
        return this.http.get<PagedList<HitDto>>(
            getBaseUrl() + '/hit/all', {
            params: <any>Object.fromEntries(
                Object.entries(filter).filter(([_, v]) => v != null))
        }
        );
    }

    getConfigNames() {
        return this.http.get<string[]>(
            getBaseUrl() + '/hit/config-names'
        );
    }

    createHit(hit: CreateHitDto) {
        return this.http.post<HitDto>(
            getBaseUrl() + '/hit', hit
        );
    }

    updateHit(updated: UpdateHitDto) {
        return this.http.patch<HitDto>(
            getBaseUrl() + '/hit', updated
        );
    }

    deleteHit(id: number) {
        return this.http.delete(
            getBaseUrl() + '/hit',
            {
                params: {
                    id
                }
            }
        );
    }

    downloadHits(filter: ListHitFiltersDto, format: string) {
        return this.http.get<Blob>(
            getBaseUrl() + '/hit/download/many', {
            params: {
                format,
                ...<any>Object.fromEntries(
                    Object.entries(filter).filter(([_, v]) => v != null))
            },
            responseType: 'blob' as 'json',
            observe: 'response'
        }
        );
    }

    deleteHits(filter: ListHitFiltersDto) {
        return this.http.delete<AffectedEntriesDto>(
            getBaseUrl() + '/hit/many', {
            params: <any>Object.fromEntries(
                Object.entries(filter).filter(([_, v]) => v != null))
        }
        );
    }

    deleteDuplicateHits() {
        return this.http.delete<AffectedEntriesDto>(
            getBaseUrl() + '/hit/duplicates'
        );
    }

    purgeHits() {
        return this.http.delete<AffectedEntriesDto>(
            getBaseUrl() + '/hit/purge'
        );
    }

    getRecentHits(days: number) {
        return this.http.get<RecentHitsDto>(
            getBaseUrl() + '/hit/recent', {
            params: {
                days
            }
        });
    }
}
