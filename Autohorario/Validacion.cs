﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Autohorario
{
    internal class Validacion
    {
        //se pasa el SqlConnection de la clase obtencion a la clase validadcion
        static private SqlConnection con = Obtencion.con;

        //metodo que obtiene la data necesaria para validad las horas disponibles 
        public static void Getdata(string codigo_asignatura, int id_seccion, int creditos_asignatura, List<(string, int)> horario_seleccionado, int modalidad, int id_profesor, List<(string, int)> horario_semanal_disponible,List<(string, int)> horario_secundario = null)
        {
            //listas necesarias para ser rellenadas de informacion
            List<(string, int)> horario_disponible = new List<(string, int)>();
            List<(string, int)> horario_secundario_disponible = new List<(string, int)>();

            //Se pasa horario seleccionado junto con otras informaciones para rellenar horario disponible
            horario_disponible = Validarhoras(codigo_asignatura, id_profesor, id_seccion,horario_seleccionado);

            //Se pasa horario semanal junto con otras informaciones para que horario semanal se corte segun las horas ocupadas
            horario_semanal_disponible = Validarhoras(codigo_asignatura, id_profesor, id_seccion,horario_semanal_disponible);

            //Si horario secundario no es nulo entonces la asignatura es hibrida o el profesor solo tiene una modalidad de horario seleccionado, por consiguiente se hace lo mismo para el horario secundario
            if (horario_secundario != null)
            {
                horario_secundario_disponible = Validarhoras(codigo_asignatura, id_profesor, id_seccion, horario_secundario);
                horario_secundario_disponible = horario_secundario_disponible.Where(horario => int.Parse(horario.Item1.Substring(3, 2)) - int.Parse(horario.Item1.Substring(0, 2)) >= 2).ToList();
            }
            //cuando ya se validaron las horas disponible, se pasa a la sigueinte parte del algoritmo, la insercion.
            Insercion.data_insercion(horario_disponible, horario_semanal_disponible, horario_secundario_disponible, horario_seleccionado, id_seccion, creditos_asignatura, modalidad);
        }
        //metodo para validar las horas disponibles
        private static List<(string, int)> Validarhoras(string codigo_asignatura, int id_profesor, int id_seccion, List<(string, int)> h_s)
        {
            //variables a usar.
            string instancia_hora = null;
            int hora_inicio_seleccion, hora_fin_seleccion, hora_inicio_asignatura, hora_fin_asignatura, dia;
            List<(string, int)> horario_disponible = new List<(string, int)>();
            List<(string, int)> horas_ocupadas = new List<(string, int)>();
            List<(string, int)> horario_seleccionado = h_s.ToList();
            //todo dentro del using pertenece al store procedure ppCheck_schedule las variables que se le pasan son el codigo de la asignatua y el id del profesor
            //este store procedure devuelve las horas coupadas por todas las asignaturas que estan junta a la asignatura cuyo codigo se paso
            //ademas, tambien se le pasa el id del profesor para evitar que si un profesor esta dando diferentes secciones de una misma asignatura, estan no choque
            using (SqlCommand cmd = new SqlCommand("ppCheck_schedule", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigo_asignatura", codigo_asignatura);
                cmd.Parameters.AddWithValue("@id_profesor", id_profesor);
                cmd.Parameters.AddWithValue("@id_seccion", id_seccion);

                //en el using se ejecutaa el data reader para rellenar la lista horas ocupadas con los datos que de datareader
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        horas_ocupadas.Add((reader.GetString(0), reader.GetInt32(1)));
                    }
                }
                //se eliminan los registros duplicados 
                horas_ocupadas = horas_ocupadas.Distinct().ToList();

                //si no hay horas ocupadas, pues horario_disponible se rellena con el horario_selecionado
                if (horas_ocupadas.Count == 0)
                {
                    horario_disponible.AddRange(horario_seleccionado);
                }

                //un for que va registro por registro en el horario disponible
                for (int i = 0; i < horario_seleccionado.Count; i++)
                {
                    dia = horario_seleccionado[i].Item2;
                    for (int j = 0; j < horas_ocupadas.Count; j++)
                    {
                        //la nstancia hora es el intervalo de hora de horario seleccionado para el registro i
                        instancia_hora = horario_seleccionado[i].Item1;
                        //la hora inicio y final del intervalo contenido en instancia hora
                        hora_inicio_seleccion = int.Parse(instancia_hora.Substring(0, 2));
                        hora_fin_seleccion = int.Parse(instancia_hora.Substring(3, 2));
                        //la hora inicio y final del intervalo contenido en el registro j
                        hora_inicio_asignatura = int.Parse(horas_ocupadas[j].Item1.Substring(0, 2));
                        hora_fin_asignatura = int.Parse(horas_ocupadas[j].Item1.Substring(3, 2));
                        //si los dias de el horario disponible y horas ocupadas son iguales, se entrara al if
                        if (horario_seleccionado[i].Item2 == horas_ocupadas[j].Item2)
                        {
                            //si las horas ocupadas se solapan de tal modo:
                            //      __________ (horas seleccionadas)
                            //  _________      (horas ocupadas)
                            // Se tomara tan solo las horas que van de desde el fin de las horas ocupadas hasta el fin de las horas seleccionadas
                            // Se toma en cuenta cuando las horas de fin da ambos intervalos son iguales
                            if (hora_inicio_asignatura <= hora_inicio_seleccion && hora_fin_asignatura < hora_fin_seleccion && hora_inicio_seleccion < hora_fin_asignatura)
                            {
                                //el registro de i se modifica segun los requerimientos del if
                                //insercion.zero es un metodo que viene de la clase insercion que sirve para colocar un zero antes de numero si numero es menor a 10
                                horario_seleccionado[i] = ($"{Insercion.zero(hora_fin_asignatura)}/{Insercion.zero(hora_fin_seleccion)}", dia);
                            }
                            //si las horas ocupadas se solapan de tal modo:
                            // __________       (horas seleccionadas)
                            //      _________   (horas ocupadas)
                            // Se tomara tan solo las horas que van de desde el inicio de las horas seleccionadas hasta el inicio de las horas ocupadas
                            //Se toma en cuenta cuando las horas de inicio da ambos intervalos son iguales
                            else if (hora_inicio_seleccion < hora_inicio_asignatura && hora_fin_seleccion <= hora_fin_asignatura && hora_fin_seleccion > hora_inicio_asignatura)
                            {
                                horario_seleccionado[i] = ($"{Insercion.zero(hora_inicio_seleccion)}/{Insercion.zero(hora_inicio_asignatura)}", dia);
                            }
                            //si las horas ocupadas se solapan de tal modo:
                            // ___________________       (horas seleccionadas)
                            //      _________           (horas ocupadas)
                            //Se tomaran las horas que van desde el inicio de las horas selecciones hasta el de las horas ocupadas
                            //Las horas que van desde el fin de las horas ocupadas hasta el final de las seleccionadas se insertan en horario seleccionado
                            else if (hora_inicio_asignatura > hora_inicio_seleccion && hora_fin_asignatura < hora_fin_seleccion)
                            {
                                horario_seleccionado[i] = ($"{Insercion.zero(hora_inicio_seleccion)}/{Insercion.zero(hora_inicio_asignatura)}", dia);
                                horario_seleccionado.Add(($"{Insercion.zero(hora_fin_asignatura)}/{Insercion.zero(hora_fin_seleccion)}", dia));
                            }
                            //si no hay espacio disponible para la hora, vease en el ejemplo, si opta por poner la hora 00/00 y se elimina mas adelante:
                            //    __________      (horas seleccionadas)
                            // __________________ (horas ocupadas)
                            else if (hora_inicio_seleccion >= hora_inicio_asignatura && hora_fin_seleccion <= hora_fin_asignatura)
                            {
                                horario_seleccionado[i] = ("00/00", dia);
                            }
                        }
                    }
                }

                
                horario_disponible = horario_seleccionado.Where(horario => int.Parse(horario.Item1.Substring(3, 2)) - int.Parse(horario.Item1.Substring(0, 2)) > 0 ).ToList();
                horario_disponible = horario_disponible.Except(horas_ocupadas).ToList();
                
                return horario_disponible;
            }
        }
    }
}