// =============================
// Gestión de proyectos recientes
// =============================
using System.IO;
using System.Collections.Generic;
using System.Linq;
namespace EndForge.Services;

public class RecientesService {

    public void GuardarProyectoReciente(string rutaRecientes, string rutaProyecto) {
        List<string> recientes = new List<string>();

        if (File.Exists(rutaRecientes)) {
            recientes = File.ReadAllLines(rutaRecientes).ToList();
        }

        string nombreProyecto = Path.GetFileName(rutaProyecto);
        string registro = nombreProyecto + "|" + rutaProyecto;

        recientes.RemoveAll(x => x.EndsWith("|" + rutaProyecto));
        recientes.Insert(0, registro);

        if (recientes.Count > 10) {
            recientes = recientes.Take(10).ToList();
        }

        File.WriteAllLines(rutaRecientes, recientes);
    }


}