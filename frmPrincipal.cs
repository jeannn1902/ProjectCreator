using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal : Form {
    private readonly ProyectoService proyectoService = new();
    private readonly ConfiguracionService configuracionService = new();
    private readonly TemasService temasService = new();
    private readonly AperturaPracticasService aperturaPracticasService = new();
    private readonly RecientesService recientesService;
    private readonly CreacionPracticasOrquestador creacionPracticasOrquestador;
    private readonly VistaPreviaPracticaService vistaPreviaPracticaService;
    private Panel panelSeleccionado = null!;
    private string rutaBase = "";
    private string rutaPlantilla = "";

    public frmPrincipal() {
        recientesService = new RecientesService(configuracionService.RutaRecientes);
        vistaPreviaPracticaService = new VistaPreviaPracticaService(temasService);
        creacionPracticasOrquestador = new CreacionPracticasOrquestador(proyectoService, recientesService, aperturaPracticasService);

        InitializeComponent();

        ConfigurarBarraTitulo();
        ConfigurarTarjetasInicio();
        ActivarBarraTituloOscura();
        ConfigurarVentana();
        ConfigurarNavegacion();
        ConfigurarRecientes();
        ConfigurarEstadoInicial();
    }
}
