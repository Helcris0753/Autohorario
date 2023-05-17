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
            List<(string, int)> horario_semanal_disponible = new List<(string, int)>() {("07/22", 1), ("07/22", 2), ("07/22", 3), ("07/22", 4), ("07/22", 5), ("07/18", 6)};

            //clase para medir el tiempo de ejecucion del programa
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //abrir la conexion
            con.Open();
            //todo dentro de using permete a la ejecucion del store procedure "ppGetasignatura"
            using (SqlCommand command = new SqlCommand("ppGetasignatura", con))
            {
                //Especifico que lo que se va a ejecutar es un store procedure
                command.CommandType = CommandType.StoredProcedure;

                //Creo un dataAdapter para tomar la informacion del store procedure
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                //creo un dataset para rellenarlo con la informacion del dataset
                DataSet ds = new DataSet();
                //Relleno el dataset con la informacion del dataadpter para mas facilidad de manejo
                adapter.Fill(ds);

                //creo dos listas que se rellenaran con los horarios se selecciono el profesor, la primera es para cuando la modalidad de la seccion y del horario que selecciono el profesor coincidan y la segunda es para los horarios hibridos
                List<(string, int)> horario_presencial = new List<(string, int)>();
                List<(string, int)> horario_virtual = new List<(string, int)>();
                //un foreach que va elemento por elemento del dataset rellenado anteriormente
                //item 0 codigo_asignatura
                //item 1 id_seccion
                //item 2 id_profesor
                //item 3 creditos_materia,
                //item 4 id_modalidad
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    horario_presencial = gethorarios_seleccionado(int.Parse(item[2].ToString()), 1);
                    horario_virtual = gethorarios_seleccionado(int.Parse(item[2].ToString()), 2);
                    Validacion.Getdata(item[0].ToString(), int.Parse(item[1].ToString()), int.Parse(item[3].ToString()), horario_presencial, int.Parse(item[4].ToString()), int.Parse(item[2].ToString()), horario_semanal_disponible, horario_virtual);
                }
            }
            //cierre de la conexion
            con.Close();
            stopwatch.Stop();
            Console.WriteLine("Tiempo transcurrido: {0} ms", stopwatch.ElapsedMilliseconds);
            Console.ReadKey();
        }
        private static List<(string, int)> gethorarios_seleccionado(int id_profesor, int id_modalidad)
        {
            //lista que guardara el horario seleccionado por el profesor
            List<(string, int)> horario_seleccionado = new List<(string, int)>();

            //todo lo que esta dentro del using pertence al store procedure "Gethorarios_seleccionado" que devuelve el horario seleccionado por un profesor en especifico y segun la modalidad
            //cabe destacar que si no encuentra el horario con una modalidad en espeficico, el store procedure devolvera el horario que seleccionó el profesor de la otra modalidad
            using (SqlCommand cmd = new SqlCommand("Gethorarios_seleccionado", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_profesor", id_profesor);
                cmd.Parameters.AddWithValue("@id_modalidad", id_modalidad);

                //dentro del using, se crea un datareader que se rellena no los datos que le devolvió el store procedure 
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    //en el while se rellena la lista con los datos del data reader
                    while (reader.Read())
                    {
                        horario_seleccionado.Add((reader.GetString(0), reader.GetInt32(1)));
                    }
                }
            }
            //se devuelve el horario_seleccionado
            return horario_seleccionado;
        }
    }
}
