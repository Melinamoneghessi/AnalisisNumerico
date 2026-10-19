using System;
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
            BuscarBoton(form, "Limpiar").PerformClick();
            Verificar(Campo<TextBox>(form, "txtFuncion").Text == "" && Campo<TextBox>(form, "txtXi").Text == ""
                && Campo<TextBox>(form, "txtXd").Text == "" && Campo<ResultadoIntegracion>(form, "resultado") == null, "Limpiar entradas y resultado");
            BuscarBoton(form, "Volver").PerformClick();
            Verificar(!form.Visible, "Volver cierra la unidad");
        }
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

    private static void VerificarGrafico(Form form, string carpeta, string archivo)
    {
        WebView2 navegador = Navegador(form);
        Control grafico = Campo<Control>(form, "grafico");
        Console.WriteLine("Verificando GeoGebra: " + archivo);
        Esperar(() => navegador.CoreWebView2 != null, "Inicializar WebView2");
        Esperar(() => Campo<string>(grafico, "datosEnviados") == Campo<string>(grafico, "datosPendientes")
            && Ejecutar(navegador, "window.geoGebraListo === true && ggbApplet.exists('curva') && ggbApplet.exists('aproximacion') && document.getElementById('estado').hidden") == "true",
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
