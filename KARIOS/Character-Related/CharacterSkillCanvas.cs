using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkillCanvas : MonoBehaviour
{
	public GameObject ba_Indicator;

	public void Set_IndicatorVisiblity(bool isVisible)
	{
		ba_Indicator.SetActive(isVisible);
	}

	public void Set_IndicatorRot(Quaternion newRotation)
	{

		ba_Indicator.transform.rotation = newRotation;
	}

	public bool Get_IndicatorVisibility()
	{
		return ba_Indicator.activeSelf;
	}


}
