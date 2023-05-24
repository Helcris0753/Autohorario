using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Autohorario
{
    internal class Insercion
    {
        static private SqlConnection con = Obtencion.con;
        static private Random random = new Random();

        public static void data_insercion(List<(string, int)> horario_presencial, List<(string, int)> horario_semanal_disponible, List<(string, int)> horario_virtual, List<(string, int)> horario_seleccionado, int id_seccion, int creditos_asignatura, int modalidad)
        {
            //variables que se van a usar
            
            List<(bool, int)> valida_insercion = new List<(bool, int)>();
            List<(string, int)> nulo = new List<(string, int)>{("00/00", 0)};
            List<List<(string, int)>> coleccion_horarios = new List<List<(string, int)>>();
            coleccion_horarios.Add(horario_presencial.ToList());
            coleccion_horarios.Add(horario_virtual.ToList());
            coleccion_horarios.Add(horario_semanal_disponible.ToList());
            coleccion_horarios.Add(horario_seleccionado.ToList());
            int dia1 = 0, dia2 = 0, dia3 = 0;
            //las inserciones de las asignaturas dependeran de sus creditos.
            

            switch (creditos_asignatura)
            {

                //las asignaturas de 0 a 2 creditos requieren metodos de insercion iguales
                case 0:
                case 1:
                case 2:
                       if (validar_horario(coleccion_horarios, id_seccion, 2, modalidad) == 0) 
                            insertar(1, id_seccion, 3, modalidad);
                    break;
                case 3:
                    //Se l mimso proceso que el anterior, por con tres creditos
                    if (validar_horario(coleccion_horarios, id_seccion, 3, modalidad) == 0)
                        insertar(1, id_seccion, 3, modalidad);
                    break;
                case 4:
                case 5:
                    switch (modalidad)
                    {
                        case 1:
                        case 2:
                            dia1 = validar_horario(coleccion_horarios, id_seccion, 2, modalidad);
                            if(dia1 == 0) insertar(1, id_seccion, 3, modalidad);
                            dia2 = validar_horario(coleccion_horarios, id_seccion, 2, modalidad, dia1);
                            if (dia2 == 0) insertar(1, id_seccion, 3, modalidad);

                            if (modalidad == 5)
                                dia3 = validar_horario(coleccion_horarios, id_seccion, 1, modalidad, dia1, dia2);
                            break;
                        case 3:
                            coleccion_horarios[1] = nulo.ToList();
                            coleccion_horarios[2] = nulo.ToList();
                            coleccion_horarios[3] = nulo.ToList();
                            dia1 = validar_horario(coleccion_horarios, id_seccion, 2, 1);
                            if (dia1 == 0)
                            {
                                coleccion_horarios[1] = horario_virtual.ToList();
                                dia1 = validar_horario(coleccion_horarios, id_seccion, 2, 2);
                                if (dia1 == 0)
                                {
                                    coleccion_horarios[0] = nulo.ToList();
                                    coleccion_horarios[1] = nulo.ToList();
                                    coleccion_horarios[2] = horario_semanal_disponible.ToList();
                                    coleccion_horarios[3] = horario_seleccionado.ToList();

                                    dia1 = validar_horario(coleccion_horarios, id_seccion, 2, 1);
                                    dia2 = validar_horario(coleccion_horarios, id_seccion, 2, 2, dia1);
                                }
                                else
                                {
                                    coleccion_horarios[2] = horario_semanal_disponible.ToList();
                                    coleccion_horarios[3] = horario_seleccionado.ToList();

                                    dia2 = validar_horario(coleccion_horarios, id_seccion, 2, 2, dia1);
                                }
                            }
                            else
                            {
                                coleccion_horarios[1] = horario_virtual.ToList();
                                dia2 = validar_horario(coleccion_horarios, id_seccion, 2, 2, dia1);
                                if (dia2 == 0)
                                {
                                    coleccion_horarios[2] = horario_semanal_disponible.ToList();
                                    coleccion_horarios[3] = horario_seleccionado.ToList();

                                    dia2 = validar_horario(coleccion_horarios, id_seccion, 2, 2, dia1);
                                }
                            }
                            if (dia1 == 0) insertar(1, id_seccion, 3, 1);
                            if (dia2 == 0) insertar(1, id_seccion, 3, 2);
                            if (modalidad == 5)
                                dia3 = validar_horario(coleccion_horarios, id_seccion, 1, modalidad, dia1, dia2);
                            break;
                    }
                    break;
                default:
                    insertar(1, id_seccion, 3, modalidad);
                    break;
            }
        }

        private static int validar_horario(List<List<(string, int)>> coleccion_horarios, int id_seccion, int horas, int modalidad, int dia1 = 0, int dia2 = 0) {
            
            List<(string, int)> horario = new List<(string, int)> ();
            List<(bool, int)> validar = new List<(bool, int)> ();
            int estado_horas = 0;
            for (int i = 0; i < coleccion_horarios.Count; i++)
            {
                horario = coleccion_horarios[i].ToList();

                if (i < 2 && modalidad != 1)
                {
                    estado_horas = 1;
                }
                else if (i > 0 && i < 3 && modalidad == 1)
                {
                    estado_horas = 2;
                }
                else
                {
                    estado_horas = i;
                }

                if (id_seccion == 8504)
                {
                    Console.WriteLine("ver");
                    for (int j = 0; j < horario.Count; j++)
                    {
                        Console.WriteLine(i + "     " + horario[j].Item1 + ' ' + horario[j].Item2 + ' ' + estado_horas + "  " + modalidad);
                    }
                    Console.WriteLine("_____________________________________");
                }
                    validar = validar_insercion(horario, id_seccion, horas, i == 0 ? 1 : estado_horas, modalidad, dia1, dia2);
                if (validar.First().Item1){
                    return validar.First().Item2;
                }
                else
                {
                    continue;
                }
            }
            return 0;
        }
        private static List<(bool, int)> validar_insercion(List<(string, int)> horario_auxiliar, int id_seccion, int horas, int estado_horas, int modalidad, int dia1 = 0, int dia2 = 0)
        {
            //variables a usar
            List<(bool, int)> validar_insercion = new List<(bool, int)>();
            List<(string, int)> horario = horario_auxiliar.ToList();
            horario = horario.OrderBy(x => random.Next()).ToList();
            validar_insercion.Add((false, 0));
            int hora_inicio, hora_fin;
            for (int i = 0; i < horario.Count; i++)
            {

                hora_inicio = int.Parse(horario[i].Item1.Substring(0, 2));
                hora_fin = int.Parse(horario[i].Item1.Substring(3, 2));
                if (estado_horas == 3)
                {
                    int inicio_random = 0, fin_random = 0;
                    if ((hora_fin - hora_inicio) >= horas && horario[i].Item2 != dia1 && horario[i].Item2 != dia2)
                    {
                        do
                        {
                            inicio_random = random.Next(hora_inicio,hora_fin);
                            fin_random = random.Next(inicio_random, hora_fin + 1);
                        } while (fin_random - inicio_random < horas);
                        insertar(horario[i].Item2, id_seccion, estado_horas, modalidad, $"{zero(inicio_random)}/{zero(inicio_random + horas)}");
                        validar_insercion[0] = (true, horario[i].Item2);
                        return validar_insercion;
                    }
                }
                else
                {
                    if (horario[i].Item2 != dia1 && horario[i].Item2 != dia2 && (hora_fin - hora_inicio) >= horas)
                    {
                        insertar(horario[i].Item2, id_seccion, estado_horas, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + horas)}");
                        validar_insercion[0] = (true, horario[i].Item2);
                        return validar_insercion;
                    }
                }
            }
            return validar_insercion;
        }
        private static void insertar(int id_dia, int id_seccion, int estado_hora, int modalidad, string hora = null)
        {
            int version_trimestral = 0;
            using (SqlCommand command = new SqlCommand("ppGetmaxVersion_trimestral", con))
            {
                command.CommandType = CommandType.StoredProcedure;
                version_trimestral = (int)command.ExecuteScalar();
            }
            using (SqlCommand cmd = new SqlCommand("ppInsert_hours", con))
            {

                //Console.WriteLine($"{hora}  {id_dia}  {id_seccion}");
                cmd.CommandType = CommandType.StoredProcedure;
                if (hora == null)
                {
                    cmd.Parameters.AddWithValue("@hora", DBNull.Value);

                }
                else
                {
                    cmd.Parameters.AddWithValue("@hora", hora);
                }
                cmd.Parameters.AddWithValue("@id_dia", id_dia);
                cmd.Parameters.AddWithValue("@id_seccion", id_seccion);
                cmd.Parameters.AddWithValue("@estado_hora", estado_hora);
                cmd.Parameters.AddWithValue("@id_version_trimestral", version_trimestral);
                cmd.Parameters.AddWithValue("@id_modalidad", modalidad);
                cmd.ExecuteNonQuery();
            }
        }
        public static string zero(int hora)
        {
            return hora < 10 ? "0" + hora : hora.ToString();
        }
    }
}
