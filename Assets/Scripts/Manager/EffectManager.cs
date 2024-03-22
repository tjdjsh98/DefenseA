using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EffectManager : ManagerBase
{ 
    Dictionary<Define.EffectName,List<Effect>> _effects = new Dictionary<Define.EffectName, List<Effect>>();
    public override void Init()
    {
    }

    public override void ManagerUpdate()
    {
    }

    public Effect GetEffect(Define.EffectName effectName)
    {
        Effect result = null;

        if (!_effects.ContainsKey(effectName))
            _effects.Add(effectName, new List<Effect>());

        foreach(var effect in _effects[effectName])
        {
            if (!effect.gameObject.activeSelf)
            {
                result = effect;
            }
        }

        if (result == null)
        {
            result = Instantiate<Effect>(Managers.GetManager<DataManager>().GetData<Effect>((int)effectName));
            _effects[effectName].Add(result);
        }

        return result;
    }

    public Effect InstantiateHitEffect(Vector3 position, Vector3 direction)
    {
        Effect hitEffectOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.Hit3);
        Effect hitEffect = Managers.GetManager<ResourceManager>().Instantiate<Effect>(hitEffectOrigin);
        hitEffect.SetProperty("Direction", direction);
        hitEffect.Play(position);

        return hitEffect;
    }
}
