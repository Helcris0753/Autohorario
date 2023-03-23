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

            switch (creditos_asignatura)
            {
                case 1:
                    int hora_inicio = int.Parse(horario_disponible[0].Item1.Substring(0,2));

                    using (SqlCommand cmd = new SqlCommand("ppInsert_hours", con))
                    {
                        insertar($"{zero(hora_inicio)}/{zero(hora_inicio + 1)}", horario_disponible[0].Item2, id_seccion);
                    }
                    break;
                case 2:
                    if (!(validar_insercion_2y3(horario_disponible, creditos_asignatura, id_seccion, 2)))
                    {
                        validar_insercion_2y3(horario_dia_disponible, creditos_asignatura, id_seccion, 2);
                    }
                    
                    break;
                case 3:
                    if (!(validar_insercion_2y3(horario_disponible, creditos_asignatura, id_seccion, 3)))
                    {
                        validar_insercion_2y3(horario_dia_disponible, creditos_asignatura, id_seccion, 3);
                    }

                    break;
                case 4:
                    if (!(validar_insercion_4y5(horario_disponible, id_seccion)))
                    {
                        validar_insercion_4y5(horario_dia_disponible, id_seccion);
                    }
                    break;
                case 5:
                    if (!(validar_insercion_4y5(horario_disponible, id_seccion)))
                    {
                        validar_insercion_4y5(horario_dia_disponible, id_seccion) ;
                    }
                    
                    break;
                default:
                    break;
            }
        }

        private static bool validar_insercion_2y3(List<(string, int)> horario_disponible, int creditos_asignatura, int id_seccion, int horas)
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
                    insertar($"{zero(hora_inicio)}/{zero(hora_inicio + horas)}", horario_disponible[i].Item2, id_seccion);
                    return true;
                }
            }
            return false;
        }

        private static bool validar_insercion_4y5(List<(string, int)> horario_disponible, int id_seccion) {
            bool validar_primero = false;
            int hora_inicio = 0, hora_fin = 0;

            for (int i = 0; i < horario_disponible.Count; i++)
            {
                hora_inicio = int.Parse(horario_disponible[i].Item1.Substring(0, 2));
                hora_fin = int.Parse(horario_disponible[i].Item1.Substring(3, 2));

                if ((hora_fin - hora_inicio) >= 2 && hora_fin < 23)
                {
                    for (int j = i; j < horario_disponible.Count; j++)
                    {
                        if (horario_disponible[i].Item2 != horario_disponible[j].Item2)
                        {
                            int hora_inicio2 = int.Parse(horario_disponible[j].Item1.Substring(0, 2));
                            int hora_fin2 = int.Parse(horario_disponible[j].Item1.Substring(3, 2));

                            if ((hora_fin2 - hora_inicio2) >= 2 && hora_fin2 < 23)
                            {
                                Console.WriteLine($"{zero(hora_inicio2)}/{zero(hora_inicio2 + 2)}");
                                insertar($"{zero(hora_inicio2)}/{zero(hora_inicio2 + 2)}", horario_disponible[j].Item2, id_seccion);
                                validar_primero = true;
                                break;
                            }
                        }
                    }
                    if (validar_primero)
                    {
                        Console.WriteLine($"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                        insertar($"{zero(hora_inicio)}/{zero(hora_inicio + 2)}", horario_disponible[i].Item2, id_seccion);
                        return true;
                    }
                }
                if ((hora_fin - hora_inicio) >= 4 && validar_primero == false && hora_fin < 23)
                {
                    Console.WriteLine($"{zero(hora_inicio)}/{zero(hora_inicio + 4)}");
                    insertar($"{zero(hora_inicio)}/{zero(hora_inicio + 4)}", horario_disponible[i].Item2, id_seccion);
                    return true;
                }
            }
            return false;

        }
        private static void insertar(string hora, int id_dia, int id_seccion) {
            using (SqlCommand cmd = new SqlCommand("ppInsert_hours", con)) {

                //Console.WriteLine($"{hora}  {id_dia}  {id_seccion}");
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@hora", hora);
                cmd.Parameters.AddWithValue("@id_dia", id_dia);
                cmd.Parameters.AddWithValue("@id_seccion", id_seccion);
                cmd.ExecuteNonQuery();
            }
        }

        public static string zero(int hora) {
            if (hora < 10)
            {
                return $"0{hora}";
            }
            return $"{hora}";
        }
    }
}
