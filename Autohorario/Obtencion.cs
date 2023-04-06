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
                List<(string, int)> horario_seleccionado = new List<(string, int)>();
                List<(string, int)> horario_secundario = new List<(string, int)>();
                //un foreach que va elemento por elemento del dataset rellenado anteriormente
                //item 0 codigo_asignatura
                //item 1 id_seccion
                //item 2 id_profesor
                //item 3 creditos_materia,
                //item 4 id_modalidad
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    //para comprar si no es una seccion hibrida la que se esta trabajando
                    if (int.Parse(item[4].ToString()) != 3)
                    {
                        //el horario seleccionado sera igual a lo que devuelva "gethorarios_seleccionado", que es el metodo que recoge el horario que selecciono el profesor
                        horario_seleccionado = gethorarios_seleccionado(int.Parse(item[2].ToString()), int.Parse(item[4].ToString()));
                        //Despues que se obtuvo el horario, se va a la siguiente etapa del algoritmo, que es la validacion.
                        Validacion.Getdata(item[0].ToString(), int.Parse(item[1].ToString()), int.Parse(item[3].ToString()), horario_seleccionado, int.Parse(item[4].ToString()), int.Parse(item[2].ToString()));
                    }
                    else
                    {
                        //si la seccion es hibrida, tomara ambos horarios seleccionados, tanto el presencial como el virtual.
                        horario_seleccionado = gethorarios_seleccionado(int.Parse(item[2].ToString()), 1);
                        horario_secundario = gethorarios_seleccionado(int.Parse(item[2].ToString()), 2);
                        //si el profesor tiene tan solo un horario seleccionado de tipo presencial o virtual, tan solo se elegira eel horario_seleccionado, en caso contrario, se pasaran ambos.
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
