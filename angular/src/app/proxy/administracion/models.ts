export interface MetricasDespensaDto {
  usuariosTotales: number;
  usuariosActivos: number;
  usuariosDeshabilitados: number;
  itemsDespensa: number;
  alertasActivas: number;
  integracionExterna: {
    requests: number;
    successes: number;
    notFound: number;
    errors: number;
    averageLatencyMilliseconds: number;
    lastErrorCode?: string | null;
    lastErrorAtUtc?: string | null;
  };
}
