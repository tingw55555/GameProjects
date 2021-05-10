using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotKey : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _text;

	public static event Action<int> onHotKeyClicked;
	public static event Action<int> onHotKeyDoubleClicked;


	private KeyCode _keyCode;
	private int _keynumber;
	public Image icon;
	public Color disconnectedColour = new Color(0,0,0,200);

	public float doubleClickDelay = 0.5f;

	private bool waitingForDouble = false;

	private void OnValidate()
	{
		InitializeKey();

	}

	public void InitializeKey()
	{
		_keynumber = transform.GetSiblingIndex() + 1;
		_keyCode = KeyCode.Alpha0 + _keynumber;

		if (_text == null) _text = GetComponent<TextMeshProUGUI>();
		_text.SetText(_keynumber.ToString());
		gameObject.name = "HotKey " + _keynumber;

	}


	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(HandleClick);
		icon = transform.GetChild(0).GetComponent<Image>();
	
		InitializeKey();
		PathMover.onBindUnitColour += SetUnitIconColour;
	}



	private void SetUnitIconColour(AgentID ID, Color unitColour, bool isActive)
	{
		if ((int)ID == _keynumber && icon != null) icon.color = isActive? unitColour : disconnectedColour;
	}




	private void HandleClick()
	{
		onHotKeyClicked?.Invoke(_keynumber);
	}


	private void Update()
	{

		if (Input.GetKeyDown(_keyCode))
		{
			if (!waitingForDouble)
			{
				HandleClick();
				waitingForDouble = true;
				StopAllCoroutines();
				StartCoroutine(WaitForDouble());
			}
			else
			{
				HandleDoubleClick();
				waitingForDouble = false;
				StopAllCoroutines();
			}
		}

	}

	private void HandleDoubleClick()
	{
		onHotKeyDoubleClicked?.Invoke(_keynumber);

		if (GameManager.instance.currentState == GameState.Tutorial && TutorialManager.instance.currentTask == TutorialTask.ControlSecondUnit)
		{
			TutorialManager.instance.DoubleTapped = true;
		}
	}

	private IEnumerator WaitForDouble()
	{
		yield return new WaitForSeconds(doubleClickDelay);
		waitingForDouble = false;
	}

}
