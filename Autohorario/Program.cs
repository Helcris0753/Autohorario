﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autohorario
{
    internal class Obtencion
    {
        internal static SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["horarioConexion"].ConnectionString);
        static void Main(string[] args)
        {
            con.Open();
            using (SqlCommand command = new SqlCommand("ppGetasignatura", con))
            {
                command.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataSet ds = new DataSet();

                adapter.Fill(ds);

                List<(string, int)> horario_seleccionado = new List<(string, int)>();

                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    horario_seleccionado = gethorarios_seleccionado(int.Parse(item[2].ToString()));
                    Validacion.Getdata(item[0].ToString(), int.Parse(item[1].ToString()), int.Parse(item[2].ToString()), int.Parse(item[3].ToString()), horario_seleccionado);
                }
            }



        }
        private static List<(string, int)> gethorarios_seleccionado(int id_profesor)
        {
            List<(string, int)> horario_seleccionado = new List<(string, int)>();

            using (SqlCommand cmd = new SqlCommand("Gethorarios_seleccionado", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_profesor", id_profesor);

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
