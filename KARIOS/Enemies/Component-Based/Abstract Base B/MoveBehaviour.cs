using UnityEngine;

public abstract class MoveBehaviour : BaseBehaviour
{
    [Header("RunTime Value")]
    public Vector3 targetPos;

    public abstract void Move();
    public abstract void Stop();

   
   
}
