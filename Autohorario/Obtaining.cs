using Autohorario.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace Autohorario
{
    internal class Obtaining
    {
        internal static SqlConnection Con = new SqlConnection(ConfigurationManager.ConnectionStrings["horarioConexion"].ConnectionString);
        static void Main(string[] args)
        {
            Console.WriteLine("Milisegundos: " + Run());
        }
        public static int Run()
        {
            List<Hours> WeeklyScheduleAvailable = new List<Hours>();
            List<Hours> SelectOnsiteSchedule = new List<Hours>();
            List<Hours> SelectVirtualSchedule = new List<Hours>();

            List<InformationForDB> Info = new List<InformationForDB>();
            for (int i = 1; i <= 6; i++)
            {
                WeeklyScheduleAvailable.Add(new Hours { Hour = i != 6 ? "07/22" : "07/18", Day = i });
            }
            //clase para medir el tiempo de ejecucion del programa
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //abrir la conexion
            Con.Open();
            Info = Con.Query<InformationForDB>(
                "ppGetInformacion",
                commandType: CommandType.StoredProcedure
                ).ToList();

            foreach (var item in Info)
            {
                SelectOnsiteSchedule = GetSelectSchedule(item.ProfessorId, 1);
                SelectVirtualSchedule = GetSelectSchedule(item.ProfessorId, 2);
                Validation.Getdata(item.SubjectId, item.SectionId, item.SubjectCredits, item.ModalityId, item.ProfessorId, SelectOnsiteSchedule, SelectVirtualSchedule, WeeklyScheduleAvailable);
            }

            Con.Close();
            stopwatch.Stop();
            return int.Parse(stopwatch.ElapsedMilliseconds.ToString());
        }
        private static List<Hours> GetSelectSchedule(int ProfessorId, int Modality)
        {
            //lista que guardara el horario seleccionado por el profesor
            List<Hours> Select_Schedule = new List<Hours>();
            var Parameters = new
            {
                ProfessorId = ProfessorId,
                Modality = Modality
            };
            Select_Schedule = Con.Query<Hours>(
                "ppGetProfessorSelection",
                Parameters,
                commandType: CommandType.StoredProcedure
                ).ToList();
            return Select_Schedule;
        }
    }
}
