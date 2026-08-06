namespace AnalisisNumericoWeb
{
    public partial class FrmMenuPrincipal : Form
    {
        public FrmMenuPrincipal()
        {
            InitializeComponent();
            CrearInterfaz();

        }

        private void FrmMenuPrincipal_Load(object sender, EventArgs e)
        {

        }
        private void CrearInterfaz()
{
    // Elimina controles antiguos del diseñador
    this.Controls.Clear();

    // Configuración de la ventana
    this.Text = "Análisis Numérico";
    this.StartPosition = FormStartPosition.CenterScreen;
    this.WindowState = FormWindowState.Normal;
    this.Size = new Size(1000, 700);
    this.BackColor = Color.WhiteSmoke;

    // Título
    Label lblTitulo = new Label();

    lblTitulo.Text = "Análisis Numérico";
    lblTitulo.Font = new Font("Segoe UI", 26, FontStyle.Bold);
    lblTitulo.ForeColor = Color.FromArgb(40, 40, 40);
    lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
    lblTitulo.Dock = DockStyle.Top;
    lblTitulo.Height = 70;

    // Subtítulo
    Label lblSubtitulo = new Label();

    lblSubtitulo.Text = "Menú principal";
    lblSubtitulo.Font = new Font("Segoe UI", 14, FontStyle.Regular);
    lblSubtitulo.ForeColor = Color.DimGray;
    lblSubtitulo.TextAlign = ContentAlignment.MiddleCenter;
    lblSubtitulo.Dock = DockStyle.Top;
    lblSubtitulo.Height = 40;

    this.Controls.Add(lblSubtitulo);
    this.Controls.Add(lblTitulo);
}
    }
}
