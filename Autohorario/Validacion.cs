using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autohorario
{
    internal class Validacion
    {
        static SqlConnection con = Obtencion.con;

        public static void Getdata(string codigo_asignatura, int id_seccion, int id_profesor, int creditos_asignatura, List<(string, int)> horario_seleccionado)
        {
            bool verdad = Validarhoras(codigo_asignatura, id_seccion, creditos_asignatura, horario_seleccionado);
        }
        private static bool Validarhoras(string codigo_asignatura, int id_seccion, int creditos_asignatura,  List<(string, int)> horario_seleccionado)
        {
            using (SqlCommand cmd = new SqlCommand("ppCheck_schedule", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigo_asignatura", codigo_asignatura);
                cmd.Parameters.AddWithValue("@id_seccion", id_seccion);
                List<(string, int)> horario_disponible = new List<(string, int)>();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    for (int i = 0; i < horario_seleccionado.Count; i++)
                    {
                        int hora_inicio_seleccion = int.Parse(horario_seleccionado[i].Item1.Substring(0,2));
                        int hora_fin_seleccion = int.Parse(horario_seleccionado[i].Item1.Substring(3, 2));
                        while (reader.Read())
                        {
                            int hora_inicio_asignatura = int.Parse(reader.GetString(0).Substring(0, 2));
                            int hora_fin_asignatura = int.Parse(reader.GetString(0).Substring(3, 2));
                            if (horario_seleccionado[i].Item2 == reader.GetInt32(1))
                            {
                                if (hora_inicio_asignatura == hora_inicio_seleccion && hora_fin_asignatura < hora_fin_seleccion)
                                {
                                    horario_disponible.Add(($"{hora_fin_asignatura}/{hora_fin_seleccion}", horario_seleccionado[i].Item2));
                                } else if (hora_inicio_asignatura < hora_inicio_seleccion && hora_fin_asignatura == hora_fin_seleccion)
                                {
                                    horario_disponible.Add(($"{hora_inicio_seleccion}/{hora_fin_seleccion}", horario_seleccionado[i].Item2));
                                }
                            }
                            
                        }
                    }
                }
            }
            return true;
        }
    }
}
