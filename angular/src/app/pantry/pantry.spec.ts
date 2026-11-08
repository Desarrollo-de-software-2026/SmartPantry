// @vitest-environment jsdom
import { getTestBed, TestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { ListService } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { FormBuilder } from '@angular/forms';
import { BehaviorSubject, Observable, of, Subject, throwError } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ItemDespensaDto, ItemDespensaService } from '../proxy/despensas';
import { Pantry } from './pantry';
import { asCalendarDate } from './pantry-date';

getTestBed().initTestEnvironment(BrowserDynamicTestingModule, platformBrowserDynamicTesting());

describe('Pantry component', () => {
  let component: Pantry;
  let listQuery: BehaviorSubject<{ skipCount: number; maxResultCount: number; sorting: string }>;
  let listService: { query$: Observable<unknown>; get: ReturnType<typeof vi.fn> };
  let service: {
    getList: ReturnType<typeof vi.fn>;
    update: ReturnType<typeof vi.fn>;
    markAsConsumed: ReturnType<typeof vi.fn>;
    delete: ReturnType<typeof vi.fn>;
  };
  let confirmation: { warn: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    listQuery = new BehaviorSubject({ skipCount: 0, maxResultCount: 10, sorting: '' });
    listService = { query$: listQuery.asObservable(), get: vi.fn() };
    service = {
      getList: vi.fn(() => new Subject()),
      update: vi.fn(() => of({})),
      markAsConsumed: vi.fn(() => of(void 0)),
      delete: vi.fn(() => of(void 0)),
    };
    confirmation = { warn: vi.fn(() => of(Confirmation.Status.dismiss)) };

    TestBed.configureTestingModule({
      providers: [
        { provide: ListService, useValue: listService },
        { provide: ItemDespensaService, useValue: service },
        { provide: ConfirmationService, useValue: confirmation },
        FormBuilder,
      ],
    });
    component = TestBed.runInInjectionContext(() => new Pantry());
  });

  afterEach(() => TestBed.resetTestingModule());

  it('exposes loading while the list request is pending', () => {
    const response = new Subject<any>();
    service.getList.mockReturnValue(response);

    expect(component.itemsResource.isLoading()).toBe(true);
    expect(component.itemsResource.value().items).toEqual([]);
  });

  it('exposes the error state when the list request fails', async () => {
    service.getList.mockReturnValue(throwError(() => new Error('server detail')));

    await vi.waitFor(() => expect(component.itemsResource.error()).toBeTruthy());

    expect(component.itemsResource.error()).toBeTruthy();
  });

  it('exposes the empty state when the user has no items', async () => {
    service.getList.mockReturnValue(of({ items: [], totalCount: 0 }));

    await vi.waitFor(() => expect(component.itemsResource.isLoading()).toBe(false));

    expect(component.itemsResource.value().items).toEqual([]);
  });

  it('exposes results returned by the proxy', async () => {
    service.getList.mockReturnValue(of({
      items: [{ id: 'item-1', nombreProducto: 'Arroz' }],
      totalCount: 1,
    }));

    await vi.waitFor(() => expect(component.itemsResource.value().items.length).toBe(1));

    expect(component.itemsResource.value().items[0].nombreProducto).toBe('Arroz');
  });

  it('does not call update when the edit form contains invalid values', () => {
    component.editingItem.set({ id: 'item-1' } as ItemDespensaDto);
    component.editForm.setValue({ cantidad: -2, unidad: 'kg', fechaVencimiento: '' });

    component.saveEdit();

    expect(service.update).not.toHaveBeenCalled();
  });

  it('updates an item with valid form values and refreshes the list', () => {
    component.editingItem.set({ id: 'item-1' } as ItemDespensaDto);
    component.editForm.setValue({ cantidad: 2, unidad: ' kg ', fechaVencimiento: '2026-12-15' });

    component.saveEdit();

    expect(service.update).toHaveBeenCalledWith('item-1', {
      cantidad: 2,
      unidad: 'kg',
      fechaVencimiento: '2026-12-15',
    }, { skipHandleError: true });
    expect(component.editOpen()).toBe(false);
    expect(component.saving()).toBe(false);
    expect(listService.get).toHaveBeenCalledOnce();
  });

  it('marks an unconsumed item as consumed and refreshes the list', () => {
    component.markConsumed({ id: 'item-1', estaConsumido: false } as ItemDespensaDto);

    expect(service.markAsConsumed).toHaveBeenCalledWith('item-1', { skipHandleError: true });
    expect(component.pendingItemId()).toBeNull();
    expect(listService.get).toHaveBeenCalledOnce();
  });

  it('does not send a consume request for an already consumed item', () => {
    component.markConsumed({ id: 'item-1', estaConsumido: true } as ItemDespensaDto);

    expect(service.markAsConsumed).not.toHaveBeenCalled();
    expect(listService.get).not.toHaveBeenCalled();
  });

  it('does not call delete when the confirmation is canceled', () => {
    confirmation.warn.mockReturnValue(of(Confirmation.Status.dismiss));

    component.confirmDelete({ id: 'item-1' } as ItemDespensaDto);

    expect(confirmation.warn).toHaveBeenCalledOnce();
    expect(service.delete).not.toHaveBeenCalled();
  });

  it('calls delete only after the confirmation is accepted', () => {
    confirmation.warn.mockReturnValue(of(Confirmation.Status.confirm));

    component.confirmDelete({ id: 'item-1' } as ItemDespensaDto);

    expect(service.delete).toHaveBeenCalledWith('item-1', { skipHandleError: true });
  });
});

describe('Pantry date display', () => {
  it('preserves the calendar day from an ISO date without timezone shifting', () => {
    const date = asCalendarDate('2026-11-04T00:00:00Z');

    expect(date).not.toBeNull();
    expect(date?.getFullYear()).toBe(2026);
    expect(date?.getMonth()).toBe(10);
    expect(date?.getDate()).toBe(4);
  });

  it('returns no date when the optional expiry is missing or invalid', () => {
    expect(asCalendarDate(null)).toBeNull();
    expect(asCalendarDate('not-a-date')).toBeNull();
  });
});
