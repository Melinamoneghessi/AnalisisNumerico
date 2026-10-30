using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using LogicaAnalisis.Unidad4;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace AnalisisNumericoWeb.Unidad4
{
    internal class GraficoIntegracion : UserControl
    {
        private readonly WebView2 navegador;
        private readonly Panel panelEstado;
        private readonly Label estado;
        private readonly Button reintentar;
        private string datosPendientes = "null", datosEnviados;
        private bool inicializando, paginaLista, enviando;
        private int versionPagina;

        public GraficoIntegracion()
        {
            BackColor = Color.White;
            navegador = new WebView2 { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.White };
            navegador.NavigationStarting += (s, e) => { paginaLista = false; datosEnviados = null; versionPagina++; };
            navegador.NavigationCompleted += (s, e) =>
            {
                if (IsDisposed) return;
                if (!e.IsSuccess) { MostrarError(); return; }
                paginaLista = true;
                panelEstado.Visible = false;
                ActualizarGrafico();
            };
            navegador.WebMessageReceived += (s, e) =>
            {
                if (!IsDisposed && e.WebMessageAsJson == "\"reintentar\"") InicializarGeoGebra();
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

        public void Mostrar(ResultadoIntegracion resultado)
        {
            datosPendientes = CrearDatos(resultado);
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
                string carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AnalisisNumericoWeb", "GeoGebra");
                Directory.CreateDirectory(carpeta);
                using (Stream recurso = typeof(GraficoIntegracion).Assembly.GetManifestResourceStream("AnalisisNumericoWeb.Unidad4.GeoGebraIntegracion.html"))
                {
                    if (recurso == null) throw new InvalidOperationException("No se encontro el recurso de GeoGebra.");
                    using (StreamReader lector = new StreamReader(recurso))
                        File.WriteAllText(Path.Combine(carpeta, "integracion.html"), lector.ReadToEnd(), Encoding.UTF8);
                }
                navegador.CoreWebView2.SetVirtualHostNameToFolderMapping("geogebra-integracion.local", carpeta, CoreWebView2HostResourceAccessKind.DenyCors);
                navegador.CoreWebView2.Navigate("https://geogebra-integracion.local/integracion.html");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                if (!IsDisposed) MostrarError();
            }
            finally { inicializando = false; }
        }

        private async void ActualizarGrafico()
        {
            if (enviando || !paginaLista || IsDisposed || navegador.CoreWebView2 == null) return;
            enviando = true;
            try
            {
                while (!IsDisposed && paginaLista && datosEnviados != datosPendientes)
                {
                    string datos = datosPendientes;
                    int version = versionPagina;
                    await navegador.ExecuteScriptAsync("window.mostrarIntegral(" + datos + ");");
                    if (version == versionPagina) datosEnviados = datos;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                if (!IsDisposed && paginaLista) MostrarError();
            }
            finally { enviando = false; }
        }

        private void MostrarError()
        {
            estado.Text = "No se pudo cargar GeoGebra.\nVerifica la conexion a internet y WebView2 Runtime.";
            reintentar.Visible = true;
            panelEstado.Visible = true;
            panelEstado.BringToFront();
        }

        internal static string CrearDatos(ResultadoIntegracion resultado)
        {
            if (resultado == null) return "null";
            double minimoY = resultado.EsAreaEntreCurvas ? resultado.Curva[0][1] : 0;
            double maximoY = minimoY;
            foreach (double[] punto in resultado.Curva)
            {
                minimoY = Math.Min(minimoY, punto[1]); maximoY = Math.Max(maximoY, punto[1]);
            }
            foreach (double[] punto in resultado.EsAreaEntreCurvas ? resultado.SegundaCurva : resultado.Aproximacion)
            {
                minimoY = Math.Min(minimoY, punto[1]); maximoY = Math.Max(maximoY, punto[1]);
            }
            double margenX = (resultado.ExtremoDerecho - resultado.ExtremoIzquierdo) * 0.12;
            double margenY = Math.Max(0.1, (maximoY / 2 - minimoY / 2) * 0.3);
            return new JavaScriptSerializer().Serialize(new
            {
                esTabla = resultado.UsaTabla,
                entreCurvas = resultado.EsAreaEntreCurvas, segundaCurva = resultado.SegundaCurva,
                curva = resultado.Curva, aproximacion = resultado.Aproximacion,
                nodos = !resultado.EsAreaEntreCurvas && resultado.Nodos.Count <= 61 ? resultado.Nodos : null,
                limites = new[] { AmpliarLimite(resultado.ExtremoIzquierdo, -margenX), AmpliarLimite(resultado.ExtremoDerecho, margenX),
                    AmpliarLimite(minimoY, -margenY), AmpliarLimite(maximoY, margenY) }
            });
        }

        private static double AmpliarLimite(double valor, double margen)
        {
            return double.IsInfinity(valor + margen) ? valor : valor + margen;
        }
    }
}
