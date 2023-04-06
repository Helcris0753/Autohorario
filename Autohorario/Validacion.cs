using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Autohorario
{
    internal class Validacion
    {
        static private SqlConnection con = Obtencion.con;

        public static void Getdata(string codigo_asignatura, int id_seccion, int creditos_asignatura, List<(string, int)> horario_seleccionado, int modalidad, int id_profesor, List<(string, int)> horario_secundario = null)
        {
            List<(string, int)> horario_disponible = new List<(string, int)>();
            List<(string, int)> horario_dia_disponible = new List<(string, int)>();
            List<(string, int)> horario_secundario_disponible = new List<(string, int)>();
            for (int i = 1; i < 6; i++)
            {
                horario_dia_disponible.Add(("07/22", i));
            }

            horario_seleccionado = horario_seleccionado.Where(horario => int.Parse(horario.Item1.Substring(3, 2)) - int.Parse(horario.Item1.Substring(0, 2)) >= 2).ToList();

            horario_disponible = Validarhoras(codigo_asignatura, id_profesor, horario_seleccionado);
            horario_disponible = horario_disponible.Where(horario => int.Parse(horario.Item1.Substring(3, 2)) - int.Parse(horario.Item1.Substring(0, 2)) >= 2).ToList();

            horario_dia_disponible = Validarhoras(codigo_asignatura, id_profesor, horario_dia_disponible);
            horario_dia_disponible = horario_dia_disponible.Where(horario => int.Parse(horario.Item1.Substring(3, 2)) - int.Parse(horario.Item1.Substring(0, 2)) >= 2).ToList();



            if (horario_secundario != null)
            {
                horario_secundario_disponible = Validarhoras(codigo_asignatura, id_profesor, horario_secundario);
                horario_secundario_disponible = horario_secundario_disponible.Where(horario => int.Parse(horario.Item1.Substring(3, 2)) - int.Parse(horario.Item1.Substring(0, 2)) >= 2).ToList();
            }



            Insercion.data_insercion(horario_disponible, horario_dia_disponible, horario_secundario_disponible, horario_seleccionado,id_seccion, creditos_asignatura, modalidad);
        }
        private static List<(string, int)> Validarhoras(string codigo_asignatura, int id_profesor, List<(string, int)> horario_seleccionado)
        {
            string instancia_hora = null;
            int hora_inicio_seleccion, hora_fin_seleccion, hora_inicio_asignatura, hora_fin_asignatura;
            List<(string, int)> horario_disponible = new List<(string, int)>();
            List<(string, int)> horas_ocupadas = new List<(string, int)>();
            using (SqlCommand cmd = new SqlCommand("ppCheck_schedule", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigo_asignatura", codigo_asignatura);
                cmd.Parameters.AddWithValue("@id_profesor", id_profesor);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        horas_ocupadas.Add((reader.GetString(0), reader.GetInt32(1)));
                    }
                }
                horas_ocupadas = horas_ocupadas.Distinct().ToList();

                if (horas_ocupadas.Count == 0)
                {
                    horario_disponible.AddRange(horario_seleccionado);
                }

                for (int i = 0; i < horario_seleccionado.Count; i++)
                {
                    
                    int dia_horario_seleccionado = horario_seleccionado[i].Item2;
                    for (int j = 0; j < horas_ocupadas.Count; j++)
                    {
                        instancia_hora = horario_seleccionado[i].Item1;
                        hora_inicio_seleccion = int.Parse(instancia_hora.Substring(0, 2));
                        hora_fin_seleccion = int.Parse(instancia_hora.Substring(3, 2));
                        hora_inicio_asignatura = int.Parse(horas_ocupadas[j].Item1.Substring(0, 2));
                        hora_fin_asignatura = int.Parse(horas_ocupadas[j].Item1.Substring(3, 2));
                        //Console.WriteLine("{0} {1}", i, j);
                        //Console.WriteLine($"{horario_disponible[i].Item2} {horas_ocupadas[j].Item2}");
                        if (horario_seleccionado[i].Item2 == horas_ocupadas[j].Item2)
                        {
                            if (hora_inicio_asignatura <= hora_inicio_seleccion && hora_fin_asignatura < hora_fin_seleccion && hora_inicio_seleccion < hora_fin_asignatura)
                            {
                                horario_seleccionado[i] = ($"{Insercion.zero(hora_fin_asignatura)}/{Insercion.zero(hora_fin_seleccion)}", horario_seleccionado[i].Item2);

                            }
                            else if (hora_inicio_seleccion < hora_inicio_asignatura && hora_fin_seleccion <= hora_fin_asignatura && hora_fin_seleccion > hora_inicio_asignatura)
                            {
                                horario_seleccionado[i] = ($"{Insercion.zero(hora_inicio_seleccion)}/{Insercion.zero(hora_inicio_asignatura)}", horario_seleccionado[i].Item2);
                            }
                            else if (hora_inicio_asignatura > hora_inicio_seleccion && hora_fin_asignatura < hora_fin_seleccion)
                            {
                                horario_seleccionado[i] = ($"{Insercion.zero(hora_inicio_seleccion)}/{Insercion.zero(hora_inicio_asignatura)}", horario_seleccionado[i].Item2);
                                horario_seleccionado.Add(($"{Insercion.zero(hora_fin_asignatura)}/{Insercion.zero(hora_fin_seleccion)}", horario_seleccionado[i].Item2));
                            }
                        }
                    }
                }

                horario_disponible = horario_seleccionado.Except(horas_ocupadas).ToList();
               
                
                return horario_disponible;
            }
        }
    }
}