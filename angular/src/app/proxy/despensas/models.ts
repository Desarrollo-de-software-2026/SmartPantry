import type { EntityDto } from '@abp/ng.core';

export interface ItemDespensaDto extends EntityDto<string> {
  idProducto?: string;
  nombreProducto?: string | null;
  cantidad?: number | null;
  unidad?: string | null;
  fechaVencimiento?: string | null;
  estaConsumido?: boolean;
}

export interface UpdateItemDespensaDto {
  cantidad?: number | null;
  unidad?: string | null;
  fechaVencimiento?: string | null;
}
