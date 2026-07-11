namespace EndForge.Models;

public class ProyectoReciente {

    public string Nombre { get; set; } = "";

    public string Ruta { get; set; } = "";

    public override string ToString() {
        return Nombre;
    }
}