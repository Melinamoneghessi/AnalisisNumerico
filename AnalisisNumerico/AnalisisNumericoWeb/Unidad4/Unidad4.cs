using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LogicaAnalisis.Unidad4;

namespace AnalisisNumericoWeb.Unidad4
{
    public class Unidad4 : Form
    {
        private readonly IntegradorNumerico integrador = new IntegradorNumerico();
        private readonly CultureInfo cultura = CultureInfo.GetCultureInfo("es-AR");
        private ComboBox cmbMetodo;
        private TextBox txtFuncion, txtXi, txtXd;
        private NumericUpDown nudSubintervalos;
        private Label lblIntegral, lblEstado, lblMetodo, lblPaso, lblSubintervalos, lblError;
        private GraficoIntegracion grafico;
        private ResultadoIntegracion resultado;
        private bool actualizandoMetodo;
        private decimal ultimoN = 10;

        public Unidad4()
        {
            Text = "Unidad 4 - Integracion numerica";
            Font = new Font("Segoe UI", 10);
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 720);
            BackColor = Color.FromArgb(255, 245, 248);
            CrearInterfaz();
            cmbMetodo.SelectedIndex = 1;
        }

        private void CrearInterfaz()
        {
            Panel encabezado = new Panel { Dock = DockStyle.Top, Height = 105, BackColor = Color.FromArgb(244, 180, 196) };
            Button volver = CrearBoton("Volver", false);
            volver.SetBounds(24, 35, 105, 35);
            volver.Click += (s, e) => Close();
            encabezado.Controls.Add(volver);
            Label titulo = CrearLabel("Analisis Numerico", 24, true);
            Label subtitulo = CrearLabel("Unidad 4 - Integracion numerica", 12);
            titulo.Dock = subtitulo.Dock = DockStyle.None;
            titulo.AutoSize = subtitulo.AutoSize = true;
            encabezado.Controls.Add(titulo);
            encabezado.Controls.Add(subtitulo);
            encabezado.Resize += (s, e) =>
            {
                titulo.Location = new Point(Math.Max(155, (encabezado.Width - titulo.Width) / 2), 14);
                subtitulo.Location = new Point(Math.Max(155, (encabezado.Width - subtitulo.Width) / 2), 62);
            };

            Panel pagina = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            TableLayoutPanel contenido = new TableLayoutPanel { ColumnCount = 3, RowCount = 1 };
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 286));
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            contenido.Controls.Add(CrearEntrada(), 0, 0);
            TableLayoutPanel zonaGrafico = CrearSeccion();
            zonaGrafico.Margin = new Padding(8, 0, 8, 0);
            zonaGrafico.RowCount = 2;
            zonaGrafico.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            zonaGrafico.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            zonaGrafico.Controls.Add(CrearLabel("Grafico", 17, true), 0, 0);
            grafico = new GraficoIntegracion { Dock = DockStyle.Fill, Margin = new Padding(0) };
            zonaGrafico.Controls.Add(grafico, 0, 1);
            contenido.Controls.Add(zonaGrafico, 1, 0);
            contenido.Controls.Add(CrearResultados(), 2, 0);
            pagina.Controls.Add(contenido);
            pagina.Resize += (s, e) =>
            {
                contenido.Location = new Point(24, 20);
                contenido.Size = new Size(Math.Max(1030, pagina.ClientSize.Width - 48), Math.Max(590, pagina.ClientSize.Height - 40));
            };
            Controls.Add(pagina);
            Controls.Add(encabezado);
        }

        private TableLayoutPanel CrearEntrada()
        {
            TableLayoutPanel tabla = CrearSeccion();
            tabla.RowCount = 15;
            int[] alturas = { 42, 28, 38, 28, 38, 28, 38, 28, 38, 28, 38, 74, -1, 44, 40 };
            foreach (int altura in alturas) tabla.RowStyles.Add(altura < 0 ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.Absolute, altura));
            tabla.Controls.Add(CrearLabel("Ingreso de datos", 16, true), 0, 0);
            tabla.Controls.Add(CrearLabel("Metodo"), 0, 1);
            cmbMetodo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, DropDownWidth = 340 };
            cmbMetodo.Items.AddRange(new object[] { "Trapecios simple", "Trapecios multiple", "Simpson 1/3 simple", "Simpson 1/3 multiple", "Simpson 3/8" });
            cmbMetodo.SelectedIndexChanged += (s, e) => ActualizarMetodo();
            tabla.Controls.Add(cmbMetodo, 0, 2);
            tabla.Controls.Add(CrearLabel("Funcion f(x)"), 0, 3);
            txtFuncion = CrearEntradaTexto("1/x + x");
            tabla.Controls.Add(txtFuncion, 0, 4);
            tabla.Controls.Add(CrearLabel("Extremo izquierdo (xi)"), 0, 5);
            txtXi = CrearEntradaTexto("0,5");
            tabla.Controls.Add(txtXi, 0, 6);
            tabla.Controls.Add(CrearLabel("Extremo derecho (xd)"), 0, 7);
            txtXd = CrearEntradaTexto("3,5");
            tabla.Controls.Add(txtXd, 0, 8);
            tabla.Controls.Add(CrearLabel("Subintervalos (N)"), 0, 9);
            nudSubintervalos = new NumericUpDown
            {
                Dock = DockStyle.Fill, Minimum = 1, Maximum = IntegradorNumerico.MaximoSubintervalos,
                Value = 10, ThousandsSeparator = false
            };
            nudSubintervalos.ValueChanged += (s, e) =>
            {
                if (actualizandoMetodo) return;
                if (nudSubintervalos.Enabled) ultimoN = nudSubintervalos.Value;
                LimpiarResultados();
            };
            tabla.Controls.Add(nudSubintervalos, 0, 10);
            lblError = CrearLabel("");
            lblError.ForeColor = Color.Firebrick;
            lblError.Font = new Font("Segoe UI", 9);
            tabla.Controls.Add(lblError, 0, 11);
            Button calcular = CrearBoton("Calcular integral", true);
            calcular.Dock = DockStyle.Fill;
            calcular.Click += (s, e) => Calcular();
            tabla.Controls.Add(calcular, 0, 13);
            AcceptButton = calcular;
            Button limpiar = CrearBoton("Limpiar", false);
            limpiar.Dock = DockStyle.Fill;
            limpiar.Click += (s, e) =>
            {
                txtFuncion.Clear(); txtXi.Clear(); txtXd.Clear();
                LimpiarResultados(); txtFuncion.Focus();
            };
            tabla.Controls.Add(limpiar, 0, 14);
            return tabla;
        }

        private TableLayoutPanel CrearResultados()
        {
            TableLayoutPanel tabla = CrearSeccion();
            int[] alturas = { 46, 32, 68, 36, 38, 32, 100, 32, 38, 32, 38, -1 };
            tabla.RowCount = alturas.Length;
            foreach (int altura in alturas) tabla.RowStyles.Add(altura < 0 ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.Absolute, altura));
            tabla.Controls.Add(CrearLabel("Resultados", 17, true), 0, 0);
            tabla.Controls.Add(CrearLabel("Integral aproximada"), 0, 1);
            lblIntegral = CrearLabel("-", 22, true);
            lblIntegral.AutoEllipsis = true;
            tabla.Controls.Add(lblIntegral, 0, 2);
            tabla.Controls.Add(CrearLabel("Estado"), 0, 3);
            lblEstado = CrearLabel("Sin calcular", 11, true);
            tabla.Controls.Add(lblEstado, 0, 4);
            tabla.Controls.Add(CrearLabel("Metodo utilizado"), 0, 5);
            lblMetodo = CrearLabel("-");
            lblMetodo.TextAlign = ContentAlignment.TopLeft;
            tabla.Controls.Add(lblMetodo, 0, 6);
            tabla.Controls.Add(CrearLabel("Subintervalos"), 0, 7);
            lblSubintervalos = CrearLabel("-");
            tabla.Controls.Add(lblSubintervalos, 0, 8);
            tabla.Controls.Add(CrearLabel("Paso (h)"), 0, 9);
            lblPaso = CrearLabel("-");
            lblPaso.AutoEllipsis = true;
            tabla.Controls.Add(lblPaso, 0, 10);
            return tabla;
        }

        private void ActualizarMetodo()
        {
            actualizandoMetodo = true;
            try
            {
                MetodoIntegracion metodo = (MetodoIntegracion)cmbMetodo.SelectedIndex;
                bool multiple = metodo == MetodoIntegracion.TrapeciosMultiple || metodo == MetodoIntegracion.SimpsonUnTercioMultiple;
                nudSubintervalos.Enabled = multiple;
                nudSubintervalos.Minimum = metodo == MetodoIntegracion.SimpsonUnTercioMultiple ? 2 : 1;
                nudSubintervalos.Value = multiple ? Math.Max(nudSubintervalos.Minimum, ultimoN)
                    : metodo == MetodoIntegracion.TrapeciosSimple ? 1 : metodo == MetodoIntegracion.SimpsonUnTercioSimple ? 2 : 3;
            }
            finally { actualizandoMetodo = false; }
            LimpiarResultados();
        }

        private void Calcular()
        {
            try
            {
                Validate();
                resultado = integrador.Calcular(txtFuncion.Text, LeerNumero(txtXi, "extremo izquierdo"),
                    LeerNumero(txtXd, "extremo derecho"), (int)nudSubintervalos.Value, (MetodoIntegracion)cmbMetodo.SelectedIndex);
                string valor = resultado.Integral.ToString("F4", cultura);
                // Evitar un cero negativo que solo provenga del redondeo.
                lblIntegral.Text = valor == "-0,0000" ? "0,0000" : valor;
                lblIntegral.Font = new Font("Segoe UI", lblIntegral.Text.Length > 12 ? 12 : lblIntegral.Text.Length > 9 ? 16 : 22, FontStyle.Bold);
                lblEstado.Text = "Exitosa";
                lblEstado.ForeColor = Color.FromArgb(35, 125, 100);
                lblMetodo.Text = resultado.MetodoUtilizado;
                lblSubintervalos.Text = resultado.Subintervalos.ToString(cultura);
                lblPaso.Text = resultado.Paso.ToString("G8", cultura);
                lblError.Text = "";
                grafico.Mostrar(resultado);
            }
            catch (ArgumentException ex)
            {
                LimpiarResultados();
                lblError.Text = ex.Message;
                lblEstado.Text = "Revise los datos";
                lblEstado.ForeColor = Color.Firebrick;
            }
        }

        private double LeerNumero(TextBox entrada, string nombre)
        {
            double valor;
            if (!double.TryParse(entrada.Text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out valor)
                || double.IsNaN(valor) || double.IsInfinity(valor))
                throw new ArgumentException("Ingrese un numero valido para el " + nombre + ".");
            return valor;
        }

        private void LimpiarResultados()
        {
            resultado = null;
            if (lblIntegral == null) return;
            lblIntegral.Text = lblMetodo.Text = lblPaso.Text = lblSubintervalos.Text = "-";
            lblEstado.Text = "Sin calcular";
            lblEstado.ForeColor = Color.FromArgb(90, 50, 65);
            lblError.Text = "";
            grafico.Mostrar(null);
        }

        private TextBox CrearEntradaTexto(string valor)
        {
            TextBox entrada = new TextBox { Dock = DockStyle.Fill, Text = valor };
            entrada.TextChanged += (s, e) => LimpiarResultados();
            return entrada;
        }

        private TableLayoutPanel CrearSeccion()
        {
            TableLayoutPanel tabla = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, BackColor = Color.White,
                Padding = new Padding(18, 14, 18, 14), Margin = new Padding(0)
            };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return tabla;
        }

        private Label CrearLabel(string texto, int tamano = 10, bool negrita = false)
        {
            return new Label
            {
                Text = texto, Dock = DockStyle.Fill, Margin = new Padding(0),
                Font = new Font("Segoe UI", tamano, negrita ? FontStyle.Bold : FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(90, 50, 65)
            };
        }

        private Button CrearBoton(string texto, bool principal)
        {
            Button boton = new Button
            {
                Text = texto, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                BackColor = principal ? Color.FromArgb(200, 90, 130) : Color.FromArgb(250, 229, 236),
                ForeColor = principal ? Color.White : Color.FromArgb(90, 50, 65),
                Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(3)
            };
            boton.FlatAppearance.BorderSize = 0;
            return boton;
        }
    }
}
