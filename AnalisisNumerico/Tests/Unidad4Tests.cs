using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using LogicaAnalisis.Unidad4;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Pantalla = AnalisisNumericoWeb.Unidad4.Unidad4;

internal static class Unidad4Tests
{
    private static int verificaciones;

    public static int Run(string[] args)
    {
        try
        {
            ProbarCalculos();
            ProbarCalculosTabla();
            ProbarCalculosEntreCurvas();
            if (args.Length < 2 || args[1] != "True")
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Task<CoreWebView2Environment> entorno = CoreWebView2Environment.CreateAsync(null,
                    Path.Combine(Path.GetTempPath(), "codex-unidad4-geogebra-perfil"),
                    new CoreWebView2EnvironmentOptions("--disable-features=CalculateNativeWinOcclusion --disable-renderer-backgrounding --disable-background-timer-throttling"));
                typeof(Pantalla).Assembly.GetType("AnalisisNumericoWeb.GeoGebraWebView")
                    .GetField("entornoCompartido", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, entorno);
                Exception error = null;
                using (Form anfitrion = new Form { Opacity = 0, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-4000, -4000) })
                {
                    anfitrion.Shown += (s, e) => anfitrion.BeginInvoke(new Action(() =>
                    {
                        try { ProbarPantalla(args.Length == 0 ? null : args[0]); ProbarMenu(); }
                        catch (Exception ex) { error = ex; }
                        finally { anfitrion.Close(); }
                    }));
                    Application.Run(anfitrion);
                }
                if (error != null) throw error;
            }
            Console.WriteLine("Correcto: " + verificaciones + " verificaciones de Unidad 4.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static void ProbarCalculos()
    {
        IntegradorNumerico integrador = new IntegradorNumerico();
        foreach (MetodoIntegracion metodo in Enum.GetValues(typeof(MetodoIntegracion)))
        {
            ResultadoIntegracion constante = integrador.Calcular("5", 2, 4, 10, metodo);
            Cerca(constante.Integral, 10, 1e-12, "Constante: " + metodo);
            Cerca(integrador.Calcular("2*x+1", -2, 3, 10, metodo).Integral, 10, 1e-12, "Recta: " + metodo);
            Cerca(integrador.Calcular("-2*x-1", -2, 3, 10, metodo).Integral, -10, 1e-12, "Integral negativa: " + metodo);
            Cerca(integrador.Calcular("x", -1, 1, 10, metodo).Integral, 0, 1e-12, "Cruce del eje: " + metodo);
            Cerca(integrador.Calcular("0", 0, 1, 10, metodo).Integral, 0, 0, "Funcion nula: " + metodo);
            Verificar(constante.Curva.Count == 401, "Muestras de la funcion: " + metodo);
            Verificar(constante.Nodos.Count == constante.Subintervalos + 1, "Cantidad de nodos: " + metodo);
        }
        Cerca(integrador.Calcular("x^2", 0, 1, 99, MetodoIntegracion.TrapeciosSimple).Integral, 0.5, 1e-12, "Trapecio simple");
        Cerca(integrador.Calcular("x^2", 0, 1, 10, MetodoIntegracion.TrapeciosMultiple).Integral, 0.335, 1e-12, "Trapecios multiples");
        Cerca(integrador.Calcular("1/x+x", 0.5, 3.5, 10, MetodoIntegracion.TrapeciosMultiple).Integral, 7.9744, 0.00005, "Ejemplo de la referencia");
        Cerca(integrador.Calcular("1,5*x", 0, 2, 10, MetodoIntegracion.TrapeciosMultiple).Integral, 3, 1e-12, "Coma decimal en la funcion");
        foreach (MetodoIntegracion metodo in new[] { MetodoIntegracion.SimpsonUnTercioSimple, MetodoIntegracion.SimpsonUnTercioMultiple, MetodoIntegracion.SimpsonTresOctavos })
            Cerca(integrador.Calcular("x^3-2*x+1", -1, 2, 10, metodo).Integral, 3.75, 1e-11, "Exactitud cubica: " + metodo);
        foreach (int n in new[] { 2, 3, 4, 5, 7, 12, 25, 1000 })
        {
            ResultadoIntegracion r = integrador.Calcular("x^3-2*x+1", -1, 2, n, MetodoIntegracion.SimpsonUnTercioMultiple);
            Cerca(r.Integral, 3.75, 1e-10, "Simpson con N=" + n);
            Verificar(r.Subintervalos == n, "Conservar N=" + n);
            if (n % 2 != 0) Verificar(r.MetodoUtilizado.Contains("3/8"), "Informar la combinacion N=" + n);
        }
        foreach (int n in new[] { 5, 25 })
        {
            double h = 1.0 / n;
            double esperado = 0.2 + ((n - 3) / 2 * (4.0 / 15) + 0.9) * Math.Pow(h, 5);
            Cerca(integrador.Calcular("x^4", 0, 1, n, MetodoIntegracion.SimpsonUnTercioMultiple).Integral,
                esperado, 1e-12, "Pesos y limite de la combinacion N=" + n);
        }
        Cerca(integrador.Calcular("sin(x)", 0, Math.PI, 100, MetodoIntegracion.SimpsonUnTercioMultiple).Integral, 2, 2e-8, "Seno");
        Cerca(integrador.Calcular("e^x", 0, 1, 100, MetodoIntegracion.SimpsonUnTercioMultiple).Integral, Math.E - 1, 1e-9, "Constante e");
        Cerca(integrador.Calcular("pi", 0, 1, 10, MetodoIntegracion.TrapeciosMultiple).Integral, Math.PI, 1e-12, "Constante pi");
        Cerca(integrador.Calcular("sqrt(x)", 1, 4, 1000, MetodoIntegracion.SimpsonUnTercioMultiple).Integral, 14.0 / 3, 1e-9, "Alias de raiz cuadrada");
        Cerca(integrador.Calcular("ln(1+x^2)", -2, 2, 1000, MetodoIntegracion.SimpsonUnTercioMultiple).Integral,
            4 * Math.Log(5) - 8 + 4 * Math.Atan(2), 1e-9, "Logaritmo del practico");
        Cerca(integrador.Calcular("x^2", 0, 1, 10000, MetodoIntegracion.TrapeciosMultiple).Integral, 1.0 / 3, 2e-9, "N maximo");
        foreach (string funcion in new[] { "", "x+", "noesfuncion(x)", "1/x", "ln(x)" })
            Rechazar(() => integrador.Calcular(funcion, -1, 1, 10, MetodoIntegracion.TrapeciosMultiple), "Funcion invalida o fuera de dominio: " + funcion);
        Rechazar(() => integrador.Calcular("x", 1, 0, 10, MetodoIntegracion.TrapeciosMultiple), "Intervalo invertido");
        Rechazar(() => integrador.Calcular("x", 1, 1, 10, MetodoIntegracion.TrapeciosMultiple), "Intervalo nulo");
        Rechazar(() => integrador.Calcular("x", double.NaN, 1, 10, MetodoIntegracion.TrapeciosMultiple), "Extremo NaN");
        Rechazar(() => integrador.Calcular("x", 0, double.PositiveInfinity, 10, MetodoIntegracion.TrapeciosMultiple), "Extremo infinito");
        Rechazar(() => integrador.Calcular("x", 0, 1, 0, MetodoIntegracion.TrapeciosMultiple), "N cero");
        Rechazar(() => integrador.Calcular("x", 0, 1, 10001, MetodoIntegracion.TrapeciosMultiple), "N excesivo");
        Rechazar(() => integrador.Calcular("x", 0, 1, 1, MetodoIntegracion.SimpsonUnTercioMultiple), "Simpson requiere dos subintervalos");
        Rechazar(() => integrador.Calcular("x", 0, 1, 10, (MetodoIntegracion)99), "Metodo inexistente");
        Rechazar(() => integrador.Calcular("x+y", 0, 1, 10, MetodoIntegracion.TrapeciosMultiple), "Variable adicional");
        Rechazar(() => integrador.Calcular("x", -double.MaxValue, double.MaxValue, 10, MetodoIntegracion.TrapeciosMultiple), "Desborde del intervalo");
        Console.WriteLine("Calculos y validaciones correctos.");
    }

    private static void ProbarCalculosTabla()
    {
        IntegradorNumerico integrador = new IntegradorNumerico();
        foreach (MetodoIntegracion metodo in Enum.GetValues(typeof(MetodoIntegracion)))
        {
            int n = metodo == MetodoIntegracion.TrapeciosSimple ? 1
                : metodo == MetodoIntegracion.SimpsonUnTercioSimple ? 2
                : metodo == MetodoIntegracion.SimpsonTresOctavos ? 3 : 10;
            ResultadoIntegracion r = integrador.CalcularTabla(Tabla(x => 2 * x + 1, -2, 5.0 / n, n), metodo);
            Cerca(r.Integral, 10, 1e-11, "Tabla lineal: " + metodo);
            Verificar(r.UsaTabla && r.PasoUniforme && r.Subintervalos == n, "Origen y segmentos de la tabla: " + metodo);
            Cerca(r.ExtremoIzquierdo, -2, 0, "Xi desde la tabla: " + metodo);
            Cerca(r.ExtremoDerecho, 3, 1e-12, "Xd desde la tabla: " + metodo);
            Cerca(integrador.CalcularTabla(Tabla(x => -2, 0, 1.0 / n, n), metodo).Integral, -2, 1e-12, "Tabla negativa: " + metodo);
            Cerca(integrador.CalcularTabla(Tabla(x => 0, 0, 1.0 / n, n), metodo).Integral, 0, 0, "Tabla nula: " + metodo);
        }
        Cerca(integrador.CalcularTabla(Tabla(x => x * x, 0, 2, 1), MetodoIntegracion.TrapeciosSimple).Integral, 4, 1e-12, "Tabla trapecio simple");
        Cerca(integrador.CalcularTabla(Tabla(x => x * x, 0, 1, 2), MetodoIntegracion.SimpsonUnTercioSimple).Integral, 8.0 / 3, 1e-12, "Tabla Simpson 1/3 simple");
        Cerca(integrador.CalcularTabla(Tabla(x => x * x * x, 0, 0.5, 3), MetodoIntegracion.SimpsonTresOctavos).Integral, 1.265625, 1e-12, "Tabla Simpson 3/8");
        foreach (int n in new[] { 2, 3, 4, 5, 7, 25, 1000 })
        {
            ResultadoIntegracion r = integrador.CalcularTabla(Tabla(x => x * x * x - 2 * x + 1, -1, 3.0 / n, n), MetodoIntegracion.SimpsonUnTercioMultiple);
            Cerca(r.Integral, 3.75, 1e-10, "Tabla Simpson N=" + n);
            if (n % 2 != 0) Verificar(r.MetodoUtilizado.Contains("3/8"), "Tabla informa combinacion N=" + n);
        }
        var irregular = new List<PuntoIntegracion> { new PuntoIntegracion(0, 0), new PuntoIntegracion(0.25, 0.0625), new PuntoIntegracion(1, 1), new PuntoIntegracion(2, 4) };
        ResultadoIntegracion trapecios = integrador.CalcularTabla(irregular, MetodoIntegracion.TrapeciosMultiple);
        Cerca(trapecios.Integral, 2.90625, 1e-12, "Trapecios con separacion variable");
        Verificar(!trapecios.PasoUniforme, "Informar paso variable");
        Verificar(trapecios.Aproximacion[1][0] == 0.25 && trapecios.Aproximacion[2][0] == 1, "Grafico respeta los nodos irregulares");
        var tablaGrande = Tabla(x => Math.Sin(x), 0, 1, 300);
        tablaGrande[150] = new PuntoIntegracion(149.5, 50);
        ResultadoIntegracion graficoGrande = integrador.CalcularTabla(tablaGrande, MetodoIntegracion.TrapeciosMultiple);
        Verificar(graficoGrande.Aproximacion.Count == tablaGrande.Count && graficoGrande.Aproximacion[150][0] == 149.5
            && graficoGrande.Aproximacion[150][1] == 50, "Conservar picos y nodos de una tabla grande en el sombreado");
        Cerca(integrador.CalcularTabla(Tabla(x => x * x, 0, 0.0001, 10000), MetodoIntegracion.TrapeciosMultiple).Integral,
            1.0 / 3, 2e-9, "Tabla con N maximo");
        Rechazar(() => integrador.CalcularTabla(null, MetodoIntegracion.TrapeciosMultiple), "Tabla nula");
        Rechazar(() => integrador.CalcularTabla(new List<PuntoIntegracion>(), MetodoIntegracion.TrapeciosMultiple), "Tabla vacia");
        Rechazar(() => integrador.CalcularTabla(Tabla(x => x, 0, 1, 0), MetodoIntegracion.TrapeciosMultiple), "Tabla con un punto");
        Rechazar(() => integrador.CalcularTabla(Tabla(x => x, 0, 1, 10001), MetodoIntegracion.TrapeciosMultiple), "Tabla excesiva");
        Rechazar(() => integrador.CalcularTabla(Tabla(x => x, 0, 1, 2), MetodoIntegracion.TrapeciosSimple), "Tabla simple requiere 2 puntos");
        Rechazar(() => integrador.CalcularTabla(Tabla(x => x, 0, 1, 3), MetodoIntegracion.SimpsonUnTercioSimple), "Tabla Simpson simple requiere 3 puntos");
        Rechazar(() => integrador.CalcularTabla(Tabla(x => x, 0, 1, 2), MetodoIntegracion.SimpsonTresOctavos), "Tabla 3/8 requiere 4 puntos");
        Rechazar(() => integrador.CalcularTabla(Tabla(x => x, 0, 1, 1), MetodoIntegracion.SimpsonUnTercioMultiple), "Tabla Simpson requiere dos segmentos");
        Rechazar(() => integrador.CalcularTabla(irregular, MetodoIntegracion.SimpsonUnTercioMultiple), "Simpson rechaza separacion variable");
        Rechazar(() => integrador.CalcularTabla(irregular, MetodoIntegracion.SimpsonTresOctavos), "3/8 rechaza separacion variable");
        Rechazar(() => integrador.CalcularTabla(Tabla(x => x, 2, -1, 2), MetodoIntegracion.TrapeciosMultiple), "Tabla desordenada");
        Rechazar(() => integrador.CalcularTabla(Tabla(x => x, 1, 0, 2), MetodoIntegracion.TrapeciosMultiple), "X repetidas");
        foreach (double valor in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            Rechazar(() => integrador.CalcularTabla(new[] { new PuntoIntegracion(0, 1), new PuntoIntegracion(valor, 2) }, MetodoIntegracion.TrapeciosSimple), "X no finita");
            Rechazar(() => integrador.CalcularTabla(new[] { new PuntoIntegracion(0, valor), new PuntoIntegracion(1, 2) }, MetodoIntegracion.TrapeciosSimple), "Y no finita");
        }
        Rechazar(() => integrador.CalcularTabla(new PuntoIntegracion[] { null, new PuntoIntegracion(1, 2) }, MetodoIntegracion.TrapeciosSimple), "Punto nulo");
        Rechazar(() => integrador.CalcularTabla(Tabla(x => x, 0, 1, 2), (MetodoIntegracion)99), "Metodo de tabla inexistente");
        Console.WriteLine("Calculos y validaciones de tablas correctos.");
    }

    private static List<PuntoIntegracion> Tabla(Func<double, double> funcion, double xi, double h, int n)
    {
        List<PuntoIntegracion> puntos = new List<PuntoIntegracion>();
        for (int i = 0; i <= n; i++)
        {
            double x = xi + h * i;
            puntos.Add(new PuntoIntegracion(x, funcion(x)));
        }
        return puntos;
    }

    private static void ProbarCalculosEntreCurvas()
    {
        IntegradorNumerico integrador = new IntegradorNumerico();
        foreach (MetodoIntegracion metodo in Enum.GetValues(typeof(MetodoIntegracion)))
        {
            ResultadoIntegracion r = integrador.CalcularEntreCurvas("2", "5", -1, 2, 10, metodo);
            Cerca(r.Integral, 9, 1e-12, "Area entre constantes: " + metodo);
            Cerca(integrador.CalcularEntreCurvas("5", "2", -1, 2, 10, metodo).Integral, 9, 1e-12, "Invertir orden de funciones: " + metodo);
            Cerca(integrador.CalcularEntreCurvas("2*x+1", "x", 0, 2, 10, metodo).Integral, 4, 1e-12, "Area entre rectas: " + metodo);
            Cerca(integrador.CalcularEntreCurvas("-5", "-2", 0, 2, 10, metodo).Integral, 6, 1e-12, "Dos curvas bajo el eje: " + metodo);
            Cerca(integrador.CalcularEntreCurvas("x^2", "x^2", -1, 1, 10, metodo).Integral, 0, 0, "Curvas coincidentes: " + metodo);
            Verificar(r.EsAreaEntreCurvas && !r.UsaTabla && r.Curva.Count == 401 && r.SegundaCurva.Count == 401, "Conservar dos curvas originales: " + metodo);
            Verificar(r.Curva[200][1] == 2 && r.SegundaCurva[200][1] == 5, "Graficar originales y no la diferencia: " + metodo);
        }
        ResultadoIntegracion ejercicio = integrador.CalcularEntreCurvas("x^4-4*x^2+2", "x^3-3*x+3", -1.95820988, 2.26666543, 25, MetodoIntegracion.SimpsonUnTercioMultiple);
        Cerca(ejercicio.Integral, 13.006888784068645, 1e-9, "Ejercicio 4 con 25 subintervalos");
        Verificar(ejercicio.Subintervalos == 25 && ejercicio.MetodoUtilizado.Contains("combinado"), "Conservar Simpson combinado entre curvas");
        Cerca(integrador.CalcularEntreCurvas("x", "0", -1, 1, 8, MetodoIntegracion.SimpsonUnTercioMultiple).Integral, 1, 1e-12, "Sumar areas a ambos lados de un cruce");
        Cerca(integrador.CalcularEntreCurvas("x", "0,123", -1, 1, 1000, MetodoIntegracion.SimpsonUnTercioMultiple).Integral, 1.015129, 2e-6, "Cruce fuera de los nodos");
        Rechazar(() => integrador.CalcularEntreCurvas("x", "", 0, 1, 10, MetodoIntegracion.TrapeciosMultiple), "Segunda funcion vacia");
        Rechazar(() => integrador.CalcularEntreCurvas("x", "x+", 0, 1, 10, MetodoIntegracion.TrapeciosMultiple), "Segunda funcion invalida");
        Rechazar(() => integrador.CalcularEntreCurvas("x", "1/x", -1, 1, 10, MetodoIntegracion.TrapeciosMultiple), "Dominio de la segunda funcion");
        Rechazar(() => integrador.CalcularEntreCurvas("1/x", "x", -1, 1, 10, MetodoIntegracion.TrapeciosMultiple), "Dominio de la primera funcion");
        Rechazar(() => integrador.CalcularEntreCurvas("x", "2*x", 1, 0, 10, MetodoIntegracion.TrapeciosMultiple), "Intervalo entre curvas invertido");
        Console.WriteLine("Calculos de areas entre curvas correctos.");
    }

    private static void ProbarPantalla(string carpeta)
    {
        using (Pantalla form = new Pantalla())
        {
            form.WindowState = FormWindowState.Normal;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-2000, -2000);
            form.ShowInTaskbar = false;
            form.Show();
            form.ClientSize = new Size(1440, 860);
            Application.DoEvents();
            ComboBox metodo = Campo<ComboBox>(form, "cmbMetodo");
            NumericUpDown n = Campo<NumericUpDown>(form, "nudSubintervalos");
            Verificar(metodo.Items.Count == 5 && metodo.SelectedIndex == 1, "Metodos disponibles");
            Invocar(form, "Calcular");
            Verificar(Campo<Label>(form, "lblIntegral").Text == "7,9744", "Resultado con cuatro decimales");
            Verificar(Campo<Label>(form, "lblEstado").Text == "Exitosa", "Estado exitoso");
            VerificarGrafico(form, carpeta, "trapecios-1440.png");
            ProbarNavegacion(form);

            metodo.SelectedIndex = 0;
            Verificar(!n.Enabled && n.Value == 1, "N fijo para trapecios simple");
            metodo.SelectedIndex = 2;
            Verificar(!n.Enabled && n.Value == 2, "N fijo para Simpson 1/3 simple");
            metodo.SelectedIndex = 4;
            Verificar(!n.Enabled && n.Value == 3, "N fijo para Simpson 3/8");
            metodo.SelectedIndex = 3;
            Verificar(n.Enabled && n.Value == 10, "Restaurar N para metodo multiple");
            Campo<TextBox>(form, "txtFuncion").Text = "x^3-2*x+1";
            Campo<TextBox>(form, "txtXi").Text = "-1";
            Campo<TextBox>(form, "txtXd").Text = "2";
            n.Text = "25";
            Invocar(form, "Calcular");
            Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, 3.75, 1e-10, "Confirmar N escrito al calcular");
            Verificar(Campo<ResultadoIntegracion>(form, "resultado").Subintervalos == 25, "Usar N=25");
            Verificar(Campo<Label>(form, "lblMetodo").Text.Contains("combinado"), "Informar Simpson combinado");
            VerificarGrafico(form, carpeta, "simpson-combinado-1440.png");
            form.Size = form.MinimumSize;
            Application.DoEvents();
            VerificarGrafico(form, carpeta, "simpson-combinado-minimo.png");
            ProbarNavegacion(form);
            form.ClientSize = new Size(1440, 860);
            Application.DoEvents();
            VerificarGrafico(form, carpeta, "simpson-ampliado.png");
            Campo<TextBox>(form, "txtXi").Text = "invalido";
            Verificar(Campo<ResultadoIntegracion>(form, "resultado") == null, "Invalidar al editar");
            Invocar(form, "Calcular");
            Verificar(Campo<ResultadoIntegracion>(form, "resultado") == null, "No conservar un resultado invalido");
            Verificar(Campo<Label>(form, "lblError").Text.Contains("izquierdo"), "Indicar el campo invalido");
            WebView2 navegador = Navegador(form);
            Esperar(() => Ejecutar(navegador, "!ggbApplet.exists('curva')") == "true", "Limpiar el grafico desactualizado");
            Campo<TextBox>(form, "txtXi").Text = "0";
            Campo<TextBox>(form, "txtFuncion").Text = "1/x";
            Invocar(form, "Calcular");
            Verificar(Campo<Label>(form, "lblError").Text.Contains("definida"), "Informar error de dominio");
            Campo<TextBox>(form, "txtFuncion").Text = "-x-1";
            Invocar(form, "Calcular");
            Verificar(Campo<Label>(form, "lblIntegral").Text == "-4,0000", "Preservar el signo de la integral");
            VerificarGrafico(form, carpeta, "integral-negativa.png");
            Campo<TextBox>(form, "txtFuncion").Text = "x";
            Campo<TextBox>(form, "txtXi").Text = "-1";
            Campo<TextBox>(form, "txtXd").Text = "1";
            Invocar(form, "Calcular");
            VerificarGrafico(form, carpeta, "integral-con-cruce.png");
            Verificar(Ejecutar(navegador, "ggbApplet.exists('region1') && ggbApplet.getVisible('region1')") == "true", "Separar el sombreado al cruzar el eje");
            Campo<TextBox>(form, "txtFuncion").Text = "0";
            Invocar(form, "Calcular");
            Esperar(() => Ejecutar(navegador, "ggbApplet.exists('curva') && !ggbApplet.exists('region0') && document.getElementById('estado').hidden") == "true", "Mostrar la funcion nula sin error de poligono");
            Verificar(Campo<Label>(form, "lblIntegral").Text == "0,0000", "Integral nula con cuatro decimales");
            Campo<TextBox>(form, "txtFuncion").Text = "-x-1";
            Campo<TextBox>(form, "txtXi").Text = "0";
            Campo<TextBox>(form, "txtXd").Text = "2";
            Invocar(form, "Calcular");
            ProbarRecarga(form);
            ProbarSinConexion(form);
            ProbarEntradaTabla(form, carpeta);
            ProbarDosFunciones(form, carpeta);
            BuscarBoton(form, "Limpiar").PerformClick();
            Verificar(Campo<TextBox>(form, "txtFuncion").Text == "" && Campo<TextBox>(form, "txtXi").Text == ""
                && Campo<TextBox>(form, "txtXd").Text == "" && Campo<ResultadoIntegracion>(form, "resultado") == null, "Limpiar entradas y resultado");
            BuscarBoton(form, "Volver").PerformClick();
            Verificar(!form.Visible, "Volver cierra la unidad");
        }
    }

    private static void ProbarEntradaTabla(Form form, string carpeta)
    {
        TabControl entrada = Campo<TabControl>(form, "tabEntrada");
        DataGridView puntos = Campo<DataGridView>(form, "dgvPuntos");
        ComboBox metodo = Campo<ComboBox>(form, "cmbMetodo");
        entrada.SelectedIndex = 1;
        Verificar(Campo<ResultadoIntegracion>(form, "resultado") == null, "Invalidar al cambiar de entrada");
        metodo.SelectedIndex = 3;
        Invocar(form, "Calcular");
        ResultadoIntegracion r = Campo<ResultadoIntegracion>(form, "resultado");
        Cerca(r.Integral, 8.0 / 3, 1e-12, "Calcular desde tabla sin formula");
        Verificar(r.UsaTabla && r.ExtremoIzquierdo == 0 && r.ExtremoDerecho == 2 && r.Subintervalos == 4, "Derivar intervalo y N de las filas");
        Verificar(Campo<Label>(form, "lblIntegral").Text == "2,6667", "Redondear resultado tabulado");
        VerificarGrafico(form, carpeta, "tabla-simpson.png");
        WebView2 navegador = Navegador(form);
        Verificar(Ejecutar(navegador, "document.getElementById('etiquetaCurva').textContent === 'Datos tabulados' && ggbApplet.exists('Dato0') && ggbApplet.getVisible('Dato0') && ggbApplet.getObjectType('Dato4') === 'point' && ggbApplet.getXcoord('Dato4') === 2 && ggbApplet.getYcoord('Dato4') === 4") == "true", "Identificar y dibujar los puntos tabulados");
        form.Size = form.MinimumSize;
        Application.DoEvents();
        VerificarGrafico(form, carpeta, "tabla-minimo.png");
        Button calcular = BuscarBoton(form, "Calcular integral");
        Rectangle boton = form.RectangleToClient(calcular.RectangleToScreen(calcular.ClientRectangle));
        Verificar(form.ClientRectangle.Contains(boton), "Boton calcular visible en ventana minima");
        form.ClientSize = new Size(1440, 860);
        Application.DoEvents();

        puntos.CurrentCell = puntos.Rows[4].Cells[1];
        puntos.BeginEdit(true);
        ((TextBox)puntos.EditingControl).Text = "5";
        Invocar(form, "Calcular");
        Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, 17.0 / 6, 1e-12, "Confirmar celda en edicion antes de integrar");
        puntos.Rows[4].Cells[1].Value = "4";
        Verificar(Campo<ResultadoIntegracion>(form, "resultado") == null, "Editar una celda invalida el resultado");
        puntos.CurrentCell = puntos.Rows[4].Cells[0];
        BuscarBoton(form, "Quitar fila").PerformClick();
        metodo.SelectedIndex = 4;
        Invocar(form, "Calcular");
        Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, 1.125, 1e-12, "Quitar fila y calcular con 3/8");
        BuscarBoton(form, "Agregar fila").PerformClick();
        Invocar(form, "Calcular");
        Verificar(Campo<ResultadoIntegracion>(form, "resultado") == null && Campo<Label>(form, "lblError").Text.Contains("fila 5"), "Detectar una fila incompleta");
        puntos.Rows[4].Cells[0].Value = "2.0";
        puntos.Rows[4].Cells[1].Value = "4,0";
        metodo.SelectedIndex = 3;
        Invocar(form, "Calcular");
        Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, 8.0 / 3, 1e-12, "Aceptar punto y coma decimal en tabla");
        puntos.Rows[2].Cells[0].Value = "1,1";
        Invocar(form, "Calcular");
        Verificar(Campo<ResultadoIntegracion>(form, "resultado") == null && Campo<Label>(form, "lblError").Text.Contains("separacion"), "Explicar la separacion requerida por Simpson");
        metodo.SelectedIndex = 1;
        Invocar(form, "Calcular");
        Verificar(Campo<ResultadoIntegracion>(form, "resultado") != null && Campo<Label>(form, "lblPaso").Text == "Variable", "Integrar tabla irregular con trapecios");
        puntos.Rows[2].Cells[0].Value = "0,5";
        Invocar(form, "Calcular");
        Verificar(Campo<ResultadoIntegracion>(form, "resultado") == null && Campo<Label>(form, "lblError").Text.Contains("creciente"), "Explicar X repetidas");
        puntos.Rows[2].Cells[0].Value = "1";
        metodo.SelectedIndex = 0;
        Invocar(form, "Calcular");
        Verificar(Campo<Label>(form, "lblError").Text.Contains("exactamente 2"), "Explicar cantidad de puntos para metodo simple");
        BuscarBoton(form, "Limpiar").PerformClick();
        Verificar(puntos.Rows.Count == 1 && puntos.Rows[0].IsNewRow && Campo<ResultadoIntegracion>(form, "resultado") == null, "Limpiar tabla y resultado");
        Invocar(form, "Calcular");
        Verificar(Campo<Label>(form, "lblError").Text.Contains("2 y"), "Rechazar tabla vacia en pantalla");
        puntos.Rows.Add("0", "0");
        puntos.Rows.Add("2", "4");
        Invocar(form, "Calcular");
        Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, 4, 1e-12, "Cargar nuevos datos despues de limpiar");
        VerificarGrafico(form, carpeta, "tabla-trapecio.png");
        entrada.SelectedIndex = 0;
        Invocar(form, "Calcular");
        Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, -4, 1e-12, "Conservar la funcion al volver desde la tabla");
        Esperar(() => Ejecutar(navegador, "document.getElementById('etiquetaCurva').textContent === 'f(x)' && !ggbApplet.exists('Dato0')") == "true", "Restaurar leyenda de funcion y quitar puntos tabulados");
    }

    private static void ProbarDosFunciones(Form form, string carpeta)
    {
        CheckBox dos = Campo<CheckBox>(form, "chkDosFunciones");
        TextBox f = Campo<TextBox>(form, "txtFuncion"), g = Campo<TextBox>(form, "txtSegundaFuncion");
        ComboBox metodo = Campo<ComboBox>(form, "cmbMetodo");
        dos.Checked = true;
        Verificar(g.Visible && Campo<ResultadoIntegracion>(form, "resultado") == null, "Mostrar g(x) e invalidar al activar dos curvas");
        Verificar(Campo<Label>(form, "lblTituloIntegral").Text == "Area entre curvas", "Identificar el resultado como area");
        f.Text = "x^4-4*x^2+2";
        g.Text = "x^3-3*x+3";
        Campo<TextBox>(form, "txtXi").Text = "-1,95820988";
        Campo<TextBox>(form, "txtXd").Text = "2,26666543";
        metodo.SelectedIndex = 3;
        Campo<NumericUpDown>(form, "nudSubintervalos").Value = 25;
        BuscarBoton(form, "Calcular area y graficar").PerformClick();
        Verificar(Campo<Label>(form, "lblIntegral").Text == "13,0069", "Resultado del ejercicio con dos funciones");
        VerificarGrafico(form, carpeta, "dos-funciones-ejercicio4.png", true);
        WebView2 navegador = Navegador(form);
        Verificar(Ejecutar(navegador, "!document.getElementById('leyendaSegunda').hidden && !document.getElementById('leyendaArea').hidden && document.getElementById('leyendaAproximacion').hidden && !ggbApplet.exists('aproximacion') && !ggbApplet.exists('division0')") == "true", "Leyenda y sombreado de dos funciones");
        Cerca(Numero(navegador, "ggbApplet.getValue('Area(region0)')"), 13.007417, 0.002, "Sombrear entre las curvas del ejercicio");
        ProbarNavegacion(form);
        form.Size = form.MinimumSize;
        Application.DoEvents();
        VerificarGrafico(form, carpeta, "dos-funciones-minimo.png", true);
        Rectangle campoG = form.RectangleToClient(g.RectangleToScreen(g.ClientRectangle));
        Verificar(form.ClientRectangle.Contains(campoG), "Segunda funcion visible en ventana minima");
        NumericUpDown entradaN = Campo<NumericUpDown>(form, "nudSubintervalos");
        Control campos = Campo<Control>(form, "camposFuncion");
        Rectangle campoN = campos.RectangleToClient(entradaN.RectangleToScreen(entradaN.ClientRectangle));
        Verificar(campos.ClientRectangle.Contains(campoN), "Subintervalos completos en ventana minima");
        form.ClientSize = new Size(1440, 860);
        Application.DoEvents();
        g.Text = "";
        Invocar(form, "Calcular");
        Verificar(Campo<ResultadoIntegracion>(form, "resultado") == null && Campo<Label>(form, "lblError").Text.Contains("g(x)"), "Pedir la segunda funcion");

        f.Text = "x"; g.Text = "0,123";
        Campo<TextBox>(form, "txtXi").Text = "-1";
        Campo<TextBox>(form, "txtXd").Text = "1";
        Campo<NumericUpDown>(form, "nudSubintervalos").Value = 1000;
        Invocar(form, "Calcular");
        Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, 1.015129, 2e-6, "Area sin cancelar al cruzarse las funciones");
        VerificarGrafico(form, carpeta, "dos-funciones-cruce.png", true);
        Verificar(Ejecutar(navegador, "ggbApplet.exists('region1') && ggbApplet.getVisible('region1')") == "true", "Separar el sombreado en un cruce entre muestras");
        Cerca(Numero(navegador, "ggbApplet.getValue('Area(region0)+Area(region1)')"), 1.015129, 1e-8, "Regiones sin autointerseccion");

        f.Text = "1000"; g.Text = "1001";
        Campo<TextBox>(form, "txtXi").Text = "0";
        Campo<TextBox>(form, "txtXd").Text = "1";
        Invocar(form, "Calcular");
        VerificarGrafico(form, carpeta, "dos-funciones-desplazadas.png", true);
        Cerca(Numero(navegador, "ggbApplet.getValue('Area(region0)')"), 1, 1e-8, "El sombreado no llega al eje X");
        Verificar(Numero(navegador, "JSON.parse(ggbApplet.getViewProperties(1)).yMin") > 990, "Ajustar escala a las dos curvas");
        g.Text = "1000";
        Invocar(form, "Calcular");
        Esperar(() => Ejecutar(navegador, "ggbApplet.exists('segundaCurva') && !ggbApplet.exists('region0') && document.getElementById('estado').hidden") == "true", "Curvas coincidentes sin poligono degenerado");
        Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, 0, 0, "Area nula en pantalla");

        Campo<TabControl>(form, "tabEntrada").SelectedIndex = 1;
        Verificar(Campo<Label>(form, "lblTituloIntegral").Text == "Integral aproximada", "Modo tabla independiente de la segunda funcion");
        metodo.SelectedIndex = 0;
        Invocar(form, "Calcular");
        Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, 4, 1e-12, "Tabla conserva su propio calculo");
        Campo<TabControl>(form, "tabEntrada").SelectedIndex = 0;
        dos.Checked = false;
        Verificar(!g.Visible && Campo<Label>(form, "lblTituloIntegral").Text == "Integral aproximada", "Restaurar modo de una funcion");
        f.Text = "-x-1";
        Campo<TextBox>(form, "txtXi").Text = "0";
        Campo<TextBox>(form, "txtXd").Text = "2";
        Invocar(form, "Calcular");
        Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, -4, 1e-12, "Preservar signo en el modo de una funcion");
        VerificarGrafico(form, carpeta, "una-funcion-restaurada.png");
        Verificar(Ejecutar(navegador, "!ggbApplet.exists('segundaCurva') && document.getElementById('leyendaSegunda').hidden") == "true", "Retirar la segunda curva al desactivar");
    }

    private static void ProbarMenu()
    {
        using (AnalisisNumericoWeb.Form1 menu = new AnalisisNumericoWeb.Form1())
        using (System.Windows.Forms.Timer temporizador = new System.Windows.Forms.Timer { Interval = 100 })
        {
            menu.WindowState = FormWindowState.Normal;
            menu.StartPosition = FormStartPosition.Manual;
            menu.Location = new Point(-3000, -3000);
            menu.ShowInTaskbar = false;
            menu.Show();
            bool abierta = false;
            temporizador.Tick += (s, e) =>
            {
                foreach (Form ventana in Application.OpenForms)
                    if (ventana is Pantalla) { abierta = ventana.Owner == menu; ventana.Close(); break; }
            };
            temporizador.Start();
            menu.GetType().GetMethod("AbrirUnidad", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(menu, new object[] { "UNIDAD 4" });
            temporizador.Stop();
            Verificar(abierta, "Abrir Unidad 4 desde el menu principal");
        }
    }

    private static Button BuscarBoton(Control raiz, string texto)
    {
        foreach (Control hijo in raiz.Controls)
        {
            Button boton = hijo as Button;
            if (boton != null && boton.Text == texto) return boton;
            Button encontrado = BuscarBoton(hijo, texto);
            if (encontrado != null) return encontrado;
        }
        return null;
    }

    private static void VerificarGrafico(Form form, string carpeta, string archivo, bool dosFunciones = false)
    {
        WebView2 navegador = Navegador(form);
        Control grafico = Campo<Control>(form, "grafico");
        Console.WriteLine("Verificando GeoGebra: " + archivo);
        Esperar(() => navegador.CoreWebView2 != null, "Inicializar WebView2");
        Esperar(() => Campo<string>(grafico, "datosEnviados") == Campo<string>(grafico, "datosPendientes")
            && Ejecutar(navegador, "window.geoGebraListo === true && ggbApplet.exists('curva') && ggbApplet.exists('" + (dosFunciones ? "segundaCurva" : "aproximacion") + "') && document.getElementById('estado').hidden") == "true",
            "Cargar GeoGebra y mostrar funcion e integral", 70000);
        Esperar(() => Ejecutar(navegador, "(() => { const a=document.getElementById('grafico').getBoundingClientRect(), c=document.querySelector('canvas').getBoundingClientRect(); return Math.abs(a.width-c.width)<=2 && Math.abs(a.height-c.height)<=2; })()") == "true",
            "El plano ocupa todo el panel");
        Verificar(Ejecutar(navegador, "ggbApplet.getMode()") == "40", "Modo de desplazamiento");
        Verificar(Ejecutar(navegador, "ggbApplet.exists('region0') && ggbApplet.getVisible('region0')") == "true", "Sombrear la region");
        using (MemoryStream memoria = new MemoryStream())
        {
            Terminar(navegador.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, memoria));
            memoria.Position = 0;
            using (Bitmap captura = new Bitmap(memoria))
            {
                int verdes = 0, rosas = 0;
                for (int x = 0; x < captura.Width; x++)
                for (int y = 40; y < captura.Height; y++)
                {
                    Color p = captura.GetPixel(x, y);
                    if (p.G - p.R > 40 && p.B - p.R > 20) verdes++;
                    if (p.R > p.G + 10 && p.B > p.G + 3) rosas++;
                }
                Verificar(verdes > 60, "Funcion visible en pixeles");
                Verificar(rosas > 500, "Aproximacion sombreada visible en pixeles");
                if (!string.IsNullOrEmpty(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                    captura.Save(Path.Combine(carpeta, "grafico-" + archivo));
                    using (Bitmap pantalla = new Bitmap(form.Width, form.Height))
                    {
                        form.DrawToBitmap(pantalla, new Rectangle(Point.Empty, pantalla.Size));
                        using (Graphics g = Graphics.FromImage(pantalla))
                        {
                            Point posicion = form.PointToClient(grafico.PointToScreen(Point.Empty));
                            posicion.Offset((form.Width - form.ClientSize.Width) / 2, form.Height - form.ClientSize.Height - (form.Width - form.ClientSize.Width) / 2);
                            g.DrawImageUnscaled(captura, posicion);
                        }
                        pantalla.Save(Path.Combine(carpeta, archivo));
                    }
                }
            }
        }
    }

    private static void ProbarNavegacion(Form form)
    {
        WebView2 navegador = Navegador(form);
        Control grafico = Campo<Control>(form, "grafico");
        Terminar(navegador.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setFocusEmulationEnabled", "{\"enabled\":true}"));
        double escala = Numero(navegador, "JSON.parse(ggbApplet.getViewProperties(1)).invXscale");
        int x = grafico.Width / 2, y = grafico.Height / 2;
        Mouse(navegador, new { type = "mouseMoved", x, y });
        Mouse(navegador, new { type = "mouseWheel", x, y, deltaX = 0, deltaY = -150, modifiers = 8 });
        Esperar(() => Numero(navegador, "JSON.parse(ggbApplet.getViewProperties(1)).invXscale") < escala, "Zoom con Shift y rueda");
        double inicio = Numero(navegador, "JSON.parse(ggbApplet.getViewProperties(1)).xMin");
        Mouse(navegador, new { type = "mousePressed", x, y, button = "left", buttons = 1, clickCount = 1 });
        for (int i = 1; i <= 5; i++) Mouse(navegador, new { type = "mouseMoved", x = x + i * 8, y = y + i * 3, button = "left", buttons = 1 });
        Mouse(navegador, new { type = "mouseReleased", x = x + 40, y = y + 15, button = "left", buttons = 0, clickCount = 1 });
        Esperar(() => Math.Abs(Numero(navegador, "JSON.parse(ggbApplet.getViewProperties(1)).xMin") - inicio) > 1e-6, "Arrastrar el plano");
    }

    private static void ProbarRecarga(Form form)
    {
        WebView2 navegador = Navegador(form);
        TaskCompletionSource<bool> final = new TaskCompletionSource<bool>();
        EventHandler<CoreWebView2NavigationCompletedEventArgs> manejador = (s, e) => final.TrySetResult(e.IsSuccess);
        navegador.NavigationCompleted += manejador;
        navegador.Reload();
        Terminar(final.Task);
        navegador.NavigationCompleted -= manejador;
        Verificar(final.Task.Result, "Recargar la pagina");
        VerificarGrafico(form, null, "recarga");
    }

    private static void ProbarSinConexion(Form form)
    {
        WebView2 navegador = Navegador(form);
        Terminar(navegador.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.enable", "{}"));
        Terminar(navegador.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.setBlockedURLs", "{\"urls\":[\"*cdn.geogebra.org*\"]}"));
        navegador.Reload();
        Esperar(() => Ejecutar(navegador, "document.getElementById('reintentar') && !document.getElementById('reintentar').hidden") == "true", "Ofrecer reintento cuando falla la descarga", 15000);
        Cerca(Campo<ResultadoIntegracion>(form, "resultado").Integral, -4, 1e-10, "El calculo no depende de internet");
        Terminar(navegador.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.setBlockedURLs", "{\"urls\":[]}"));
        Ejecutar(navegador, "document.getElementById('reintentar').click()");
        VerificarGrafico(form, null, "reintento");
    }

    private static WebView2 Navegador(Form form) { return Campo<WebView2>(Campo<Control>(form, "grafico"), "navegador"); }
    private static T Campo<T>(object objeto, string nombre) { return (T)objeto.GetType().GetField(nombre, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(objeto); }
    private static void Invocar(object objeto, string nombre) { objeto.GetType().GetMethod(nombre, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(objeto, null); }
    private static string Ejecutar(WebView2 navegador, string script) { Task<string> tarea = navegador.ExecuteScriptAsync(script); Terminar(tarea); return tarea.Result; }
    private static double Numero(WebView2 navegador, string script) { return double.Parse(Ejecutar(navegador, script), CultureInfo.InvariantCulture); }
    private static void Mouse(WebView2 navegador, object evento) { Terminar(navegador.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.dispatchMouseEvent", new JavaScriptSerializer().Serialize(evento))); }
    private static void Terminar(Task tarea) { Esperar(() => tarea.IsCompleted, "Operacion asincrona", 15000, false); tarea.GetAwaiter().GetResult(); }
    private static void Esperar(Func<bool> condicion, string nombre, int milisegundos = 7000, bool contar = true)
    {
        Stopwatch reloj = Stopwatch.StartNew();
        while (!condicion())
        {
            if (reloj.ElapsedMilliseconds > milisegundos) throw new Exception(nombre + ": tiempo de espera agotado.");
            Application.DoEvents(); Thread.Sleep(20);
        }
        if (contar) verificaciones++;
    }
    private static void Cerca(double obtenido, double esperado, double tolerancia, string nombre)
    {
        Verificar(!double.IsNaN(obtenido) && Math.Abs(obtenido - esperado) <= tolerancia, nombre + ": esperado " + esperado + ", obtenido " + obtenido);
    }
    private static void Verificar(bool condicion, string nombre) { if (!condicion) throw new Exception(nombre); verificaciones++; }
    private static void Rechazar(Action accion, string nombre)
    {
        try { accion(); }
        catch (ArgumentException) { verificaciones++; return; }
        throw new Exception("Debio rechazar: " + nombre);
    }
}
