using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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
            Text = "Análisis Numérico";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 650);
            Size = new Size(1120, 730);
            BackColor = Color.FromArgb(236, 243, 252);

            Controls.Clear();

            Panel encabezado = new Panel
            {
                Dock = DockStyle.Top,
                Height = 155
            };
            encabezado.Paint += DibujarEncabezado;

            Label lblMateria = new Label
            {
                Text = "ANÁLISIS NUMÉRICO",
                Dock = DockStyle.Top,
                Height = 82,
                Padding = new Padding(0, 22, 0, 0),
                TextAlign = ContentAlignment.BottomCenter,
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            Label lblSubtitulo = new Label
            {
                Text = "Seleccioná una unidad para comenzar",
                Dock = DockStyle.Top,
                Height = 42,
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(30, 64, 175)
            };

            encabezado.Controls.Add(lblSubtitulo);
            encabezado.Controls.Add(lblMateria);

            TableLayoutPanel contenedor = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(236, 243, 252),
                Padding = new Padding(110, 38, 110, 50),
                ColumnCount = 2,
                RowCount = 2
            };

            contenedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            contenedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            contenedor.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            contenedor.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            contenedor.Controls.Add(CrearTarjeta("Unidad 1", "Raíces de funciones", "Bisección, regla falsa y métodos iterativos", TipoDibujo.Raices, Color.FromArgb(37, 99, 235)), 0, 0);
            contenedor.Controls.Add(CrearTarjeta("Unidad 2", "Sistemas de ecuaciones", "Resolución de sistemas lineales", TipoDibujo.Sistemas, Color.FromArgb(124, 58, 237)), 1, 0);
            contenedor.Controls.Add(CrearTarjeta("Unidad 3", "Ajuste de curvas", "Interpolación, regresión y aproximación", TipoDibujo.Ajuste, Color.FromArgb(5, 150, 105)), 0, 1);
            contenedor.Controls.Add(CrearTarjeta("Unidad 4", "Integración numérica", "Trapecios, Simpson y estimación de áreas", TipoDibujo.Integracion, Color.FromArgb(234, 88, 12)), 1, 1);

            Controls.Add(contenedor);
            Controls.Add(encabezado);
        }

        private Control CrearTarjeta(string unidad, string titulo, string detalle, TipoDibujo tipoDibujo, Color colorAcento)
        {
            Panel margen = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18)
            };

            TarjetaUnidad tarjeta = new TarjetaUnidad(tipoDibujo, colorAcento)
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = unidad
            };

            Label lblUnidad = new Label
            {
                Text = unidad.ToUpper(),
                Dock = DockStyle.Top,
                Height = 42,
                Padding = new Padding(24, 12, 24, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = colorAcento,
                Cursor = Cursors.Hand
            };

            Label lblTitulo = new Label
            {
                Text = titulo,
                Dock = DockStyle.Bottom,
                Height = 34,
                Padding = new Padding(24, 0, 24, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Cursor = Cursors.Hand
            };

            Label lblDetalle = new Label
            {
                Text = detalle,
                Dock = DockStyle.Bottom,
                Height = 30,
                Padding = new Padding(24, 0, 24, 8),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Cursor = Cursors.Hand
            };

            Label lblAccion = new Label
            {
                Text = "Abrir unidad  →",
                Dock = DockStyle.Bottom,
                Height = 30,
                Padding = new Padding(24, 0, 24, 12),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = colorAcento,
                Cursor = Cursors.Hand
            };

            tarjeta.Click += AbrirUnidad;
            lblUnidad.Click += AbrirUnidad;
            lblTitulo.Click += AbrirUnidad;
            lblDetalle.Click += AbrirUnidad;
            lblAccion.Click += AbrirUnidad;

            tarjeta.MouseEnter += ActivarHoverTarjeta;
            tarjeta.MouseLeave += DesactivarHoverTarjeta;
            lblUnidad.MouseEnter += ActivarHoverTarjeta;
            lblUnidad.MouseLeave += DesactivarHoverTarjeta;
            lblTitulo.MouseEnter += ActivarHoverTarjeta;
            lblTitulo.MouseLeave += DesactivarHoverTarjeta;
            lblDetalle.MouseEnter += ActivarHoverTarjeta;
            lblDetalle.MouseLeave += DesactivarHoverTarjeta;
            lblAccion.MouseEnter += ActivarHoverTarjeta;
            lblAccion.MouseLeave += DesactivarHoverTarjeta;

            tarjeta.Controls.Add(lblAccion);
            tarjeta.Controls.Add(lblDetalle);
            tarjeta.Controls.Add(lblTitulo);
            tarjeta.Controls.Add(lblUnidad);
            margen.Controls.Add(tarjeta);

            return margen;
        }

        private void ActivarHoverTarjeta(object sender, EventArgs e)
        {
            TarjetaUnidad tarjeta = ObtenerTarjeta(sender as Control);

            if (tarjeta != null)
            {
                tarjeta.EstaSeleccionada = true;
            }
        }

        private void DesactivarHoverTarjeta(object sender, EventArgs e)
        {
            TarjetaUnidad tarjeta = ObtenerTarjeta(sender as Control);

            if (tarjeta != null && !tarjeta.ClientRectangle.Contains(tarjeta.PointToClient(Cursor.Position)))
            {
                tarjeta.EstaSeleccionada = false;
            }
        }

        private TarjetaUnidad ObtenerTarjeta(Control control)
        {
            while (control != null && !(control is TarjetaUnidad))
            {
                control = control.Parent;
            }

            return control as TarjetaUnidad;
        }

        private void AbrirUnidad(object sender, EventArgs e)
        {
            TarjetaUnidad tarjeta = ObtenerTarjeta(sender as Control);
            string unidad = tarjeta != null && tarjeta.Tag != null ? tarjeta.Tag.ToString() : "Unidad";

            MessageBox.Show(
                "Acá se abriría la pantalla de " + unidad + ".",
                "Análisis Numérico",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void DibujarEncabezado(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;

            using (LinearGradientBrush fondo = new LinearGradientBrush(
                panel.ClientRectangle,
                Color.FromArgb(219, 234, 254),
                Color.FromArgb(191, 219, 254),
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(fondo, panel.ClientRectangle);
            }

            using (Brush brillo = new SolidBrush(Color.FromArgb(90, 255, 255, 255)))
            {
                e.Graphics.FillEllipse(brillo, -80, -120, 260, 260);
                e.Graphics.FillEllipse(brillo, panel.Width - 170, 35, 260, 260);
            }

            using (Pen linea = new Pen(Color.FromArgb(147, 197, 253), 1))
            {
                e.Graphics.DrawLine(linea, 0, panel.Height - 1, panel.Width, panel.Height - 1);
            }
        }

        private enum TipoDibujo
        {
            Raices,
            Sistemas,
            Ajuste,
            Integracion
        }

        private class TarjetaUnidad : Panel
        {
            private readonly TipoDibujo tipoDibujo;
            private readonly Color colorAcento;
            private bool estaSeleccionada;

            public bool EstaSeleccionada
            {
                get { return estaSeleccionada; }
                set
                {
                    if (estaSeleccionada == value)
                    {
                        return;
                    }

                    estaSeleccionada = value;
                    Invalidate();
                }
            }

            public TarjetaUnidad(TipoDibujo tipoDibujo, Color colorAcento)
            {
                this.tipoDibujo = tipoDibujo;
                this.colorAcento = colorAcento;
                DoubleBuffered = true;
                Margin = new Padding(10);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle tarjeta = new Rectangle(0, 0, Width - 1, Height - 1);
                Rectangle sombra = new Rectangle(4, 6, Width - 8, Height - 8);

                using (GraphicsPath sombraPath = CrearRectanguloRedondeado(sombra, 22))
                using (Brush sombraBrush = new SolidBrush(estaSeleccionada ? Color.FromArgb(42, colorAcento) : Color.FromArgb(18, 15, 23, 42)))
                {
                    e.Graphics.FillPath(sombraBrush, sombraPath);
                }

                using (GraphicsPath tarjetaPath = CrearRectanguloRedondeado(tarjeta, 22))
                using (Brush fondo = new SolidBrush(estaSeleccionada ? Color.FromArgb(250, 252, 255) : Color.White))
                using (Pen borde = new Pen(estaSeleccionada ? colorAcento : Color.FromArgb(226, 232, 240), estaSeleccionada ? 2 : 1))
                {
                    e.Graphics.FillPath(fondo, tarjetaPath);
                    e.Graphics.DrawPath(borde, tarjetaPath);
                }

                using (Brush acento = new SolidBrush(colorAcento))
                {
                    e.Graphics.FillRectangle(acento, 0, 0, Width, estaSeleccionada ? 11 : 7);
                }

                if (estaSeleccionada)
                {
                    using (Brush luz = new SolidBrush(ColorSuave(colorAcento)))
                    {
                        e.Graphics.FillEllipse(luz, Width - 110, 24, 82, 82);
                    }
                }

                Rectangle areaDibujo = new Rectangle(54, 54, Width - 108, Math.Min(82, Math.Max(54, Height - 172)));

                switch (tipoDibujo)
                {
                    case TipoDibujo.Raices:
                        DibujarRaices(e.Graphics, areaDibujo, colorAcento);
                        break;
                    case TipoDibujo.Sistemas:
                        DibujarSistemas(e.Graphics, areaDibujo, colorAcento);
                        break;
                    case TipoDibujo.Ajuste:
                        DibujarAjuste(e.Graphics, areaDibujo, colorAcento);
                        break;
                    case TipoDibujo.Integracion:
                        DibujarIntegracion(e.Graphics, areaDibujo, colorAcento);
                        break;
                }
            }

            private static GraphicsPath CrearRectanguloRedondeado(Rectangle rectangulo, int radio)
            {
                GraphicsPath path = new GraphicsPath();
                int diametro = radio * 2;

                path.AddArc(rectangulo.X, rectangulo.Y, diametro, diametro, 180, 90);
                path.AddArc(rectangulo.Right - diametro, rectangulo.Y, diametro, diametro, 270, 90);
                path.AddArc(rectangulo.Right - diametro, rectangulo.Bottom - diametro, diametro, diametro, 0, 90);
                path.AddArc(rectangulo.X, rectangulo.Bottom - diametro, diametro, diametro, 90, 90);
                path.CloseFigure();

                return path;
            }

            private static Color ColorSuave(Color color)
            {
                return Color.FromArgb(34, color.R, color.G, color.B);
            }

            private static void DibujarEjes(Graphics g, Rectangle area, Color colorAcento)
            {
                using (Pen grilla = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    for (int x = area.Left + 35; x < area.Right; x += 42)
                    {
                        g.DrawLine(grilla, x, area.Top, x, area.Bottom);
                    }

                    for (int y = area.Top + 18; y < area.Bottom; y += 22)
                    {
                        g.DrawLine(grilla, area.Left, y, area.Right, y);
                    }
                }

                int ejeX = area.Bottom - 16;
                int ejeY = area.Left + 28;

                using (Pen eje = new Pen(Color.FromArgb(100, 116, 139), 2))
                {
                    g.DrawLine(eje, area.Left, ejeX, area.Right, ejeX);
                    g.DrawLine(eje, ejeY, area.Top, ejeY, area.Bottom);
                }

                using (Brush punto = new SolidBrush(colorAcento))
                {
                    g.FillEllipse(punto, ejeY - 5, ejeX - 5, 10, 10);
                }
            }

            private static void DibujarRaices(Graphics g, Rectangle area, Color colorAcento)
            {
                DibujarEjes(g, area, colorAcento);

                int baseY = area.Bottom - 16;
                int alto = Math.Max(32, area.Height - 22);

                Point[] puntos =
                {
                    new Point(area.Left + 35, baseY),
                    new Point(area.Left + area.Width * 18 / 100, baseY - alto),
                    new Point(area.Left + area.Width * 34 / 100, baseY),
                    new Point(area.Left + area.Width * 48 / 100, baseY - alto * 7 / 10),
                    new Point(area.Left + area.Width * 62 / 100, baseY),
                    new Point(area.Left + area.Width * 76 / 100, baseY - alto / 2),
                    new Point(area.Right - 18, baseY - 8)
                };

                using (Pen curva = new Pen(colorAcento, 4))
                {
                    g.DrawCurve(curva, puntos);
                }

                using (Brush fondoPunto = new SolidBrush(Color.White))
                using (Pen bordePunto = new Pen(colorAcento, 3))
                {
                    foreach (Point punto in new[] { puntos[0], puntos[2], puntos[4] })
                    {
                        g.FillEllipse(fondoPunto, punto.X - 6, baseY - 6, 12, 12);
                        g.DrawEllipse(bordePunto, punto.X - 6, baseY - 6, 12, 12);
                    }
                }
            }

            private static void DibujarSistemas(Graphics g, Rectangle area, Color colorAcento)
            {
                Rectangle caja = new Rectangle(area.Left + 10, area.Top + 2, area.Width - 20, area.Height - 4);

                using (GraphicsPath path = CrearRectanguloRedondeado(caja, 16))
                using (Brush fondo = new SolidBrush(ColorSuave(colorAcento)))
                {
                    g.FillPath(fondo, path);
                }

                using (Font fuente = new Font("Consolas", 11, FontStyle.Bold))
                using (Brush texto = new SolidBrush(Color.FromArgb(30, 41, 59)))
                {
                    string matriz = "A · X = B\n" +
                                    "[ 3  -1   2 ] [x]   [ 7]\n" +
                                    "[ 1   4  -2 ] [y] = [-1]\n" +
                                    "[ 2   1   5 ] [z]   [12]";

                    g.DrawString(matriz, fuente, texto, caja.Left + 24, caja.Top + 12);
                }
            }

            private static void DibujarAjuste(Graphics g, Rectangle area, Color colorAcento)
            {
                DibujarEjes(g, area, colorAcento);

                int baseY = area.Bottom - 16;
                int alto = Math.Max(32, area.Height - 22);

                using (Pen curva = new Pen(colorAcento, 3))
                {
                    curva.DashStyle = DashStyle.Dash;
                    g.DrawCurve(curva, new[]
                    {
                        new Point(area.Left + 35, baseY - 2),
                        new Point(area.Left + area.Width * 26 / 100, baseY - alto / 2),
                        new Point(area.Left + area.Width * 44 / 100, baseY - alto * 6 / 10),
                        new Point(area.Left + area.Width * 58 / 100, baseY - alto / 3),
                        new Point(area.Right - 20, area.Top + 8)
                    });
                }

                using (Brush blanco = new SolidBrush(Color.White))
                using (Pen borde = new Pen(colorAcento, 3))
                {
                    Point[] puntos =
                    {
                        new Point(area.Left + 42, baseY - alto / 4),
                        new Point(area.Left + area.Width * 18 / 100, baseY - 6),
                        new Point(area.Left + area.Width * 28 / 100, baseY - alto / 2),
                        new Point(area.Left + area.Width * 38 / 100, baseY - 4),
                        new Point(area.Left + area.Width * 48 / 100, baseY - alto * 6 / 10),
                        new Point(area.Left + area.Width * 58 / 100, baseY - alto / 3),
                        new Point(area.Left + area.Width * 70 / 100, baseY - alto * 7 / 10),
                        new Point(area.Right - 40, area.Top + 8)
                    };

                    foreach (Point punto in puntos)
                    {
                        g.FillEllipse(blanco, punto.X - 5, punto.Y - 5, 10, 10);
                        g.DrawEllipse(borde, punto.X - 5, punto.Y - 5, 10, 10);
                    }
                }
            }

            private static void DibujarIntegracion(Graphics g, Rectangle area, Color colorAcento)
            {
                DibujarEjes(g, area, colorAcento);

                int baseY = area.Bottom - 16;
                int altoMaximo = Math.Max(28, area.Height - 22);

                using (Pen barras = new Pen(Color.FromArgb(148, 163, 184), 2))
                using (Brush relleno = new SolidBrush(ColorSuave(colorAcento)))
                {
                    for (int x = area.Left + 50; x <= area.Right - 40; x += 28)
                    {
                        int alto = altoMaximo / 2 + (int)(altoMaximo / 4 * Math.Sin((x - area.Left) / 25.0));
                        Rectangle barra = new Rectangle(x - 8, baseY - alto, 16, alto);
                        g.FillRectangle(relleno, barra);
                        g.DrawLine(barras, x, baseY, x, baseY - alto);
                    }
                }

                using (Pen curva = new Pen(colorAcento, 4))
                {
                    g.DrawCurve(curva, new[]
                    {
                        new Point(area.Left + 35, baseY - altoMaximo / 3),
                        new Point(area.Left + area.Width * 24 / 100, baseY - altoMaximo),
                        new Point(area.Left + area.Width * 42 / 100, baseY - altoMaximo / 2),
                        new Point(area.Left + area.Width * 60 / 100, baseY - altoMaximo * 9 / 10),
                        new Point(area.Right - 25, baseY - altoMaximo * 2 / 3)
                    });
                }

                using (Font fuente = new Font("Segoe UI", 11, FontStyle.Bold))
                using (Brush texto = new SolidBrush(colorAcento))
                {
                    g.DrawString("a", fuente, texto, area.Left + 42, baseY + 5);
                    g.DrawString("b", fuente, texto, area.Right - 35, baseY + 5);
                }
            }
        }
    }
}
