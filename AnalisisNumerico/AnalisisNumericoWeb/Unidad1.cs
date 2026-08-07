using System;
using System.Drawing;
using System.Windows.Forms;

namespace AnalisisNumericoWeb
{
    public partial class Unidad1 : Form
    {
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
                new Size(530, 450);

            areaGrafico.BackColor =
                Color.White;

            areaGrafico.BorderStyle =
                BorderStyle.FixedSingle;

            panelGrafico.Controls.Add(areaGrafico);


            // TEXTO TEMPORAL

            Label lblGrafico = new Label();

            lblGrafico.Text =
                "El gráfico de la función\naparecerá aquí";

            lblGrafico.Font =
                new Font("Segoe UI", 14);

            lblGrafico.ForeColor =
                Color.FromArgb(170, 130, 145);

            lblGrafico.TextAlign =
                ContentAlignment.MiddleCenter;

            lblGrafico.Dock =
                DockStyle.Fill;

            areaGrafico.Controls.Add(lblGrafico);


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

            CrearResultado(
                panelResultados,
                "Función utilizada",
                "-",
                ref y
            );


            // MÉTODO

            CrearResultado(
                panelResultados,
                "Método utilizado",
                "-",
                ref y
            );


            // ITERACIONES

            CrearResultado(
                panelResultados,
                "Iteraciones",
                "-",
                ref y
            );


            // TOLERANCIA

            CrearResultado(
                panelResultados,
                "Tolerancia",
                "-",
                ref y
            );


            // INTERVALO

            CrearResultado(
                panelResultados,
                "Intervalo",
                "-",
                ref y
            );


            // CONVERGENCIA

            CrearResultado(
                panelResultados,
                "¿Converge?",
                "-",
                ref y
            );


            // RAÍZ

            CrearResultado(
                panelResultados,
                "Raíz",
                "-",
                ref y
            );


            // ERROR

            CrearResultado(
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


            // POR AHORA EL BOTÓN NO CALCULA
            btnCalcular.Click += (s, e) =>
            {
                MessageBox.Show(
                    "La interfaz ya está lista.\n" +
                    "Después conectaremos este botón con calculus.dll.",
                    "Calcular",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
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

        private void CrearResultado(
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
        }
    }
}