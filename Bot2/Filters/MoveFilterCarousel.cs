using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFilterCarousel
{
    //public List<MoveFilter>[] filterCarousel;


    /// <summary>
    /// return lines of 3 filters for white moves, gets sourse from unit's personality
    /// </summary>
    /// <param name="units"></param>
    /// <returns></returns>
    public static List<MoveFilter[]> GetLinesFromUnits(UnitModel[] units, Field field = null, bool isOpening = false)
    {
        List<MoveFilter[]> lines = new List<MoveFilter[]>();
        List<MoveFilter> filters = new List<MoveFilter>();

        IMoveFilerSource[] sources = new IMoveFilerSource[3];

        for (int i = 0; i < units.Length; i++)
        {
            sources[i] = BotUnitPersonality.GetUnitPersonality(units[i].UnitType);

            filters.AddRange(isOpening ? sources[i].GetOpeningFilters(units[i], field) : sources[i].GetFilters(units[i], field));
        }

        foreach (MoveFilter filter in filters)
        {
            if (filter.Priority >= 100)
                continue;

            if (filter.HasFollowUp)
            {
                foreach (MoveFilter f2 in filters)
                {
                    if (f2.HasFollowUp)
                        continue;
                    if (f2 == filter)
                        continue;
                    if (f2.Priority < 0)
                        continue;

                    if (f2.Priority >= 100 && f2.Priority < 200)
                        continue;

                    if (f2.HasChargeCollision(filter.Followup))
                        continue;

                    if (f2.HasChargeCollision(filter)) //TODO: check if helps of breaks
                        continue;

                    if (f2.IsUnreachibleFrom(filter))
                        continue;

                    if (f2.IsUnreachibleFrom(filter.Followup)) //TODO: check if helps of breaks
                        continue;

                    MoveFilter[] line = new MoveFilter[] { filter, filter.Followup, f2 };
                    lines.Add(line);
                }
            }
            else
            {
                foreach (MoveFilter f1 in filters)
                {
                    if (f1 == filter)
                        continue;
                    if (f1.Priority < 0)
                        continue;

                    if (f1.HasChargeCollision(filter))
                        continue;

                    if (f1.IsUnreachibleFrom(filter))
                        continue;

                    if (f1.HasFollowUp)
                    {
                        continue;
                        //if (f1.Priority == 0)
                        //    continue;
                        //
                        //MoveFilter[] line = new MoveFilter[] { filter, f1, f1.Followup};
                        //lines.Add(line);
                    }
                    else
                    {
                        foreach (MoveFilter f2 in filters)
                        {
                            if (f2 == f1)
                                continue;
                            if (f2 == filter)
                                continue;
                            if (f2.Priority < 0)
                                continue;

                            if (f2.Priority >= 100 && f2.Priority < 200)
                                continue;

                            if (f2.HasChargeCollision(f1))
                                continue;
                            else if (f1.IgnoreUncharge == false && filter.HasChargeCollision(f2)) 
                                continue;

                            if (f2.IsUnreachibleFrom(f1))
                                continue;
                            else if (f2.HasEqualExecutor(f1) == false && f2.IsUnreachibleFrom(filter))
                                continue;

                            //if (f2.HasFollowUp)
                            //    continue;
                            MoveFilter[] line = new MoveFilter[] { filter, f1, f2 };
                            lines.Add(line);
                        }
                    }
                    
                }
            }
        }

        return lines;
    }

    public static List<MoveFilter[]> GetOpeningLines(UnitModel[] units, Field field)
    {
        return GetLinesFromUnits(units, field, true);
    }
}
