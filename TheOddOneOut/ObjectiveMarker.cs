using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveMarker : MonoBehaviour
{
    public float disappearTime = 5f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerInputController>() !=null)
        {
            GameManager.instance.OnOddOneCapture();
            Invoke("Deactivate", disappearTime);
        }   
    }

    public void Deactivate()
    {
        AudioManager.instance.OnObjectiveDisappear();
        gameObject.SetActive(false);
    }


}
