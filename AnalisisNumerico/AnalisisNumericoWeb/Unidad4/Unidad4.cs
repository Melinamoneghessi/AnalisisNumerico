using System;
using System.Collections.Generic;
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
        private TabControl tabEntrada;
        private DataGridView dgvPuntos;
        private Label lblAyudaTabla, lblDatosTabla;
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
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
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
                contenido.Size = new Size(Math.Max(1030, pagina.ClientSize.Width - 48), Math.Max(550, pagina.ClientSize.Height - 40));
            };
            Controls.Add(pagina);
            Controls.Add(encabezado);
        }

        private TableLayoutPanel CrearEntrada()
        {
            TableLayoutPanel tabla = CrearSeccion();
            tabla.RowCount = 7;
            int[] alturas = { 40, 26, 36, -1, 74, 44, 40 };
            foreach (int altura in alturas) tabla.RowStyles.Add(altura < 0 ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.Absolute, altura));
            tabla.Controls.Add(CrearLabel("Ingreso de datos", 16, true), 0, 0);
            tabla.Controls.Add(CrearLabel("Metodo"), 0, 1);
            cmbMetodo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, DropDownWidth = 340 };
            cmbMetodo.Items.AddRange(new object[] { "Trapecios simple", "Trapecios multiple", "Simpson 1/3 simple", "Simpson 1/3 multiple", "Simpson 3/8" });
            cmbMetodo.SelectedIndexChanged += (s, e) => ActualizarMetodo();
            tabla.Controls.Add(cmbMetodo, 0, 2);

            tabEntrada = new TabControl { Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 0) };
            TabPage funcion = new TabPage("Funcion") { BackColor = Color.White, Padding = new Padding(6) };
            TabPage puntos = new TabPage("Tabla de valores") { BackColor = Color.White, Padding = new Padding(6) };
            tabEntrada.TabPages.Add(funcion);
            tabEntrada.TabPages.Add(puntos);
            tabEntrada.SelectedIndexChanged += (s, e) => LimpiarResultados();
            TableLayoutPanel camposFuncion = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 9, Margin = new Padding(0)
            };
            camposFuncion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 8; i++) camposFuncion.RowStyles.Add(new RowStyle(SizeType.Absolute, i % 2 == 0 ? 24 : 30));
            camposFuncion.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            camposFuncion.Controls.Add(CrearLabel("Funcion f(x)"), 0, 0);
            txtFuncion = CrearEntradaTexto("1/x + x");
            camposFuncion.Controls.Add(txtFuncion, 0, 1);
            camposFuncion.Controls.Add(CrearLabel("Extremo izquierdo (xi)"), 0, 2);
            txtXi = CrearEntradaTexto("0,5");
            camposFuncion.Controls.Add(txtXi, 0, 3);
            camposFuncion.Controls.Add(CrearLabel("Extremo derecho (xd)"), 0, 4);
            txtXd = CrearEntradaTexto("3,5");
            camposFuncion.Controls.Add(txtXd, 0, 5);
            camposFuncion.Controls.Add(CrearLabel("Subintervalos (N)"), 0, 6);
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
            camposFuncion.Controls.Add(nudSubintervalos, 0, 7);
            funcion.Controls.Add(camposFuncion);
            puntos.Controls.Add(CrearTablaPuntos());
            tabla.Controls.Add(tabEntrada, 0, 3);
            lblError = CrearLabel("");
            lblError.ForeColor = Color.Firebrick;
            lblError.Font = new Font("Segoe UI", 9);
            tabla.Controls.Add(lblError, 0, 4);
            Button calcular = CrearBoton("Calcular integral", true);
            calcular.Dock = DockStyle.Fill;
            calcular.Click += (s, e) => Calcular();
            tabla.Controls.Add(calcular, 0, 5);
            AcceptButton = calcular;
            Button limpiar = CrearBoton("Limpiar", false);
            limpiar.Dock = DockStyle.Fill;
            limpiar.Click += (s, e) =>
            {
                if (tabEntrada.SelectedIndex == 1)
                {
                    dgvPuntos.CancelEdit();
                    dgvPuntos.Rows.Clear();
                    dgvPuntos.Focus();
                }
                else
                {
                    txtFuncion.Clear(); txtXi.Clear(); txtXd.Clear();
                    txtFuncion.Focus();
                }
                LimpiarResultados();
            };
            tabla.Controls.Add(limpiar, 0, 6);
            return tabla;
        }

        private TableLayoutPanel CrearTablaPuntos()
        {
            TableLayoutPanel tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = new Padding(0) };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            lblAyudaTabla = CrearLabel("");
            lblAyudaTabla.Font = new Font("Segoe UI", 9);
            tabla.Controls.Add(lblAyudaTabla, 0, 0);
            dgvPuntos = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = true, AllowUserToDeleteRows = true, AllowUserToResizeRows = false,
                RowHeadersVisible = false, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.CellSelect, EditMode = DataGridViewEditMode.EditOnEnter,
                EnableHeadersVisualStyles = false, ColumnHeadersHeight = 30,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                Margin = new Padding(0)
            };
            dgvPuntos.Columns.Add("X", "X");
            dgvPuntos.Columns.Add("Y", "f(x)");
            dgvPuntos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(250, 229, 236);
            dgvPuntos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(250, 214, 227);
            dgvPuntos.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvPuntos.RowTemplate.Height = 28;
            lblDatosTabla = CrearLabel("");
            lblDatosTabla.Font = new Font("Segoe UI", 9);
            dgvPuntos.CellValueChanged += (s, e) => ActualizarDatosTabla();
            dgvPuntos.RowsAdded += (s, e) => ActualizarDatosTabla();
            dgvPuntos.RowsRemoved += (s, e) => ActualizarDatosTabla();
            tabla.Controls.Add(dgvPuntos, 0, 1);

            TableLayoutPanel acciones = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
            acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            Button agregar = CrearBoton("Agregar fila", false);
            agregar.Dock = DockStyle.Fill;
            agregar.Font = new Font("Segoe UI", 9);
            agregar.Click += (s, e) =>
            {
                dgvPuntos.EndEdit();
                int fila = dgvPuntos.Rows.Add();
                dgvPuntos.CurrentCell = dgvPuntos.Rows[fila].Cells[0];
                dgvPuntos.Focus();
                dgvPuntos.BeginEdit(true);
            };
            Button quitar = CrearBoton("Quitar fila", false);
            quitar.Dock = DockStyle.Fill;
            quitar.Font = new Font("Segoe UI", 9);
            quitar.Click += (s, e) =>
            {
                DataGridViewRow fila = dgvPuntos.CurrentRow;
                if (fila != null && !fila.IsNewRow)
                {
                    dgvPuntos.CancelEdit();
                    dgvPuntos.Rows.Remove(fila);
                }
            };
            acciones.Controls.Add(agregar, 0, 0);
            acciones.Controls.Add(quitar, 1, 0);
            tabla.Controls.Add(acciones, 0, 2);
            tabla.Controls.Add(lblDatosTabla, 0, 3);
            dgvPuntos.Rows.Add("0", "0");
            dgvPuntos.Rows.Add("0,5", "0,25");
            dgvPuntos.Rows.Add("1", "1");
            dgvPuntos.Rows.Add("1,5", "2,25");
            dgvPuntos.Rows.Add("2", "4");
            return tabla;
        }

        private void ActualizarDatosTabla()
        {
            int cantidad = dgvPuntos.Rows.Count - (dgvPuntos.AllowUserToAddRows ? 1 : 0);
            lblDatosTabla.Text = cantidad < 2 ? "Ingrese al menos 2 puntos."
                : "xi = " + Convert.ToString(dgvPuntos.Rows[0].Cells[0].Value, cultura)
                    + "   xd = " + Convert.ToString(dgvPuntos.Rows[cantidad - 1].Cells[0].Value, cultura)
                    + "\nN = " + (cantidad - 1) + " (filas - 1)";
            LimpiarResultados();
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
                lblAyudaTabla.Text = metodo == MetodoIntegracion.TrapeciosSimple ? "Cargue 2 puntos: los extremos del intervalo. X debe ser creciente."
                    : metodo == MetodoIntegracion.TrapeciosMultiple ? "Cargue al menos 2 puntos con X crecientes. La separacion puede variar."
                    : metodo == MetodoIntegracion.SimpsonUnTercioSimple ? "Cargue 3 puntos con X crecientes e igual separacion."
                    : metodo == MetodoIntegracion.SimpsonTresOctavos ? "Cargue 4 puntos con X crecientes e igual separacion."
                    : "Al menos 3 puntos con igual separacion. N impar combina Simpson 1/3 y 3/8.";
            }
            finally { actualizandoMetodo = false; }
            LimpiarResultados();
        }

        private void Calcular()
        {
            try
            {
                Validate();
                MetodoIntegracion metodo = (MetodoIntegracion)cmbMetodo.SelectedIndex;
                if (tabEntrada.SelectedIndex == 1)
                {
                    dgvPuntos.EndEdit();
                    resultado = integrador.CalcularTabla(LeerPuntos(), metodo);
                }
                else
                    resultado = integrador.Calcular(txtFuncion.Text, LeerNumero(txtXi, "extremo izquierdo"),
                        LeerNumero(txtXd, "extremo derecho"), (int)nudSubintervalos.Value, metodo);
                string valor = resultado.Integral.ToString("F4", cultura);
                // Evitar un cero negativo que solo provenga del redondeo.
                lblIntegral.Text = valor == "-0,0000" ? "0,0000" : valor;
                lblIntegral.Font = new Font("Segoe UI", lblIntegral.Text.Length > 12 ? 12 : lblIntegral.Text.Length > 9 ? 16 : 22, FontStyle.Bold);
                lblEstado.Text = "Exitosa";
                lblEstado.ForeColor = Color.FromArgb(35, 125, 100);
                lblMetodo.Text = resultado.MetodoUtilizado;
                lblSubintervalos.Text = resultado.Subintervalos.ToString(cultura);
                lblPaso.Text = resultado.PasoUniforme ? resultado.Paso.ToString("G8", cultura) : "Variable";
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
            return LeerNumero(entrada.Text, "el " + nombre);
        }

        private double LeerNumero(string texto, string nombre)
        {
            double valor;
            if (!double.TryParse(texto.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out valor)
                || double.IsNaN(valor) || double.IsInfinity(valor))
                throw new ArgumentException("Ingrese un numero valido para " + nombre + ".");
            return valor;
        }

        private List<PuntoIntegracion> LeerPuntos()
        {
            List<PuntoIntegracion> puntos = new List<PuntoIntegracion>();
            foreach (DataGridViewRow fila in dgvPuntos.Rows)
            {
                if (fila.IsNewRow) continue;
                double x = LeerNumero(Convert.ToString(fila.Cells[0].Value, cultura), "X en la fila " + (fila.Index + 1));
                double y = LeerNumero(Convert.ToString(fila.Cells[1].Value, cultura), "f(x) en la fila " + (fila.Index + 1));
                puntos.Add(new PuntoIntegracion(x, y));
            }
            return puntos;
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
