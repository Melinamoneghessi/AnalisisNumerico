using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogicaAnalisis;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace AnalisisNumericoWeb
{
    public partial class Unidad1 : Form
    {
        private TextBox txtResultadoFuncion;
        private TextBox txtResultadoMetodo;
        private TextBox txtResultadoIteraciones;
        private TextBox txtResultadoTolerancia;
        private TextBox txtResultadoIntervalo;
        private TextBox txtResultadoConverge;
        private TextBox txtResultadoRaiz;
        private TextBox txtResultadoError;
        private DataGridView dgvIteraciones;
        private WebView2 navegadorGeoGebra;
        private bool geoGebraCargado;
        private string funcionPendienteGeoGebra;
        private double raizPendienteGeoGebra;

        public Unidad1()
        {
            InitializeComponent();
            CrearInterfaz();
        }

        private void CrearInterfaz()
        {
            // =========================
            // CONFIGURACIÓN GENERAL
            // =========================

            this.Text = "Unidad 1 - Raíces de funciones";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(255, 245, 248);


            // =========================
            // ENCABEZADO
            // =========================

            Panel encabezado = new Panel();

            encabezado.Dock = DockStyle.Top;
            encabezado.Height = 125;
            encabezado.BackColor = Color.FromArgb(244, 180, 196);

            this.Controls.Add(encabezado);


            // BOTÓN VOLVER

            Button btnVolver = new Button();

            btnVolver.Text = "← Volver";
            btnVolver.Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Bold
            );

            btnVolver.Size = new Size(120, 42);
            btnVolver.Location = new Point(30, 40);

            btnVolver.FlatStyle = FlatStyle.Flat;
            btnVolver.FlatAppearance.BorderSize = 0;

            btnVolver.BackColor = Color.White;
            btnVolver.ForeColor = Color.FromArgb(200, 90, 130);

            btnVolver.Cursor = Cursors.Hand;

            encabezado.Controls.Add(btnVolver);


            btnVolver.Click += (s, e) =>
            {
                this.Close();
            };


            // TÍTULO

            Label titulo = new Label();

            titulo.Text = "ANÁLISIS NUMÉRICO";

            titulo.Font = new Font(
                "Segoe UI",
                24,
                FontStyle.Bold
            );

            titulo.ForeColor = Color.FromArgb(90, 50, 65);
            titulo.AutoSize = true;

            encabezado.Controls.Add(titulo);


            // SUBTÍTULO

            Label subtitulo = new Label();

            subtitulo.Text = "Unidad 1 · Raíces de funciones";

            subtitulo.Font = new Font(
                "Segoe UI",
                13
            );

            subtitulo.ForeColor =
                Color.FromArgb(110, 70, 85);

            subtitulo.AutoSize = true;

            encabezado.Controls.Add(subtitulo);


            // CENTRAR TÍTULO

            void CentrarTitulo()
            {
                titulo.Left =
                    (encabezado.Width - titulo.Width) / 2;

                titulo.Top = 22;


                subtitulo.Left =
                    (encabezado.Width - subtitulo.Width) / 2;

                subtitulo.Top = 72;
            }

            CentrarTitulo();

            encabezado.Resize += (s, e) =>
            {
                CentrarTitulo();
            };


            // =========================
            // CONTENEDOR PRINCIPAL
            // =========================

            Panel contenido = new Panel();

            contenido.Size = new Size(1400, 570);
            contenido.BackColor = Color.Transparent;

            this.Controls.Add(contenido);


            // =========================
            // PANEL INGRESO DE DATOS
            // =========================

            Panel panelDatos = CrearPanelSeccion(
                "Ingreso de datos",
                new Point(0, 0),
                new Size(370, 570)
            );

            contenido.Controls.Add(panelDatos);


            // FUNCIÓN

            Label lblFuncion = CrearLabel(
                "Función",
                new Point(30, 80)
            );

            panelDatos.Controls.Add(lblFuncion);


            TextBox txtFuncion = CrearTextBox(
                new Point(30, 110),
                new Size(310, 40)
            );

            txtFuncion.Text = "x^2 - 2";

            panelDatos.Controls.Add(txtFuncion);


            // MÉTODO

            Label lblMetodo = CrearLabel(
                "Método",
                new Point(30, 165)
            );

            panelDatos.Controls.Add(lblMetodo);


            ComboBox cmbMetodo = new ComboBox();

            cmbMetodo.Location = new Point(30, 195);
            cmbMetodo.Size = new Size(310, 40);

            cmbMetodo.Font =
                new Font("Segoe UI", 11);

            cmbMetodo.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbMetodo.Items.Add("Bisección");
            cmbMetodo.Items.Add("Regla Falsa");
            cmbMetodo.Items.Add("Newton-Raphson");
            cmbMetodo.Items.Add("Secante");

            cmbMetodo.SelectedIndex = 0;

            panelDatos.Controls.Add(cmbMetodo);


            // ITERACIONES

            Label lblIteraciones = CrearLabel(
                "Iteraciones máximas",
                new Point(30, 250)
            );

            panelDatos.Controls.Add(lblIteraciones);


            TextBox txtIteraciones = CrearTextBox(
                new Point(30, 280),
                new Size(310, 40)
            );

            txtIteraciones.Text = "100";

            panelDatos.Controls.Add(txtIteraciones);


            // TOLERANCIA

            Label lblTolerancia = CrearLabel(
                "Tolerancia",
                new Point(30, 335)
            );

            panelDatos.Controls.Add(lblTolerancia);


            TextBox txtTolerancia = CrearTextBox(
                new Point(30, 365),
                new Size(310, 40)
            );

            txtTolerancia.Text = "0.0001";

            panelDatos.Controls.Add(txtTolerancia);


            // INTERVALO

            Label lblIntervalo = CrearLabel(
                "Intervalo",
                new Point(30, 420)
            );

            panelDatos.Controls.Add(lblIntervalo);


            Label lblXi = new Label();

            lblXi.Text = "Xi";
            lblXi.Font = new Font("Segoe UI", 10);
            lblXi.ForeColor = Color.Gray;

            lblXi.Location = new Point(30, 450);
            lblXi.AutoSize = true;

            panelDatos.Controls.Add(lblXi);


            Label lblXd = new Label();

            lblXd.Text = "Xd";
            lblXd.Font = new Font("Segoe UI", 10);
            lblXd.ForeColor = Color.Gray;

            lblXd.Location = new Point(190, 450);
            lblXd.AutoSize = true;

            panelDatos.Controls.Add(lblXd);


            TextBox txtXi = CrearTextBox(
                new Point(30, 475),
                new Size(145, 38)
            );

            txtXi.Text = "1";

            panelDatos.Controls.Add(txtXi);


            TextBox txtXd = CrearTextBox(
                new Point(195, 475),
                new Size(145, 38)
            );

            txtXd.Text = "2";

            panelDatos.Controls.Add(txtXd);


            // BOTÓN CALCULAR

            Button btnCalcular = new Button();

            btnCalcular.Text = "CALCULAR";

            btnCalcular.Font = new Font(
                "Segoe UI",
                12,
                FontStyle.Bold
            );

            btnCalcular.Size = new Size(310, 45);
            btnCalcular.Location = new Point(30, 520);

            btnCalcular.BackColor =
                Color.FromArgb(225, 120, 160);

            btnCalcular.ForeColor = Color.White;

            btnCalcular.FlatStyle =
                FlatStyle.Flat;

            btnCalcular.FlatAppearance.BorderSize = 0;

            btnCalcular.Cursor =
                Cursors.Hand;

            panelDatos.Controls.Add(btnCalcular);


            // =========================
            // PANEL GRÁFICO
            // =========================

            Panel panelGrafico = CrearPanelSeccion(
                "Gráfico",
                new Point(395, 0),
                new Size(580, 570)
            );

            contenido.Controls.Add(panelGrafico);


            // ZONA DONDE DESPUÉS GRAFICAMOS

            Panel areaGrafico = new Panel();

            areaGrafico.Location =
                new Point(25, 80);

            areaGrafico.Size =
                new Size(530, 300);

            areaGrafico.BackColor =
                Color.White;

            areaGrafico.BorderStyle =
                BorderStyle.FixedSingle;

            panelGrafico.Controls.Add(areaGrafico);


            navegadorGeoGebra = new WebView2();
            navegadorGeoGebra.Dock = DockStyle.Fill;
            navegadorGeoGebra.NavigationCompleted += async (s, e) =>
            {
                geoGebraCargado = true;

                if (!string.IsNullOrWhiteSpace(funcionPendienteGeoGebra))
                {
                    await GraficarEnGeoGebra(
                        funcionPendienteGeoGebra,
                        raizPendienteGeoGebra
                    );
                }
            };

            areaGrafico.Controls.Add(navegadorGeoGebra);
            InicializarGeoGebra();


            dgvIteraciones = new DataGridView();

            dgvIteraciones.Location =
                new Point(25, 395);

            dgvIteraciones.Size =
                new Size(530, 145);

            dgvIteraciones.ReadOnly = true;
            dgvIteraciones.AllowUserToAddRows = false;
            dgvIteraciones.AllowUserToDeleteRows = false;
            dgvIteraciones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvIteraciones.BackgroundColor = Color.White;
            dgvIteraciones.BorderStyle = BorderStyle.FixedSingle;
            dgvIteraciones.RowHeadersVisible = false;

            panelGrafico.Controls.Add(dgvIteraciones);


            // =========================
            // PANEL RESULTADOS
            // =========================

            Panel panelResultados = CrearPanelSeccion(
                "Resultados",
                new Point(1000, 0),
                new Size(400, 570)
            );

            contenido.Controls.Add(panelResultados);


            int y = 85;


            // FUNCIÓN UTILIZADA

            txtResultadoFuncion = CrearResultado(
                panelResultados,
                "Función utilizada",
                "-",
                ref y
            );


            // MÉTODO

            txtResultadoMetodo = CrearResultado(
                panelResultados,
                "Método utilizado",
                "-",
                ref y
            );


            // ITERACIONES

            txtResultadoIteraciones = CrearResultado(
                panelResultados,
                "Iteraciones",
                "-",
                ref y
            );


            // TOLERANCIA

            txtResultadoTolerancia = CrearResultado(
                panelResultados,
                "Tolerancia",
                "-",
                ref y
            );


            // INTERVALO

            txtResultadoIntervalo = CrearResultado(
                panelResultados,
                "Intervalo",
                "-",
                ref y
            );


            // CONVERGENCIA

            txtResultadoConverge = CrearResultado(
                panelResultados,
                "¿Converge?",
                "-",
                ref y
            );


            // RAÍZ

            txtResultadoRaiz = CrearResultado(
                panelResultados,
                "Raíz",
                "-",
                ref y
            );


            // ERROR

            txtResultadoError = CrearResultado(
                panelResultados,
                "Error",
                "-",
                ref y
            );


            // =========================
            // CENTRAR CONTENIDO
            // =========================

            void CentrarContenido()
            {
                contenido.Left =
                    (this.ClientSize.Width -
                    contenido.Width) / 2;

                contenido.Top =
                    encabezado.Height +
                    ((this.ClientSize.Height
                    - encabezado.Height
                    - contenido.Height) / 2);
            }


            CentrarContenido();

            this.Resize += (s, e) =>
            {
                CentrarContenido();
            };


            btnCalcular.Click += async (s, e) =>
            {
                try
                {
                    if (cmbMetodo.Text != "Bisección")
                    {
                        MessageBox.Show(
                            "Por ahora está conectado el método de bisección.",
                            "Método no disponible",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                        return;
                    }

                    MetodoBiseccion metodo = new MetodoBiseccion();

                    ResultadoBiseccion resultado = metodo.Calcular(
                        txtFuncion.Text,
                        LeerDouble(txtXi.Text),
                        LeerDouble(txtXd.Text),
                        LeerDouble(txtTolerancia.Text),
                        LeerEntero(txtIteraciones.Text)
                    );

                    MostrarResultado(resultado);
                    await GraficarEnGeoGebra(resultado.Funcion, resultado.Raiz);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            };
        }



        // ==========================================
        // CREA LOS PANELES GRANDES
        // ==========================================

        private Panel CrearPanelSeccion(
            string titulo,
            Point posicion,
            Size tamaño)
        {
            Panel panel = new Panel();

            panel.Location = posicion;
            panel.Size = tamaño;
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;


            Label lblTitulo = new Label();

            lblTitulo.Text = titulo;

            lblTitulo.Font = new Font(
                "Segoe UI",
                19,
                FontStyle.Bold
            );

            lblTitulo.ForeColor =
                Color.FromArgb(90, 50, 65);

            lblTitulo.AutoSize = true;

            panel.Controls.Add(lblTitulo);


            panel.Resize += (s, e) =>
            {
                lblTitulo.Left =
                    (panel.Width - lblTitulo.Width) / 2;

                lblTitulo.Top = 25;
            };


            lblTitulo.Left =
                (panel.Width - lblTitulo.Width) / 2;

            lblTitulo.Top = 25;


            return panel;
        }



        // ==========================================
        // CREA LABELS
        // ==========================================

        private Label CrearLabel(
            string texto,
            Point posicion)
        {
            Label label = new Label();

            label.Text = texto;

            label.Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Bold
            );

            label.ForeColor =
                Color.FromArgb(80, 65, 70);

            label.Location = posicion;
            label.AutoSize = true;

            return label;
        }



        // ==========================================
        // CREA TEXTBOX
        // ==========================================

        private TextBox CrearTextBox(
            Point posicion,
            Size tamaño)
        {
            TextBox textbox =
                new TextBox();

            textbox.Location = posicion;
            textbox.Size = tamaño;

            textbox.Font =
                new Font("Segoe UI", 11);

            textbox.BorderStyle =
                BorderStyle.FixedSingle;

            return textbox;
        }



        // ==========================================
        // CREA FILAS DE RESULTADOS
        // ==========================================

        private TextBox CrearResultado(
            Panel panel,
            string nombre,
            string valor,
            ref int y)
        {
            Label lblNombre = new Label();

            lblNombre.Text = nombre;

            lblNombre.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold
                );

            lblNombre.ForeColor =
                Color.FromArgb(90, 75, 80);

            lblNombre.Location =
                new Point(25, y);

            lblNombre.Size =
                new Size(155, 30);

            panel.Controls.Add(lblNombre);


            TextBox txtValor = new TextBox();

            txtValor.Text = valor;

            txtValor.Font =
                new Font("Segoe UI", 10);

            txtValor.Location =
                new Point(185, y - 3);

            txtValor.Size =
                new Size(185, 32);

            txtValor.ReadOnly = true;

            txtValor.BackColor =
                Color.FromArgb(255, 248, 250);

            panel.Controls.Add(txtValor);


            y += 55;

            return txtValor;
        }



        private double LeerDouble(string texto)
        {
            texto = texto.Trim().Replace(',', '.');
            return double.Parse(texto, CultureInfo.InvariantCulture);
        }



        private int LeerEntero(string texto)
        {
            return int.Parse(texto.Trim(), CultureInfo.InvariantCulture);
        }



        private void MostrarResultado(ResultadoBiseccion resultado)
        {
            txtResultadoFuncion.Text = resultado.Funcion;
            txtResultadoMetodo.Text = resultado.Metodo;
            txtResultadoIteraciones.Text = resultado.IteracionesRealizadas.ToString();
            txtResultadoTolerancia.Text = FormatearNumero(resultado.Tolerancia);
            txtResultadoIntervalo.Text =
                "[" +
                FormatearNumero(resultado.XiInicial) +
                " ; " +
                FormatearNumero(resultado.XdInicial) +
                "]";
            txtResultadoConverge.Text = resultado.Converge ? "Si" : "No";
            txtResultadoRaiz.Text = FormatearNumero(resultado.Raiz);
            txtResultadoError.Text = FormatearNumero(resultado.Error);

            dgvIteraciones.DataSource = null;
            dgvIteraciones.DataSource = resultado.Iteraciones;
        }



        private string FormatearNumero(double numero)
        {
            return numero.ToString("0.##########", CultureInfo.InvariantCulture);
        }



        private async void InicializarGeoGebra()
        {
            try
            {
                string carpetaWebView = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AnalisisNumericoWeb",
                    "WebView2"
                );

                CoreWebView2Environment entornoWebView =
                    await CoreWebView2Environment.CreateAsync(
                        null,
                        carpetaWebView
                    );

                await navegadorGeoGebra.EnsureCoreWebView2Async(entornoWebView);
                geoGebraCargado = false;

                string rutaHtml = Path.Combine(
                    Application.StartupPath,
                    "geogebra.html"
                );

                File.WriteAllText(
                    rutaHtml,
                    CrearHtmlGeoGebra(),
                    Encoding.UTF8
                );

                navegadorGeoGebra.Source = new Uri(rutaHtml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar GeoGebra. Verifica que WebView2 Runtime este instalado.\n\n" + ex.Message,
                    "GeoGebra",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }



        private async Task GraficarEnGeoGebra(string funcion, double raiz)
        {
            funcionPendienteGeoGebra = funcion;
            raizPendienteGeoGebra = raiz;

            if (navegadorGeoGebra.CoreWebView2 == null)
            {
                return;
            }

            if (!geoGebraCargado)
            {
                return;
            }

            string funcionJs = EscaparJavaScript(funcion);
            string raizJs = FormatearNumero(raiz);
            string script = "graficar('" + funcionJs + "', '" + raizJs + "');";

            await navegadorGeoGebra.ExecuteScriptAsync(script);
        }



        private string EscaparJavaScript(string texto)
        {
            return texto
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "")
                .Replace("\n", "");
        }



        private string CrearHtmlGeoGebra()
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<meta http-equiv='X-UA-Compatible' content='IE=edge'>");
            html.AppendLine("<script src='https://cdn.geogebra.org/apps/deployggb.js'></script>");
            html.AppendLine("</head>");
            html.AppendLine("<body style='margin:0; overflow:hidden; background:white; font-family:Segoe UI, Arial;'>");
            html.AppendLine("<div style='position:relative; width:530px; height:300px;'>");
            html.AppendLine("<div id='ggb-element' style='width:530px; height:300px;'></div>");
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
            html.AppendLine("ggbApplet.evalCommand('SetColor(f, 220, 100, 145)');");
            html.AppendLine("ggbApplet.evalCommand('SetColor(R, 80, 65, 70)');");
            html.AppendLine("ggbApplet.evalCommand('ShowLabel(R, true)');");
            html.AppendLine("ggbApplet.evalCommand('SetPointSize(R, 5)');");
            html.AppendLine("ggbApplet.setCoordSystem(-3.5, 3.5, -3.5, 3.5);");
            html.AppendLine("}");
            html.AppendLine("</script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}
