import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { AdministracionService } from '../proxy/administracion/administracion.service';
import { MetricasDespensaDto } from '../proxy/administracion/models';

@Component({
  selector: 'app-admin-metrics',
  standalone: true,
  imports: [LocalizationPipe, DatePipe, DecimalPipe],
  templateUrl: './metrics.component.html',
  styleUrl: './metrics.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MetricsComponent implements OnInit {
  private readonly service = inject(AdministracionService);
  readonly metrics = signal<MetricasDespensaDto | null>(null);
  readonly error = signal(false);

  ngOnInit(): void {
    this.service.getMetricas({ skipHandleError: true }).subscribe({
      next: value => this.metrics.set(value),
      error: () => this.error.set(true),
    });
  }
}
