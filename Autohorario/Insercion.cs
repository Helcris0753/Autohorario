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

        public static void data_insercion(List<(string, int)> horario_seleccionado, int id_seccion, int creditos_asignatura) {

            int hora_inicio = 0;
            int hora_fin = 0;
            switch (creditos_asignatura)
            {
                case 1:
                    hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0,2));

                    using (SqlCommand cmd = new SqlCommand("ppInsert_hours", con))
                    {
                        insertar($"{zero(hora_inicio)}/{zero(hora_inicio + 1)}", horario_seleccionado[0].Item2, id_seccion);
                    }
                    break;
                case 2:

                    for (int i = 0; i < horario_seleccionado.Count; i++)
                    {
                        hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0, 2));
                        hora_fin = int.Parse(horario_seleccionado[0].Item1.Substring(3, 2));

                        if ( (hora_fin - hora_inicio) >= creditos_asignatura)
                        {
                            insertar($"{zero(hora_inicio)}/{zero(hora_inicio+2)}", horario_seleccionado[i].Item2, id_seccion);
                            break;
                        }
                    }
                    break;
                case 3:
                    for (int i = 0; i < horario_seleccionado.Count; i++)
                    {
                        hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0, 2));
                        hora_fin = int.Parse(horario_seleccionado[0].Item1.Substring(3, 2));

                        if ((hora_fin - hora_inicio) >= creditos_asignatura)
                        {
                            insertar($"{zero(hora_inicio)}/{zero(hora_inicio + 2)}", horario_seleccionado[i].Item2, id_seccion);
                            break;
                        }
                    }
                    break;
                case 4:
                    break;
                default:
                    break;
            }
        }

        private static void insertar(string hora, int id_dia, int id_seccion) {
            using (SqlCommand cmd = new SqlCommand("ppInsert_hours", con)) {

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@hora", hora);
                cmd.Parameters.AddWithValue("@id_dia", id_dia);
                cmd.Parameters.AddWithValue("@id_seccion", id_seccion);
                cmd.ExecuteNonQuery();
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
