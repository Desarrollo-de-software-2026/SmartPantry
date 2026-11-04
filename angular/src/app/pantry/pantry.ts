import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, Injector, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ListService, LocalizationPipe, PagedResultDto } from '@abp/ng.core';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import {
  Confirmation,
  ConfirmationService,
  NgxDatatableDefaultDirective,
  NgxDatatableListDirective,
} from '@abp/ng.theme.shared';
import { ItemDespensaDto, ItemDespensaService } from '../proxy/despensas';
import { asCalendarDate } from './pantry-date';

const quantityUnitPairValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const quantity = group.get('cantidad')?.value;
  const unit = group.get('unidad')?.value?.trim();
  return (quantity === null || quantity === undefined) === !unit ? null : { quantityUnitPair: true };
};

const validCalendarDate: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  if (!control.value) return null;
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(control.value);
  if (!match) return { invalidDate: true };
  const year = Number(match[1]);
  const month = Number(match[2]);
  const day = Number(match[3]);
  const parsed = new Date(year, month - 1, day);
  return parsed.getFullYear() === year && parsed.getMonth() === month - 1 && parsed.getDate() === day
    ? null
    : { invalidDate: true };
};

@Component({
  selector: 'app-pantry',
  imports: [
    DatePipe,
    LocalizationPipe,
    NgxDatatableModule,
    NgxDatatableDefaultDirective,
    NgxDatatableListDirective,
    ReactiveFormsModule,
  ],
  templateUrl: './pantry.html',
  styleUrl: './pantry.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ListService],
})
export class Pantry {
  readonly list = inject(ListService);
  private readonly injector = inject(Injector);
  private readonly itemDespensaService = inject(ItemDespensaService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  readonly editOpen = signal(false);
  readonly saving = signal(false);
  readonly pendingItemId = signal<string | null>(null);
  readonly editingItem = signal<ItemDespensaDto | null>(null);
  readonly editError = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);
  readonly editForm = this.formBuilder.group(
    {
      cantidad: [null as number | null, [Validators.min(0)]],
      unidad: ['', [Validators.maxLength(20)]],
      fechaVencimiento: ['', [validCalendarDate]],
    },
    { validators: quantityUnitPairValidator },
  );

  private readonly listQuery = toSignal(this.list.query$, {
    initialValue: { skipCount: 0, maxResultCount: 10, sorting: '' },
    injector: this.injector,
  });

  readonly itemsResource = rxResource<
    PagedResultDto<ItemDespensaDto>,
    { skipCount: number; maxResultCount: number; sorting: string }
  >({
    params: () => this.listQuery(),
    stream: ({ params }) => this.itemDespensaService.getList(params),
    defaultValue: { items: [], totalCount: 0 },
    injector: this.injector,
  });

  retry(): void {
    this.list.get();
  }

  openEdit(item: ItemDespensaDto): void {
    this.editingItem.set(item);
    this.editError.set(null);
    this.editForm.reset({
      cantidad: item.cantidad ?? null,
      unidad: item.unidad ?? '',
      fechaVencimiento: item.fechaVencimiento?.slice(0, 10) ?? '',
    });
    this.editOpen.set(true);
  }

  closeEdit(): void {
    if (this.saving()) return;
    this.editOpen.set(false);
    this.editError.set(null);
  }

  saveEdit(): void {
    const item = this.editingItem();
    if (!item?.id || this.editForm.invalid || this.saving()) {
      this.editForm.markAllAsTouched();
      return;
    }

    const value = this.editForm.getRawValue();
    this.saving.set(true);
    this.editError.set(null);
    this.itemDespensaService.update(item.id, {
      cantidad: value.cantidad,
      unidad: value.cantidad === null ? null : value.unidad?.trim() || null,
      fechaVencimiento: value.fechaVencimiento || null,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.editOpen.set(false);
        this.list.get();
      },
      error: error => {
        this.saving.set(false);
        this.editError.set(this.serverError(error));
      },
    });
  }

  markConsumed(item: ItemDespensaDto): void {
    if (!item.id || item.estaConsumido || this.pendingItemId()) return;
    this.actionError.set(null);
    this.pendingItemId.set(item.id);
    this.itemDespensaService.markAsConsumed(item.id).subscribe({
      next: () => {
        this.pendingItemId.set(null);
        this.list.get();
      },
      error: error => {
        this.pendingItemId.set(null);
        this.actionError.set(this.serverError(error));
      },
    });
  }

  confirmDelete(item: ItemDespensaDto): void {
    if (!item.id || this.pendingItemId()) return;
    this.confirmation.warn('::Pantry:DeleteConfirmation', '::Pantry:DeleteTitle').subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionError.set(null);
      this.pendingItemId.set(item.id!);
      this.itemDespensaService.delete(item.id!).subscribe({
        next: () => {
          this.pendingItemId.set(null);
          this.list.get();
        },
        error: error => {
          this.pendingItemId.set(null);
          this.actionError.set(this.serverError(error));
        },
      });
    });
  }

  readonly asCalendarDate = asCalendarDate;

  private serverError(error: any): string {
    return error?.error?.error?.message || error?.error?.message || error?.message || '::Pantry:ActionError';
  }
}
