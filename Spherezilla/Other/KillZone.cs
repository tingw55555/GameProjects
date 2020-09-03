using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerInputController>() != null)
        {
            GameManager.instance.OnPlayerReset();
            //SceneManager.LoadScene("GameScene");
        }
    }
}
