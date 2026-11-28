namespace SmartPantry.Administracion;

public class MetricasDespensaDto
{
    public int UsuariosTotales { get; set; }
    public int UsuariosActivos { get; set; }
    public int UsuariosDeshabilitados { get; set; }
    public int ItemsDespensa { get; set; }
    public int AlertasActivas { get; set; }
    public SmartPantry.Productos.ExternalCatalogMetricsDto IntegracionExterna { get; set; } = new();
}
