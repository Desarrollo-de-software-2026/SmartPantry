import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { ProductoService } from '../proxy/productos/producto.service';
import { ExternalProductDto } from '../proxy/productos/models';

@Component({
  selector: 'app-catalog-lookup',
  standalone: true,
  imports: [ReactiveFormsModule, LocalizationPipe],
  templateUrl: './lookup.component.html',
  styleUrl: './lookup.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LookupComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly products = inject(ProductoService);
  readonly loading = signal(false);
  readonly error = signal(false);
  readonly product = signal<ExternalProductDto | null>(null);
  readonly form = this.formBuilder.nonNullable.group({ barcode: ['', [Validators.required, Validators.pattern(/^\d{8,14}$/)]] });

  lookup(): void {
    if (this.form.invalid || this.loading()) return;
    this.loading.set(true);
    this.error.set(false);
    this.product.set(null);
    this.products.getExternal({ barcode: this.form.controls.barcode.value }, { skipHandleError: true }).subscribe({
      next: value => { this.product.set(value ?? null); this.loading.set(false); },
      error: () => { this.error.set(true); this.loading.set(false); },
    });
  }
}
