using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using LogicaAnalisis.Unidad3;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace AnalisisNumericoWeb.Unidad3
{
    internal class GraficoAjuste : UserControl
    {
        private const string RecursoHtml = "AnalisisNumericoWeb.Unidad3.GeoGebraAjuste.html";
        private readonly WebView2 navegador;
        private readonly Panel panelEstado;
        private readonly Label estado;
        private readonly Button reintentar;
        private string datosPendientes = "null";
        private string datosEnviados;
        private bool inicializando;
        private bool paginaLista;
        private bool enviando;
        private int versionPagina;

        public GraficoAjuste()
        {
            BackColor = Color.White;
            navegador = new WebView2 { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.White };
            navegador.NavigationStarting += (s, e) =>
            {
                paginaLista = false;
                datosEnviados = null;
                versionPagina++;
            };
            navegador.NavigationCompleted += (s, e) =>
            {
                if (IsDisposed) return;
                if (!e.IsSuccess)
                {
                    MostrarError();
                    return;
                }
                paginaLista = true;
                panelEstado.Visible = false;
                ActualizarGrafico();
            };
            navegador.WebMessageReceived += (s, e) =>
            {
                if (!IsDisposed && e.WebMessageAsJson == "\"reintentar\"")
                {
                    InicializarGeoGebra();
                }
            };
            Controls.Add(navegador);

            panelEstado = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            estado = new Label
            {
                Dock = DockStyle.Fill, Text = "Cargando GeoGebra...", TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(110, 70, 85)
            };
            reintentar = new Button { Dock = DockStyle.Bottom, Height = 34, Text = "Reintentar", Visible = false };
            reintentar.Click += (s, e) => InicializarGeoGebra();
            panelEstado.Controls.Add(estado);
            panelEstado.Controls.Add(reintentar);
            Controls.Add(panelEstado);
            panelEstado.BringToFront();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime) InicializarGeoGebra();
        }

        public void Mostrar(ResultadoAjuste original, ResultadoAjuste modificada)
        {
            datosPendientes = CrearDatos(original, modificada);
            ActualizarGrafico();
        }

        private async void InicializarGeoGebra()
        {
            if (inicializando || IsDisposed) return;
            inicializando = true;
            estado.Text = "Cargando GeoGebra...";
            reintentar.Visible = false;
            panelEstado.Visible = true;
            try
            {
                var entorno = await GeoGebraWebView.ObtenerEntornoAsync();
                if (IsDisposed) return;
                await navegador.EnsureCoreWebView2Async(entorno);
                if (IsDisposed) return;
                navegador.CoreWebView2.Settings.IsZoomControlEnabled = false;
                navegador.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                NavegarPagina();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                if (!IsDisposed) MostrarError();
            }
            finally
            {
                inicializando = false;
            }
        }

        private async void ActualizarGrafico()
        {
            if (enviando || !paginaLista || IsDisposed || navegador.CoreWebView2 == null) return;
            enviando = true;
            try
            {
                // Conservar siempre el ultimo ajuste, incluso si GeoGebra sigue cargando.
                while (!IsDisposed && paginaLista && datosEnviados != datosPendientes)
                {
                    string datos = datosPendientes;
                    int version = versionPagina;
                    await navegador.ExecuteScriptAsync("window.mostrarAjuste(" + datos + ");");
                    if (version == versionPagina) datosEnviados = datos;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                if (!IsDisposed && paginaLista) MostrarError();
            }
            finally
            {
                enviando = false;
            }
        }

        private void MostrarError()
        {
            estado.Text = "No se pudo cargar GeoGebra.\nVerifica la conexion a internet y WebView2 Runtime.";
            reintentar.Visible = true;
            panelEstado.Visible = true;
            panelEstado.BringToFront();
        }

        private string LeerHtml()
        {
            using (Stream recurso = typeof(GraficoAjuste).Assembly.GetManifestResourceStream(RecursoHtml))
            {
                if (recurso == null) throw new InvalidOperationException("No se encontro el recurso de GeoGebra.");
                using (StreamReader lector = new StreamReader(recurso)) return lector.ReadToEnd();
            }
        }

        private void NavegarPagina()
        {
            string carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AnalisisNumericoWeb", "GeoGebra");
            Directory.CreateDirectory(carpeta);
            File.WriteAllText(Path.Combine(carpeta, "ajuste.html"), LeerHtml(), Encoding.UTF8);
            // GeoGebra necesita un origen HTTP valido para acceder a document.cookie.
            navegador.CoreWebView2.SetVirtualHostNameToFolderMapping("geogebra-ajuste.local", carpeta, CoreWebView2HostResourceAccessKind.DenyCors);
            navegador.Source = new Uri("https://geogebra-ajuste.local/ajuste.html");
        }

        internal static string CrearDatos(ResultadoAjuste original, ResultadoAjuste modificada)
        {
            if (original == null) return "null";
            double[][] puntos = new double[original.Puntos.Count][];
            double minimoX = original.Puntos[0].X, maximoX = minimoX;
            double minimoY = original.Puntos[0].Y, maximoY = minimoY;
            for (int i = 0; i < puntos.Length; i++)
            {
                PuntoAjuste punto = original.Puntos[i];
                puntos[i] = new[] { punto.X, punto.Y };
                minimoX = Math.Min(minimoX, punto.X);
                maximoX = Math.Max(maximoX, punto.X);
                minimoY = Math.Min(minimoY, punto.Y);
                maximoY = Math.Max(maximoY, punto.Y);
            }
            AmpliarRango(ref minimoX, ref maximoX);
            for (int i = 0; i <= 200; i++)
            {
                double fraccion = i / 200.0;
                double x = minimoX * (1 - fraccion) + maximoX * fraccion;
                IncluirValor(original.Evaluar(x), ref minimoY, ref maximoY);
                if (modificada != null) IncluirValor(modificada.Evaluar(x), ref minimoY, ref maximoY);
            }
            AmpliarRango(ref minimoY, ref maximoY);
            return new JavaScriptSerializer().Serialize(new
            {
                puntos,
                original = original.Expresion,
                modificada = modificada == null ? null : modificada.Expresion,
                limites = new[] { minimoX, maximoX, minimoY, maximoY }
            });
        }

        private static void AmpliarRango(ref double minimo, ref double maximo)
        {
            double margen = minimo == maximo ? Math.Max(1, Math.Abs(minimo) * 0.15) : (maximo / 2 - minimo / 2) * 0.3;
            if (!double.IsInfinity(minimo - margen)) minimo -= margen;
            if (!double.IsInfinity(maximo + margen)) maximo += margen;
        }

        private static void IncluirValor(double valor, ref double minimo, ref double maximo)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor)) return;
            minimo = Math.Min(minimo, valor);
            maximo = Math.Max(maximo, valor);
        }
    }
}
