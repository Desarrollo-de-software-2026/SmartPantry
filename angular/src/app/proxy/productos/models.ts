import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateProductoDto {
  nombre?: string;
  marca?: string;
  categoria?: string;
  urlImagen?: string;
}

export interface ExternalProductDto {
  code?: string;
  name?: string;
  brand?: string;
  category?: string;
  quantity?: string;
  imageUrl?: string;
  isAlreadySaved?: boolean;
  ingredients?: string | null;
  allergens?: string | null;
  nutriScore?: string | null;
  novaGroup?: number | null;
  energyKcal100g?: number | null;
  fat100g?: number | null;
  sugars100g?: number | null;
  salt100g?: number | null;
}

export interface GetExternalProductInput {
  barcode: string;
}

export interface GetProductosInput extends PagedAndSortedResultRequestDto {
  filter?: string;
}

export interface ProductoDto extends AuditedEntityDto<string> {
  nombre?: string;
  marca?: string;
  categoria?: string;
  urlImagen?: string;
  codigoBarras?: string | null;
}

export interface SaveExternalProductResultDto {
  producto?: ProductoDto;
  yaExistia?: boolean;
}

export interface SearchExternalProductsInput {
  query: string;
  page?: number;
  pageSize?: number;
}

export interface UpdateProductoDto {
  nombre?: string;
  marca?: string;
  categoria?: string;
  urlImagen?: string;
}
