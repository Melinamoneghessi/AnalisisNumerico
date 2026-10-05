using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LogicaAnalisis.Unidad3;

namespace AnalisisNumericoWeb.Unidad3
{
    public class Unidad3 : Form
    {
        private readonly MetodoAjusteCurvas metodo = new MetodoAjusteCurvas();
        private readonly Color rosa = Color.FromArgb(200, 90, 130);
        private ComboBox cmbMetodo;
        private NumericUpDown nudGrado;
        private NumericUpDown nudTolerancia;
        private DataGridView dgvPuntos;
        private TableLayoutPanel tablaDatos;
        private TableLayoutPanel tablaResultados;
        private TextBox txtFuncion;
        private Label lblCorrelacion;
        private Label lblEfectividad;
        private Label lblError;
        private Panel panelModificacion;
        private TextBox txtFuncionModificada;
        private Button btnCalcularModificada;
        private Label lblEfectividadModificada;
        private Label lblErrorModificacion;
        private GraficoAjuste grafico;
        private ResultadoAjuste resultadoOriginal;
        private ResultadoAjuste resultadoModificado;

        public Unidad3()
        {
            CrearInterfaz();
            dgvPuntos.Rows.Add("P1", "3", "15");
            dgvPuntos.Rows.Add("P2", "4", "7");
            dgvPuntos.Rows.Add("P3", "5", "11");
            dgvPuntos.Rows.Add("P4", "6", "16");
            ActualizarMetodo();
        }

        private void CrearInterfaz()
        {
            Text = "Unidad 3 - Ajuste de curvas";
            Font = new Font("Segoe UI", 10);
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1020, 720);
            BackColor = Color.FromArgb(255, 245, 248);

            Panel encabezado = new Panel { Dock = DockStyle.Top, Height = 105, BackColor = Color.FromArgb(244, 180, 196) };
            Button volver = CrearBoton("Volver", false);
            volver.SetBounds(24, 35, 105, 35);
            volver.Click += (s, e) => Close();
            encabezado.Controls.Add(volver);
            Label titulo = CrearLabel("Analisis Numerico", 24, true);
            Label subtitulo = CrearLabel("Unidad 3 - Ajuste de curvas", 12);
            titulo.Dock = subtitulo.Dock = DockStyle.None;
            titulo.AutoSize = subtitulo.AutoSize = true;
            encabezado.Controls.Add(titulo);
            encabezado.Controls.Add(subtitulo);
            encabezado.Resize += (s, e) =>
            {
                titulo.Location = new Point(Math.Max(150, (encabezado.Width - titulo.Width) / 2), 14);
                subtitulo.Location = new Point(Math.Max(150, (encabezado.Width - subtitulo.Width) / 2), 62);
            };

            Panel pagina = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            TableLayoutPanel contenido = new TableLayoutPanel
            {
                ColumnCount = 2, RowCount = 1, BackColor = BackColor
            };
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tablaDatos = CrearTablaSeccion();
            tablaDatos.Margin = new Padding(0, 0, 12, 0);
            tablaResultados = CrearTablaSeccion();
            tablaResultados.Margin = new Padding(12, 0, 0, 0);
            CrearEntrada();
            CrearResultados();
            contenido.Controls.Add(tablaDatos, 0, 0);
            contenido.Controls.Add(tablaResultados, 1, 0);
            pagina.Controls.Add(contenido);
            pagina.Resize += (s, e) =>
            {
                contenido.Location = new Point(24, 20);
                contenido.Size = new Size(Math.Max(960, pagina.ClientSize.Width - 48), Math.Max(530, pagina.ClientSize.Height - 40));
            };
            Controls.Add(pagina);
            Controls.Add(encabezado);
        }

        private void CrearEntrada()
        {
            tablaDatos.RowCount = 8;
            AgregarFilas(tablaDatos, 40, 44, 40, -1, 42, 44, 58, 44);
            tablaDatos.Controls.Add(CrearLabel("Ingreso de datos", 17, true), 0, 0);

            TableLayoutPanel filaMetodo = CrearFila(75);
            filaMetodo.Controls.Add(CrearLabel("Metodo"), 0, 0);
            cmbMetodo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMetodo.Items.AddRange(new object[] { "Regresion lineal", "Regresion polinomial" });
            cmbMetodo.SelectedIndex = 0;
            cmbMetodo.SelectedIndexChanged += (s, e) => ActualizarMetodo();
            filaMetodo.Controls.Add(cmbMetodo, 1, 0);
            tablaDatos.Controls.Add(filaMetodo, 0, 1);

            TableLayoutPanel filaGrado = CrearFila(75);
            filaGrado.Controls.Add(CrearLabel("Grado"), 0, 0);
            nudGrado = new NumericUpDown { Dock = DockStyle.Left, Width = 90, Minimum = 2, Maximum = 10, Value = 2 };
            nudGrado.ValueChanged += (s, e) => LimpiarResultados();
            filaGrado.Controls.Add(nudGrado, 1, 0);
            tablaDatos.Controls.Add(filaGrado, 0, 2);

            dgvPuntos = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                AllowUserToOrderColumns = false, RowHeadersVisible = false, MultiSelect = false,
                ReadOnly = false, EditMode = DataGridViewEditMode.EditOnEnter,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false, ColumnHeadersHeight = 34,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            dgvPuntos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(250, 229, 236);
            dgvPuntos.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(90, 50, 65);
            dgvPuntos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(250, 214, 227);
            dgvPuntos.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvPuntos.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPuntos.RowTemplate.Height = 36;
            dgvPuntos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Punto", HeaderText = "Punto", ReadOnly = true, FillWeight = 55 });
            dgvPuntos.Columns.Add(new DataGridViewTextBoxColumn { Name = "X", HeaderText = "X", ReadOnly = false, FillWeight = 100 });
            dgvPuntos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Y", HeaderText = "Y", ReadOnly = false, FillWeight = 100 });
            dgvPuntos.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Eliminar", HeaderText = "", Text = "X", UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 38
            });
            foreach (DataGridViewColumn columna in dgvPuntos.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            dgvPuntos.CellValueChanged += (s, e) =>
            {
                if (e.RowIndex >= 0 && (e.ColumnIndex == 1 || e.ColumnIndex == 2))
                {
                    dgvPuntos.Rows[e.RowIndex].ErrorText = "";
                    LimpiarResultados();
                }
            };
            dgvPuntos.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 3)
                {
                    dgvPuntos.Rows.RemoveAt(e.RowIndex);
                    RenumerarPuntos();
                    LimpiarResultados();
                }
            };
            dgvPuntos.CellToolTipTextNeeded += (s, e) =>
            {
                if (e.ColumnIndex == 3) e.ToolTipText = "Eliminar punto";
            };
            dgvPuntos.EditingControlShowing += (s, e) =>
            {
                e.Control.TextChanged -= AlEditarCoordenada;
                e.Control.TextChanged += AlEditarCoordenada;
            };
            tablaDatos.Controls.Add(dgvPuntos, 0, 3);

            Button agregar = CrearBoton("Agregar punto", false);
            agregar.Dock = DockStyle.Fill;
            agregar.Click += (s, e) =>
            {
                dgvPuntos.EndEdit();
                int fila = dgvPuntos.Rows.Add("P" + (dgvPuntos.Rows.Count + 1), "", "");
                LimpiarResultados();
                dgvPuntos.CurrentCell = dgvPuntos.Rows[fila].Cells[1];
                dgvPuntos.Focus();
                dgvPuntos.BeginEdit(true);
            };
            tablaDatos.Controls.Add(agregar, 0, 4);

            TableLayoutPanel filaTolerancia = CrearFila(145);
            filaTolerancia.Controls.Add(CrearLabel("Tolerancia (%)"), 0, 0);
            nudTolerancia = new NumericUpDown
            {
                Dock = DockStyle.Left, Width = 100, Minimum = 0, Maximum = 100,
                DecimalPlaces = 2, Value = 80, TextAlign = HorizontalAlignment.Center
            };
            nudTolerancia.ValueChanged += (s, e) => LimpiarResultados();
            filaTolerancia.Controls.Add(nudTolerancia, 1, 0);
            tablaDatos.Controls.Add(filaTolerancia, 0, 5);
            lblError = CrearLabel("");
            lblError.ForeColor = Color.Firebrick;
            tablaDatos.Controls.Add(lblError, 0, 6);

            TableLayoutPanel acciones = CrearFila(0);
            acciones.ColumnStyles.Clear();
            acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            Button calcular = CrearBoton("Calcular", true);
            calcular.Dock = DockStyle.Fill;
            calcular.Click += (s, e) => Calcular();
            Button limpiar = CrearBoton("Limpiar", false);
            limpiar.Dock = DockStyle.Fill;
            limpiar.Click += (s, e) =>
            {
                dgvPuntos.CancelEdit();
                dgvPuntos.Rows.Clear();
                LimpiarResultados();
            };
            acciones.Controls.Add(calcular, 0, 0);
            acciones.Controls.Add(limpiar, 1, 0);
            tablaDatos.Controls.Add(acciones, 0, 7);
        }

        private void CrearResultados()
        {
            tablaResultados.RowCount = 7;
            AgregarFilas(tablaResultados, 36, 24, 48, 28, 30, -1, 124);
            tablaResultados.Controls.Add(CrearLabel("Resultados", 17, true), 0, 0);
            tablaResultados.Controls.Add(CrearLabel("Funcion obtenida"), 0, 1);
            txtFuncion = new TextBox
            {
                Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, BorderStyle = BorderStyle.None,
                BackColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), ScrollBars = ScrollBars.Vertical
            };
            tablaResultados.Controls.Add(txtFuncion, 0, 2);
            lblCorrelacion = CrearLabel("Correlacion (r): -");
            lblEfectividad = CrearLabel("Efectividad del ajuste: -");
            tablaResultados.Controls.Add(lblCorrelacion, 0, 3);
            tablaResultados.Controls.Add(lblEfectividad, 0, 4);
            grafico = new GraficoAjuste { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 8) };
            tablaResultados.Controls.Add(grafico, 0, 5);

            panelModificacion = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            TableLayoutPanel modificacion = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            AgregarFilas(modificacion, 30, 36, 32, 26);
            modificacion.Controls.Add(CrearLabel("Modificacion de recta", 12, true), 0, 0);
            TableLayoutPanel filaFuncion = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(0) };
            filaFuncion.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            filaFuncion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filaFuncion.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94));
            filaFuncion.Controls.Add(CrearLabel("Funcion modificada"), 0, 0);
            txtFuncionModificada = new TextBox { Dock = DockStyle.Fill, Enabled = false };
            txtFuncionModificada.TextChanged += (s, e) => LimpiarModificacion();
            filaFuncion.Controls.Add(txtFuncionModificada, 1, 0);
            btnCalcularModificada = CrearBoton("Calcular", false);
            btnCalcularModificada.Dock = DockStyle.Fill;
            btnCalcularModificada.Enabled = false;
            btnCalcularModificada.Click += (s, e) => CalcularModificada();
            filaFuncion.Controls.Add(btnCalcularModificada, 2, 0);
            modificacion.Controls.Add(filaFuncion, 0, 1);
            lblEfectividadModificada = CrearLabel("Efectividad: -");
            lblErrorModificacion = CrearLabel("");
            lblErrorModificacion.ForeColor = Color.Firebrick;
            modificacion.Controls.Add(lblEfectividadModificada, 0, 2);
            modificacion.Controls.Add(lblErrorModificacion, 0, 3);
            panelModificacion.Controls.Add(modificacion);
            tablaResultados.Controls.Add(panelModificacion, 0, 6);
        }

        private void Calcular()
        {
            try
            {
                // Confirmar tambien la celda que sigue en edicion al pulsar Calcular.
                if (!dgvPuntos.EndEdit())
                {
                    throw new ArgumentException("Debe confirmar la coordenada que esta editando.");
                }
                int grado = cmbMetodo.SelectedIndex == 0 ? 1 : (int)nudGrado.Value;
                resultadoOriginal = metodo.Calcular(LeerPuntos(), grado, (double)nudTolerancia.Value);
                txtFuncion.Text = resultadoOriginal.Funcion;
                lblCorrelacion.Text = "Correlacion (r): " + FormatearPorcentaje(resultadoOriginal.CorrelacionPorcentaje);
                lblEfectividad.Text = "Efectividad del ajuste: " + (resultadoOriginal.EsAceptable ? "Aceptable" : "No aceptable");
                lblEfectividad.ForeColor = resultadoOriginal.EsAceptable ? Color.FromArgb(35, 125, 100) : Color.Firebrick;
                lblError.Text = "";
                LimpiarModificacion();
                bool lineal = cmbMetodo.SelectedIndex == 0;
                txtFuncionModificada.Enabled = btnCalcularModificada.Enabled = lineal;
                txtFuncionModificada.Text = lineal ? resultadoOriginal.Funcion : "";
                grafico.Mostrar(resultadoOriginal, null);
            }
            catch (ArgumentException ex)
            {
                LimpiarResultados();
                lblError.Text = ex.Message;
            }
        }

        private List<PuntoAjuste> LeerPuntos()
        {
            List<PuntoAjuste> puntos = new List<PuntoAjuste>();
            foreach (DataGridViewRow fila in dgvPuntos.Rows)
            {
                double[] coordenadas = new double[2];
                for (int columna = 1; columna <= 2; columna++)
                {
                    string texto = Convert.ToString(fila.Cells[columna].Value).Trim().Replace(',', '.');
                    double valor;
                    if (!double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out valor)
                        || double.IsNaN(valor) || double.IsInfinity(valor))
                    {
                        string mensaje = "P" + (fila.Index + 1) + ": ingrese un numero valido en " + dgvPuntos.Columns[columna].Name + ".";
                        fila.ErrorText = mensaje;
                        dgvPuntos.CurrentCell = fila.Cells[columna];
                        throw new ArgumentException(mensaje);
                    }
                    coordenadas[columna - 1] = valor;
                }
                puntos.Add(new PuntoAjuste(coordenadas[0], coordenadas[1]));
            }
            return puntos;
        }

        private void CalcularModificada()
        {
            if (resultadoOriginal == null || cmbMetodo.SelectedIndex != 0) return;
            try
            {
                resultadoModificado = metodo.EvaluarRectaModificada(resultadoOriginal.Puntos, txtFuncionModificada.Text,
                    resultadoOriginal.ToleranciaPorcentaje);
                lblEfectividadModificada.Text = "Efectividad: " + FormatearPorcentaje(resultadoModificado.CorrelacionPorcentaje)
                    + "   -   " + (resultadoModificado.EsAceptable ? "Aceptable" : "No aceptable");
                lblEfectividadModificada.ForeColor = resultadoModificado.EsAceptable ? Color.FromArgb(35, 125, 100) : Color.Firebrick;
                lblErrorModificacion.Text = "";
                grafico.Mostrar(resultadoOriginal, resultadoModificado);
            }
            catch (ArgumentException ex)
            {
                LimpiarModificacion();
                lblErrorModificacion.Text = ex.Message;
            }
        }

        private void ActualizarMetodo()
        {
            bool polinomial = cmbMetodo.SelectedIndex == 1;
            tablaDatos.GetControlFromPosition(0, 2).Visible = polinomial;
            tablaDatos.RowStyles[2].Height = polinomial ? 40 : 0;
            panelModificacion.Visible = !polinomial;
            tablaResultados.RowStyles[6].Height = polinomial ? 0 : 124;
            LimpiarResultados();
        }

        private void RenumerarPuntos()
        {
            for (int i = 0; i < dgvPuntos.Rows.Count; i++)
            {
                dgvPuntos.Rows[i].Cells[0].Value = "P" + (i + 1);
            }
        }

        private void AlEditarCoordenada(object sender, EventArgs e)
        {
            TextBox editor = sender as TextBox;
            if (editor == null || dgvPuntos.CurrentCell == null
                || editor.Text == Convert.ToString(dgvPuntos.CurrentCell.Value)) return;
            LimpiarResultados();
        }

        private void LimpiarResultados()
        {
            resultadoOriginal = null;
            txtFuncion.Text = "";
            lblCorrelacion.Text = "Correlacion (r): -";
            lblEfectividad.Text = "Efectividad del ajuste: -";
            lblEfectividad.ForeColor = ForeColor;
            lblError.Text = "";
            txtFuncionModificada.Enabled = btnCalcularModificada.Enabled = false;
            txtFuncionModificada.Text = "";
            LimpiarModificacion();
        }

        private void LimpiarModificacion()
        {
            resultadoModificado = null;
            lblEfectividadModificada.Text = "Efectividad: -";
            lblEfectividadModificada.ForeColor = ForeColor;
            lblErrorModificacion.Text = "";
            grafico.Mostrar(resultadoOriginal, null);
        }

        private string FormatearPorcentaje(double valor)
        {
            return valor.ToString("0.####", CultureInfo.GetCultureInfo("es-AR")) + "%";
        }

        private TableLayoutPanel CrearTablaSeccion()
        {
            TableLayoutPanel tabla = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(20, 14, 20, 14), BackColor = Color.White
            };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return tabla;
        }

        private TableLayoutPanel CrearFila(int anchoEtiqueta)
        {
            TableLayoutPanel fila = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, anchoEtiqueta));
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return fila;
        }

        private void AgregarFilas(TableLayoutPanel tabla, params int[] alturas)
        {
            foreach (int altura in alturas)
            {
                tabla.RowStyles.Add(altura < 0 ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.Absolute, altura));
            }
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
                BackColor = principal ? rosa : Color.FromArgb(250, 229, 236),
                ForeColor = principal ? Color.White : Color.FromArgb(90, 50, 65),
                Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(3)
            };
            boton.FlatAppearance.BorderSize = 0;
            return boton;
        }
    }
}
