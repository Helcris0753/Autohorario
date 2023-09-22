using Autohorario.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Autohorario
{
    internal class Insertion
    {
        static private Random random = new Random();

        public static void GetDataforInsertion(int SectionId, int SubjectCredits, int Modality, List<Hours> OnsideSchedule, List<Hours> WeeklyScheduleAvailable, List<Hours> VirtualSchedule, List<Hours> SelectSchedule)
        {
            //variables que se van a usar

            List<(bool, int)> InsertionValid = new List<(bool, int)>();
            List<Hours> Null = new List<Hours>();
            Null.Add(new Hours { Hour = "00/00", Day = 0 });
            List<List<Hours>> ScheduleColection = new List<List<Hours>>();
            ScheduleColection.Add(OnsideSchedule);
            ScheduleColection.Add(VirtualSchedule);
            ScheduleColection.Add(WeeklyScheduleAvailable);
            ScheduleColection.Add(SelectSchedule);
            int Day1 = 0, Day2 = 0, Day3 = 0;
            //las inserciones de las asignaturas dependeran de sus creditos.


            switch (SubjectCredits)
            {

                //las asignaturas de 0 a 2 creditos requieren metodos de insercion iguales
                case 0:
                case 1:
                case 2:
                    if (ValidateSchedule(ScheduleColection, SectionId, 2, Modality) == 0)
                        InsertionToDB(1, SectionId, 3, Modality);
                    break;
                case 3:
                    //Es el mismo proceso que el anterior, por con tres creditos
                    if (ValidateSchedule(ScheduleColection, SectionId, 3, Modality) == 0)
                        InsertionToDB(1, SectionId, 3, Modality);
                    break;
                case 4:
                case 5:
                    switch (Modality)
                    {
                        case 1:
                        case 2:
                            Day1 = ValidateSchedule(ScheduleColection, SectionId, 2, Modality);
                            if (Day1 == 0) InsertionToDB(1, SectionId, 3, Modality);
                            Day2 = ValidateSchedule(ScheduleColection, SectionId, 2, Modality, Day1);
                            if (Day2 == 0) InsertionToDB(1, SectionId, 3, Modality);

                            if (Modality == 5)
                                Day3 = ValidateSchedule(ScheduleColection, SectionId, 1, Modality, Day1, Day2);
                            break;
                        case 3:
                            ScheduleColection[1] = Null;
                            ScheduleColection[2] = Null;
                            ScheduleColection[3] = Null;
                            Day1 = ValidateSchedule(ScheduleColection, SectionId, 2, 1);
                            if (Day1 == 0)
                            {
                                ScheduleColection[1] = VirtualSchedule;
                                Day1 = ValidateSchedule(ScheduleColection, SectionId, 2, 2);
                                if (Day1 == 0)
                                {
                                    ScheduleColection[0] = Null;
                                    ScheduleColection[1] = Null;
                                    ScheduleColection[2] = WeeklyScheduleAvailable;
                                    ScheduleColection[3] = SelectSchedule;

                                    Day1 = ValidateSchedule(ScheduleColection, SectionId, 2, 1);
                                    Day2 = ValidateSchedule(ScheduleColection, SectionId, 2, 2, Day1);
                                }
                                else
                                {
                                    ScheduleColection[2] = WeeklyScheduleAvailable;
                                    ScheduleColection[3] = SelectSchedule;

                                    Day2 = ValidateSchedule(ScheduleColection, SectionId, 2, 2, Day1);
                                }
                            }
                            else
                            {
                                ScheduleColection[1] = VirtualSchedule;
                                Day2 = ValidateSchedule(ScheduleColection, SectionId, 2, 2, Day1);
                                if (Day2 == 0)
                                {
                                    ScheduleColection[2] = WeeklyScheduleAvailable;
                                    ScheduleColection[3] = SelectSchedule;

                                    Day2 = ValidateSchedule(ScheduleColection, SectionId, 2, 2, Day1);
                                }
                            }
                            if (Day1 == 0) InsertionToDB(1, SectionId, 3, 1);
                            if (Day2 == 0) InsertionToDB(1, SectionId, 3, 2);
                            if (Modality == 5)
                                Day3 = ValidateSchedule(ScheduleColection, SectionId, 1, Modality, Day1, Day2);
                            break;
                    }
                    break;
                default:
                    InsertionToDB(1, SectionId, 3, Modality);
                    break;
            }
        }

        private static int ValidateSchedule(List<List<Hours>> ScheduleColection, int SectionId, int horas, int Modality, int Day1 = 0, int Day2 = 0)
        {

            List<Hours> Schedule = new List<Hours>();
            List<(bool, int)> Validate = new List<(bool, int)>();
            int HourState = 0;
            for (int i = 0; i < ScheduleColection.Count; i++)
            {
                Schedule = ScheduleColection[i];

                if (i < 2 && Modality != 1)
                {
                    HourState = 1;
                }
                else if (i > 0 && i < 3 && Modality == 1)
                {
                    HourState = 2;
                }
                else
                {
                    HourState = i;
                }
                Validate = ValidateInsertion(Schedule, SectionId, horas, i == 0 ? 1 : HourState, Modality, Day1, Day2);
                if (Validate.First().Item1)
                {
                    return Validate.First().Item2;
                }
                else
                {
                    continue;
                }
            }
            return 0;
        }
        private static List<(bool, int)> ValidateInsertion(List<Hours> AuxiliarSchedule, int SectionId, int Hours, int HourStates, int Modality, int Day1 = 0, int Day2 = 0)
        {
            //variables a usar
            List<(bool, int)> ValidateInsertion = new List<(bool, int)>();
            List<Hours> Schedule = AuxiliarSchedule;
            Schedule = Schedule.OrderBy(x => random.Next()).ToList();
            ValidateInsertion.Add((false, 0));
            int StartHour, EndHour;
            for (int i = 0; i < Schedule.Count; i++)
            {

                StartHour = int.Parse(Schedule[i].Hour.Substring(0, 2));
                EndHour = int.Parse(Schedule[i].Hour.Substring(3, 2));
                if (HourStates == 3)
                {
                    int StartRandom = 0, EndRandom = 0;
                    if ((EndHour - StartHour) >= Hours && Schedule[i].Day != Day1 && Schedule[i].Day != Day2)
                    {
                        do
                        {
                            StartRandom = random.Next(StartHour, EndHour);
                            EndRandom = random.Next(StartRandom, EndHour + 1);
                        } while (EndRandom - StartRandom < Hours);
                        InsertionToDB(Schedule[i].Day, SectionId, HourStates, Modality, $"{zero(StartRandom)}/{zero(StartRandom + Hours)}");
                        ValidateInsertion[0] = (true, Schedule[i].Day);
                        return ValidateInsertion;
                    }
                }
                else
                {
                    if (Schedule[i].Day != Day1 && Schedule[i].Day != Day2 && (EndHour - StartHour) >= Hours)
                    {
                        InsertionToDB(Schedule[i].Day, SectionId, HourStates, Modality, $"{zero(StartHour)}/{zero(StartHour + Hours)}");
                        ValidateInsertion[0] = (true, Schedule[i].Day);
                        return ValidateInsertion;
                    }
                }
            }
            return ValidateInsertion;
        }
        private static void InsertionToDB(int Day, int SectionId, int HourState, int Modality, string Hours = "null")
        {
            var Parameters = new { 
            Hours = Hours != "null" ? Hours : (object)DBNull.Value,
            Day = Day,
            SectionId = SectionId,
            HourState = HourState,
            Modality = Modality
            };

            Obtaining.Con.Query<object>(
                "ppInsertHours",
                Parameters,
                commandType: CommandType.StoredProcedure);

        }
        public static string zero(int Hour)
        {
            return Hour < 10 ? "0" + Hour : Hour.ToString();
        }
    }
}
