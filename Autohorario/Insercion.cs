using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Autohorario
{
    internal class Insercion
    {
        static private SqlConnection con = Obtencion.con;

        public static void data_insercion(List<(string, int)> horario_disponible, List<(string, int)> horario_dia_disponible, int id_seccion, int creditos_asignatura)
        {

            switch (creditos_asignatura)
            {
                case 0:
                    if (!(validar_insercion(horario_disponible, creditos_asignatura, id_seccion, 2, 1)))
                    {
                        validar_insercion(horario_dia_disponible, creditos_asignatura, id_seccion, 2, 2);
                    }
                    break;
                case 1:
                    if (!(validar_insercion(horario_disponible, creditos_asignatura, id_seccion, 2, 1)))
                    {
                        validar_insercion(horario_dia_disponible, creditos_asignatura, id_seccion, 2, 2);
                    }
                    break;
                case 2:
                    if (!(validar_insercion(horario_disponible, creditos_asignatura, id_seccion, 2, 1)))
                    {
                        validar_insercion(horario_dia_disponible, creditos_asignatura, id_seccion, 2, 2);
                    }
                    break;
                case 3:
                    if (!(validar_insercion(horario_disponible, creditos_asignatura, id_seccion, 3, 1)))
                    {
                        validar_insercion(horario_dia_disponible, creditos_asignatura, id_seccion, 3, 2);
                    }

                    break;
                case 4:
                    if (!(validar_insercion_4creditos(horario_disponible, id_seccion, 1)))
                    {
                        validar_insercion_4creditos(horario_dia_disponible, id_seccion, 2);
                    }
                    break;
                case 5:
                    if (!(validar_insercion_4creditos(horario_disponible, id_seccion, 1)))
                    {
                        validar_insercion_4creditos(horario_dia_disponible, id_seccion, 2);
                    }
                    break;
                default:
                    break;
            }
        }

        private static bool validar_insercion(List<(string, int)> horario_disponible, int creditos_asignatura, int id_seccion, int horas, int estado_horas)
        {
            int hora_inicio = 0;
            int hora_fin = 0;
            for (int i = 0; i < horario_disponible.Count; i++)
            {
                hora_inicio = int.Parse(horario_disponible[i].Item1.Substring(0, 2));
                hora_fin = int.Parse(horario_disponible[i].Item1.Substring(3, 2));

                if ((hora_fin - hora_inicio) >= creditos_asignatura && hora_fin < 23)
                {
                    //Console.WriteLine($"{zero(hora_inicio)}/{zero(hora_inicio + horas)}");
                    insertar(horario_disponible[i].Item2, id_seccion, estado_horas, $"{zero(hora_inicio)}/{zero(hora_inicio + horas)}");
                    return true;
                }
            }
            return false;
        }

        private static bool validar_insercion_4creditos(List<(string, int)> horario_disponible, int id_seccion, int estado_hora)
        {
            bool validar_primero = false;
            int hora_inicio = 0;

            for (int i = 0; i < horario_disponible.Count; i++)
            {
                hora_inicio = int.Parse(horario_disponible[i].Item1.Substring(0, 2));

                for (int j = i; j < horario_disponible.Count; j++)
                {
                    if (horario_disponible[i].Item2 != horario_disponible[j].Item2)
                    {
                        int hora_inicio2 = int.Parse(horario_disponible[j].Item1.Substring(0, 2));

                        //Console.WriteLine($"{zero(hora_inicio2)}/{zero(hora_inicio2 + 2)}");
                        insertar(horario_disponible[j].Item2, id_seccion, estado_hora, $"{zero(hora_inicio2)}/{zero(hora_inicio2 + 2)}");
                        validar_primero = true;
                        break;

                    }
                }
                if (validar_primero)
                {
                    //Console.WriteLine($"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                    insertar(horario_disponible[i].Item2, id_seccion, estado_hora, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                    return true;
                }

            }
            return false;

        }
        private static void insertar(int id_dia, int id_seccion, int estado_hora, string hora = null)
        {
            using (SqlCommand cmd = new SqlCommand("ppInsert_hours", con))
            {

                //Console.WriteLine($"{hora}  {id_dia}  {id_seccion}");
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@hora", hora);
                cmd.Parameters.AddWithValue("@id_dia", id_dia);
                cmd.Parameters.AddWithValue("@id_seccion", id_seccion);
                cmd.Parameters.AddWithValue("@estado_hora", estado_hora);
                cmd.ExecuteNonQuery();
            }
        }

        public static string zero(int hora)
        {
            if (hora < 10)
            {
                return $"0{hora}";
            }
            return $"{hora}";
        }
    }
}
