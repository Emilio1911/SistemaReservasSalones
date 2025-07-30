// ReservaForm.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SistemaReservasSalones
{
    public partial class ReservaForm : Form
    {
        private SQLiteDataAccess _db = new SQLiteDataAccess();
        private List<string> _horariosDisponibles = new List<string>();
        private string _salonSeleccionado;
        private string _numeroNotaActual; // Para el modo edición

        // Constructor para nuevas reservas (recibe el tipo de salón)
        public ReservaForm(string initialSalonType)
        {
            InitializeComponent();
            _salonSeleccionado = initialSalonType;
            lblLugar.Text = _salonSeleccionado; // Muestra el salón seleccionado
            dtpFecha.MinDate = DateTime.Today; // No permitir reservas en fechas pasadas
            UpdateHorariosList(); // Actualiza los horarios para el salón y fecha inicial
        }

        // Constructor para editar una reserva existente
        public ReservaForm(Reservation reservationToEdit)
        {
            InitializeComponent();
            _numeroNotaActual = reservationToEdit.NumeroNota; // Guarda el NumeroNota original para la actualización
            _salonSeleccionado = reservationToEdit.Salon; // Establece el salón desde la reserva a editar
            lblLugar.Text = _salonSeleccionado; // Muestra el salón de la reserva a editar

            // Cargar datos de la reserva existente en los controles
            txtNumeroNota.Text = reservationToEdit.NumeroNota;
            //cmbSalon.SelectedItem = reservationToEdit.Salon; // Esto ya no es necesario si cmbSalon se elimina/deshabilita
            dtpFecha.Value = DateTime.Parse(reservationToEdit.Fecha);
            txtHoraInicio.Text = reservationToEdit.HoraInicio;
            txtHoraFin.Text = reservationToEdit.HoraFin;
            txtSolicitante.Text = reservationToEdit.Solicitante;
            txtContacto.Text = reservationToEdit.Contacto;
            txtMotivo.Text = reservationToEdit.Motivo;

            // Bloquear campos que no deben modificarse al editar
            txtNumeroNota.Enabled = false;
            //cmbSalon.Enabled = false; // Esto también se podría deshabilitar si no se elimina
            dtpFecha.Enabled = false; // Normalmente la fecha y hora no se cambian en edición simple, se cancela y se crea nueva
            txtHoraInicio.Enabled = false;
            txtHoraFin.Enabled = false;

            // Actualizar horarios para reflejar la disponibilidad (excluyendo la reserva actual si es el caso)
            UpdateHorariosList(reservationToEdit.Id);
        }

        private void dtpFecha_ValueChanged(object sender, EventArgs e)
        {
            UpdateHorariosList();
        }

        private void UpdateHorariosList(int? reservationIdToExclude = null)
        {
            lstHorarios.Items.Clear();
            _horariosDisponibles = new List<string>();

            // Generar franjas horarias de 8:00 a 22:00
            for (int i = 8; i < 22; i++)
            {
                _horariosDisponibles.Add($"{i:00}:00 - {i + 1:00}:00");
            }

            // Obtener las reservas para el salón y la fecha seleccionados
            List<Reservation> reservasOcupadas = _db.LoadReservationsForSalonAndDate(_salonSeleccionado, dtpFecha.Value.ToShortDateString());

            foreach (string horario in _horariosDisponibles)
            {
                bool ocupado = false;
                foreach (Reservation r in reservasOcupadas)
                {
                    // Excluir la reserva que estamos editando si se especificó
                    if (reservationIdToExclude.HasValue && r.Id == reservationIdToExclude.Value)
                    {
                        continue;
                    }

                    // Verificar si el horario se superpone con una reserva existente
                    DateTime horaInicioActual = DateTime.ParseExact(horario.Substring(0, 5), "HH:mm", null);
                    DateTime horaFinActual = DateTime.ParseExact(horario.Substring(8, 5), "HH:mm", null);
                    DateTime reservaInicio = DateTime.ParseExact(r.HoraInicio, "HH:mm", null);
                    DateTime reservaFin = DateTime.ParseExact(r.HoraFin, "HH:mm", null);

                    if (!(horaFinActual <= reservaInicio || horaInicioActual >= reservaFin))
                    {
                        ocupado = true;
                        break;
                    }
                }

                if (ocupado)
                {
                    lstHorarios.Items.Add($"{horario} - OCUPADO");
                    lstHorarios.Items.Insert(lstHorarios.Items.Count - 1, $"{horario} - OCUPADO"); // Inserta en la posición correcta para mantener el orden
                    lstHorarios.Items.RemoveAt(lstHorarios.Items.Count - 1); // Remueve el duplicado al final
                }
                else
                {
                    lstHorarios.Items.Add(horario);
                }
            }
            // Ordenar los elementos del ListBox si es necesario (ej. si se insertó un ocupado desordenado)
            var sortedList = lstHorarios.Items.Cast<string>().OrderBy(s => s).ToList();
            lstHorarios.Items.Clear();
            foreach (var item in sortedList)
            {
                lstHorarios.Items.Add(item);
            }
        }


        private void lstHorarios_DoubleClick(object sender, EventArgs e)
        {
            if (lstHorarios.SelectedItem != null)
            {
                string selectedText = lstHorarios.SelectedItem.ToString();
                if (!selectedText.Contains("OCUPADO"))
                {
                    string[] parts = selectedText.Split(new string[] { " - " }, StringSplitOptions.None);
                    txtHoraInicio.Text = parts[0];
                    txtHoraFin.Text = parts[1];
                }
                else
                {
                    MessageBox.Show("Este horario ya está ocupado. Por favor, elija otro.", "Horario Ocupado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNumeroNota.Text) ||
                string.IsNullOrWhiteSpace(txtHoraInicio.Text) ||
                string.IsNullOrWhiteSpace(txtHoraFin.Text) ||
                string.IsNullOrWhiteSpace(txtSolicitante.Text) ||
                string.IsNullOrWhiteSpace(txtContacto.Text) ||
                string.IsNullOrWhiteSpace(txtMotivo.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar formato de hora
            if (!TimeSpan.TryParse(txtHoraInicio.Text, out TimeSpan horaInicio) ||
                !TimeSpan.TryParse(txtHoraFin.Text, out TimeSpan horaFin))
            {
                MessageBox.Show("El formato de la hora debe ser HH:mm (ej. 08:00).", "Formato de Hora Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar que HoraFin sea posterior a HoraInicio
            if (horaFin <= horaInicio)
            {
                MessageBox.Show("La hora de fin debe ser posterior a la hora de inicio.", "Error de Horario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Reservation newReservation = new Reservation
            {
                NumeroNota = txtNumeroNota.Text,
                Salon = _salonSeleccionado, // Usa el salón predefinido
                Fecha = dtpFecha.Value.ToShortDateString(),
                HoraInicio = txtHoraInicio.Text,
                HoraFin = txtHoraFin.Text,
                Solicitante = txtSolicitante.Text,
                Contacto = txtContacto.Text,
                Motivo = txtMotivo.Text,
                FechaRegistro = DateTime.Now.ToShortDateString()
            };

            if (_numeroNotaActual == null) // Es una nueva reserva
            {
                // Verificar superposición para nuevas reservas
                if (_db.IsReservationTimeSlotBooked(_salonSeleccionado, newReservation.Fecha, newReservation.HoraInicio, newReservation.HoraFin))
                {
                    MessageBox.Show("El horario seleccionado ya está ocupado para este salón y fecha. Por favor, elija otro.", "Horario Ocupado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _db.SaveReservation(newReservation);
                MessageBox.Show("Reserva guardada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else // Es una edición (actualización)
            {
                // No se necesita verificar superposición aquí si la fecha y horas están deshabilitadas para edición simple
                _db.UpdateReservation(_numeroNotaActual, newReservation);
                MessageBox.Show("Reserva actualizada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}