// GestionReservasForm.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SistemaReservasSalones;

namespace SistemaReservasSalones
{
    public partial class GestionReservasForm : Form
    {
        private SQLiteDataAccess _db;
        private List<Reservation> _allReservations;

        public GestionReservasForm()
        {
             InitializeComponent();
            _db = new SQLiteDataAccess();
            _allReservations = new List<Reservation>();

            // Configuración inicial del DateTimePicker
            dtpFecha.MinDate = DateTime.Today;
            dtpFecha.MaxDate = DateTime.Today.AddMonths(3);
            dtpFecha.Value = DateTime.Today;
        }

        private void GestionReservasForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Cargar todas las reservas una sola vez al inicio
                _allReservations = _db.LoadReservations();
                PopulateSolicitantesComboBox();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las reservas: {ex.Message}", "Error", 
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateSolicitantesComboBox()
        {
            var solicitantes = _allReservations
                .Select(r => r.Solicitante)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            cmbSolicitantes.Items.Clear();
            cmbSolicitantes.Items.Add("- Seleccione un Solicitante -");
            foreach (var solicitante in solicitantes)
            {
                cmbSolicitantes.Items.Add(solicitante);
            }
            cmbSolicitantes.SelectedIndex = 0;
        }

        private void LoadData()
        {
            try
            {
                string selectedSolicitante = cmbSolicitantes.SelectedItem?.ToString();
                DateTime selectedDate = dtpFecha.Value.Date;

                IEnumerable<Reservation> filteredResults = _allReservations;

                // Filtro por solicitante
                if (selectedSolicitante != "- Seleccione un Solicitante -")
                {
                    filteredResults = filteredResults
                        .Where(r => r.Solicitante.Equals(selectedSolicitante, StringComparison.OrdinalIgnoreCase));
                }

                // Filtro por fecha
                filteredResults = filteredResults
                    .Where(r => r.FechaDateTime.Date == selectedDate);

                // Ordenamiento
                var orderedResults = filteredResults.OrderBy(r => r.FechaDateTime)
                    .ThenBy(r => r.HoraInicioTimeSpan)
                    .ToList();

                ConfigureDataGridViewColumns();
                dgvReservas.DataSource = orderedResults;

                // Mostrar mensaje si no hay reservas después del filtro
                if (!orderedResults.Any())
                {
                    string message = $"No hay reservas registradas para la fecha {selectedDate.ToShortDateString()}";
                    if (selectedSolicitante != "- Seleccione un Solicitante -")
                    {
                        message += $" para el solicitante '{selectedSolicitante}'";
                    }
                    MessageBox.Show(message + ".", "Sin Reservas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos: {ex.Message}", "Error", 
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureDataGridViewColumns()
        {
            dgvReservas.AutoGenerateColumns = true;
            dgvReservas.Columns.Clear();
            dgvReservas.AutoGenerateColumns = true;

            dgvReservas.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Configurar después de que se generen las columnas automáticamente
            Application.DoEvents(); // Permitir que se generen las columnas

            // Ocultar columnas no deseadas
            HideColumnIfExists("Id");
            HideColumnIfExists("FechaDateTime");
            HideColumnIfExists("HoraInicioTimeSpan");
            HideColumnIfExists("HoraFinTimeSpan");
            HideColumnIfExists("FechaRegistroDateTime");
            HideColumnIfExists("FullDateTimeInicio");
            HideColumnIfExists("FullDateTimeFin");

            // Renombrar encabezados
            RenameColumnIfExists("NumeroNota", "Nº Nota");
            RenameColumnIfExists("FechaRegistro", "Fecha Sol.");
            RenameColumnIfExists("Salon", "Salón");
            RenameColumnIfExists("Solicitante", "Solicitante");
            RenameColumnIfExists("Contacto", "Contacto");
            RenameColumnIfExists("Motivo", "Motivo");
            RenameColumnIfExists("Fecha", "Fecha Uso");
            RenameColumnIfExists("HoraInicio", "H. Inicio");
            RenameColumnIfExists("HoraFin", "H. Fin");

            // Configurar anchos
            SetColumnWidthIfExists("Nº Nota", 80);
            SetColumnWidthIfExists("Fecha Sol.", 90);
            SetColumnWidthIfExists("Salón", 80);
            SetColumnWidthIfExists("Solicitante", 120);
            SetColumnWidthIfExists("Contacto", 100);
            SetColumnWidthIfExists("Fecha Uso", 90);
            SetColumnWidthIfExists("H. Inicio", 70);
            SetColumnWidthIfExists("H. Fin", 70);
            
            // Motivo ocupa el espacio restante
            if (dgvReservas.Columns.Contains("Motivo"))
            {
                dgvReservas.Columns["Motivo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        // Métodos auxiliares para evitar errores de columnas inexistentes
        private void HideColumnIfExists(string columnName)
        {
            if (dgvReservas.Columns.Contains(columnName))
            {
                dgvReservas.Columns[columnName].Visible = false;
            }
        }

        private void RenameColumnIfExists(string columnName, string newHeaderText)
        {
            if (dgvReservas.Columns.Contains(columnName))
            {
                dgvReservas.Columns[columnName].HeaderText = newHeaderText;
            }
        }

        private void SetColumnWidthIfExists(string headerText, int width)
        {
            var column = dgvReservas.Columns.Cast<DataGridViewColumn>()
                .FirstOrDefault(c => c.HeaderText == headerText);
            if (column != null)
            {
                column.Width = width;
            }
        }

        private void cmbSolicitantes_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void dtpFecha_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnEliminarReserva_Click(object sender, EventArgs e)
        {
            if (dgvReservas.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("¿Está seguro que desea eliminar las reservas seleccionadas?", 
                                   "Confirmar Eliminación",
                                   MessageBoxButtons.YesNo, 
                                   MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        foreach (DataGridViewRow row in dgvReservas.SelectedRows)
                        {
                            if (row.DataBoundItem is Reservation reservationToDelete)
                            {
                                _db.DeleteReservation(reservationToDelete.NumeroNota);
                            }
                        }

                        // Refrescar los datos después de la eliminación
                        _allReservations = _db.LoadReservations();
                        PopulateSolicitantesComboBox(); // Recargar solicitantes por si se eliminó el último de alguno
                        LoadData();

                        MessageBox.Show("Reservas eliminadas exitosamente.", "Eliminación", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar las reservas: {ex.Message}", "Error", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione al menos una reserva para eliminar.", 
                               "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCerrar_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}