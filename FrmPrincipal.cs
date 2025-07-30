// FrmPrincipal.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaReservasSalones
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            // Puedes agregar aquí cualquier lógica de inicialización para el formulario principal.
        }

        private void GestionarReservas_Click(object sender, EventArgs e) // <<<< ESTE MÉTODO
        {
            GestionReservasForm gestionForm = new GestionReservasForm();
            gestionForm.ShowDialog();
        }

        // Método general para Realizar Reserva (si lo usas para algún otro botón)
        private void RealizarReserva_Click(object sender, EventArgs e)
        {
            using (var reservaForm = new ReservaForm())
            {
                reservaForm.ShowDialog();
            }
        }

        // NUEVO MÉTODO para el botón Polideportivo
        private void btnPolideportivo_Click(object sender, EventArgs e)
        {
            using (var reservaForm = new ReservaForm("POLIDEPORTIVO")) // Abre con "POLIDEPORTIVO" preseleccionado
            {
                reservaForm.ShowDialog();
            }
        }

        // NUEVO MÉTODO para el botón SUM
        private void btnSum_Click(object sender, EventArgs e)
        {
            using (var reservaForm = new ReservaForm("SUM")) // Abre con "SUM" preseleccionado
            {
                reservaForm.ShowDialog();
            }
        }

        private void btnSalir_Click(object sender, EventArgs e) // <<<< ESTE MÉTODO
        {
            Application.Exit(); // Cierra toda la aplicación
        }
    }
}