using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using LogicaAnalisis.Unidad3;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using PantallaUnidad3 = AnalisisNumericoWeb.Unidad3.Unidad3;

internal static class Unidad3Tests
{
    private static int verificaciones;

    [STAThread]
    public static int Run(string[] args)
    {
        try
        {
            ProbarCalculos();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var entornoPruebas = CoreWebView2Environment.CreateAsync(null,
                Path.Combine(Path.GetTempPath(), "codex-unidad3-geogebra-perfil"),
                new CoreWebView2EnvironmentOptions("--disable-features=CalculateNativeWinOcclusion --disable-renderer-backgrounding --disable-background-timer-throttling"));
            typeof(AnalisisNumericoWeb.GeoGebraWebView).GetField("entornoCompartido", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, entornoPruebas);
            Exception errorInterfaz = null;
            using (Form anfitrion = new Form { Opacity = 0, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-4000, -4000) })
            {
                anfitrion.Shown += (s, e) => anfitrion.BeginInvoke(new Action(() =>
                {
                    try { ProbarPantalla(args.Length == 0 ? null : args[0]); }
                    catch (Exception error) { errorInterfaz = error; }
                    finally { anfitrion.Close(); }
                }));
                Application.Run(anfitrion);
            }
            if (errorInterfaz != null) throw errorInterfaz;
            Console.WriteLine("Correcto: " + verificaciones + " verificaciones de Unidad 3.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static void ProbarCalculos()
    {
        MetodoAjusteCurvas metodo = new MetodoAjusteCurvas();
        List<PuntoAjuste> puntos = CrearPuntos(new[] { -1.0, 0, 2, 3, 5, 6 }, new[] { 6.0, 4, 1, 1, 2, 5 });
        ResultadoAjuste cuadratica = metodo.Calcular(puntos, 2, 80);
        Cerca(cuadratica.Coeficientes[0], 3.7333333333, 1e-8, "Intercepto del ejemplo cuadratico");
        Cerca(cuadratica.Coeficientes[1], -2.1016666667, 1e-8, "Coeficiente lineal del ejemplo cuadratico");
        Cerca(cuadratica.Coeficientes[2], 0.375, 1e-9, "Coeficiente cuadratico");
        Cerca(cuadratica.CorrelacionPorcentaje, 98.55, 0.01, "Correlacion del documento");
        Verificar(cuadratica.EsAceptable, "Aceptable con tolerancia de 80%");
        Verificar(!metodo.Calcular(puntos, 2, 99).EsAceptable, "No aceptable al superar la correlacion");

        List<PuntoAjuste> cuarto = CrearPuntos(new[] { -3.2, -2.75, -1, 0, 0.5, 1.5, 2, 2.33 }, new[] { 4.8, -4.05, 0, 3, 2, -1.4, 0, 4 });
        ResultadoAjuste polinomio = metodo.Calcular(cuarto, 4, 80);
        Cerca(polinomio.Coeficientes[4], 0.5085, 0.0001, "Grado cuatro del documento");
        Cerca(polinomio.Coeficientes[0], 3.0355, 0.0001, "Intercepto del grado cuatro");
        Cerca(polinomio.CorrelacionPorcentaje, 99.9973, 0.0001, "Correlacion del grado cuatro");

        List<PuntoAjuste> recta = CrearPuntos(new[] { 0.0, 1, 2, 3 }, new[] { -3.0, -1, 1, 3 });
        ResultadoAjuste lineal = metodo.Calcular(recta, 1, 80);
        Cerca(lineal.Coeficientes[1], 2, 1e-12, "Pendiente exacta");
        Cerca(lineal.Coeficientes[0], -3, 1e-12, "Intercepto exacto");
        Cerca(lineal.CorrelacionPorcentaje, 100, 1e-9, "Recta exacta");
        Cerca(metodo.EvaluarRectaModificada(recta, "y = 2*x - 3", 80).CorrelacionPorcentaje, 100, 1e-9, "Recta modificada exacta");
        Cerca(metodo.EvaluarRectaModificada(recta, "y = 2,0x - 3", 80).Evaluar(2), 1, 1e-12, "Coma decimal en la funcion");
        Cerca(metodo.EvaluarRectaModificada(recta, "-3+2e0x", 80).Evaluar(2), 1, 1e-12, "Terminos invertidos y exponente");
        Verificar(!metodo.EvaluarRectaModificada(recta, "y=20x+50", 80).EsAceptable, "Recta modificada con ajuste pobre");
        Rechaza(() => metodo.EvaluarRectaModificada(recta, "y=x^2", 80), "Rechazar funciones no lineales");
        Rechaza(() => metodo.EvaluarRectaModificada(recta, "2x3", 80), "Rechazar terminos sin separador");

        List<PuntoAjuste> constante = CrearPuntos(new[] { 0.0, 1, 2 }, new[] { 5.0, 5, 5 });
        Cerca(metodo.Calcular(constante, 1, 80).CorrelacionPorcentaje, 100, 1e-9, "Valores Y constantes");
        Cerca(metodo.EvaluarRectaModificada(constante, "y=6", 80).CorrelacionPorcentaje, 0, 1e-9, "Constante modificada incorrecta");
        Cerca(metodo.Calcular(CrearPuntos(new[] { 0.0, 1, 2, 3, 4, 5, 6 }, new[] { 5.0, 5, 5, 5, 5, 5, 5 }), 2, 80).CorrelacionPorcentaje,
            100, 1e-9, "Constante con media que requiere redondeo");
        Rechaza(() => metodo.Calcular(recta, 4, 80), "Cantidad insuficiente de puntos");
        Rechaza(() => metodo.Calcular(recta, 11, 80), "Limite de grado diez");
        Rechaza(() => metodo.Calcular(recta, 1, double.NaN), "Tolerancia no finita");
        Rechaza(() => metodo.Calcular(CrearPuntos(new[] { 1.0, 1 }, new[] { 2.0, 3 }), 1, 80), "X sin variacion");
        Rechaza(() => metodo.Calcular(CrearPuntos(new[] { 0.0, double.PositiveInfinity }, new[] { 2.0, 3 }), 1, 80), "Coordenadas no finitas");
        List<PuntoAjuste> repetidos = CrearPuntos(new[] { 0.0, 0, 1, 1 }, new[] { 0.0, 2, 2, 4 });
        Cerca(metodo.Calcular(repetidos, 1, 80).Evaluar(0), 1, 1e-12, "Repeticiones de X con rango suficiente");

        List<PuntoAjuste> gradoDiez = new List<PuntoAjuste>();
        for (int i = 0; i <= 15; i++)
        {
            double x = -1 + i * 2.0 / 15;
            gradoDiez.Add(new PuntoAjuste(x, 1 - 2 * x + Math.Pow(x, 10)));
        }
        ResultadoAjuste diez = metodo.Calcular(gradoDiez, 10, 80);
        Cerca(diez.Evaluar(0.37), 1 - 2 * 0.37 + Math.Pow(0.37, 10), 1e-7, "Evaluacion de grado diez");
        Cerca(diez.Coeficientes[10], 1, 1e-6, "Coeficiente de grado diez");

        List<PuntoAjuste> desplazados = new List<PuntoAjuste>();
        for (int i = 0; i < 7; i++) desplazados.Add(new PuntoAjuste(10000 + i, 2 * i * i - 3 * i + 1));
        Cerca(metodo.Calcular(desplazados, 2, 80).Evaluar(10002.5), 6, 1e-8, "Coordenadas X desplazadas");
        puntos[0].Y = 100;
        Cerca(cuadratica.Puntos[0].Y, 6, 0, "El resultado conserva una copia de los puntos");
    }

    private static void ProbarPantalla(string carpetaCapturas)
    {
        using (PantallaUnidad3 form = new PantallaUnidad3())
        {
            form.WindowState = FormWindowState.Normal;
            form.Opacity = 1;
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-2000, -2000);
            WebView2 navegadorDiagnostico = Campo<WebView2>(Campo<Control>(form, "grafico"), "navegador");
            navegadorDiagnostico.CoreWebView2InitializationCompleted += async (s, e) =>
            {
                if (!e.IsSuccess) return;
                navegadorDiagnostico.CoreWebView2.GetDevToolsProtocolEventReceiver("Runtime.exceptionThrown").DevToolsProtocolEventReceived +=
                    (sender, evento) => Console.WriteLine("Error JavaScript: " + evento.ParameterObjectAsJson);
                await navegadorDiagnostico.CoreWebView2.CallDevToolsProtocolMethodAsync("Runtime.enable", "{}");
            };
            form.Show();
            form.ClientSize = new Size(1280, 780);
            CrearControles(form);
            form.PerformLayout();
            Application.DoEvents();
            DataGridView tabla = Campo<DataGridView>(form, "dgvPuntos");
            Verificar(!tabla.ReadOnly && !tabla.Columns[1].ReadOnly && !tabla.Columns[2].ReadOnly, "X e Y editables");
            Verificar(tabla.Columns[0].ReadOnly, "Nombre del punto protegido");
            Invocar(form, "Calcular");
            Cerca(Campo<ResultadoAjuste>(form, "resultadoOriginal").Coeficientes[1], 0.7, 1e-10, "Calculo inicial de pantalla");

            Campo<TextBox>(form, "txtFuncionModificada").Text = "y = 2x - 3";
            Invocar(form, "CalcularModificada");
            Verificar(Campo<ResultadoAjuste>(form, "resultadoModificado") != null, "Calcular recta modificada");
            GuardarCaptura(form, carpetaCapturas, "unidad3-lineal-1280.png");
            VerificarGrafico(form, true, carpetaCapturas, "geogebra-lineal-1280.png");
            ProbarNavegacion(form);
            ProbarExpresionesGeoGebra(form);

            tabla.CurrentCell = tabla.Rows[0].Cells[1];
            Verificar(tabla.BeginEdit(true), "Comenzar edicion de X");
            ((TextBox)tabla.EditingControl).Text = "3,5";
            Verificar(Campo<ResultadoAjuste>(form, "resultadoOriginal") == null, "Invalidar resultados al editar");
            WebView2 navegador = Campo<WebView2>(Campo<Control>(form, "grafico"), "navegador");
            EsperarCondicion(() => EjecutarScript(navegador, "!ggbApplet.exists('f') && !ggbApplet.exists('g')") == "true", 5000,
                "Eliminar los ajustes desactualizados de GeoGebra");
            Invocar(form, "Calcular");
            Cerca(Campo<ResultadoAjuste>(form, "resultadoOriginal").Puntos[0].X, 3.5, 0, "Confirmar celda activa al calcular");

            tabla.Rows[0].Cells[2].Value = "no numerico";
            Invocar(form, "Calcular");
            Verificar(Campo<ResultadoAjuste>(form, "resultadoOriginal") == null, "Rechazar punto invalido");
            Verificar(Campo<Label>(form, "lblError").Text.Contains("P1"), "Indicar el punto invalido");

            ComboBox selector = Campo<ComboBox>(form, "cmbMetodo");
            selector.SelectedIndex = 1;
            Verificar(!Campo<Panel>(form, "panelModificacion").Visible, "Modificacion solo para lineal");
            Verificar(Campo<NumericUpDown>(form, "nudGrado").Maximum == 10, "Grado maximo en pantalla");
            tabla.CancelEdit();
            tabla.Rows.Clear();
            double[] xs = { -1, 0, 2, 3, 5, 6 };
            double[] ys = { 6, 4, 1, 1, 2, 5 };
            for (int i = 0; i < xs.Length; i++) tabla.Rows.Add("P" + (i + 1), xs[i].ToString(), ys[i].ToString());
            Invocar(form, "Calcular");
            Verificar(Campo<ResultadoAjuste>(form, "resultadoOriginal").Grado == 2, "Seleccion de polinomial");
            GuardarCaptura(form, carpetaCapturas, "unidad3-polinomial-1280.png");
            VerificarGrafico(form, false, carpetaCapturas, "geogebra-polinomial-1280.png");

            form.Size = form.MinimumSize;
            form.PerformLayout();
            GuardarCaptura(form, carpetaCapturas, "unidad3-polinomial-minimo.png");
            Verificar(Campo<Control>(form, "grafico").Height >= 200, "Grafico visible en ventana minima");
            VerificarGrafico(form, false, carpetaCapturas, "geogebra-polinomial-minimo.png");
            Verificar(EjecutarScript(navegador,
                "(() => { const vista = JSON.parse(ggbApplet.getViewProperties(1)); "
                + "return datosPendientes.puntos.every(p => p[0] > vista.xMin && p[0] < vista.xMin + vista.width * vista.invXscale "
                + "&& p[1] > vista.yMin && p[1] < vista.yMin + vista.height * vista.invYscale); })()") == "true",
                "Los puntos siguen encuadrados al achicar la ventana");
            selector.SelectedIndex = 0;
            Invocar(form, "Calcular");
            GuardarCaptura(form, carpetaCapturas, "unidad3-lineal-minimo.png");
            Verificar(Campo<Control>(form, "grafico").Height >= 200, "Comparacion visible en ventana minima");
            VerificarGrafico(form, false, carpetaCapturas, "geogebra-lineal-minimo.png");
            ProbarNavegacion(form);
            form.ClientSize = new Size(1280, 780);
            form.PerformLayout();
            VerificarGrafico(form, false, carpetaCapturas, "geogebra-lineal-ampliado.png");
            TaskCompletionSource<bool> recarga = new TaskCompletionSource<bool>();
            EventHandler<CoreWebView2NavigationCompletedEventArgs> alRecargar = (s, e) => recarga.TrySetResult(e.IsSuccess);
            navegador.NavigationCompleted += alRecargar;
            navegador.Reload();
            Terminar(recarga.Task);
            navegador.NavigationCompleted -= alRecargar;
            Verificar(recarga.Task.Result, "Recargar la pagina de GeoGebra");
            VerificarGrafico(form, false, null, null);
        }
    }

    private static void CrearControles(Control control)
    {
        IntPtr handle = control.Handle;
        foreach (Control hijo in control.Controls) CrearControles(hijo);
    }

    private static void VerificarGrafico(Form form, bool modificada, string carpeta, string archivo)
    {
        Control grafico = Campo<Control>(form, "grafico");
        Verificar(Campo<ResultadoAjuste>(form, "resultadoOriginal") != null, "Conservar el resultado al mostrar el grafico");
        WebView2 navegador = Campo<WebView2>(grafico, "navegador");
        Console.WriteLine("Verificando GeoGebra " + (archivo ?? "despues de recargar") + "...");
        EsperarCondicion(() => navegador.CoreWebView2 != null, 20000, "Inicializar WebView2");
        try
        {
            EsperarCondicion(() => Campo<string>(grafico, "datosEnviados") == Campo<string>(grafico, "datosPendientes")
                && EjecutarScript(navegador, "window.geoGebraListo === true && window.ggbApplet && ggbApplet.exists('f')") == "true", 60000,
                "GeoGebra debe cargar y dibujar la funcion");
        }
        catch
        {
            Console.WriteLine("Pendiente: " + Campo<string>(grafico, "datosPendientes"));
            Console.WriteLine("Enviado: " + Campo<string>(grafico, "datosEnviados"));
            Console.WriteLine("Enviando: " + Campo<bool>(grafico, "enviando") + ", pagina lista: " + Campo<bool>(grafico, "paginaLista")
                + ", estado: " + Campo<Label>(grafico, "estado").Text);
            Console.WriteLine(EjecutarScript(navegador, "JSON.stringify({listo:window.geoGebraListo, api:!!window.ggbApplet, texto:document.body.innerText, visibilidad:document.visibilityState, scripts:Array.from(document.scripts).map(s=>s.src)})"));
            if (carpeta != null)
            {
                Directory.CreateDirectory(carpeta);
                using (FileStream captura = File.Create(Path.Combine(carpeta, "geogebra-error.png")))
                    Terminar(navegador.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, captura));
            }
            throw;
        }
        Verificar(EjecutarScript(navegador, "document.getElementById('estado').hidden") == "true", "Ocultar el estado de carga");
        Verificar(EjecutarScript(navegador, "ggbApplet.getMode()") == "40", "Modo de desplazamiento del plano");
        Verificar(EjecutarScript(navegador, "ggbApplet.exists('g')") == (modificada ? "true" : "false"), "Mostrar u ocultar la recta modificada");
        ResultadoAjuste resultado = Campo<ResultadoAjuste>(form, "resultadoOriginal");
        Verificar(EjecutarScript(navegador, "ggbApplet.exists('P" + resultado.Puntos.Count + "')") == "true", "Dibujar todos los puntos");
        string puntoX = resultado.Puntos[0].X.ToString("R", CultureInfo.InvariantCulture);
        Cerca(LeerNumero(navegador, "ggbApplet.getXcoord('P1')"), resultado.Puntos[0].X, 1e-8, "X del punto en GeoGebra");
        Cerca(LeerNumero(navegador, "ggbApplet.getYcoord('P1')"), resultado.Puntos[0].Y, 1e-8, "Y del punto en GeoGebra");
        Cerca(LeerNumero(navegador, "ggbApplet.getValue('f(" + puntoX + ")')"), resultado.Evaluar(resultado.Puntos[0].X), 1e-8, "Funcion calculada en GeoGebra");
        EsperarCondicion(() => LeerNumero(navegador, "document.querySelector('#ggb-element canvas').width") > 0, 5000, "Canvas visible");
        EsperarCondicion(() => EjecutarScript(navegador,
            "(() => { const area = document.getElementById('grafico').getBoundingClientRect(); "
            + "const canvas = document.querySelector('#ggb-element canvas').getBoundingClientRect(); "
            + "return Math.abs(area.width - canvas.width) <= 2 && Math.abs(area.height - canvas.height) <= 2; })()") == "true",
            5000, "El plano de GeoGebra ocupa todo el panel al redimensionar");
        using (MemoryStream captura = new MemoryStream())
        {
            Terminar(navegador.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, captura));
            captura.Position = 0;
            if (carpeta != null)
            {
                Directory.CreateDirectory(carpeta);
                File.WriteAllBytes(Path.Combine(carpeta, archivo), captura.ToArray());
            }
            using (Bitmap imagen = new Bitmap(captura))
            {
            int rosas = 0, verdes = 0;
            for (int x = 0; x < imagen.Width; x++)
            for (int y = 40; y < imagen.Height; y++)
            {
                Color pixel = imagen.GetPixel(x, y);
                if (pixel.R > 160 && pixel.R - pixel.G > 45 && pixel.B - pixel.G > 15 && pixel.B < 200) rosas++;
                if (pixel.G > 100 && pixel.G - pixel.R > 45 && pixel.B - pixel.R > 25) verdes++;
            }
            Verificar(rosas > 50, "Curva original dibujada");
            if (modificada) Verificar(verdes > 50, "Recta modificada dibujada junto a la original");
            Verificar(imagen.Width == grafico.Width && imagen.Height == grafico.Height, "GeoGebra se adapta al panel");
            }
        }
    }

    private static void ProbarNavegacion(Form form)
    {
        Control grafico = Campo<Control>(form, "grafico");
        WebView2 navegador = Campo<WebView2>(grafico, "navegador");
        Terminar(navegador.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setFocusEmulationEnabled", "{\"enabled\":true}"));
        double puntoInicial = LeerNumero(navegador, "ggbApplet.getXcoord('P1')");
        EjecutarScript(navegador, "document.addEventListener('wheel', e => { window.ultimoWheel = {target:e.target.className,x:e.clientX,y:e.clientY,delta:e.deltaY,shift:e.shiftKey}; }, true)");
        double escalaInicial = LeerNumero(navegador, "JSON.parse(ggbApplet.getViewProperties(1)).invXscale");
        EnviarMouse(navegador, new { type = "mouseMoved", x = grafico.Width / 2, y = grafico.Height / 2 });
        EnviarMouse(navegador, new { type = "mouseWheel", x = grafico.Width / 2, y = grafico.Height / 2, deltaX = 0, deltaY = -150, modifiers = 8 });
        try
        {
            EsperarCondicion(() => LeerNumero(navegador, "JSON.parse(ggbApplet.getViewProperties(1)).invXscale") < escalaInicial,
                5000, "Hacer zoom con Shift y la rueda del mouse");
        }
        catch
        {
            Console.WriteLine(EjecutarScript(navegador, "JSON.stringify({wheel:window.ultimoWheel,vista:ggbApplet.getViewProperties(1)})"));
            throw;
        }
        double posicionInicial = LeerNumero(navegador, "JSON.parse(ggbApplet.getViewProperties(1)).xMin");
        int x = grafico.Width / 2, y = grafico.Height / 2;
        EnviarMouse(navegador, new { type = "mousePressed", x, y, button = "left", buttons = 1, clickCount = 1 });
        for (int i = 1; i <= 5; i++) EnviarMouse(navegador, new { type = "mouseMoved", x = x + 12 * i, y = y + 4 * i, button = "left", buttons = 1 });
        EnviarMouse(navegador, new { type = "mouseReleased", x = x + 60, y = y + 20, button = "left", buttons = 0, clickCount = 1 });
        EsperarCondicion(() => Math.Abs(LeerNumero(navegador, "JSON.parse(ggbApplet.getViewProperties(1)).xMin") - posicionInicial) > 1e-6,
            5000, "Mover el plano arrastrando con el mouse");
        Cerca(LeerNumero(navegador, "ggbApplet.getXcoord('P1')"), puntoInicial, 0, "Desplazarse no modifica las coordenadas");
    }

    private static void ProbarExpresionesGeoGebra(Form form)
    {
        WebView2 navegador = Campo<WebView2>(Campo<Control>(form, "grafico"), "navegador");
        MetodoAjusteCurvas metodo = new MetodoAjusteCurvas();
        List<PuntoAjuste> puntos = new List<PuntoAjuste>();
        for (int i = 0; i <= 15; i++)
        {
            double x = -1 + i * 2.0 / 15;
            puntos.Add(new PuntoAjuste(x, 1 - 2 * x + Math.Pow(x, 10)));
        }
        ProbarExpresion(navegador, metodo.Calcular(puntos, 10, 80), 0.37, 1e-7);
        puntos.Clear();
        for (int i = 0; i < 7; i++) puntos.Add(new PuntoAjuste(10000 + i, 2 * i * i - 3 * i + 1));
        ProbarExpresion(navegador, metodo.Calcular(puntos, 2, 80), 10002.5, 1e-8);
        ProbarExpresion(navegador, metodo.Calcular(CrearPuntos(new[] { 0.0, 1, 2 }, new[] { 5.0, 5, 5 }), 2, 80), 1.5, 1e-8);
    }

    private static void ProbarExpresion(WebView2 navegador, ResultadoAjuste resultado, double x, double tolerancia)
    {
        string comando = new JavaScriptSerializer().Serialize("h(x)=" + resultado.Expresion);
        Verificar(EjecutarScript(navegador, "ggbApplet.evalCommand(" + comando + ")") == "true", "Aceptar expresion normalizada en GeoGebra");
        Cerca(LeerNumero(navegador, "ggbApplet.getValue('h(" + x.ToString("R", CultureInfo.InvariantCulture) + ")')"), resultado.Evaluar(x), tolerancia,
            "Precision de la funcion graficada");
        EjecutarScript(navegador, "ggbApplet.deleteObject('h')");
    }

    private static void EnviarMouse(WebView2 navegador, object evento)
    {
        Terminar(navegador.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.dispatchMouseEvent", new JavaScriptSerializer().Serialize(evento)));
    }

    private static double LeerNumero(WebView2 navegador, string expresion)
    {
        return double.Parse(EjecutarScript(navegador, expresion), CultureInfo.InvariantCulture);
    }

    private static string EjecutarScript(WebView2 navegador, string script)
    {
        Task<string> tarea = navegador.ExecuteScriptAsync(script);
        Terminar(tarea);
        return tarea.Result;
    }

    private static void Terminar(Task tarea)
    {
        EsperarCondicion(() => tarea.IsCompleted, 10000, "Completar operacion de WebView2", false);
        tarea.GetAwaiter().GetResult();
    }

    private static void EsperarCondicion(Func<bool> condicion, int tiempoMaximo, string nombre, bool contar = true)
    {
        Stopwatch reloj = Stopwatch.StartNew();
        while (!condicion())
        {
            if (reloj.ElapsedMilliseconds > tiempoMaximo) throw new Exception(nombre + ": tiempo de espera agotado.");
            Application.DoEvents();
            Thread.Sleep(20);
        }
        if (contar) verificaciones++;
    }

    private static void GuardarCaptura(Form form, string carpeta, string nombre)
    {
        if (carpeta == null) return;
        Directory.CreateDirectory(carpeta);
        using (Bitmap imagen = new Bitmap(form.Width, form.Height))
        {
            form.DrawToBitmap(imagen, new Rectangle(Point.Empty, imagen.Size));
            imagen.Save(Path.Combine(carpeta, nombre));
        }
    }

    private static T Campo<T>(object objeto, string nombre)
    {
        return (T)objeto.GetType().GetField(nombre, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(objeto);
    }

    private static void Invocar(object objeto, string nombre)
    {
        objeto.GetType().GetMethod(nombre, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(objeto, null);
    }

    private static List<PuntoAjuste> CrearPuntos(double[] xs, double[] ys)
    {
        List<PuntoAjuste> puntos = new List<PuntoAjuste>();
        for (int i = 0; i < xs.Length; i++) puntos.Add(new PuntoAjuste(xs[i], ys[i]));
        return puntos;
    }

    private static void Cerca(double obtenido, double esperado, double tolerancia, string nombre)
    {
        Verificar(!double.IsNaN(obtenido) && Math.Abs(obtenido - esperado) <= tolerancia,
            nombre + ": esperado " + esperado + ", obtenido " + obtenido);
    }

    private static void Rechaza(Action accion, string nombre)
    {
        try { accion(); }
        catch (ArgumentException) { verificaciones++; return; }
        throw new Exception(nombre);
    }

    private static void Verificar(bool condicion, string nombre)
    {
        if (!condicion) throw new Exception(nombre);
        verificaciones++;
    }
}
