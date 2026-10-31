import type { CreateProductoDto, ExternalProductDto, GetExternalProductInput, GetProductosInput, ProductoDto, UpdateProductoDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ProductoService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateProductoDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductoDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/producto',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/producto/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductoDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/producto/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getExternal = (input: GetExternalProductInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExternalProductDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/producto/external',
      params: { barcode: input.barcode },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetProductosInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProductoDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/producto',
      params: { filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateProductoDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductoDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/producto/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}