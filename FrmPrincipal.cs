using System;
using System.Windows.Forms;

namespace SistemaReservasSalones
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();
            ConfigureButtons();
        }

        private void ConfigureButtons()
        {
            // Configuración visual de botones (opcional)
            btnPolideportivo.Tag = "POLIDEPORTIVO";
            btnSum.Tag = "SUM";
        }

        private void GestionarReservas_Click(object sender, EventArgs e)
        {
            OpenFormWithDispose<GestionReservasForm>();
        }

        private void RealizarReserva_Click(object sender, EventArgs e)
        {
            OpenFormWithDispose<ReservaForm>();
        }

        private void btnPolideportivo_Click(object sender, EventArgs e)
        {
            OpenFormWithDispose<ReservaForm>("POLIDEPORTIVO");
        }

        private void btnSum_Click(object sender, EventArgs e)
        {
            OpenFormWithDispose<ReservaForm>("SUM");
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            if (ConfirmExit())
            {
                Application.Exit();
            }
        }

        // Método genérico para abrir formularios
        private void OpenFormWithDispose<T>(string salon = null) where T : Form, new()
        {
            using (var form = salon == null ? new T() : (T)Activator.CreateInstance(typeof(T), salon))
            {
                form.ShowDialog();
            }
        }

        // Método para confirmar salida
        private bool ConfirmExit()
        {
            return MessageBox.Show("¿Está seguro que desea salir?", 
                                "Confirmar salida", 
                                MessageBoxButtons.YesNo, 
                                MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}