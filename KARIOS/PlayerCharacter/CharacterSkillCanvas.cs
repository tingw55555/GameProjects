using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkillCanvas : MonoBehaviour
{
	public GameObject ba_Indicator;

	public void SetBAIndicator_Visiblity(bool isVisible)
	{
		ba_Indicator.SetActive(isVisible);
	}

	public void SetBAIndicator_Rotation(Quaternion newRotation)
	{

		ba_Indicator.transform.rotation = newRotation;
	}

	public bool GetBAIndicator_Visibility()
	{
		return ba_Indicator.activeSelf;
	}


}
