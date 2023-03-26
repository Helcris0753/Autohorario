using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Autohorario
{
    internal class Validacion
    {
        static private SqlConnection con = Obtencion.con;

        public static void Getdata(string codigo_asignatura, int id_seccion, int creditos_asignatura, List<(string, int)> horario_seleccionado)
        {
            List<(string, int)> horario_disponible = new List<(string, int)>();
            List<(string, int)> horario_dia_disponible = new List<(string, int)>();

            for (int i = 1; i < 6; i++)
            {
                horario_dia_disponible.Add(("07/22", i));
            }
            horario_disponible = Validarhoras(codigo_asignatura, id_seccion, horario_seleccionado);
            horario_disponible = horario_disponible.Where(horario => int.Parse(horario.Item1.Substring(3, 2)) - int.Parse(horario.Item1.Substring(0, 2)) >= 2).ToList(); ;

            horario_dia_disponible = Validarhoras(codigo_asignatura, id_seccion, horario_dia_disponible);
            horario_dia_disponible = horario_dia_disponible.Where(horario => int.Parse(horario.Item1.Substring(3, 2)) - int.Parse(horario.Item1.Substring(0, 2)) >= 2).ToList();

            Insercion.data_insercion(horario_disponible, horario_dia_disponible, id_seccion, creditos_asignatura);
        }
        private static List<(string, int)> Validarhoras(string codigo_asignatura, int id_seccion, List<(string, int)> horario_seleccionado)
        {
            string instancia_hora = null;
            int hora_inicio_seleccion, hora_fin_seleccion, hora_inicio_asignatura, hora_fin_asignatura;
            List<(string, int)> horario_disponible = horario_seleccionado;
            List<(string, int)> horas_ocupadas = new List<(string, int)>();
            using (SqlCommand cmd = new SqlCommand("ppCheck_schedule", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigo_asignatura", codigo_asignatura);
                cmd.Parameters.AddWithValue("@id_seccion", id_seccion);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        horas_ocupadas.Add((reader.GetString(0), reader.GetInt32(1)));
                    }
                }

                for (int i = 0; i < horario_disponible.Count; i++)
                {
                    instancia_hora = horario_disponible[i].Item1;

                    hora_inicio_seleccion = int.Parse(instancia_hora.Substring(0, 2));
                    hora_fin_seleccion = int.Parse(instancia_hora.Substring(3, 2));

                    for (int j = 0; j < horas_ocupadas.Count; j++)
                    {

                        hora_inicio_asignatura = int.Parse(horas_ocupadas[j].Item1.Substring(0, 2));
                        hora_fin_asignatura = int.Parse(horas_ocupadas[j].Item1.Substring(3, 2));

                        if (horario_disponible[i].Item2 == horas_ocupadas[j].Item2)
                        {
                            if (hora_inicio_asignatura <= hora_inicio_seleccion && hora_inicio_seleccion < hora_fin_asignatura && hora_fin_asignatura != hora_fin_seleccion)
                            {

                                horario_disponible[i] = ($"{Insercion.zero(hora_fin_asignatura)}/{Insercion.zero(hora_fin_seleccion)}", horario_disponible[i].Item2);
                                instancia_hora = horario_disponible[i].Item1;
                                hora_inicio_seleccion = int.Parse(instancia_hora.Substring(0, 2));
                                hora_fin_seleccion = int.Parse(instancia_hora.Substring(3, 2));
                            }
                            else if (hora_inicio_asignatura <= hora_fin_seleccion && hora_fin_seleccion < hora_fin_asignatura && hora_inicio_asignatura != hora_inicio_seleccion)
                            {

                                horario_disponible[i] = ($"{Insercion.zero(hora_inicio_seleccion)}/{Insercion.zero(hora_inicio_asignatura)}", horario_disponible[i].Item2);
                                instancia_hora = horario_disponible[i].Item1;
                                hora_inicio_seleccion = int.Parse(instancia_hora.Substring(0, 2));
                                hora_fin_seleccion = int.Parse(instancia_hora.Substring(3, 2));

                            }
                            else if (hora_inicio_asignatura >= hora_inicio_seleccion && hora_fin_asignatura <= hora_fin_seleccion)
                            {
                                if (hora_inicio_asignatura == hora_inicio_seleccion)
                                {
                                    horario_disponible[i] = ($"{Insercion.zero(hora_fin_asignatura)}/{Insercion.zero(hora_fin_seleccion)}", horario_disponible[i].Item2);
                                    instancia_hora = horario_disponible[i].Item1;
                                    hora_inicio_seleccion = int.Parse(instancia_hora.Substring(0, 2));
                                    hora_fin_seleccion = int.Parse(instancia_hora.Substring(3, 2));
                                }
                                else if (hora_fin_asignatura == hora_fin_seleccion)
                                {
                                    horario_disponible[i] = ($"{Insercion.zero(hora_inicio_seleccion)}/{Insercion.zero(hora_inicio_asignatura)}", horario_disponible[i].Item2);
                                    instancia_hora = horario_disponible[i].Item1;
                                    hora_inicio_seleccion = int.Parse(instancia_hora.Substring(0, 2));
                                    hora_fin_seleccion = int.Parse(instancia_hora.Substring(3, 2));
                                }
                                else
                                {
                                    horario_disponible[i] = ($"{Insercion.zero(hora_inicio_seleccion)}/{Insercion.zero(hora_inicio_asignatura)}", horario_disponible[i].Item2);
                                    horario_disponible.Add(($"{Insercion.zero(hora_fin_asignatura)}/{Insercion.zero(hora_fin_seleccion)}", horario_disponible[i].Item2));
                                    instancia_hora = horario_disponible[i].Item1;
                                    hora_inicio_seleccion = int.Parse(instancia_hora.Substring(0, 2));
                                    hora_fin_seleccion = int.Parse(instancia_hora.Substring(3, 2));
                                }


                            }
                            if (hora_inicio_seleccion == hora_inicio_asignatura && hora_fin_asignatura == hora_fin_seleccion)
                            {
                                horario_disponible.Remove((instancia_hora, horario_disponible[i].Item2));
                            }
                        }
                    }
                }
                return horario_disponible;
            }
        }
    }
}
