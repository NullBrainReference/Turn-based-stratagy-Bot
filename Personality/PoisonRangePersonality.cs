using UnityEngine;

public class PoisonRangePersonality: PoisonerPersonality
{
	public PoisonRangePersonality()
	{
		unitType = UnitType.PoisonRange;
	}

	public override bool IsOfCenterAllowed()
		=> true;
}
