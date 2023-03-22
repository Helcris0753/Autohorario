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

        public static void data_insercion(List<(string, int)> horario_disponible, List<(string, int)> horario_dia_disponible, int id_seccion, int creditos_asignatura) {

            int hora_inicio = 0;
            int hora_fin = 0;

            switch (creditos_asignatura)
            {
                case 1:
                    hora_inicio = int.Parse(horario_disponible[0].Item1.Substring(0,2));

                    using (SqlCommand cmd = new SqlCommand("ppInsert_hours", con))
                    {
                        insertar($"{zero(hora_inicio)}/{zero(hora_inicio + 1)}", horario_disponible[0].Item2, id_seccion);
                    }
                    break;
                case 2:
                    validar_insercion(horario_disponible, creditos_asignatura, id_seccion, 2);
                    
                    break;
                case 3:
                    validar_insercion(horario_disponible, creditos_asignatura, id_seccion, 3);

                    break;
                case 4:
                    string hora_primer_dia, hora_segundo_dia;

                    List<int> dias_disponibles = dias_disponibilidad(horario_disponible);

                    
                    for (int i = 0; i < horario_disponible.Count; i++)
                    {
                        hora_inicio = int.Parse(horario_disponible[0].Item1.Substring(0, 2));
                        hora_fin = int.Parse(horario_disponible[0].Item1.Substring(3, 2));

                        if ((hora_fin - hora_inicio) >= 2 && (hora_fin - hora_inicio) < 4)
                        {
                            for (int j = i; j < horario_disponible.Count; j++)
                            {
                                if (horario_disponible[i].Item2 != horario_disponible[j].Item2)
                                {
                                    
                                }
                            }
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        private static bool validar_insercion(List<(string, int)> horario_disponible, int creditos_asignatura, int id_seccion, int horas)
        {
            int hora_inicio = 0;
            int hora_fin = 0;
            for (int i = 0; i < horario_disponible.Count; i++)
            {
                hora_inicio = int.Parse(horario_disponible[i].Item1.Substring(0, 2));
                hora_fin = int.Parse(horario_disponible[i].Item1.Substring(3, 2));

                if ((hora_fin - hora_inicio) >= creditos_asignatura)
                {
                    insertar($"{zero(hora_inicio)}/{zero(hora_inicio + horas)}", horario_disponible[i].Item2, id_seccion);
                    return true;
                }
            }
            return false;
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

        private static List<int> dias_disponibilidad(List<(string, int)> horario_disponible)
        {

            List<int> dias_disponibles = new List<int>();
            dias_disponibles.Add(horario_disponible[0].Item2);

            for (int i = 1; i < horario_disponible.Count; i++)
            {
                if (horario_disponible[i].Item2 != horario_disponible[i - 1].Item2)
                {
                    dias_disponibles.Add(horario_disponible[i].Item2);
                }
            }
            return dias_disponibles;
        }


    }
}
