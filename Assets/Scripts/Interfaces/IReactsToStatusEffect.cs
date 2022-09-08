using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReactsToStatusEffect
{
    void OnAddEffect(StatusEffect effect);
    void OnRemoveEffect(StatusEffect effect);
    void OnStackEffect(StatusEffect effect);
}
