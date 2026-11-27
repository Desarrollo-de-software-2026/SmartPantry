import type { EntityDto } from '@abp/ng.core';

export interface CreateItemDespensaDto {
  idProducto?: string;
  cantidad?: number | null;
  unidad?: string | null;
  fechaVencimiento?: string | null;
  nota?: string | null;
}

export interface ItemDespensaDto extends EntityDto<string> {
  idProducto?: string;
  nombreProducto?: string | null;
  cantidad?: number | null;
  unidad?: string | null;
  fechaVencimiento?: string | null;
  estaConsumido?: boolean;
  nota?: string | null;
  estaVencido?: boolean;
  proximoAVencer?: boolean;
}

export interface UpdateItemDespensaDto {
  cantidad?: number | null;
  unidad?: string | null;
  fechaVencimiento?: string | null;
  nota?: string | null;
}
