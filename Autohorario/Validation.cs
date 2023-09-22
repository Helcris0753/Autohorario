using Autohorario.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Autohorario
{
    internal class Validation
    {

        public static void Getdata(int SubjectId, int SectionId, int SubjectCredits, int Modality, int ProfessorId, List<Hours> SelectOnsiteSchedule, List<Hours> SelectVirtualSchedule, List<Hours> AvailableWeeklySchedule)
        {
            //foreach (var hour in SelectOnsiteSchedule)
            //{
            //    Console.WriteLine($"Hour: {hour.Hour}, Day: {hour.Day}");
            //}
            //foreach (var hour in SelectVirtualSchedule)
            //{
            //    Console.WriteLine($"Hour: {hour.Hour}, Day: {hour.Day}");
            //}

            //Console.ReadKey();
            List<Hours> AvailableOnsiteSchedule = new List<Hours>();
            List<Hours> AvailableVirtualSchedule = new List<Hours>();

            AvailableOnsiteSchedule = ValidateHours(SubjectId, ProfessorId, SectionId, SelectOnsiteSchedule);

            AvailableVirtualSchedule = ValidateHours(SubjectId, ProfessorId, SectionId, SelectVirtualSchedule);

            AvailableWeeklySchedule = ValidateHours(SubjectId, ProfessorId, SectionId, AvailableWeeklySchedule);

            //foreach (var hour in AvailableOnsiteSchedule)
            //{
            //    Console.WriteLine($"Hour: {hour.Hour}, Day: {hour.Day}");
            //}
            //foreach (var hour in AvailableVirtualSchedule)
            //{
            //    Console.WriteLine($"Hour: {hour.Hour}, Day: {hour.Day}");
            //}
            //foreach (var hour in AvailableWeeklySchedule)
            //{
            //    Console.WriteLine($"Hour: {hour.Hour}, Day: {hour.Day}");
            //}

            //Console.ReadKey();
            Insertion.GetDataforInsertion(SectionId, SubjectCredits, Modality, AvailableOnsiteSchedule, AvailableWeeklySchedule, AvailableVirtualSchedule, SelectOnsiteSchedule);
        }
        private static List<Hours> ValidateHours(int SubjectId, int ProfessorId, int SectionId, List<Hours> Schedule)
        {
            string HourInstance;
            int SelectionHourStart, SelectionHourEnd, SubjectHourStart, SubjectHourEnd, Day;
            List<Hours> AvailableSchedule = new List<Hours>();
            List<Hours> NonAvailableSchedule = new List<Hours>();
            List<Hours> SelectSchedule = Schedule;

            var Parameters = new
            {
                SubjectId = SubjectId,
                ProfessorId = ProfessorId,
                SectionId = SectionId,
            };
            NonAvailableSchedule = Obtaining.Con.Query<Hours>(
                "ppCheckSchedule",
                Parameters,
                commandType: CommandType.StoredProcedure
                ).Distinct().ToList();


            //si no hay horas ocupadas, pues AvailableSchedule se rellena con el horario_selecionado
            if (NonAvailableSchedule.Count == 0)
            {
                AvailableSchedule.AddRange(SelectSchedule);
            }

            for (int i = 0; i < SelectSchedule.Count; i++)
            {
                Day = SelectSchedule[i].Day;
                for (int j = 0; j < NonAvailableSchedule.Count; j++)
                {
                    //la nstancia hora es el intervalo de hora de horario seleccionado para el registro i
                    HourInstance = SelectSchedule[i].Hour;
                    //la hora inicio y final del intervalo contenido en instancia hora
                    SelectionHourStart = int.Parse(HourInstance.Substring(0, 2));
                    SelectionHourEnd = int.Parse(HourInstance.Substring(3, 2));
                    //la hora inicio y final del intervalo contenido en el registro j
                    SubjectHourStart = int.Parse(NonAvailableSchedule[j].Hour.Substring(0, 2));
                    SubjectHourEnd = int.Parse(NonAvailableSchedule[j].Hour.Substring(3, 2));
                    //si los Days de el horario disponible y horas ocupadas son iguales, se entrara al if
                    if (SelectSchedule[i].Day == NonAvailableSchedule[j].Day)
                    {
                        //si las horas ocupadas se solapan de tal modo:
                        //      __________ (horas seleccionadas)
                        //  _________      (horas ocupadas)
                        // Se tomara tan solo las horas que van de desde el fin de las horas ocupadas hasta el fin de las horas seleccionadas
                        // Se toma en cuenta cuando las horas de fin da ambos intervalos son iguales
                        if (SubjectHourStart <= SelectionHourStart && SubjectHourEnd < SelectionHourEnd && SelectionHourStart < SubjectHourEnd)
                        {
                            //el registro de i se modifica segun los requerimientos del if
                            //insercion.zero es un metodo que viene de la clase insercion que sirve para colocar un zero antes de numero si numero es menor a 10
                            SelectSchedule[i].Hour = $"{Insertion.zero(SubjectHourEnd)}/{Insertion.zero(SelectionHourEnd)}";
                            SelectSchedule[i].Day = Day;
                        }
                        //si las horas ocupadas se solapan de tal modo:
                        // __________       (horas seleccionadas)
                        //      _________   (horas ocupadas)
                        // Se tomara tan solo las horas que van de desde el inicio de las horas seleccionadas hasta el inicio de las horas ocupadas
                        //Se toma en cuenta cuando las horas de inicio da ambos intervalos son iguales
                        else if (SelectionHourStart < SubjectHourStart && SelectionHourEnd <= SubjectHourEnd && SelectionHourEnd > SubjectHourStart)
                        {
                            SelectSchedule[i].Hour = $"{Insertion.zero(SelectionHourStart)}/{Insertion.zero(SubjectHourStart)}";
                            SelectSchedule[i].Day = Day;
                        }
                        //si las horas ocupadas se solapan de tal modo:
                        // ___________________       (horas seleccionadas)
                        //      _________           (horas ocupadas)
                        //Se tomaran las horas que van desde el inicio de las horas selecciones hasta el de las horas ocupadas
                        //Las horas que van desde el fin de las horas ocupadas hasta el final de las seleccionadas se insertan en horario seleccionado
                        else if (SubjectHourStart > SelectionHourStart && SubjectHourEnd < SelectionHourEnd)
                        {
                            SelectSchedule[i].Hour = $"{Insertion.zero(SelectionHourStart)}/{Insertion.zero(SubjectHourStart)}";
                            SelectSchedule[i].Day = Day;
                            SelectSchedule.Add( 
                                new Hours { 
                                    Hour = $"{Insertion.zero(SubjectHourEnd)}/{Insertion.zero(SelectionHourEnd)}", 
                                    Day = Day 
                            });
                        }
                        //si no hay espacio disponible para la hora, vease en el ejemplo, se opta por poner la hora 00/00 y se elimina mas adelante:
                        //    __________      (horas seleccionadas)
                        // __________________ (horas ocupadas)
                        else if (SelectionHourStart >= SubjectHourStart && SelectionHourEnd <= SubjectHourEnd)
                        {
                            SelectSchedule[i].Hour = "00/00";
                            SelectSchedule[i].Day = Day;
                        }
                    }
                }
            }

            for (int i = 0; i < SelectSchedule.Count; i++)
            {
                SubjectHourStart = int.Parse(SelectSchedule[i].Hour.Substring(0, 2));
                SubjectHourEnd = int.Parse(SelectSchedule[i].Hour.Substring(3, 2));
                if (SubjectHourStart > 13 && SubjectHourStart % 2 != 0 && SubjectHourEnd - SubjectHourStart > 1)
                {
                    SelectSchedule[i].Hour = $"{SubjectHourStart + 1}/{SubjectHourEnd}";
                }
            }
            AvailableSchedule = SelectSchedule.Where(horario => int.Parse(horario.Hour.Substring(3, 2)) - int.Parse(horario.Hour.Substring(0, 2)) > 0).ToList();
            AvailableSchedule = AvailableSchedule.Except(NonAvailableSchedule).ToList();


            return AvailableSchedule;
        }
    }
}