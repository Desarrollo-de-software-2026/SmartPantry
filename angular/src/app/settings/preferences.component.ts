import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { PreferenciasAlimentariasDto } from '../proxy/preferencias/models';
import { PreferenciasService } from '../proxy/preferencias/preferencias.service';

@Component({
  selector: 'app-preferences',
  standalone: true,
  imports: [ReactiveFormsModule, LocalizationPipe],
  templateUrl: './preferences.component.html',
  styleUrl: './preferences.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PreferencesComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly service = inject(PreferenciasService);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly saved = signal(false);
  readonly error = signal(false);
  readonly customTerm = signal('');
  readonly options = [
    'Gluten', 'Crustáceos', 'Huevos', 'Pescado', 'Maní', 'Soja', 'Leche',
    'Frutos secos', 'Apio', 'Mostaza', 'Sésamo', 'Sulfitos', 'Altramuces', 'Moluscos',
  ];
  readonly form = this.formBuilder.group({
    alergenos: this.formBuilder.nonNullable.control<string[]>([]),
    alertasVencimiento: this.formBuilder.nonNullable.control(true),
    diasAnticipacion: this.formBuilder.nonNullable.control(7, [Validators.min(1), Validators.max(30)]),
  });

  ngOnInit(): void {
    this.service.get({ skipHandleError: true }).subscribe({
      next: value => {
        this.form.patchValue({
          alergenos: value.alergenos ?? [],
          alertasVencimiento: value.alertasVencimiento ?? true,
          diasAnticipacion: value.diasAnticipacion ?? 7,
        });
        this.loading.set(false);
      },
      error: () => { this.error.set(true); this.loading.set(false); },
    });
  }

  isSelected(option: string): boolean {
    return this.form.controls.alergenos.value.includes(option);
  }

  toggle(option: string, checked: boolean): void {
    const selected = new Set(this.form.controls.alergenos.value);
    checked ? selected.add(option) : selected.delete(option);
    this.form.controls.alergenos.setValue([...selected]);
  }

  addCustomTerm(): void {
    const value = this.customTerm().trim();
    if (!value) return;
    const selected = new Set(this.form.controls.alergenos.value);
    selected.add(value);
    this.form.controls.alergenos.setValue([...selected]);
    this.customTerm.set('');
  }

  removeTerm(term: string): void {
    this.form.controls.alergenos.setValue(this.form.controls.alergenos.value.filter(value => value !== term));
  }

  save(): void {
    if (this.form.invalid || this.saving()) return;
    this.saving.set(true);
    this.saved.set(false);
    this.error.set(false);
    this.service.update(this.form.getRawValue() as PreferenciasAlimentariasDto, { skipHandleError: true }).subscribe({
      next: value => {
        this.form.patchValue(value);
        this.saved.set(true);
        this.saving.set(false);
      },
      error: () => { this.error.set(true); this.saving.set(false); },
    });
  }
}
