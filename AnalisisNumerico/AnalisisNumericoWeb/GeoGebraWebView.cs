using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace AnalisisNumericoWeb
{
    // Recursos de GeoGebra compartidos entre Form1 (precarga en segundo plano
    // mientras el usuario esta en el menu) y Unidad1 (uso real del grafico).
    // Compartir el mismo CoreWebView2Environment evita bloquear la carpeta de
    // perfil y permite que Unidad1 reaproveche la cache que dejo la precarga.
    internal static class GeoGebraWebView
    {
        private static Task<CoreWebView2Environment> entornoCompartido;

        public static Task<CoreWebView2Environment> ObtenerEntornoAsync()
        {
            if (entornoCompartido == null)
            {
                entornoCompartido = CoreWebView2Environment.CreateAsync(
                    null,
                    ObtenerCarpetaPerfil()
                );
            }

            return entornoCompartido;
        }

        private static string ObtenerCarpetaPerfil()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AnalisisNumericoWeb",
                "WebView2"
            );
        }

        public static string AsegurarHtmlEscrito()
        {
            string rutaHtml = Path.Combine(
                Application.StartupPath,
                "geogebra.html"
            );

            File.WriteAllText(
                rutaHtml,
                CrearHtml(),
                Encoding.UTF8
            );

            return rutaHtml;
        }

        private static string CrearHtml()
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<meta http-equiv='X-UA-Compatible' content='IE=edge'>");
            html.AppendLine("<link rel='preconnect' href='https://cdn.geogebra.org' crossorigin>");
            html.AppendLine("<link rel='dns-prefetch' href='https://cdn.geogebra.org'>");
            html.AppendLine("<script src='https://cdn.geogebra.org/apps/deployggb.js'></script>");
            html.AppendLine("</head>");
            html.AppendLine("<body style='margin:0; overflow:hidden; background:white; font-family:Segoe UI, Arial;'>");
            html.AppendLine("<div style='position:relative; width:530px; height:300px; overflow:hidden;'>");
            html.AppendLine("<div id='ggb-element' style='position:absolute; left:0; top:0; width:530px; height:300px;'></div>");
            html.AppendLine("<div id='estado' style='position:absolute; left:0; top:0; width:530px; height:300px; display:flex; align-items:center; justify-content:center; color:#aa8291; font-size:18px; background:white;'>Cargando GeoGebra...</div>");
            html.AppendLine("</div>");
            html.AppendLine("<script>");
            html.AppendLine("window.geoGebraListo = false;");
            html.AppendLine("window.funcionPendiente = null;");
            html.AppendLine("window.raizPendiente = null;");
            html.AppendLine("window.addEventListener('error', function() {");
            html.AppendLine("document.getElementById('estado').innerHTML = 'No se pudo cargar GeoGebra.<br>Revisa la conexion a internet.';");
            html.AppendLine("});");
            html.AppendLine("function marcarListo(api) {");
            html.AppendLine("if (api) { window.ggbApplet = api; }");
            html.AppendLine("window.geoGebraListo = true;");
            html.AppendLine("document.getElementById('estado').style.display = 'none';");
            html.AppendLine("if (window.funcionPendiente !== null) { graficar(window.funcionPendiente, window.raizPendiente); }");
            html.AppendLine("}");
            html.AppendLine("function ggbOnInit() { marcarListo(window.ggbApplet); }");
            html.AppendLine("var params = {");
            html.AppendLine("id: 'ggbApplet',");
            html.AppendLine("appName: 'graphing',");
            html.AppendLine("width: 530,");
            html.AppendLine("height: 300,");
            html.AppendLine("showToolBar: false,");
            html.AppendLine("showAlgebraInput: false,");
            html.AppendLine("showMenuBar: false,");
            html.AppendLine("perspective: 'G',");
            html.AppendLine("appletOnLoad: function(api) { marcarListo(api); }");
            html.AppendLine("};");
            html.AppendLine("var applet = new GGBApplet(params, true);");
            html.AppendLine("window.addEventListener('load', function() { applet.inject('ggb-element'); });");
            html.AppendLine("function graficar(funcion, raiz) {");
            html.AppendLine("window.funcionPendiente = funcion;");
            html.AppendLine("window.raizPendiente = raiz;");
            html.AppendLine("if (!window.geoGebraListo || typeof ggbApplet === 'undefined') { setTimeout(function() { graficar(funcion, raiz); }, 500); return; }");
            html.AppendLine("ggbApplet.reset();");
            html.AppendLine("ggbApplet.setCoordSystem(-3.5, 3.5, -3.5, 3.5);");
            html.AppendLine("ggbApplet.setAxesVisible(true, true);");
            html.AppendLine("ggbApplet.setGridVisible(true);");
            html.AppendLine("ggbApplet.evalCommand('f(x)=' + funcion);");
            html.AppendLine("ggbApplet.evalCommand('R=(' + raiz + ',0)');");
            html.AppendLine("ggbApplet.setVisible('f', true);");
            html.AppendLine("ggbApplet.setVisible('R', true);");
            html.AppendLine("ggbApplet.setColor('f', 225, 120, 160);");
            html.AppendLine("ggbApplet.setLineThickness('f', 5);");
            html.AppendLine("ggbApplet.setColor('R', 80, 65, 70);");
            html.AppendLine("ggbApplet.evalCommand('ShowLabel(R, true)');");
            html.AppendLine("ggbApplet.setPointSize('R', 6);");
            html.AppendLine("ggbApplet.setCoordSystem(-3.5, 3.5, -3.5, 3.5);");
            html.AppendLine("}");
            html.AppendLine("</script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}
