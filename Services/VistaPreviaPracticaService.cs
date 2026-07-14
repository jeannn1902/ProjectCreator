using EndForge.Models;

namespace EndForge.Services;

public sealed class VistaPreviaPracticaService {
    private readonly TemasService temasService;

    public VistaPreviaPracticaService(TemasService temasService) {
        this.temasService = temasService;
    }

    public ResultadoVistaPreviaPractica Calcular(
        string rutaBase,
        string? temaSeleccionado,
        string nombreIntroducido) {
        string nombreNormalizado = nombreIntroducido.Trim();

        if (string.IsNullOrEmpty(temaSeleccionado) || nombreNormalizado == "") {
            return new ResultadoVistaPreviaPractica {
                Estado = EstadoVistaPreviaPractica.Vacia
            };
        }

        int siguienteNumero = temasService.ObtenerSiguienteNumero(
            rutaBase,
            temaSeleccionado
        );

        return new ResultadoVistaPreviaPractica {
            Estado = EstadoVistaPreviaPractica.Completa,
            NombreFinal = siguienteNumero.ToString("00") + "_" + nombreNormalizado
        };
    }
}
