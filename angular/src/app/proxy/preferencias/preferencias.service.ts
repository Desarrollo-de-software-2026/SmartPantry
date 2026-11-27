import { Injectable, inject } from '@angular/core';
import { Rest, RestService } from '@abp/ng.core';
import type { PreferenciasAlimentariasDto } from './models';

@Injectable({ providedIn: 'root' })
export class PreferenciasService {
  private readonly rest = inject(RestService);
  private readonly apiName = 'Default';

  get(config?: Partial<Rest.Config>) {
    return this.rest.request<any, PreferenciasAlimentariasDto>(
      { method: 'GET', url: '/api/app/preferencias-alimentarias' },
      { apiName: this.apiName, ...config },
    );
  }

  update(input: PreferenciasAlimentariasDto, config?: Partial<Rest.Config>) {
    return this.rest.request<any, PreferenciasAlimentariasDto>(
      { method: 'PUT', url: '/api/app/preferencias-alimentarias', body: input },
      { apiName: this.apiName, ...config },
    );
  }
}
