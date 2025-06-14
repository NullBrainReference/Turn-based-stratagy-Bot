using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveFilerSource
{
    public List<MoveFilter> GetFilters(UnitModel unitModel, Field field = null);

    public List<MoveFilter> GetOpeningFilters(UnitModel unitModel, Field field);
}
