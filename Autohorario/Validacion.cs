using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autohorario
{
    internal class Validacion
    {
        static private SqlConnection con = Obtencion.con;

        public static void Getdata(string codigo_asignatura, int id_seccion, int id_profesor, int creditos_asignatura, List<(string, int)> horario_seleccionado)
        {
            List<(string, int)> horario_disponible = Validarhoras(codigo_asignatura, id_seccion, horario_seleccionado);
            horario_disponible = Validarcreditos(horario_disponible, creditos_asignatura);
            Insercion.data_insercion(horario_disponible, id_seccion, creditos_asignatura);
        }
        private static List<(string, int)> Validarhoras(string codigo_asignatura, int id_seccion,  List<(string, int)> horario_seleccionado)
        {
            List<(string, int)> horario_disponible = new List<(string, int)>();
            using (SqlCommand cmd = new SqlCommand("ppCheck_schedule", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigo_asignatura", codigo_asignatura);
                cmd.Parameters.AddWithValue("@id_seccion", id_seccion);
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    for (int i = 0; i < horario_seleccionado.Count; i++)
                    {
                        string instancia_hora = horario_seleccionado[i].Item1;

                        int hora_inicio_seleccion = int.Parse(instancia_hora.Substring(0,2));
                        int hora_fin_seleccion = int.Parse(instancia_hora.Substring(3, 2));

                        while (reader.Read())
                        {
                            int hora_inicio_asignatura = int.Parse(reader.GetString(0).Substring(0, 2));
                            int hora_fin_asignatura = int.Parse(reader.GetString(0).Substring(3, 2));
                            Console.WriteLine($"{hora_inicio_asignatura}, {hora_fin_asignatura}");

                            if (hora_inicio_asignatura <= hora_inicio_seleccion && hora_fin_asignatura < hora_fin_seleccion)
                            {
                                instancia_hora = $"{reader.GetString(0).Substring(3, 2)}/{instancia_hora.Substring(3, 2)}";
                            }
                            else if (hora_inicio_seleccion < hora_inicio_asignatura && hora_fin_asignatura >= hora_fin_seleccion)
                            {
                                //instancia_hora = $"{}/{}";
                                instancia_hora = $"{instancia_hora.Substring(0, 2)}/{reader.GetString(0).Substring(0, 2)}";
                            }
                            else if (hora_inicio_seleccion < hora_inicio_asignatura && hora_fin_asignatura < hora_fin_seleccion)
                            {
                                instancia_hora = $"{instancia_hora.Substring(0, 2)}/{reader.GetString(0).Substring(0, 2)}";
                                horario_seleccionado.Add(($"{reader.GetString(0).Substring(3, 2)}/{instancia_hora.Substring(3, 2)}", horario_seleccionado[i].Item2));
                            }
                            
                        }
                        horario_disponible.Add((instancia_hora, horario_seleccionado[i].Item2));
                    }
                    return horario_disponible;
                }
            }
        }

        private static List<(string, int)> Validarcreditos(List<(string, int)> horario_disponible,  int creditos) {

            List<(string, int)> horario_credito  = new List<(string, int)> ();

            for (int i = 0; i < horario_disponible.Count; i++)
            {
                int hora_inicio = int.Parse(horario_disponible[i].Item1.Substring(0,2));
                int hora_fin = int.Parse(horario_disponible[i].Item1.Substring(3, 2)); ;

                int diferencia_horas = hora_fin - hora_inicio;

                if (!(diferencia_horas == 1 && diferencia_horas > creditos))
                {
                    horario_credito.Add(($"{horario_disponible[i].Item1.Substring(0, 2)}/{horario_disponible[i].Item1.Substring(3, 2)}", horario_disponible[i].Item2));
                }
            } 

            return horario_credito;
        }
    }
}
