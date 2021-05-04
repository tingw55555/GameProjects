using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthPack : MonoBehaviour
{
	private TextMeshProUGUI amountText;
	private Collider co;
	private Rigidbody rb;
	public Image fillCircle;

	public GameObject onEndEffect;
	public GameObject onHitEffect;

	public float lifeTime = 3f;
	[SerializeField] private float healAmount;
	[SerializeField] private float pickUpDelay = 0.25f;
	public float releaseAngle = 10f;
	public float releasePower = 10f;

	private void Awake()
	{
		amountText = GetComponentInChildren<TextMeshProUGUI>();
		co = GetComponent<Collider>();
		rb = GetComponent<Rigidbody>();
		Invoke("EnableCollider", pickUpDelay);
	}

	private void Start()
	{
		Launch();
		StartCoroutine(WaitToDestroy());
	}

	private void Launch()
	{
		Vector3 shootDir = Vector3.up;
		shootDir = Quaternion.Euler(releaseAngle, Random.Range(0, 360), 0) * shootDir;
		rb.AddForce(shootDir * releasePower, ForceMode.Impulse);
	}

	public void OnInit(float healAmount)
	{
		this.healAmount = healAmount;
		amountText.text = "+" + this.healAmount.ToString();


	}

	IEnumerator WaitToDestroy()
	{
		float currentTime = 0;
		while (currentTime < 1)
		{
			fillCircle.fillAmount = currentTime;
			yield return null;

			currentTime += Time.deltaTime / lifeTime;
		}
		GameObjectUtil.Instantiate(onEndEffect, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}



	private void OnTriggerEnter(Collider other)
	{
		Character player = other.gameObject.GetComponent<Character>();

		if (player)
		{
			player.Heal(healAmount);
			GameObjectUtil.Instantiate(onHitEffect, transform.position, Quaternion.identity);
			Destroy(gameObject);
		}

	}


	private void EnableCollider()
	{
		co.enabled = true;
	}

}
