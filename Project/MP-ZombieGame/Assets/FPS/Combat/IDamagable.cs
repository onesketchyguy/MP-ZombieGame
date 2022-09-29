using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public void RecieveDamage(float damage, ref bool killedTarget);
    public int GetKillScore();
}
