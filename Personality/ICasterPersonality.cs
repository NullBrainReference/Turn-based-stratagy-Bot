using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICasterPersonality
{
    public abstract int GetAbilityWeight(BotTileModel tileFrom, BotTileModel tileTo);
    public abstract void OnPush(BotTileModel tileFrom, BotTileModel tileTo);
}
