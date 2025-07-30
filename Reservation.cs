// Reservation.cs
using System;

namespace SistemaReservasSalones
{
    public class Reservation
    {
        public int Id { get; set; }
        public string NumeroNota { get; set; }
        public string Salon { get; set; }

        // Mapeamos Fecha y FechaRegistro como string, ya que tu base de datos SQLite
        // parece almacenarlos como TEXT.
        // Si los cargaras como DateTime, Dapper podría manejarlo si el formato es compatible,
        // pero para evitar errores, string es más seguro si es TEXT en DB.
        public string Fecha { get; set; }
        public string HoraInicio { get; set; } // <--- ¡Importante! DEBE SER string para que coincida con TEXT en DB
        public string HoraFin { get; set; }   // <--- ¡Importante! DEBE SER string para que coincida con TEXT en DB
        public string Solicitante { get; set; }
        public string Contacto { get; set; }
        public string Motivo { get; set; }
        public string FechaRegistro { get; set; } // <--- ¡Importante! DEBE SER string si es TEXT en DB

        // --- Propiedades Auxiliares (NO mapeadas directamente desde la DB por Dapper) ---
        // Estas propiedades te permiten trabajar con tipos DateTime y TimeSpan si los necesitas en la UI
        // o para lógica de negocio, pero no son las propiedades que Dapper mapeará directamente.

        public DateTime FechaDateTime
        {
            get
            {
                // Intenta parsear la string Fecha a DateTime.
                // Asegúrate de que el formato de Fecha en tu DB sea parseable (ej. "yyyy-MM-dd" o "MM/dd/yyyy").
                if (DateTime.TryParse(Fecha, out DateTime parsedDate))
                {
                    return parsedDate.Date; // Solo la parte de la fecha
                }
                return DateTime.MinValue; // Retorna un valor por defecto si falla el parseo
            }
        }

        public TimeSpan HoraInicioTimeSpan
        {
            get
            {
                // Intenta parsear la string HoraInicio a TimeSpan.
                // El formato en la DB debe ser compatible (ej. "HH:mm" o "HH:mm:ss").
                if (TimeSpan.TryParse(HoraInicio, out TimeSpan parsedTime))
                {
                    return parsedTime;
                }
                return TimeSpan.Zero; // Retorna un valor por defecto si falla el parseo
            }
        }

        public TimeSpan HoraFinTimeSpan
        {
            get
            {
                // Intenta parsear la string HoraFin a TimeSpan.
                if (TimeSpan.TryParse(HoraFin, out TimeSpan parsedTime))
                {
                    return parsedTime;
                }
                return TimeSpan.Zero; // Retorna un valor por defecto si falla el parseo
            }
        }

        public DateTime FullDateTimeInicio
        {
            get
            {
                // Combina la fecha (como DateTime) y la hora de inicio (como TimeSpan)
                return FechaDateTime.Add(HoraInicioTimeSpan);
            }
        }

        public DateTime FullDateTimeFin
        {
            get
            {
                // Combina la fecha (como DateTime) y la hora de fin (como TimeSpan)
                return FechaDateTime.Add(HoraFinTimeSpan);
            }
        }

        public DateTime FechaRegistroDateTime
        {
            get
            {
                if (DateTime.TryParse(FechaRegistro, out DateTime parsedDate))
                {
                    return parsedDate;
                }
                return DateTime.MinValue;
            }
        }
    }
}