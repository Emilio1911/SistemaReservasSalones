using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaReservasSalones
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // IMPORTANTE: Inicializar la base de datos antes de mostrar cualquier formulario
            try
            {
                var db = new SQLiteDataAccess();
                db.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar la base de datos: {ex.Message}", 
                               "Error de Inicialización", 
                               MessageBoxButtons.OK, 
                               MessageBoxIcon.Error);
                return; // No continuar si no se puede inicializar la DB
            }
            
            Application.Run(new FrmPrincipal());
        }
    }
}