import { Injectable, inject } from '@angular/core';
import { Rest, RestService } from '@abp/ng.core';
import type { MetricasDespensaDto } from './models';

@Injectable({ providedIn: 'root' })
export class AdministracionService {
  private readonly rest = inject(RestService);
  getMetricas(config?: Partial<Rest.Config>) {
    return this.rest.request<any, MetricasDespensaDto>(
      { method: 'GET', url: '/api/app/administracion/metricas' },
      { apiName: 'Default', ...config },
    );
  }
}
