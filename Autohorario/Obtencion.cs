using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Autohorario
{
    internal class Obtencion
    {
        internal static SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["horarioConexion"].ConnectionString);
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            con.Open();
            using (SqlCommand command = new SqlCommand("ppGetasignatura", con))
            {
                command.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataSet ds = new DataSet();

                adapter.Fill(ds);

                List<(string, int)> horario_seleccionado = new List<(string, int)>();
                List<(string, int)> horario_secundario = new List<(string, int)>();
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    if (int.Parse(item[4].ToString()) != 3)
                    {
                        horario_seleccionado = gethorarios_seleccionado(int.Parse(item[2].ToString()), int.Parse(item[4].ToString()));
                        Validacion.Getdata(item[0].ToString(), int.Parse(item[1].ToString()), int.Parse(item[3].ToString()), horario_seleccionado, int.Parse(item[4].ToString()), int.Parse(item[2].ToString()));
                    }
                    else
                    {
                        horario_seleccionado = gethorarios_seleccionado(int.Parse(item[2].ToString()), 1);
                        horario_secundario = gethorarios_seleccionado(int.Parse(item[2].ToString()), 2);
                        if (horario_seleccionado.Equals(horario_secundario))
                        {
                            Validacion.Getdata(item[0].ToString(), int.Parse(item[1].ToString()), int.Parse(item[3].ToString()), horario_seleccionado, int.Parse(item[4].ToString()), int.Parse(item[2].ToString()));

                        }
                        else
                        {
                            Validacion.Getdata(item[0].ToString(), int.Parse(item[1].ToString()), int.Parse(item[3].ToString()), horario_seleccionado, int.Parse(item[4].ToString()), int.Parse(item[2].ToString()), horario_secundario);
                        }
                    }
                }
            }
            stopwatch.Stop();
            Console.WriteLine("Tiempo transcurrido: {0} ms", stopwatch.ElapsedMilliseconds);
            Console.ReadKey();
        }
        private static List<(string, int)> gethorarios_seleccionado(int id_profesor, int id_modalidad)
        {
            List<(string, int)> horario_seleccionado = new List<(string, int)>();

            using (SqlCommand cmd = new SqlCommand("Gethorarios_seleccionado", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_profesor", id_profesor);
                cmd.Parameters.AddWithValue("@id_modalidad", id_modalidad);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        horario_seleccionado.Add((reader.GetString(0), reader.GetInt32(1)));
                    }
                }
            }
            return horario_seleccionado;
        }
    }
}
