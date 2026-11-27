namespace SmartPantry.Productos;

public class SaveExternalProductResultDto
{
    public ProductoDto Producto { get; set; } = null!;
    public bool YaExistia { get; set; }
}
