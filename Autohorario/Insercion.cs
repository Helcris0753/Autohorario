using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Autohorario
{
    internal class Insercion
    {
        static private SqlConnection con = Obtencion.con;

        public static void data_insercion(List<(string, int)> horario_disponible, List<(string, int)> horario_semanal_disponible, List<(string, int)> horario_secundario_disponible, List<(string, int)> horario_seleccionado, int id_seccion, int creditos_asignatura, int modalidad)
        {
            List<(bool, int)> valida_insercion = new List<(bool, int)>();
            int dia1, dia2;
            switch (creditos_asignatura)
            {
                case 0:
                case 1:
                case 2:

                    valida_insercion = validar_insercion(horario_disponible, id_seccion, 2, 1, modalidad);
                    if (!(valida_insercion[0].Item1))
                    {
                        valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, modalidad);
                        if (!(valida_insercion[0].Item1))
                        {
                            int hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0,2));
                            insertar(horario_seleccionado[0].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                        }
                    }
                    break;
                case 3:

                    valida_insercion = validar_insercion(horario_disponible, id_seccion, 3, 1, modalidad);
                    if (!(valida_insercion[0].Item1))
                    {
                        valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 3, 2, modalidad);
                        if (!(valida_insercion[0].Item1))
                        {
                            int hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0, 2));
                            insertar(horario_seleccionado[0].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                        }
                    }
                    break;
                case 4:
                case 5:
                    if (modalidad != 3 || horario_secundario_disponible == null)
                    {
                        valida_insercion = validar_insercion(horario_disponible, id_seccion, 2, 1, modalidad);
                        if (!(valida_insercion[0].Item1))
                        {
                            valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, modalidad);
                            if (!(valida_insercion[0].Item1))
                            {
                                int hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0, 2));
                                insertar(horario_seleccionado[0].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                            }
                        }

                        dia1 = valida_insercion[0].Item2;
                        valida_insercion = validar_insercion(horario_disponible, id_seccion, 2, 1, modalidad, dia1);
                        if (!(valida_insercion[0].Item1))
                        {
                            valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, modalidad, dia1);
                            if (!(valida_insercion[0].Item1))
                            {
                                for (int i = 0; i < horario_seleccionado.Count; i++)
                                {
                                    if (dia1 != horario_seleccionado[i].Item2)
                                    {
                                        int hora_inicio = int.Parse(horario_seleccionado[i].Item1.Substring(0, 2));
                                        insertar(horario_seleccionado[i].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                                    }
                                }
                            }
                        }

                        if (creditos_asignatura == 5)
                        {
                            dia2 = valida_insercion[0].Item2;
                            valida_insercion = validar_insercion(horario_disponible, id_seccion, 1, 1, modalidad, dia1, dia2);
                            if (!(valida_insercion[0].Item1))
                            {
                                valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 1, 2, modalidad, dia1, dia2);
                                if (!(valida_insercion[0].Item1))
                                {
                                    for (int i = 0; i < horario_seleccionado.Count; i++)
                                    {
                                        if (dia1 != horario_seleccionado[i].Item2 && dia2 != horario_seleccionado[i].Item2)
                                        {
                                            int hora_inicio = int.Parse(horario_seleccionado[i].Item1.Substring(0, 2));
                                            insertar(horario_seleccionado[i].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 1)}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        valida_insercion = validar_insercion(horario_disponible, id_seccion, 2, 1, 1);
                        if (!(valida_insercion[0].Item1))
                        {
                            valida_insercion = validar_insercion(horario_secundario_disponible, id_seccion, 2, 1, 1);
                            if (!(valida_insercion[0].Item1))
                            {
                                valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, 1);
                                if (!(valida_insercion[0].Item1))
                                {
                                    int hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0, 2));
                                    insertar(horario_seleccionado[0].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                                }
                            }

                            dia1 = valida_insercion[0].Item2;
                            valida_insercion = validar_insercion(horario_secundario_disponible, id_seccion, 2, 1, 2, dia1);
                            if (!(valida_insercion[0].Item1))
                            {
                                valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, 2, dia1);
                                if (!(valida_insercion[0].Item1))
                                {
                                    for (int i = 0; i < horario_seleccionado.Count; i++)
                                    {
                                        if (dia1 != horario_seleccionado[i].Item2)
                                        {
                                            int hora_inicio = int.Parse(horario_seleccionado[i].Item1.Substring(0, 2));
                                            insertar(horario_seleccionado[i].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                                        }
                                    }
                                }
                            }

                            if (creditos_asignatura == 5)
                            {
                                dia2 = valida_insercion[0].Item2;
                                valida_insercion = validar_insercion(horario_disponible, id_seccion, 1, 1, 2, dia1, dia2);
                                if (!(valida_insercion[0].Item1))
                                {
                                    valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 1, 2, 2, dia1, dia2);
                                    if (!(valida_insercion[0].Item1))
                                    {
                                        for (int i = 0; i < horario_seleccionado.Count; i++)
                                        {
                                            if (dia1 != horario_seleccionado[i].Item2 && dia2 != horario_seleccionado[i].Item2) 
                                            {
                                                int hora_inicio = int.Parse(horario_seleccionado[i].Item1.Substring(0, 2));
                                                insertar(horario_seleccionado[i].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 1)}");
                                            }
                                        }
                                        
                                    }
                                }
                            }
                        }
                        else
                        {
                            dia1 = valida_insercion[0].Item2;
                            valida_insercion = validar_insercion(horario_secundario_disponible, id_seccion, 2, 1, 2, dia1);
                            if (!(valida_insercion[0].Item1))
                            {
                                valida_insercion = validar_insercion(horario_disponible, id_seccion, 2, 1, 2, dia1);
                                if (!(valida_insercion[0].Item1))
                                {
                                    valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, 2, dia1);
                                    if (!(valida_insercion[0].Item1))
                                    {
                                        for (int i = 0; i < horario_seleccionado.Count; i++)
                                        {
                                            if (dia1 != horario_seleccionado[i].Item2)
                                            {
                                                int hora_inicio = int.Parse(horario_seleccionado[i].Item1.Substring(0, 2));
                                                insertar(horario_seleccionado[i].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                                            }
                                        }
                                    }
                                }
                            }

                            if (creditos_asignatura == 5)
                            {
                                dia2 = valida_insercion[0].Item2;
                                valida_insercion = validar_insercion(horario_secundario_disponible, id_seccion, 1, 1, 2, dia1, dia2);
                                if (!(valida_insercion[0].Item1))
                                {
                                    valida_insercion = validar_insercion(horario_disponible, id_seccion, 1, 1, 2, dia1, dia2);
                                    if (!(valida_insercion[0].Item1))
                                    {
                                        valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 1, 2, 2, dia1, dia2);
                                        if (!(valida_insercion[0].Item1))
                                        {
                                            for (int i = 0; i < horario_seleccionado.Count; i++)
                                            {
                                                if (dia1 != horario_seleccionado[i].Item2 && dia2 != horario_seleccionado[i].Item2)
                                                {
                                                    int hora_inicio = int.Parse(horario_seleccionado[i].Item1.Substring(0, 2));
                                                    insertar(horario_seleccionado[i].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 1)}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }

                    break;
                default:
                    insertar(0, id_seccion, 3, 0);
                    break;
            }
        }
        private static List<(bool, int)> validar_insercion(List<(string, int)> horario, int id_seccion, int horas, int estado_horas, int modaliidad, int dia1 = 0, int dia2 = 0)
        {
            List<(bool, int)> validar_insercion = new List<(bool, int)>();
            validar_insercion.Add((false, 0));
            int hora_inicio = 0;
            for (int i = 0; i < horario.Count; i++)
            {
                hora_inicio = int.Parse(horario[i].Item1.Substring(0, 2));

                if (horario[i].Item2 != dia1 && horario[i].Item2 != dia2)
                {
                    insertar(horario[i].Item2, id_seccion, estado_horas, modaliidad, $"{zero(hora_inicio)}/{zero(hora_inicio + horas)}");
                    validar_insercion[0] = (true, horario[i].Item2);
                    return validar_insercion;
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

                cmd.Parameters.AddWithValue("@hora", hora);
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
