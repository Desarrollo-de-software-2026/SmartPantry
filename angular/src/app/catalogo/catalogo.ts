import { Component, inject, signal } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { FormsModule } from '@angular/forms';
import type { ExternalProductDto } from '../proxy/productos/models';
import { ProductoService } from '../proxy/productos/producto.service';

type SearchMode = 'text' | 'barcode';

@Component({
  selector: 'app-catalogo',
  imports: [FormsModule, LocalizationPipe],
  templateUrl: './catalogo.html',
  styleUrl: './catalogo.scss',
})
export class Catalogo {
  private readonly productoService = inject(ProductoService);
  readonly mode = signal<SearchMode>('text');
  readonly results = signal<ExternalProductDto[]>([]);
  readonly selected = signal<ExternalProductDto | null>(null);
  readonly totalCount = signal(0);
  readonly page = signal(1);
  readonly pageSize = 10;
  readonly searching = signal(false);
  readonly loadingDetails = signal(false);
  readonly saving = signal(false);
  readonly searched = signal(false);
  readonly errorKey = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly saveMessageKey = signal<string | null>(null);
  readonly saveState = signal<'saved' | 'already-saved' | null>(null);
  query = '';
  barcode = '';

  get pageCount(): number { return Math.max(1, Math.ceil(this.totalCount() / this.pageSize)); }

  setMode(mode: SearchMode): void {
    this.mode.set(mode);
    this.results.set([]);
    this.selected.set(null);
    this.totalCount.set(0);
    this.searched.set(false);
    this.errorKey.set(null);
    this.errorMessage.set(null);
    this.saveMessageKey.set(null);
    this.saveState.set(null);
  }

  search(): void {
    this.errorKey.set(null);
    this.saveMessageKey.set(null);
    this.selected.set(null);
    this.searched.set(false);
    this.page.set(1);

    if (this.mode() === 'text') {
      const normalizedQuery = this.query
        .replace(/[^\p{L}\p{N}\s]/gu, ' ')
        .replace(/\s+/g, ' ')
        .trim();
      if (normalizedQuery.length < 2) {
        this.errorKey.set('::Catalog:QueryTooShort');
        return;
      }
      // Envía texto plano al buscador externo, nunca operadores o símbolos de consulta.
      this.query = normalizedQuery;
      this.searchByText();
      return;
    }

    const code = this.barcode.trim();
    if (!/^\d{8,14}$/.test(code)) {
      this.errorKey.set('::Catalog:InvalidBarcode');
      return;
    }

    this.searching.set(true);
    this.results.set([]);
    this.productoService.getExternal({ barcode: code }).subscribe({
      next: product => {
        const found = product?.code ? product : null;
        this.results.set(found ? [found] : []);
        this.selected.set(found);
        this.totalCount.set(found ? 1 : 0);
        this.searched.set(true);
        this.searching.set(false);
      },
      error: error => {
        this.setApiError('::Catalog:SearchFailed', error);
        this.searched.set(true);
        this.searching.set(false);
      },
    });
  }

  openDetails(product: ExternalProductDto): void {
    if (!product.code) {
      this.errorKey.set('::Catalog:MissingBarcode');
      return;
    }
    // El resultado de búsqueda ya trae los datos disponibles: mostramos el panel
    // enseguida y luego lo completamos con la consulta específica por código.
    this.selected.set(product);
    this.loadingDetails.set(true);
    this.errorKey.set(null);
    this.errorMessage.set(null);
    this.saveMessageKey.set(null);
    this.saveState.set(null);
    requestAnimationFrame(() => {
      document.getElementById('catalog-detail')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
    this.productoService.getExternal({ barcode: product.code }).subscribe({
      next: details => {
        this.selected.set(details ?? product);
        this.loadingDetails.set(false);
      },
      error: error => {
        this.setApiError('::Catalog:DetailFailed', error);
        this.loadingDetails.set(false);
      },
    });
  }

  saveSelected(): void {
    const product = this.selected();
    if (!product?.code) {
      this.errorKey.set('::Catalog:MissingBarcode');
      return;
    }
    this.saving.set(true);
    this.errorKey.set(null);
    this.errorMessage.set(null);
    this.saveMessageKey.set(null);
    this.productoService.saveExternal({ barcode: product.code }).subscribe({
      next: result => {
        this.saveState.set(result.yaExistia ? 'already-saved' : 'saved');
        this.saveMessageKey.set(result.yaExistia ? '::Catalog:AlreadySaved' : '::Catalog:Saved');
        this.selected.update(current => current ? { ...current, isAlreadySaved: true } : current);
        this.results.update(items => items.map(item => item.code === product.code
          ? { ...item, isAlreadySaved: true }
          : item));
        this.saving.set(false);
      },
      error: error => {
        this.setApiError('::Catalog:SaveFailed', error);
        this.saving.set(false);
      },
    });
  }

  changePage(offset: number): void {
    const nextPage = this.page() + offset;
    if (nextPage < 1 || nextPage > this.pageCount) return;
    this.page.set(nextPage);
    this.searchByText();
  }

  private setApiError(fallbackKey: string, error: unknown): void {
    const response = error as { error?: { error?: { message?: string } } };
    const serverMessage = response?.error?.error?.message;
    if (serverMessage?.startsWith('Catalog:')) {
      this.errorKey.set(`::${serverMessage}`);
      this.errorMessage.set(null);
      return;
    }
    this.errorKey.set(fallbackKey);
    this.errorMessage.set(serverMessage ?? null);
  }
  private searchByText(): void {
    this.searching.set(true);
    this.searched.set(false);
    this.errorKey.set(null);
    this.errorMessage.set(null);
    this.selected.set(null);
    this.saveMessageKey.set(null);
    this.productoService.searchExternal({
      query: this.query.trim(), page: this.page(), pageSize: this.pageSize,
    }).subscribe({
      next: response => {
        this.results.set(response.items ?? []);
        this.totalCount.set(response.totalCount ?? 0);
        this.searched.set(true);
        this.searching.set(false);
      },
      error: error => {
        this.setApiError('::Catalog:SearchFailed', error);
        this.searched.set(true);
        this.searching.set(false);
      },
    });
  }
}


