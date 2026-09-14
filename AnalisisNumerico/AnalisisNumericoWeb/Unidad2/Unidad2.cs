using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LogicaAnalisis.Unidad2;

namespace AnalisisNumericoWeb.Unidad2
{
    public class Unidad2 : Form
    {
        private NumericUpDown nudDimension;
        private ComboBox cmbMetodo;
        private Panel panelMatriz;
        private TextBox[,] txtMatriz;
        private TextBox txtResultadoMetodo;
        private TextBox txtResultadoDimension;
        private TextBox txtResultadoConverge;
        private TextBox txtResultadoNumeroCondicion;
        private TextBox txtResultadoCondicionamiento;
        private TextBox txtResultadoCambiosFilas;
        private DataGridView dgvComparacion;

        public Unidad2()
        {
            CrearInterfaz();
        }

        private void CrearInterfaz()
        {
            Text = "Unidad 2 - Sistemas de ecuaciones";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(255, 245, 248);

            Panel encabezado = new Panel();
            encabezado.Dock = DockStyle.Top;
            encabezado.Height = 115;
            encabezado.BackColor = Color.FromArgb(244, 180, 196);
            Controls.Add(encabezado);

            Button btnVolver = new Button();
            btnVolver.Text = "Volver";
            btnVolver.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnVolver.Size = new Size(105, 35);
            btnVolver.Location = new Point(25, 38);
            btnVolver.FlatStyle = FlatStyle.Flat;
            btnVolver.FlatAppearance.BorderSize = 0;
            btnVolver.BackColor = Color.White;
            btnVolver.ForeColor = Color.FromArgb(200, 90, 130);
            btnVolver.Cursor = Cursors.Hand;
            btnVolver.Click += (s, e) => Close();
            encabezado.Controls.Add(btnVolver);

            Label titulo = new Label();
            titulo.Text = "Analisis Numerico";
            titulo.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(90, 50, 65);
            titulo.AutoSize = true;
            encabezado.Controls.Add(titulo);

            Label subtitulo = new Label();
            subtitulo.Text = "Unidad 2 - Sistemas de ecuaciones";
            subtitulo.Font = new Font("Segoe UI", 12);
            subtitulo.ForeColor = Color.FromArgb(110, 70, 85);
            subtitulo.AutoSize = true;
            encabezado.Controls.Add(subtitulo);

            void CentrarTitulo()
            {
                titulo.Left = (encabezado.Width - titulo.Width) / 2;
                titulo.Top = 18;
                subtitulo.Left = (encabezado.Width - subtitulo.Width) / 2;
                subtitulo.Top = 66;
            }

            CentrarTitulo();
            encabezado.Resize += (s, e) => CentrarTitulo();

            Panel contenido = new Panel();
            contenido.Size = new Size(1120, 560);
            contenido.BackColor = Color.Transparent;
            Controls.Add(contenido);

            Panel panelDatos = CrearPanelSeccion("Ingreso de datos", new Point(0, 0), new Size(665, 560));
            contenido.Controls.Add(panelDatos);

            Label lblDimension = CrearLabel("Dimension", new Point(45, 82));
            panelDatos.Controls.Add(lblDimension);

            nudDimension = new NumericUpDown();
            nudDimension.Location = new Point(150, 78);
            nudDimension.Size = new Size(115, 32);
            nudDimension.Font = new Font("Segoe UI", 10);
            nudDimension.Minimum = 1;
            nudDimension.Maximum = 10;
            nudDimension.Value = 3;
            nudDimension.TextAlign = HorizontalAlignment.Center;
            nudDimension.ValueChanged += (s, e) =>
            {
                GenerarMatriz();
                LimpiarResultados();
            };
            panelDatos.Controls.Add(nudDimension);

            Label lblMetodo = CrearLabel("Metodo", new Point(45, 126));
            panelDatos.Controls.Add(lblMetodo);

            cmbMetodo = new ComboBox();
            cmbMetodo.Location = new Point(150, 122);
            cmbMetodo.Size = new Size(170, 32);
            cmbMetodo.Font = new Font("Segoe UI", 10);
            cmbMetodo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMetodo.Items.Add("Gauss-Jordan");
            cmbMetodo.Items.Add("Gauss-Seidel");
            cmbMetodo.SelectedIndex = 0;
            cmbMetodo.SelectedIndexChanged += (s, e) => LimpiarResultados();
            panelDatos.Controls.Add(cmbMetodo);

            panelMatriz = new Panel();
            panelMatriz.Location = new Point(45, 185);
            panelMatriz.Size = new Size(565, 220);
            panelMatriz.AutoScroll = true;
            panelMatriz.BackColor = Color.Transparent;
            panelDatos.Controls.Add(panelMatriz);

            Button btnCalcular = CrearBoton("Calcular", new Point(180, 470), new Size(140, 38));
            btnCalcular.Click += (s, e) => Calcular();
            panelDatos.Controls.Add(btnCalcular);

            Button btnLimpiar = CrearBoton("Limpiar", new Point(345, 470), new Size(140, 38));
            btnLimpiar.Click += (s, e) => Limpiar();
            panelDatos.Controls.Add(btnLimpiar);

            Panel panelResultados = CrearPanelSeccion("Resultados", new Point(700, 0), new Size(420, 560));
            contenido.Controls.Add(panelResultados);

            int y = 82;
            txtResultadoMetodo = CrearResultado(panelResultados, "Metodo utilizado", "Gauss-Jordan", ref y);
            txtResultadoDimension = CrearResultado(panelResultados, "Dimension", "3 x 3", ref y);
            txtResultadoConverge = CrearResultado(panelResultados, "Converge?", "-", ref y);
            txtResultadoNumeroCondicion = CrearResultado(panelResultados, "Nro condicion", "-", ref y);
            txtResultadoCondicionamiento = CrearResultado(panelResultados, "Condicionamiento", "-", ref y);
            txtResultadoCambiosFilas = CrearResultado(panelResultados, "Cambios filas", "-", ref y);

            Label lblSolucion = CrearLabel("Solucion", new Point(45, y + 8));
            panelResultados.Controls.Add(lblSolucion);

            dgvComparacion = CrearTablaComparacion(new Point(45, y + 40), new Size(320, 150));
            panelResultados.Controls.Add(dgvComparacion);

            void CentrarContenido()
            {
                contenido.Left = (ClientSize.Width - contenido.Width) / 2;
                contenido.Top = encabezado.Height + ((ClientSize.Height - encabezado.Height - contenido.Height) / 2);
            }

            CentrarContenido();
            Resize += (s, e) => CentrarContenido();

            GenerarMatriz();
        }

        private void GenerarMatriz()
        {
            string[,] valoresAnteriores = ObtenerTextosMatrizActual();
            int dimension = LeerDimension();
            panelMatriz.SuspendLayout();
            panelMatriz.AutoScrollPosition = new Point(0, 0);
            panelMatriz.Controls.Clear();
            txtMatriz = new TextBox[dimension, dimension + 1];

            int anchoCaja = dimension <= 4 ? 66 : 58;
            int altoCaja = 30;
            int separacionX = anchoCaja + (dimension <= 4 ? 18 : 12);
            int separacionY = 40;
            int inicioX = 20;
            int inicioY = 34;
            int xIgual = inicioX + (dimension * separacionX) + 4;
            int xTermino = xIgual + 34;

            for (int columna = 0; columna < dimension; columna++)
            {
                Label label = CrearLabel("x" + (columna + 1), new Point(inicioX + (columna * separacionX) + 18, 0));
                panelMatriz.Controls.Add(label);
            }

            Label lblTi = CrearLabel("TI", new Point(xTermino + 20, 0));
            panelMatriz.Controls.Add(lblTi);

            for (int fila = 0; fila < dimension; fila++)
            {
                for (int columna = 0; columna < dimension + 1; columna++)
                {
                    int x = columna == dimension
                        ? xTermino
                        : inicioX + (columna * separacionX);

                    TextBox texto = CrearTextBox(new Point(x, inicioY + (fila * separacionY)), new Size(anchoCaja, altoCaja));

                    if (valoresAnteriores != null &&
                        fila < valoresAnteriores.GetLength(0) &&
                        columna < valoresAnteriores.GetLength(1))
                    {
                        texto.Text = valoresAnteriores[fila, columna];
                    }

                    txtMatriz[fila, columna] = texto;
                    panelMatriz.Controls.Add(texto);

                    if (columna == dimension - 1)
                    {
                        Label igual = CrearLabel("=", new Point(xIgual, inicioY + 4 + (fila * separacionY)));
                        panelMatriz.Controls.Add(igual);
                    }
                }
            }

            panelMatriz.AutoScrollMinSize = new Size(
                xTermino + anchoCaja + 35,
                inicioY + (dimension * separacionY) + 20
            );
            panelMatriz.ResumeLayout();
            MostrarFilasComparacion(dimension);
            txtResultadoDimension.Text = dimension + " x " + dimension;
        }

        private void Calcular()
        {
            try
            {
                if (cmbMetodo.Text == "Gauss-Jordan")
                {
                    MetodoGaussJordan metodo = new MetodoGaussJordan();
                    ResultadoGaussJordan resultado = metodo.Calcular(LeerMatriz());
                    MostrarResultado(resultado);
                }
                else if (cmbMetodo.Text == "Gauss-Seidel")
                {
                    MetodoGaussSeidel metodo = new MetodoGaussSeidel();
                    ResultadoGaussSeidel resultado = metodo.Calcular(LeerMatriz());
                    MostrarResultado(resultado);
                }
            }
            catch (Exception ex)
            {
                txtResultadoConverge.Text = "No";
                txtResultadoNumeroCondicion.Text = "-";
                txtResultadoCondicionamiento.Text = "-";
                txtResultadoCambiosFilas.Text = "-";
                MostrarFilasComparacion(LeerDimension());

                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private double[,] LeerMatriz()
        {
            int dimension = LeerDimension();
            double[,] matriz = new double[dimension, dimension + 1];

            for (int fila = 0; fila < dimension; fila++)
            {
                for (int columna = 0; columna < dimension + 1; columna++)
                {
                    string valor = txtMatriz[fila, columna].Text;

                    if (string.IsNullOrWhiteSpace(valor))
                    {
                        throw new ArgumentException("Debe completar todos los coeficientes y terminos independientes.");
                    }

                    matriz[fila, columna] = LeerDouble(valor);
                }
            }

            return matriz;
        }

        private void MostrarResultado(ResultadoGaussJordan resultado)
        {
            txtResultadoMetodo.Text = resultado.Metodo;
            txtResultadoDimension.Text = resultado.Dimension + " x " + resultado.Dimension;
            txtResultadoConverge.Text = "Si";
            txtResultadoNumeroCondicion.Text = FormatearNumero(resultado.NumeroCondicion);
            txtResultadoCondicionamiento.Text = resultado.Condicionamiento;
            txtResultadoCambiosFilas.Text = "-";

            CargarTablaComparacion(resultado.VectorResultado, resultado.VectorResultadoModificado);
        }

        private void MostrarResultado(ResultadoGaussSeidel resultado)
        {
            txtResultadoMetodo.Text = resultado.Metodo;
            txtResultadoDimension.Text = resultado.Dimension + " x " + resultado.Dimension;
            txtResultadoConverge.Text = resultado.Converge ? "Si" : "No";
            txtResultadoNumeroCondicion.Text = "-";
            txtResultadoCondicionamiento.Text = "-";
            txtResultadoCambiosFilas.Text = FormatearCambiosFilas(resultado);

            CargarTablaComparacion(resultado.VectorResultado, null);

            if (!resultado.Converge)
            {
                MessageBox.Show(
                    resultado.Mensaje,
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private void Limpiar()
        {
            int dimension = LeerDimension();

            for (int fila = 0; fila < dimension; fila++)
            {
                for (int columna = 0; columna < dimension + 1; columna++)
                {
                    txtMatriz[fila, columna].Text = "";
                }
            }

            LimpiarResultados();
        }

        private void LimpiarResultados()
        {
            txtResultadoMetodo.Text = cmbMetodo.Text;
            txtResultadoDimension.Text = LeerDimension() + " x " + LeerDimension();
            txtResultadoConverge.Text = "-";
            txtResultadoNumeroCondicion.Text = "-";
            txtResultadoCondicionamiento.Text = "-";
            txtResultadoCambiosFilas.Text = "-";

            MostrarFilasComparacion(LeerDimension());
        }

        private string[,] ObtenerTextosMatrizActual()
        {
            if (txtMatriz == null)
            {
                return null;
            }

            int filas = txtMatriz.GetLength(0);
            int columnas = txtMatriz.GetLength(1);
            string[,] valores = new string[filas, columnas];

            for (int fila = 0; fila < filas; fila++)
            {
                for (int columna = 0; columna < columnas; columna++)
                {
                    valores[fila, columna] = txtMatriz[fila, columna] == null
                        ? ""
                        : txtMatriz[fila, columna].Text;
                }
            }

            return valores;
        }

        private void MostrarFilasComparacion(int dimension)
        {
            if (dgvComparacion == null)
            {
                return;
            }

            dgvComparacion.Rows.Clear();

            for (int i = 0; i < dimension; i++)
            {
                dgvComparacion.Rows.Add("x" + (i + 1), "", "");
            }
        }

        private void CargarTablaComparacion(double[] normal, double[] modificado)
        {
            dgvComparacion.Rows.Clear();

            for (int i = 0; i < normal.Length; i++)
            {
                string valorModificado = modificado == null || i >= modificado.Length
                    ? "-"
                    : FormatearNumero(modificado[i]);

                dgvComparacion.Rows.Add(
                    "x" + (i + 1),
                    FormatearNumero(normal[i]),
                    valorModificado
                );
            }
        }

        private string FormatearCambiosFilas(ResultadoGaussSeidel resultado)
        {
            if (resultado.CambiosFilas == null || resultado.CambiosFilas.Count == 0)
            {
                return "Sin cambios";
            }

            return string.Join("; ", resultado.CambiosFilas.ToArray());
        }

        private Panel CrearPanelSeccion(string titulo, Point posicion, Size tamanio)
        {
            Panel panel = new Panel();
            panel.Location = posicion;
            panel.Size = tamanio;
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;

            Label lblTitulo = new Label();
            lblTitulo.Text = titulo;
            lblTitulo.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(90, 50, 65);
            lblTitulo.AutoSize = true;
            panel.Controls.Add(lblTitulo);

            void Centrar()
            {
                lblTitulo.Left = (panel.Width - lblTitulo.Width) / 2;
                lblTitulo.Top = 22;
            }

            Centrar();
            panel.Resize += (s, e) => Centrar();

            return panel;
        }

        private Label CrearLabel(string texto, Point posicion)
        {
            Label label = new Label();
            label.Text = texto;
            label.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(80, 65, 70);
            label.Location = posicion;
            label.AutoSize = true;

            return label;
        }

        private TextBox CrearTextBox(Point posicion, Size tamanio)
        {
            TextBox textbox = new TextBox();
            textbox.Location = posicion;
            textbox.Size = tamanio;
            textbox.Font = new Font("Segoe UI", 10);
            textbox.TextAlign = HorizontalAlignment.Center;
            textbox.BorderStyle = BorderStyle.FixedSingle;

            return textbox;
        }

        private Button CrearBoton(string texto, Point posicion, Size tamanio)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            boton.Size = tamanio;
            boton.Location = posicion;
            boton.BackColor = Color.FromArgb(225, 120, 160);
            boton.ForeColor = Color.White;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Cursor = Cursors.Hand;

            return boton;
        }

        private TextBox CrearResultado(Panel panel, string nombre, string valor, ref int y)
        {
            Label lblNombre = CrearLabel(nombre, new Point(45, y + 4));
            panel.Controls.Add(lblNombre);

            TextBox txtValor = CrearTextBox(new Point(205, y), new Size(160, 30));
            txtValor.Text = valor;
            txtValor.ReadOnly = true;
            txtValor.BackColor = Color.FromArgb(255, 248, 250);
            panel.Controls.Add(txtValor);

            y += 44;

            return txtValor;
        }

        private DataGridView CrearTablaComparacion(Point posicion, Size tamanio)
        {
            DataGridView tabla = new DataGridView();
            tabla.Location = posicion;
            tabla.Size = tamanio;
            tabla.AllowUserToAddRows = false;
            tabla.AllowUserToDeleteRows = false;
            tabla.AllowUserToResizeColumns = false;
            tabla.AllowUserToResizeRows = false;
            tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            tabla.BackgroundColor = Color.FromArgb(255, 248, 250);
            tabla.BorderStyle = BorderStyle.FixedSingle;
            tabla.ColumnHeadersHeight = 30;
            tabla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            tabla.EnableHeadersVisualStyles = false;
            tabla.Font = new Font("Segoe UI", 9);
            tabla.GridColor = Color.FromArgb(220, 210, 215);
            tabla.MultiSelect = false;
            tabla.ReadOnly = true;
            tabla.RowHeadersVisible = false;
            tabla.RowTemplate.Height = 28;
            tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            tabla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(244, 180, 196);
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(80, 65, 70);
            tabla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            tabla.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            tabla.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 250);
            tabla.DefaultCellStyle.ForeColor = Color.FromArgb(80, 65, 70);
            tabla.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 210, 222);
            tabla.DefaultCellStyle.SelectionForeColor = Color.FromArgb(80, 65, 70);

            tabla.Columns.Add("Variable", "Variable");
            tabla.Columns.Add("Normal", "Normal");
            tabla.Columns.Add("Modificado", "Modificado");
            tabla.Columns[0].Width = 75;
            tabla.Columns[1].Width = 115;
            tabla.Columns[2].Width = 115;

            return tabla;
        }

        private int LeerDimension()
        {
            return (int)nudDimension.Value;
        }

        private double LeerDouble(string texto)
        {
            texto = texto.Trim().Replace(',', '.');
            return double.Parse(texto, CultureInfo.InvariantCulture);
        }

        private string FormatearNumero(double numero)
        {
            if (Math.Abs(numero) < 0.000000000001)
            {
                numero = 0;
            }

            return numero.ToString("0.##########", CultureInfo.InvariantCulture);
        }
    }
}
