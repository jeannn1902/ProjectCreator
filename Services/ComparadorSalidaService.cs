using EndForge.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EndForge.Services;

public sealed partial class ComparadorSalidaService {
    public ResultadoComparacionSalida Comparar(CasoPrueba caso, string? salidaObtenida) {
        ArgumentNullException.ThrowIfNull(caso);

        string salida = salidaObtenida ?? string.Empty;
        string salidaNormalizada = NormalizarTexto(salida);

        List<string> tokensFaltantes = caso.TokensObligatorios
            .Where(token => !ContieneToken(salidaNormalizada, token))
            .ToList();

        List<string> gruposFaltantes = caso.GruposTokensAlternativos
            .Where(grupo =>
                grupo.Alternativas.Count == 0 ||
                !CumpleGrupoAlternativo(salida, salidaNormalizada, grupo))
            .Select(grupo => grupo.Nombre)
            .ToList();

        HashSet<PosicionNumero> numerosUtilizados = new();
        List<ResultadoValorNumericoComparado> valoresComparados = caso
            .ValoresNumericosEsperados
            .Select(valor => CompararValorNumerico(salida, valor, numerosUtilizados))
            .ToList();

        bool cumpleEstructura =
            tokensFaltantes.Count == 0 && gruposFaltantes.Count == 0;
        bool valoresCorrectos =
            valoresComparados.Count > 0 && valoresComparados.All(valor => valor.Coincide);
        bool salidaLegible = EsLegible(
            salida,
            caso.TokensObligatorios.Count - tokensFaltantes.Count,
            caso.TokensObligatorios.Count);
        bool esCorrecta = cumpleEstructura && valoresCorrectos;

        return new ResultadoComparacionSalida {
            EsCorrecta = esCorrecta,
            CumpleEstructura = cumpleEstructura,
            EsSalidaLegible = salidaLegible,
            TokensFaltantes = tokensFaltantes.AsReadOnly(),
            GruposAlternativosFaltantes = gruposFaltantes.AsReadOnly(),
            ValoresNumericos = valoresComparados.AsReadOnly(),
            Mensaje = CrearMensaje(
                salida,
                esCorrecta,
                cumpleEstructura,
                valoresComparados)
        };
    }

    private static ResultadoValorNumericoComparado CompararValorNumerico(
        string salida,
        ValorNumericoEsperado esperado,
        HashSet<PosicionNumero> numerosUtilizados) {
        string[] lineas = SepararLineas(salida);
        IReadOnlyList<string> etiquetas = CrearEtiquetasBusqueda(esperado);
        CandidatoNumero? primerCandidato = null;

        foreach (string etiqueta in etiquetas) {
            string etiquetaNormalizada = NormalizarTexto(etiqueta);

            if (string.IsNullOrWhiteSpace(etiquetaNormalizada)) {
                continue;
            }

            for (int indiceLinea = 0; indiceLinea < lineas.Length; indiceLinea++) {
                string lineaNormalizada = NormalizarLineaPreservandoIndices(
                    lineas[indiceLinea]);

                foreach (Match coincidenciaEtiqueta in CrearRegexToken(etiquetaNormalizada)
                    .Matches(lineaNormalizada)
                    .Cast<Match>()) {
                    foreach (CandidatoNumero candidato in ObtenerNumerosAsociados(
                        lineas,
                        indiceLinea,
                        coincidenciaEtiqueta,
                        etiqueta)) {
                        PosicionNumero posicion = new(
                            candidato.IndiceLinea,
                            candidato.IndiceCaracter,
                            candidato.Longitud);

                        if (numerosUtilizados.Contains(posicion)) {
                            continue;
                        }

                        primerCandidato ??= candidato;

                        if (!SonEquivalentes(
                            candidato.Valor,
                            esperado.Valor,
                            esperado.Tolerancia)) {
                            continue;
                        }

                        numerosUtilizados.Add(posicion);
                        return CrearResultadoValor(
                            esperado,
                            candidato.Valor,
                            coincide: true,
                            etiqueta);
                    }
                }
            }
        }

        return CrearResultadoValor(
            esperado,
            primerCandidato?.Valor,
            coincide: false,
            primerCandidato?.Etiqueta ?? string.Empty);
    }

    private static IEnumerable<CandidatoNumero> ObtenerNumerosAsociados(
        string[] lineas,
        int indiceLinea,
        Match coincidenciaEtiqueta,
        string etiqueta) {
        string linea = lineas[indiceLinea];
        int inicioBusqueda = Math.Min(coincidenciaEtiqueta.Index + coincidenciaEtiqueta.Length, linea.Length);
        string segmento = linea[inicioBusqueda..];
        int indiceSeparador = segmento.IndexOfAny(new[] { ':', '=' });

        if (indiceSeparador >= 0) {
            segmento = segmento[(indiceSeparador + 1)..];
            inicioBusqueda += indiceSeparador + 1;
        }

        MatchCollection numeros = RegexNumero().Matches(segmento);

        foreach (Match numero in numeros.Cast<Match>()) {
            if (!IntentarConvertirNumero(numero.Value, out double valor)) {
                continue;
            }

            yield return new CandidatoNumero(
                indiceLinea,
                inicioBusqueda + numero.Index,
                numero.Length,
                valor,
                etiqueta);

            // Un campo etiquetado representa un único valor. No se permite
            // tomar un cálculo posterior de la misma línea para hacerlo pasar.
            yield break;
        }

        for (int siguiente = indiceLinea + 1; siguiente < lineas.Length; siguiente++) {
            string siguienteLinea = lineas[siguiente].Trim();

            if (siguienteLinea.Length == 0) {
                continue;
            }

            Match numeroInicial = RegexNumeroInicial().Match(siguienteLinea);

            if (numeroInicial.Success &&
                IntentarConvertirNumero(numeroInicial.Value, out double valor)) {
                yield return new CandidatoNumero(
                    siguiente,
                    numeroInicial.Index,
                    numeroInicial.Length,
                    valor,
                    etiqueta);
            }

            yield break;
        }
    }

    private static IReadOnlyList<string> CrearEtiquetasBusqueda(
        ValorNumericoEsperado esperado) {
        List<string> etiquetas = new() { esperado.Nombre };
        etiquetas.AddRange(esperado.EtiquetasAlternativas);

        string nombreSinOrdinal = RegexOrdinalFinal()
            .Replace(esperado.Nombre, string.Empty)
            .Trim();

        if (!string.IsNullOrWhiteSpace(nombreSinOrdinal)) {
            etiquetas.Add(nombreSinOrdinal);
        }

        return etiquetas
            .Where(etiqueta => !string.IsNullOrWhiteSpace(etiqueta))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(etiqueta => etiqueta.Length)
            .ToArray();
    }

    private static ResultadoValorNumericoComparado CrearResultadoValor(
        ValorNumericoEsperado esperado,
        double? valorObtenido,
        bool coincide,
        string etiquetaEncontrada) {
        return new ResultadoValorNumericoComparado {
            Nombre = esperado.Nombre,
            ValorEsperado = esperado.Valor,
            ValorObtenido = valorObtenido,
            Tolerancia = esperado.Tolerancia,
            Coincide = coincide,
            EtiquetaEncontrada = etiquetaEncontrada
        };
    }

    private static bool SonEquivalentes(double obtenido, double esperado, double tolerancia) {
        return double.IsFinite(obtenido) &&
            double.IsFinite(esperado) &&
            Math.Abs(obtenido - esperado) <= Math.Max(0D, tolerancia);
    }

    private static bool IntentarConvertirNumero(string texto, out double valor) {
        string normalizado = texto.Replace(',', '.');
        return double.TryParse(
            normalizado,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out valor);
    }

    private static bool ContieneToken(string textoNormalizado, string token) {
        string tokenNormalizado = NormalizarTexto(token);
        return tokenNormalizado.Length > 0 &&
            CrearRegexToken(tokenNormalizado).IsMatch(textoNormalizado);
    }

    private static bool CumpleGrupoAlternativo(
        string salida,
        string salidaNormalizada,
        GrupoTokensEsperados grupo) {
        if (grupo.EtiquetasAsociadas.Count == 0) {
            return grupo.Alternativas.Any(alternativa =>
                ContieneToken(salidaNormalizada, alternativa));
        }

        string[] lineas = SepararLineas(salida);

        foreach (string etiqueta in grupo.EtiquetasAsociadas) {
            string etiquetaNormalizada = NormalizarTexto(etiqueta);

            if (etiquetaNormalizada.Length == 0) {
                continue;
            }

            for (int indiceLinea = 0; indiceLinea < lineas.Length; indiceLinea++) {
                string lineaNormalizada = NormalizarLineaPreservandoIndices(
                    lineas[indiceLinea]);

                foreach (Match coincidencia in CrearRegexToken(etiquetaNormalizada)
                    .Matches(lineaNormalizada)
                    .Cast<Match>()) {
                    string segmento = lineaNormalizada[
                        Math.Min(coincidencia.Index + coincidencia.Length, lineaNormalizada.Length)..];

                    if (EmpiezaConAlternativa(segmento, grupo.Alternativas)) {
                        return true;
                    }

                    for (int siguiente = indiceLinea + 1; siguiente < lineas.Length; siguiente++) {
                        string siguienteLinea = NormalizarTexto(lineas[siguiente]);

                        if (siguienteLinea.Length == 0) {
                            continue;
                        }

                        if (EmpiezaConAlternativa(
                            siguienteLinea,
                            grupo.Alternativas)) {
                            return true;
                        }

                        break;
                    }
                }
            }
        }

        return false;
    }

    private static bool EmpiezaConAlternativa(
        string segmento,
        IReadOnlyList<string> alternativas) {
        string valor = NormalizarTexto(segmento)
            .TrimStart(' ', ':', '=', '-', '–', '—');

        foreach (string alternativa in alternativas) {
            string alternativaNormalizada = NormalizarTexto(alternativa);

            if (alternativaNormalizada.Length == 0) {
                continue;
            }

            if (valor.Equals(alternativaNormalizada, StringComparison.Ordinal) ||
                valor.StartsWith(
                    alternativaNormalizada + " ",
                    StringComparison.Ordinal)) {
                return true;
            }
        }

        return false;
    }

    private static Regex CrearRegexToken(string tokenNormalizado) {
        string[] partes = tokenNormalizado
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string cuerpo = string.Join(@"\s+", partes.Select(Regex.Escape));

        return new Regex(
            $@"(?<![\p{{L}}\p{{N}}_]){cuerpo}(?![\p{{L}}\p{{N}}_])",
            RegexOptions.CultureInvariant);
    }

    private static string NormalizarTexto(string texto) {
        string descompuesto = texto.Normalize(NormalizationForm.FormD);
        StringBuilder resultado = new(descompuesto.Length);
        bool espacioPendiente = false;

        foreach (char caracter in descompuesto) {
            UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);

            if (categoria == UnicodeCategory.NonSpacingMark) {
                continue;
            }

            if (char.IsWhiteSpace(caracter)) {
                espacioPendiente = resultado.Length > 0;
                continue;
            }

            if (espacioPendiente) {
                resultado.Append(' ');
                espacioPendiente = false;
            }

            resultado.Append(char.ToLowerInvariant(caracter));
        }

        return resultado.ToString().Trim();
    }

    private static string NormalizarLineaPreservandoIndices(string texto) {
        string descompuesto = texto.Normalize(NormalizationForm.FormD);
        StringBuilder resultado = new(texto.Length);

        foreach (char caracter in descompuesto) {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) ==
                UnicodeCategory.NonSpacingMark) {
                continue;
            }

            resultado.Append(char.ToLowerInvariant(caracter));
        }

        return resultado.ToString();
    }

    private static string[] SepararLineas(string texto) {
        return texto
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    private static bool EsLegible(
        string salida,
        int tokensEncontrados,
        int totalTokens) {
        if (string.IsNullOrWhiteSpace(salida) || salida.IndexOf('\0') >= 0) {
            return false;
        }

        int caracteresVisibles = 0;
        int controlesInvalidos = 0;

        foreach (char caracter in salida) {
            if (!char.IsWhiteSpace(caracter)) {
                caracteresVisibles++;
            }

            if (char.IsControl(caracter) && caracter is not '\r' and not '\n' and not '\t') {
                controlesInvalidos++;
            }
        }

        int minimoTokens = totalTokens == 0
            ? 0
            : Math.Max(1, (int)Math.Ceiling(totalTokens / 2D));
        bool tieneEstructuraVisual =
            salida.Contains(':') ||
            salida.Contains('=') ||
            salida.Contains('\n') ||
            salida.Contains('\r');

        return caracteresVisibles >= 3 &&
            controlesInvalidos == 0 &&
            tokensEncontrados >= minimoTokens &&
            tieneEstructuraVisual;
    }

    private static string CrearMensaje(
        string salida,
        bool esCorrecta,
        bool cumpleEstructura,
        IReadOnlyList<ResultadoValorNumericoComparado> valores) {
        if (string.IsNullOrWhiteSpace(salida)) {
            return "El programa terminó sin mostrar una salida que pueda evaluarse.";
        }

        if (esCorrecta) {
            return "La salida contiene los datos y cálculos esperados.";
        }

        if (!cumpleEstructura) {
            return "La salida necesita identificar con mayor claridad todos los datos solicitados.";
        }

        string[] valoresIncorrectos = valores
            .Where(valor => !valor.Coincide)
            .Select(valor => valor.Nombre)
            .ToArray();

        return valoresIncorrectos.Length > 0
            ? $"Revisa el cálculo o la etiqueta de: {string.Join(", ", valoresIncorrectos)}."
            : "La salida todavía necesita algunos ajustes.";
    }

    [GeneratedRegex(@"(?<![\p{L}\p{N}_])[-+]?(?:\d+(?:[.,]\d+)?|[.,]\d+)(?![\p{L}\p{N}_])", RegexOptions.CultureInvariant)]
    private static partial Regex RegexNumero();

    [GeneratedRegex(@"^\s*[-+]?(?:\d+(?:[.,]\d+)?|[.,]\d+)(?![\p{L}\p{N}_])", RegexOptions.CultureInvariant)]
    private static partial Regex RegexNumeroInicial();

    [GeneratedRegex(@"\s+\d+\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex RegexOrdinalFinal();

    private readonly record struct PosicionNumero(
        int IndiceLinea,
        int IndiceCaracter,
        int Longitud);

    private sealed record CandidatoNumero(
        int IndiceLinea,
        int IndiceCaracter,
        int Longitud,
        double Valor,
        string Etiqueta);
}
