// SQLiteDataAccess.cs
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace SistemaReservasSalones
{
    public class SQLiteDataAccess
    {
        // Método para obtener la cadena de conexión a la base de datos SQLite
        private static string LoadConnectionString()
        {
            return "Data Source=./ReservasDB.db;Version=3;";
        }

        // Método para cargar todas las reservas desde la base de datos
        public List<Reservation> LoadReservations()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Reservation>("select Id, NumeroNota, Salon, Fecha, HoraInicio, HoraFin, Solicitante, Contacto, Motivo, FechaRegistro from Reservas", new DynamicParameters());
                return output.ToList();
            }
        }

        // MÉTODO FALTANTE: Cargar reservas para un salón y fecha específicos
        public List<Reservation> LoadReservationsForSalonAndDate(string salon, string fecha)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Reservation>(
                    "select Id, NumeroNota, Salon, Fecha, HoraInicio, HoraFin, Solicitante, Contacto, Motivo, FechaRegistro from Reservas where Salon = @Salon and Fecha = @Fecha", 
                    new { Salon = salon, Fecha = fecha });
                return output.ToList();
            }
        }

        // MÉTODO FALTANTE: Verificar si un horario está ocupado
        public bool IsReservationTimeSlotBooked(string salon, string fecha, string horaInicio, string horaFin)
        {
            try
            {
                // Convertir las horas a TimeSpan para comparación
                if (!TimeSpan.TryParse(horaInicio, out TimeSpan inicioTime) || 
                    !TimeSpan.TryParse(horaFin, out TimeSpan finTime))
                {
                    return false; // Si no se pueden parsear las horas, asumimos que no hay conflicto
                }

                var reservasExistentes = LoadReservationsForSalonAndDate(salon, fecha);

                foreach (var reserva in reservasExistentes)
                {
                    if (TimeSpan.TryParse(reserva.HoraInicio, out TimeSpan reservaInicio) &&
                        TimeSpan.TryParse(reserva.HoraFin, out TimeSpan reservaFin))
                    {
                        // Verificar si hay superposición de horarios
                        // No hay superposición si: finTime <= reservaInicio OR inicioTime >= reservaFin
                        // Hay superposición si: !(finTime <= reservaInicio OR inicioTime >= reservaFin)
                        if (!(finTime <= reservaInicio || inicioTime >= reservaFin))
                        {
                            return true; // Hay conflicto de horario
                        }
                    }
                }

                return false; // No hay conflicto
            }
            catch (Exception)
            {
                return false; // En caso de error, asumimos que no hay conflicto
            }
        }

        // Método para guardar una nueva reserva en la base de datos
        public void SaveReservation(Reservation reservation)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("insert into Reservas (NumeroNota, Salon, Fecha, HoraInicio, HoraFin, Solicitante, Contacto, Motivo, FechaRegistro) " +
                           "values (@NumeroNota, @Salon, @Fecha, @HoraInicio, @HoraFin, @Solicitante, @Contacto, @Motivo, @FechaRegistro)", reservation);
            }
        }

        // MÉTODO FALTANTE: Actualizar una reserva existente
        public void UpdateReservation(string numeroNotaOriginal, Reservation reservation)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute(@"UPDATE Reservas 
                             SET NumeroNota = @NumeroNota, 
                                 Salon = @Salon, 
                                 Fecha = @Fecha, 
                                 HoraInicio = @HoraInicio, 
                                 HoraFin = @HoraFin, 
                                 Solicitante = @Solicitante, 
                                 Contacto = @Contacto, 
                                 Motivo = @Motivo 
                             WHERE NumeroNota = @NumeroNotaOriginal", 
                           new 
                           {
                               NumeroNota = reservation.NumeroNota,
                               Salon = reservation.Salon,
                               Fecha = reservation.Fecha,
                               HoraInicio = reservation.HoraInicio,
                               HoraFin = reservation.HoraFin,
                               Solicitante = reservation.Solicitante,
                               Contacto = reservation.Contacto,
                               Motivo = reservation.Motivo,
                               NumeroNotaOriginal = numeroNotaOriginal
                           });
            }
        }

        // Método para eliminar una reserva por su Id
        public void DeleteReservation(int id)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("delete from Reservas where Id = @Id", new { Id = id });
            }
        }

        // Método para inicializar la base de datos (crear la tabla 'Reservas' si no existe)
        public void InitializeDatabase()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Reservas (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    NumeroNota TEXT NOT NULL,
                    Salon TEXT NOT NULL,
                    Fecha TEXT NOT NULL,
                    HoraInicio TEXT NOT NULL,
                    HoraFin TEXT NOT NULL,
                    Solicitante TEXT NOT NULL,
                    Contacto TEXT NOT NULL,
                    Motivo TEXT NOT NULL,
                    FechaRegistro TEXT NOT NULL
                );";
                cnn.Execute(createTableSql);
            }
        }
    }
}