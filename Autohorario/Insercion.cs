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

        public static void data_insercion(List<(string, int)> horario_presencial, List<(string, int)> horario_semanal_disponible, List<(string, int)> horario_virtual, List<(string, int)> horario_seleccionado, int id_seccion, int creditos_asignatura, int modalidad)
        {
            //variables que se van a usar
            List<(bool, int)> valida_insercion = new List<(bool, int)>();
            //las inserciones de las asignaturas dependeran de sus creditos.
            switch (creditos_asignatura)
            {
                //las asignaturas de 0 a 2 creditos requieren metodos de insercion iguales
                case 0:
                case 1:
                case 2:
                    //validar_insercion un metodo que me devuelve si se pudo insertar segun el horario seleccionado por el profesor previamente pasado por validacion
                    // y el dia correspodiente donde se inserto
                    valida_insercion = validar_insercion(horario_presencial, id_seccion, 2, 1, modalidad);
                    if (!(valida_insercion[0].Item1))
                    {
                        //si no se pudo insertar con el horario disponible, se hace con el horario semanal disponible 
                        valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, modalidad);
                        if (!(valida_insercion[0].Item1))
                        {
                            //si tampoco se pudo con el horario semanal disponible, es que el horario no tiene espacio, por lo que se colocara independientemente que choca.
                            int hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0, 2));
                            insertar(horario_seleccionado[0].Item2, id_seccion, 3, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                        }
                    }
                    break;
                case 3:
                    //Se l mimso proceso que el anterior, por con tres creditos
                    valida_insercion = validar_insercion(horario_presencial, id_seccion, 3, 1, modalidad);
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
                    // Se verifica si la modalidad de la asignatura no es hibrida
                    switch (modalidad)
                    {
                        case 1:
                            asignaturas_modalidad_presencial(horario_presencial, horario_virtual, horario_semanal_disponible, horario_seleccionado, id_seccion, creditos_asignatura);
                            break;
                        case 2:
                            asignaturas_modalidad_virtual(horario_virtual, horario_semanal_disponible, horario_seleccionado, id_seccion, creditos_asignatura);
                            break;
                        case 3:
                            asignaturas_modalidad_hibrida(horario_presencial, horario_virtual, horario_semanal_disponible, horario_seleccionado, id_seccion, creditos_asignatura);
                            break;
                    }
                    break;
                default:
                    insertar(1, id_seccion, 3, 0);
                    break;
            }
        }
        private static List<(bool, int)> validar_insercion(List<(string, int)> horario_auxiliar, int id_seccion, int horas, int estado_horas, int modalidad, int dia1 = 0, int dia2 = 0)
        {
            //variables a usar
            Random random = new Random();
            List<(bool, int)> validar_insercion = new List<(bool, int)>();
            List<(string, int)> horario = horario_auxiliar.ToList();
            horario = horario.OrderBy(x => random.Next()).ToList();
            validar_insercion.Add((false, 0));
            int hora_inicio, hora_fin;
            for (int i = 0; i < horario.Count; i++)
            {
                hora_inicio = int.Parse(horario[i].Item1.Substring(0, 2));
                hora_fin = int.Parse(horario[i].Item1.Substring(3, 2));
                if (horario[i].Item2 != dia1 && horario[i].Item2 != dia2 && (hora_fin - hora_inicio) >= horas)
                {
                    insertar(horario[i].Item2, id_seccion, estado_horas, modalidad, $"{zero(hora_inicio)}/{zero(hora_inicio + horas)}");
                    validar_insercion[0] = (true, horario[i].Item2);
                    return validar_insercion;
                }
            }
            return validar_insercion;
        }

        private static void asignaturas_modalidad_virtual(List<(string, int)> horario_virtual, List<(string, int)> horario_semanal_disponible, List<(string, int)> horario_seleccionado, int id_seccion, int creditos)
        {
            int dia1, dia2;
            List<(bool, int)> valida_insercion = new List<(bool, int)>();
            valida_insercion = validar_insercion(horario_virtual, id_seccion, 2, 1, 1);
            dia1 = valida_insercion[0].Item2;
            if (!(valida_insercion[0].Item1))
            {
                valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, 1);
                dia1 = valida_insercion[0].Item2;
                if (!(valida_insercion[0].Item1))
                {
                    dia1 = horario_seleccionado[0].Item2;
                    int hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0, 2));
                    insertar(horario_seleccionado[0].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                }
            }

            valida_insercion = validar_insercion(horario_virtual, id_seccion, 2, 1, 1, dia1);
            dia2 = valida_insercion[0].Item2;
            if (!(valida_insercion[0].Item1))
            {
                valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, 1, dia1);
                dia2 = valida_insercion[0].Item2;
                if (!(valida_insercion[0].Item1))
                {
                    for (int i = 0; i < horario_seleccionado.Count; i++)
                    {
                        if (dia1 != horario_seleccionado[i].Item2)
                        {
                            dia2 = horario_seleccionado[i].Item2;
                            int hora_inicio = int.Parse(horario_seleccionado[i].Item1.Substring(0, 2));
                            insertar(horario_seleccionado[i].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                        }
                    }
                }
            }

            if (creditos == 5)
            {
                valida_insercion = validar_insercion(horario_virtual, id_seccion, 1, 1, 1, dia1, dia2);
                if (!(valida_insercion[0].Item1))
                {
                    valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 1, 2, 1, dia1, dia2);
                    if (!(valida_insercion[0].Item1))
                    {
                        for (int i = 0; i < horario_seleccionado.Count; i++)
                        {
                            if (dia1 != horario_seleccionado[i].Item2 && dia2 != horario_seleccionado[i].Item2)
                            {
                                int hora_inicio = int.Parse(horario_seleccionado[i].Item1.Substring(0, 2));
                                insertar(horario_seleccionado[i].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 1)}");
                            }
                        }
                    }
                }
            }
        }

        private static void asignaturas_modalidad_presencial(List<(string, int)> horario_presencial, List<(string, int)> horario_virtual, List<(string, int)> horario_semanal_disponible, List<(string, int)> horario_seleccionado, int id_seccion, int creditos)
        {
            int dia1, dia2;
            List<(bool, int)> valida_insercion = new List<(bool, int)>();
            valida_insercion = validar_insercion(horario_presencial, id_seccion, 2, 1, 2);
            dia1 = valida_insercion[0].Item2;
            if (!(valida_insercion[0].Item1))
            {
                valida_insercion = validar_insercion(horario_virtual, id_seccion, 2, 1, 2);
                dia1 = valida_insercion[0].Item2;
                if (!(valida_insercion[0].Item1))
                {
                    valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, 2);
                    dia1 = valida_insercion[0].Item2;
                    if (!(valida_insercion[0].Item1))
                    {
                        dia1 = horario_seleccionado[0].Item2;
                        int hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0, 2));
                        insertar(horario_seleccionado[0].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                    }
                }
            }

            valida_insercion = validar_insercion(horario_presencial, id_seccion, 2, 1, 2, dia1);
            dia2 = valida_insercion[0].Item2;
            if (!(valida_insercion[0].Item1))
            {
                valida_insercion = validar_insercion(horario_virtual, id_seccion, 2, 1, 2, dia1);
                dia2 = valida_insercion[0].Item2;
                if (!(valida_insercion[0].Item1))
                {
                    valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, 2, dia1);
                    dia2 = valida_insercion[0].Item2;
                    if (!(valida_insercion[0].Item1))
                    {
                        for (int i = 0; i < horario_seleccionado.Count; i++)
                        {
                            if (dia1 != horario_seleccionado[i].Item2)
                            {
                                dia2 = horario_seleccionado[i].Item2;
                                int hora_inicio = int.Parse(horario_seleccionado[i].Item1.Substring(0, 2));
                                insertar(horario_seleccionado[i].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                            }
                        }
                    }
                }
                
            }

            if (creditos == 5)
            {
                valida_insercion = validar_insercion(horario_presencial, id_seccion, 1, 1, 2, dia1, dia2);
                if (!(valida_insercion[0].Item1))
                {
                    valida_insercion = validar_insercion(horario_virtual, id_seccion, 1, 1, 2, dia1, dia2);
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
                                    insertar(horario_seleccionado[i].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 1)}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void asignaturas_modalidad_hibrida(List<(string, int)> horario_presencial, List<(string, int)> horario_virtual, List<(string, int)> horario_semanal_disponible, List<(string, int)> horario_seleccionado, int id_seccion, int creditos)
        {
            int dia1, dia2;
            List<(bool, int)> valida_insercion = new List<(bool, int)>();

            valida_insercion = validar_insercion(horario_presencial, id_seccion, 2, 1, 2);
            dia1 = valida_insercion[0].Item2;
            valida_insercion = validar_insercion(horario_presencial, id_seccion, 2, 1, 1);
            if (!(valida_insercion[0].Item1))
            {
                valida_insercion = validar_insercion(horario_virtual, id_seccion, 2, 1, 1);
                if (!(valida_insercion[0].Item1))
                {
                    valida_insercion = validar_insercion(horario_semanal_disponible, id_seccion, 2, 2, 1);
                    if (!(valida_insercion[0].Item1))
                    {
                        int hora_inicio = int.Parse(horario_seleccionado[0].Item1.Substring(0, 2));
                        insertar(horario_seleccionado[0].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                    }
                }

                dia1 = valida_insercion[0].Item2;
                valida_insercion = validar_insercion(horario_virtual, id_seccion, 2, 1, 2, dia1);
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
                                insertar(horario_seleccionado[i].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                            }
                        }
                    }
                }

                if (creditos == 5)
                {
                    dia2 = valida_insercion[0].Item2;
                    valida_insercion = validar_insercion(horario_presencial, id_seccion, 1, 1, 2, dia1, dia2);
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
                                    insertar(horario_seleccionado[i].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 1)}");
                                }
                            }

                        }
                    }
                }
            }
            else
            {
                dia1 = valida_insercion[0].Item2;
                valida_insercion = validar_insercion(horario_virtual, id_seccion, 2, 1, 2, dia1);
                if (!(valida_insercion[0].Item1))
                {
                    valida_insercion = validar_insercion(horario_presencial, id_seccion, 2, 1, 2, dia1);
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
                                    insertar(horario_seleccionado[i].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 2)}");
                                }
                            }
                        }
                    }
                }

                if (creditos == 5)
                {
                    dia2 = valida_insercion[0].Item2;
                    valida_insercion = validar_insercion(horario_virtual, id_seccion, 1, 1, 2, dia1, dia2);
                    if (!(valida_insercion[0].Item1))
                    {
                        valida_insercion = validar_insercion(horario_presencial, id_seccion, 1, 1, 2, dia1, dia2);
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
                                        insertar(horario_seleccionado[i].Item2, id_seccion, 3, 1, $"{zero(hora_inicio)}/{zero(hora_inicio + 1)}");
                                    }
                                }
                            }
                        }
                    }




                }
            }
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
