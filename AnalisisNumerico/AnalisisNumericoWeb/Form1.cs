using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace AnalisisNumericoWeb
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CrearMenuPrincipal();
        }


        private void CrearMenuPrincipal()
        {
            // CONFIGURACIÓN GENERAL
            this.Text = "Análisis Numérico";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(255, 245, 248);


            // ENCABEZADO
            Panel encabezado = new Panel();

            encabezado.Dock = DockStyle.Top;
            encabezado.Height = 130;
            encabezado.BackColor = Color.FromArgb(244, 180, 196);

            this.Controls.Add(encabezado);


            // TÍTULO
            Label titulo = new Label();

            titulo.Text = "ANÁLISIS NUMÉRICO";

            titulo.Font = new Font(
                "Segoe UI",
                25,
                FontStyle.Bold
            );

            titulo.ForeColor = Color.FromArgb(90, 50, 65);
            titulo.AutoSize = true;

            encabezado.Controls.Add(titulo);


            // SUBTÍTULO
            Label subtitulo = new Label();

            subtitulo.Text = "Menú principal";

            subtitulo.Font = new Font(
                "Segoe UI",
                13
            );

            subtitulo.ForeColor = Color.FromArgb(110, 70, 85);
            subtitulo.AutoSize = true;

            encabezado.Controls.Add(subtitulo);


            // CENTRAR ENCABEZADO
            void CentrarEncabezado()
            {
                titulo.Left =
                    (encabezado.Width - titulo.Width) / 2;

                titulo.Top = 25;


                subtitulo.Left =
                    (encabezado.Width - subtitulo.Width) / 2;

                subtitulo.Top = 75;
            }


            CentrarEncabezado();


            encabezado.Resize += (s, e) =>
            {
                CentrarEncabezado();
            };


            // PANEL QUE CONTIENE LAS TARJETAS
            Panel panelTarjetas = new Panel();

            panelTarjetas.Size = new Size(900, 430);
            panelTarjetas.BackColor = Color.Transparent;

            this.Controls.Add(panelTarjetas);


            // TARJETA UNIDAD 1
            CrearTarjeta(
                "UNIDAD 1",
                "Raíces de funciones",
                "Métodos para encontrar raíces de ecuaciones.",
                new Point(10, 10),
                Color.FromArgb(244, 170, 190),
                panelTarjetas,
                1
            );


            // TARJETA UNIDAD 2
            CrearTarjeta(
                "UNIDAD 2",
                "Sistemas de ecuaciones",
                "Resolución de sistemas lineales.",
                new Point(460, 10),
                Color.FromArgb(235, 157, 185),
                panelTarjetas,
                2
            );


            // TARJETA UNIDAD 3
            CrearTarjeta(
                "UNIDAD 3",
                "Ajuste de curvas",
                "Interpolación y aproximación de funciones.",
                new Point(10, 225),
                Color.FromArgb(248, 185, 203),
                panelTarjetas,
                3
            );


            // TARJETA UNIDAD 4
            CrearTarjeta(
                "UNIDAD 4",
                "Integración numérica",
                "Métodos numéricos para calcular integrales.",
                new Point(460, 225),
                Color.FromArgb(229, 145, 177),
                panelTarjetas,
                4
            );


            // CENTRAR LAS TARJETAS EN LA PANTALLA
            void CentrarTarjetas()
            {
                panelTarjetas.Left =
                    (this.ClientSize.Width - panelTarjetas.Width) / 2;


                panelTarjetas.Top =
                    encabezado.Height +
                    ((this.ClientSize.Height
                    - encabezado.Height
                    - panelTarjetas.Height) / 2);
            }


            CentrarTarjetas();


            this.Resize += (s, e) =>
            {
                CentrarTarjetas();
            };
        }



        private void CrearTarjeta(
            string unidad,
            string tema,
            string descripcion,
            Point posicion,
            Color colorTarjeta,
            Panel panelTarjetas,
            int numeroUnidad)
        {
            // TARJETA
            Panel tarjeta = new Panel();

            tarjeta.Size = new Size(420, 190);
            tarjeta.Location = posicion;
            tarjeta.BackColor = Color.White;
            tarjeta.BorderStyle = BorderStyle.FixedSingle;
            tarjeta.Cursor = Cursors.Hand;

            panelTarjetas.Controls.Add(tarjeta);


            // PANEL PARA EL GRÁFICO
            Panel grafico = new Panel();

            grafico.Size = new Size(135, 110);
            if (numeroUnidad == 2)
{
    grafico.Location = new Point(285, 20);
}
else
{
    grafico.Location = new Point(270, 20);
}   
            grafico.BackColor = Color.Transparent;
            grafico.Cursor = Cursors.Hand;

            tarjeta.Controls.Add(grafico);


            // DIBUJAR GRÁFICO
            grafico.Paint += (s, e) =>
            {
                DibujarGrafico(
                    e.Graphics,
                    numeroUnidad,
                    grafico.ClientRectangle
                );
            };


            // UNIDAD
            Label lblUnidad = new Label();

            lblUnidad.Text = unidad;

            lblUnidad.Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Bold
            );

            lblUnidad.ForeColor =
                Color.FromArgb(210, 95, 135);

            lblUnidad.AutoSize = true;
            lblUnidad.Location = new Point(25, 28);
            lblUnidad.Cursor = Cursors.Hand;

            tarjeta.Controls.Add(lblUnidad);


            // TEMA
            Label lblTema = new Label();

            lblTema.Text = tema;

            // TAMAÑO DEL TÍTULO
            if (numeroUnidad == 2)
            {
                lblTema.Font = new Font(
                    "Segoe UI",
                    14,
                    FontStyle.Bold
                );

                lblTema.Size = new Size(260, 55);
            }
            else
            {
                lblTema.Font = new Font(
                    "Segoe UI",
                    16,
                    FontStyle.Bold
                );

                lblTema.Size = new Size(240, 50);
            }

            lblTema.ForeColor =
                Color.FromArgb(70, 55, 65);

            lblTema.Location = new Point(25, 60);
            lblTema.Cursor = Cursors.Hand;

            tarjeta.Controls.Add(lblTema);


            // DESCRIPCIÓN
            Label lblDescripcion = new Label();

            lblDescripcion.Text = descripcion;

            lblDescripcion.Font = new Font(
                "Segoe UI",
                12
            );

            lblDescripcion.ForeColor = Color.Gray;

            lblDescripcion.Location =
                new Point(27, 115);

            lblDescripcion.Size =
                new Size(230, 50);

            lblDescripcion.Cursor =
                Cursors.Hand;

            tarjeta.Controls.Add(lblDescripcion);



            // HOVER
            void ActivarHover()
            {
                tarjeta.BackColor = colorTarjeta;

                lblUnidad.ForeColor = Color.White;
                lblTema.ForeColor = Color.White;
                lblDescripcion.ForeColor = Color.White;
            }


            void DesactivarHover()
            {
                tarjeta.BackColor = Color.White;

                lblUnidad.ForeColor =
                    Color.FromArgb(210, 95, 135);

                lblTema.ForeColor =
                    Color.FromArgb(70, 55, 65);

                lblDescripcion.ForeColor =
                    Color.Gray;
            }



            // HOVER EN LA TARJETA
            tarjeta.MouseEnter += (s, e) =>
            {
                ActivarHover();
            };


            tarjeta.MouseLeave += (s, e) =>
            {
                DesactivarHover();
            };


            // HOVER SOBRE LOS ELEMENTOS
            lblUnidad.MouseEnter += (s, e) =>
            {
                ActivarHover();
            };


            lblTema.MouseEnter += (s, e) =>
            {
                ActivarHover();
            };


            lblDescripcion.MouseEnter += (s, e) =>
            {
                ActivarHover();
            };


            grafico.MouseEnter += (s, e) =>
            {
                ActivarHover();
            };



            // CLICK EN TODA LA TARJETA
            tarjeta.Click += (s, e) =>
            {
                AbrirUnidad(unidad);
            };


            lblUnidad.Click += (s, e) =>
            {
                AbrirUnidad(unidad);
            };


            lblTema.Click += (s, e) =>
            {
                AbrirUnidad(unidad);
            };


            lblDescripcion.Click += (s, e) =>
            {
                AbrirUnidad(unidad);
            };


            grafico.Click += (s, e) =>
            {
                AbrirUnidad(unidad);
            };
        }



        // DECIDE QUÉ GRÁFICO DIBUJAR
        private void DibujarGrafico(
            Graphics g,
            int unidad,
            Rectangle area)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen lapiz =
                new Pen(Color.FromArgb(90, 70, 80), 2))
            {
                switch (unidad)
                {
                    case 1:
                        DibujarRaices(g, lapiz, area);
                        break;

                    case 2:
                        DibujarSistema(g, lapiz, area);
                        break;

                    case 3:
                        DibujarAjuste(g, lapiz, area);
                        break;

                    case 4:
                        DibujarIntegracion(g, lapiz, area);
                        break;
                }
            }
        }



        // GRÁFICO UNIDAD 1
        private void DibujarRaices(
            Graphics g,
            Pen lapiz,
            Rectangle area)
        {
            int ejeY = area.Height / 2;


            // EJE X
            g.DrawLine(
                lapiz,
                10,
                ejeY,
                area.Width - 10,
                ejeY
            );


            // EJE Y
            g.DrawLine(
                lapiz,
                20,
                10,
                20,
                area.Height - 10
            );


            // CURVA
            Point[] curva =
            {
                new Point(20, 75),
                new Point(35, 35),
                new Point(50, 25),
                new Point(65, 55),
                new Point(80, 75),
                new Point(95, 40),
                new Point(110, 35),
                new Point(125, 70)
            };


            g.DrawCurve(
                lapiz,
                curva
            );


            // RAÍCES
            using (Brush rosa =
                new SolidBrush(
                    Color.FromArgb(220, 100, 145)))
            {
                g.FillEllipse(
                    rosa,
                    61,
                    ejeY - 3,
                    7,
                    7
                );


                g.FillEllipse(
                    rosa,
                    93,
                    ejeY - 3,
                    7,
                    7
                );
            }
        }



        // GRÁFICO UNIDAD 2
        private void DibujarSistema(
            Graphics g,
            Pen lapiz,
            Rectangle area)
        {
            using (Font fuente =
                new Font("Segoe UI", 8))
            {
                using (Brush pincel =
                    new SolidBrush(
                        Color.FromArgb(90, 70, 80)))
                {
                    g.DrawString(
                        "[ a11  a12  a1n ]",
                        fuente,
                        pincel,
                        5,
                        15
                    );


                    g.DrawString(
                        "[ a21  a22  a2n ]",
                        fuente,
                        pincel,
                        5,
                        38
                    );


                    g.DrawString(
                        "[ an1  an2  ann ]",
                        fuente,
                        pincel,
                        5,
                        61
                    );


                    g.DrawString(
                        "×",
                        new Font(
                            "Segoe UI",
                            13,
                            FontStyle.Bold),
                        pincel,
                        88,
                        38
                    );


                    g.DrawString(
                        "[ x ]",
                        new Font(
                            "Segoe UI",
                            10),
                        pincel,
                        105,
                        38
                    );
                }
            }
        }



        // GRÁFICO UNIDAD 3
        private void DibujarAjuste(
            Graphics g,
            Pen lapiz,
            Rectangle area)
        {
            // LÍNEA DE AJUSTE
            g.DrawLine(
                lapiz,
                15,
                area.Height - 15,
                area.Width - 10,
                20
            );


            using (Brush rosa =
                new SolidBrush(
                    Color.FromArgb(220, 100, 145)))
            {
                Point[] puntos =
                {
                    new Point(20, 85),
                    new Point(35, 75),
                    new Point(50, 72),
                    new Point(65, 58),
                    new Point(80, 62),
                    new Point(95, 42),
                    new Point(110, 45),
                    new Point(125, 25)
                };


                foreach (Point punto in puntos)
                {
                    g.FillEllipse(
                        rosa,
                        punto.X - 3,
                        punto.Y - 3,
                        6,
                        6
                    );
                }
            }
        }



        // GRÁFICO UNIDAD 4
        private void DibujarIntegracion(
            Graphics g,
            Pen lapiz,
            Rectangle area)
        {
            int baseY = area.Height - 20;


            // EJE X
            g.DrawLine(
                lapiz,
                10,
                baseY,
                area.Width - 10,
                baseY
            );


            // EJE Y
            g.DrawLine(
                lapiz,
                20,
                10,
                20,
                baseY
            );


            // CURVA
            Point[] curva =
            {
                new Point(20, 70),
                new Point(35, 35),
                new Point(50, 25),
                new Point(70, 45),
                new Point(90, 50),
                new Point(110, 25),
                new Point(125, 35)
            };


            g.DrawCurve(
                lapiz,
                curva
            );


            // LÍNEAS QUE SIMULAN EL ÁREA DE INTEGRACIÓN
            using (Pen rosa =
                new Pen(
                    Color.FromArgb(220, 120, 155),
                    1))
            {
                for (int x = 30; x <= 120; x += 15)
                {
                    g.DrawLine(
                        rosa,
                        x,
                        baseY,
                        x,
                        40
                    );
                }
            }
        }



        // POR AHORA SOLO MUESTRA QUÉ UNIDAD SE SELECCIONÓ
        private void AbrirUnidad(string unidad)
        {
            if (unidad == "UNIDAD 1")
            {
                Unidad1 paginaUnidad1 = new Unidad1();
                paginaUnidad1.ShowDialog();
            }
        }
    }
}