using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autohorario
{
    internal class Insercion
    {
        static private SqlConnection con = Obtencion.con;

        public static void insertar(List<(string, int)> horario_seleccionado, int id_seccion, int creditos_asignatura) {

            switch (creditos_asignatura)
            {
                case 1:
                    int hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0,2));
                    using (SqlCommand cmd = new SqlCommand("ppInsert_hours", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@hora", $"{zero(hora_inicio)}/{zero(hora_inicio+1)}");
                        cmd.Parameters.AddWithValue("@id_dia", horario_seleccionado[0].Item2);
                        cmd.Parameters.AddWithValue("@id_seccion", id_seccion);
                    }
                    break;
                default:
                    break;
            }
        }

        private static string zero(int hora) {
            if (hora < 10)
            {
                return $"0{hora}";
            }
            return $"{hora}";
        }
    }
}
