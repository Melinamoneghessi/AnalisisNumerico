using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LogicaAnalisis.Unidad2;

namespace AnalisisNumericoWeb.Unidad2
{
    public class Unidad2 : Form
    {
        private ComboBox cmbDimension;
        private ComboBox cmbMetodo;
        private Panel panelMatriz;
        private TextBox[,] txtMatriz;
        private TextBox txtResultadoMetodo;
        private TextBox txtResultadoDimension;
        private TextBox txtResultadoConverge;
        private TextBox[] txtSoluciones;
        private Label[] lblSoluciones;

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
            contenido.Size = new Size(1120, 470);
            contenido.BackColor = Color.Transparent;
            Controls.Add(contenido);

            Panel panelDatos = CrearPanelSeccion("Ingreso de datos", new Point(0, 0), new Size(665, 470));
            contenido.Controls.Add(panelDatos);

            Label lblDimension = CrearLabel("Dimension", new Point(45, 82));
            panelDatos.Controls.Add(lblDimension);

            cmbDimension = new ComboBox();
            cmbDimension.Location = new Point(150, 78);
            cmbDimension.Size = new Size(115, 32);
            cmbDimension.Font = new Font("Segoe UI", 10);
            cmbDimension.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDimension.Items.Add("3");
            cmbDimension.Items.Add("4");
            cmbDimension.SelectedIndex = 0;
            cmbDimension.SelectedIndexChanged += (s, e) => GenerarMatriz();
            panelDatos.Controls.Add(cmbDimension);

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

            Button btnGenerar = CrearBoton("Generar", new Point(440, 122), new Size(170, 35));
            btnGenerar.Click += (s, e) =>
            {
                GenerarMatriz();
                LimpiarResultados();
            };
            panelDatos.Controls.Add(btnGenerar);

            panelMatriz = new Panel();
            panelMatriz.Location = new Point(45, 185);
            panelMatriz.Size = new Size(565, 170);
            panelMatriz.BackColor = Color.Transparent;
            panelDatos.Controls.Add(panelMatriz);

            Button btnCalcular = CrearBoton("Calcular", new Point(180, 385), new Size(140, 38));
            btnCalcular.Click += (s, e) => Calcular();
            panelDatos.Controls.Add(btnCalcular);

            Button btnLimpiar = CrearBoton("Limpiar", new Point(345, 385), new Size(140, 38));
            btnLimpiar.Click += (s, e) => Limpiar();
            panelDatos.Controls.Add(btnLimpiar);

            Panel panelResultados = CrearPanelSeccion("Resultados", new Point(700, 0), new Size(420, 470));
            contenido.Controls.Add(panelResultados);

            int y = 82;
            txtResultadoMetodo = CrearResultado(panelResultados, "Metodo utilizado", "Gauss-Jordan", ref y);
            txtResultadoDimension = CrearResultado(panelResultados, "Dimension", "3 x 3", ref y);
            txtResultadoConverge = CrearResultado(panelResultados, "Converge?", "-", ref y);

            Label lblSolucion = CrearLabel("Solucion", new Point(45, y + 8));
            panelResultados.Controls.Add(lblSolucion);

            txtSoluciones = new TextBox[4];
            lblSoluciones = new Label[4];

            for (int i = 0; i < 4; i++)
            {
                lblSoluciones[i] = CrearLabel("x" + (i + 1) + " =", new Point(105, y + 48 + (i * 42)));
                panelResultados.Controls.Add(lblSoluciones[i]);

                txtSoluciones[i] = CrearTextBox(new Point(180, y + 43 + (i * 42)), new Size(145, 30));
                txtSoluciones[i].ReadOnly = true;
                txtSoluciones[i].BackColor = Color.FromArgb(255, 248, 250);
                panelResultados.Controls.Add(txtSoluciones[i]);
            }

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
            int dimension = LeerDimension();
            panelMatriz.Controls.Clear();
            txtMatriz = new TextBox[dimension, dimension + 1];

            int anchoCaja = dimension == 3 ? 78 : 66;
            int altoCaja = 30;
            int separacionX = dimension == 3 ? 96 : 78;
            int separacionY = 42;
            int inicioX = dimension == 3 ? 65 : 42;
            int inicioY = 34;
            int xTermino = inicioX + (dimension * separacionX) + 42;

            for (int columna = 0; columna < dimension; columna++)
            {
                Label label = CrearLabel("x" + (columna + 1), new Point(inicioX + (columna * separacionX) + 22, 0));
                panelMatriz.Controls.Add(label);
            }

            Label lblTi = CrearLabel("TI", new Point(xTermino + 25, 0));
            panelMatriz.Controls.Add(lblTi);

            for (int fila = 0; fila < dimension; fila++)
            {
                for (int columna = 0; columna < dimension + 1; columna++)
                {
                    int x = columna == dimension
                        ? xTermino
                        : inicioX + (columna * separacionX);

                    TextBox texto = CrearTextBox(new Point(x, inicioY + (fila * separacionY)), new Size(anchoCaja, altoCaja));
                    txtMatriz[fila, columna] = texto;
                    panelMatriz.Controls.Add(texto);

                    if (columna == dimension - 1)
                    {
                        Label igual = CrearLabel("=", new Point(x + anchoCaja + 20, inicioY + 4 + (fila * separacionY)));
                        panelMatriz.Controls.Add(igual);
                    }
                }
            }

            MostrarSolucionesSegunDimension(dimension);
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

            for (int i = 0; i < txtSoluciones.Length; i++)
            {
                txtSoluciones[i].Text = "";
            }

            for (int i = 0; i < resultado.VectorResultado.Length; i++)
            {
                txtSoluciones[i].Text = FormatearNumero(resultado.VectorResultado[i]);
            }
        }

        private void MostrarResultado(ResultadoGaussSeidel resultado)
        {
            txtResultadoMetodo.Text = resultado.Metodo;
            txtResultadoDimension.Text = resultado.Dimension + " x " + resultado.Dimension;
            txtResultadoConverge.Text = resultado.Converge ? "Si" : "No";

            for (int i = 0; i < txtSoluciones.Length; i++)
            {
                txtSoluciones[i].Text = "";
            }

            for (int i = 0; i < resultado.VectorResultado.Length; i++)
            {
                txtSoluciones[i].Text = FormatearNumero(resultado.VectorResultado[i]);
            }

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

            for (int i = 0; i < txtSoluciones.Length; i++)
            {
                txtSoluciones[i].Text = "";
            }
        }

        private void MostrarSolucionesSegunDimension(int dimension)
        {
            if (txtSoluciones == null || lblSoluciones == null)
            {
                return;
            }

            for (int i = 0; i < txtSoluciones.Length; i++)
            {
                bool visible = i < dimension;
                txtSoluciones[i].Visible = visible;
                lblSoluciones[i].Visible = visible;
            }
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

            TextBox txtValor = CrearTextBox(new Point(205, y), new Size(145, 30));
            txtValor.Text = valor;
            txtValor.ReadOnly = true;
            txtValor.BackColor = Color.FromArgb(255, 248, 250);
            panel.Controls.Add(txtValor);

            y += 44;

            return txtValor;
        }

        private int LeerDimension()
        {
            return int.Parse(cmbDimension.Text, CultureInfo.InvariantCulture);
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
