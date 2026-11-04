import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, Injector } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { ListService, LocalizationPipe, PagedResultDto } from '@abp/ng.core';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { NgxDatatableDefaultDirective, NgxDatatableListDirective } from '@abp/ng.theme.shared';
import { ItemDespensaDto, ItemDespensaService } from '../proxy/despensas';
import { asCalendarDate } from './pantry-date';

@Component({
  selector: 'app-pantry',
  imports: [
    DatePipe,
    LocalizationPipe,
    NgxDatatableModule,
    NgxDatatableDefaultDirective,
    NgxDatatableListDirective,
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

  readonly asCalendarDate = asCalendarDate;
}
