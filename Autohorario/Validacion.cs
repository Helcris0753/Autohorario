using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autohorario
{
    internal class Validacion
    {
        SqlConnection con = Obtencion.con;

        public static void Getdata(string codigo_asignatura, int id_seccion, int id_profesor, int creditos_asignatura, List<(string, int)> horario_seleccionado)
        {
             
        }
    }
}
