using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace SistemaReservasSalones
{
    public class SQLiteDataAccess
    {
        private readonly string _connectionString;
        private const string DatabaseFile = "ReservasDB.sqlite";

        public SQLiteDataAccess()
        {
            _connectionString = $"Data Source={DatabaseFile};Version=3;FailIfMissing=False;";
            InitializeDatabase();
        }

        public void InitializeDatabase()
        {
            if (!File.Exists(DatabaseFile))
            {
                SQLiteConnection.CreateFile(DatabaseFile);
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    new SQLiteCommand(@"
                        CREATE TABLE Reservas (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            NumeroNota TEXT NOT NULL UNIQUE,
                            Salon TEXT NOT NULL,
                            Fecha TEXT NOT NULL,
                            HoraInicio TEXT NOT NULL,
                            HoraFin TEXT NOT NULL,
                            Solicitante TEXT NOT NULL,
                            Contacto TEXT,
                            Motivo TEXT,
                            FechaRegistro TEXT NOT NULL
                        )", conn).ExecuteNonQuery();
                }
            }
        }

        public bool IsTimeSlotAvailable(string salon, string fecha, string horaInicio, string horaFin, string excluirNumeroNota = null)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(@"
                    SELECT COUNT(*) FROM Reservas 
                    WHERE Salon = @Salon AND Fecha = @Fecha
                    AND (HoraInicio < @HoraFin AND HoraFin > @HoraInicio)
                    AND (@ExcluirNumeroNota IS NULL OR NumeroNota != @ExcluirNumeroNota)", conn))
                {
                    cmd.Parameters.AddWithValue("@Salon", salon);
                    cmd.Parameters.AddWithValue("@Fecha", fecha);
                    cmd.Parameters.AddWithValue("@HoraInicio", horaInicio);
                    cmd.Parameters.AddWithValue("@HoraFin", horaFin);
                    cmd.Parameters.AddWithValue("@ExcluirNumeroNota", excluirNumeroNota ?? (object)DBNull.Value);
                    return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
                }
            }
        }

        public bool CreateReservation(Reservation reserva)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(@"
                    INSERT INTO Reservas (
                        NumeroNota, Salon, Fecha, HoraInicio, HoraFin,
                        Solicitante, Contacto, Motivo, FechaRegistro
                    ) VALUES (
                        @NumeroNota, @Salon, @Fecha, @HoraInicio, @HoraFin,
                        @Solicitante, @Contacto, @Motivo, @FechaRegistro
                    )", conn))
                {
                    cmd.Parameters.AddWithValue("@NumeroNota", reserva.NumeroNota);
                    cmd.Parameters.AddWithValue("@Salon", reserva.Salon);
                    cmd.Parameters.AddWithValue("@Fecha", reserva.Fecha);
                    cmd.Parameters.AddWithValue("@HoraInicio", reserva.HoraInicio);
                    cmd.Parameters.AddWithValue("@HoraFin", reserva.HoraFin);
                    cmd.Parameters.AddWithValue("@Solicitante", reserva.Solicitante);
                    cmd.Parameters.AddWithValue("@Contacto", reserva.Contacto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Motivo", reserva.Motivo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaRegistro", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<Reservation> LoadReservations(string salon = null)
        {
            var reservas = new List<Reservation>();
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    salon == null
                        ? "SELECT * FROM Reservas ORDER BY Fecha, HoraInicio"
                        : "SELECT * FROM Reservas WHERE Salon = @Salon ORDER BY Fecha, HoraInicio", conn))
                {
                    if (salon != null) cmd.Parameters.AddWithValue("@Salon", salon);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservas.Add(new Reservation
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                NumeroNota = reader["NumeroNota"].ToString(),
                                Salon = reader["Salon"].ToString(),
                                Fecha = reader["Fecha"].ToString(),
                                HoraInicio = reader["HoraInicio"].ToString(),
                                HoraFin = reader["HoraFin"].ToString(),
                                Solicitante = reader["Solicitante"].ToString(),
                                Contacto = reader["Contacto"] == DBNull.Value ? null : reader["Contacto"].ToString(),
                                Motivo = reader["Motivo"] == DBNull.Value ? null : reader["Motivo"].ToString(),
                                FechaRegistro = reader["FechaRegistro"].ToString()
                            });
                        }
                    }
                }
            }
            return reservas;
        }

        public bool DeleteReservation(string numeroNota)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Reservas WHERE NumeroNota = @NumeroNota", conn))
                {
                    cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateReservation(string numeroNotaOriginal, Reservation reserva)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(@"
                    UPDATE Reservas SET
                        NumeroNota = @NumeroNota,
                        Salon = @Salon,
                        Fecha = @Fecha,
                        HoraInicio = @HoraInicio,
                        HoraFin = @HoraFin,
                        Solicitante = @Solicitante,
                        Contacto = @Contacto,
                        Motivo = @Motivo
                    WHERE NumeroNota = @NumeroNotaOriginal", conn))
                {
                    cmd.Parameters.AddWithValue("@NumeroNota", reserva.NumeroNota);
                    cmd.Parameters.AddWithValue("@Salon", reserva.Salon);
                    cmd.Parameters.AddWithValue("@Fecha", reserva.Fecha);
                    cmd.Parameters.AddWithValue("@HoraInicio", reserva.HoraInicio);
                    cmd.Parameters.AddWithValue("@HoraFin", reserva.HoraFin);
                    cmd.Parameters.AddWithValue("@Solicitante", reserva.Solicitante);
                    cmd.Parameters.AddWithValue("@Contacto", reserva.Contacto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Motivo", reserva.Motivo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NumeroNotaOriginal", numeroNotaOriginal);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}