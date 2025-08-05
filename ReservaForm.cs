using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SistemaReservasSalones
{
    public partial class ReservaForm : Form
    {
        private readonly SQLiteDataAccess _db = new SQLiteDataAccess();
        private readonly string _salonSeleccionado;
        private readonly string _numeroNotaOriginal;
        private readonly bool _modoEdicion;

        // Constructor para nueva reserva
        public ReservaForm(string salon) : this()
        {
            _salonSeleccionado = salon;
            lblSalon.Text = salon;
            Text = "Nueva Reserva - " + salon;
        }

        // Constructor para edición
        public ReservaForm(Reservation reserva) : this()
        {
            _modoEdicion = true;
            _numeroNotaOriginal = reserva.NumeroNota;
            _salonSeleccionado = reserva.Salon;

            Text = "Editar Reserva - " + reserva.NumeroNota;
            CargarDatosReserva(reserva);
            ConfigurarModoEdicion();
        }

        // Constructor base
        private ReservaForm()
        {
            InitializeComponent();
            dtpFecha.MinDate = DateTime.Today;
            ConfigurarControles();
        }

        private void ConfigurarControles()
        {
            // Configuración inicial común
            dtpFecha.Value = DateTime.Today;
            txtHoraInicio.Text = "08:00";
            txtHoraFin.Text = "09:00";
            btnGuardar.Text = _modoEdicion ? "Actualizar" : "Guardar";
        }

        private void CargarDatosReserva(Reservation reserva)
        {
            lblSalon.Text = reserva.Salon;
            txtNumeroNota.Text = reserva.NumeroNota;
            dtpFecha.Value = DateTime.Parse(reserva.Fecha);
            txtHoraInicio.Text = reserva.HoraInicio;
            txtHoraFin.Text = reserva.HoraFin;
            txtSolicitante.Text = reserva.Solicitante;
            txtContacto.Text = reserva.Contacto;
            txtMotivo.Text = reserva.Motivo;
        }

        private void ConfigurarModoEdicion()
        {
            txtNumeroNota.Enabled = false;
            dtpFecha.Enabled = false;
        }

        private void dtpFecha_ValueChanged(object sender, EventArgs e)
        {
            ActualizarHorariosDisponibles();
        }

        private void ActualizarHorariosDisponibles()
        {
            try
            {
                lstHorarios.Items.Clear();
                string fecha = dtpFecha.Value.ToString("yyyy-MM-dd");

                // Generar horarios de 00:00 a 23:00
                for (int hora = 0; hora < 24; hora++)
                {
                    string inicio = $"{hora:00}:00";
                    string fin = $"{(hora + 1) % 24:00}:00";
                    string horario = $"{inicio} - {fin}";

                    bool disponible = _db.IsTimeSlotAvailable(
                        _salonSeleccionado,
                        fecha,
                        inicio,
                        fin,
                        _modoEdicion ? _numeroNotaOriginal : null
                    );

                    lstHorarios.Items.Add(disponible ? horario : $"{horario} [OCUPADO]");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar horarios: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lstHorarios_DoubleClick(object sender, EventArgs e)
        {
            if (lstHorarios.SelectedItem == null) return;

            string horario = lstHorarios.SelectedItem.ToString();
            if (horario.Contains("[OCUPADO]"))
            {
                MessageBox.Show("Este horario no está disponible", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] partes = horario.Split(new[] { " - " }, StringSplitOptions.None);
            txtHoraInicio.Text = partes[0];
            txtHoraFin.Text = partes[1];
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            var reserva = new Reservation
            {
                NumeroNota = txtNumeroNota.Text,
                Salon = _salonSeleccionado,
                Fecha = dtpFecha.Value.ToString("yyyy-MM-dd"),
                HoraInicio = txtHoraInicio.Text,
                HoraFin = txtHoraFin.Text,
                Solicitante = txtSolicitante.Text,
                Contacto = txtContacto.Text,
                Motivo = txtMotivo.Text
            };

            bool resultado;
            string mensaje;

            if (_modoEdicion)
            {
                resultado = _db.UpdateReservation(_numeroNotaOriginal, reserva);
                mensaje = resultado ? "Reserva actualizada con éxito" : "Error al actualizar";
            }
            else
            {
                resultado = _db.CreateReservation(reserva);
                mensaje = resultado ? "Reserva creada con éxito" : "Error al crear";
            }

            if (resultado)
            {
                MessageBox.Show(mensaje, "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(mensaje, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarFormulario()
        {
            // Validación de campos obligatorios
            if (string.IsNullOrWhiteSpace(txtNumeroNota.Text))
            {
                MostrarError("El número de nota es requerido", txtNumeroNota);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSolicitante.Text))
            {
                MostrarError("El solicitante es requerido", txtSolicitante);
                return false;
            }

            // Validación de formato de horas
            if (!TimeSpan.TryParse(txtHoraInicio.Text, out TimeSpan inicio) ||
                !TimeSpan.TryParse(txtHoraFin.Text, out TimeSpan fin))
            {
                MostrarError("Formato de hora inválido (use HH:mm)", txtHoraInicio);
                return false;
            }

            // Validación de rango horario
            if (fin <= inicio)
            {
                MostrarError("La hora final debe ser posterior a la inicial", txtHoraFin);
                return false;
            }

            return true;
        }

        private void MostrarError(string mensaje, Control control)
        {
            MessageBox.Show(mensaje, "Error de validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            control.Focus();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ReservaForm_Load(object sender, EventArgs e)
        {
            ActualizarHorariosDisponibles();
        }
    }
}