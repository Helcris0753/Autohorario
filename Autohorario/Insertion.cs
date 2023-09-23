using Autohorario.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;

namespace Autohorario
{
    internal class Insertion
    {
        static private Random random = new Random();

        public static void GetDataforInsertion(int SectionId, int SubjectCredits, int Modality, List<Hours> OnsideSchedule,  List<Hours> VirtualSchedule, List<Hours> WeeklyScheduleAvailable, List<Hours> SelectSchedule)
        {
            //variables que se van a usar

            List<(bool, int)> InsertionValid = new List<(bool, int)>();
            List<Hours> Null = new List<Hours>();
            Hours RHour = new Hours();
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
                    {
                        RHour = RandomHour(2);
                        InsertionToDB(RHour.Day, SectionId, 3, Modality, RHour.Hour);
                    }
                    break;
                case 3:
                    //Es el mismo proceso que el anterior, por con tres creditos
                    if (ValidateSchedule(ScheduleColection, SectionId, 3, Modality) == 0)
                    {
                        RHour = RandomHour(3);
                        InsertionToDB(RHour.Day, SectionId, 3, Modality, RHour.Hour);
                    }
                    break;
                case 4:
                case 5:
                    switch (Modality)
                    {
                        case 1:
                        case 2:
                            Day1 = ValidateSchedule(ScheduleColection, SectionId, 2, Modality);
                            if (Day1 == 0) 
                            {
                                RHour = RandomHour(2);
                                InsertionToDB(RHour.Day, SectionId, 3, Modality, RHour.Hour);
                                Day1 = RHour.Day;
                            }
                            Day2 = ValidateSchedule(ScheduleColection, SectionId, 2, Modality, Day1);
                            if (Day2 == 0) 
                            {
                                    RHour = RandomHour(2, Day1);
                                    InsertionToDB(RHour.Day, SectionId, 3, Modality, RHour.Hour);
                                    Day2 = RHour.Day;
                            }

                            if (SubjectCredits == 5)
                                Day3 = ValidateSchedule(ScheduleColection, SectionId, 1, Modality, Day1, Day2);
                            break;
                        case 3:
                            ScheduleColection[1] = Null;
                            ScheduleColection[2] = Null;
                            ScheduleColection[3] = Null;
                            Day1 = ValidateSchedule(ScheduleColection, SectionId, 2, 1);
                            if (Day1 == 0)
                            {
                                ScheduleColection[0] = Null;
                                ScheduleColection[1] = VirtualSchedule;
                                Day1 = ValidateSchedule(ScheduleColection, SectionId, 2, 2);
                                if (Day1 == 0)
                                {
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
                                ScheduleColection[0] = Null;
                                ScheduleColection[1] = VirtualSchedule;
                                Day2 = ValidateSchedule(ScheduleColection, SectionId, 2, 2, Day1);
                                if (Day2 == 0)
                                {
                                    ScheduleColection[0] = OnsideSchedule;
                                    ScheduleColection[2] = WeeklyScheduleAvailable;
                                    ScheduleColection[3] = SelectSchedule;

                                    Day2 = ValidateSchedule(ScheduleColection, SectionId, 2, 2, Day1);
                                }
                            }

                            if (Day1 == 0) 
                            {
                                    RHour = RandomHour(2);
                                    InsertionToDB(RHour.Day, SectionId, 3, Modality, RHour.Hour);
                                    Day1 = RHour.Day;
                            }
                            if (Day2 == 0)
                            {
                                RHour = RandomHour(2, Day1);
                                InsertionToDB(RHour.Day, SectionId, 3, Modality, RHour.Hour);
                                Day2 = RHour.Day;
                            }
                            if (SubjectCredits == 5)
                                Day3 = ValidateSchedule(ScheduleColection, SectionId, 1, Modality, Day1, Day2);
                            break;
                    }
                    break;
                default:
                    InsertionToDB(1, SectionId, 3, Modality);
                    break;
            }
        }

        private static int ValidateSchedule(List<List<Hours>> ScheduleColection, int SectionId, int Hours, int Modality, int Day1 = 0, int Day2 = 0)
        {

            List<Hours> Schedule = new List<Hours>();
            ValidateInsertion Validate = new ValidateInsertion();
            int count = ScheduleColection.Count;
            int HourState;
            for (int i = 0; i < count; i++)
            {
                Schedule = ScheduleColection[i];

                HourState = (i == Modality - 1) ? 1 : (i == count) ? 3 : 2;

                Validate = ValidateInsertion(Schedule, SectionId, Hours, HourState, Modality, Day1, Day2);

                if (!Validate.Validation)
                {
                    continue;
                }

                return Validate.Day;

            }
            return 0;
        }
        private static ValidateInsertion ValidateInsertion(List<Hours> AuxiliarSchedule, int SectionId, int Hours, int HourStates, int Modality, int Day1 = 0, int Day2 = 0)
        {
            //foreach (var hour in AuxiliarSchedule)
            //{
            //    Console.WriteLine($"Hour: {hour.Hour}, Day: {hour.Day}, Hours:{Hours}" );
            //}
            //variables a usar
            ValidateInsertion ValidateInsertion = new ValidateInsertion() { 
                Validation = false,
                Day = 0
            };
            List<Hours> Schedule = AuxiliarSchedule;
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
                        ValidateInsertion.Validation = true;
                        ValidateInsertion.Day = Schedule[i].Day;
                        return ValidateInsertion;
                    }
                }
                else
                {
                    if ((EndHour - StartHour) >= Hours && Schedule[i].Day != Day1 && Schedule[i].Day != Day2)
                    {
                        InsertionToDB(Schedule[i].Day, SectionId, HourStates, Modality, $"{zero(StartHour)}/{zero(StartHour + Hours)}");
                        ValidateInsertion.Validation = true;
                        ValidateInsertion.Day = Schedule[i].Day;
                        return ValidateInsertion;
                    }
                }
            }
            return ValidateInsertion;
        }
        private static void InsertionToDB(int Day, int SectionId, int HourState, int Modality, string Hours = "null")
        {
            if (Hours == "null" && false)
            {
                Console.ReadKey();
            }
            //Console.WriteLine(Hours);
            //Console.ReadKey();
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
        private static Hours RandomHour(int SubjectHours, int ExcludeDay = 0) { 
            Hours hours = new Hours();
            Random random = new Random();
            int StartHour, EndHour, Day ;

            if (ExcludeDay == 0)
            {
                Day = random.Next(1, 7);
            }
            else
            {
                do
                {
                    Day = random.Next(1, 7);

                } while (Day == ExcludeDay);
            }
            
            StartHour = random.Next(7, Day == 6 ? 15 : 21);
            if ((StartHour == 16 || StartHour == 20) && SubjectHours > 2)
            {
                StartHour = StartHour - 1;
                EndHour = StartHour + 3;
            }
            else
            {
                EndHour = StartHour + SubjectHours;
            }


            hours.Hour = $"{zero(StartHour)}/{zero(EndHour)}";
            hours.Day = Day;

            return hours;
        }
    }
}
