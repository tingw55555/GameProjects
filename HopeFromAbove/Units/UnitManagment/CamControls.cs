using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CamControls : Singleton<CamControls>
{

	public mapBounds gameArea;

	[Header("Panning")]
	public bool enableEdgePan;
	public float mousePanSpeed = 0.5f;
	public float keyBoardPanSpeed = 1.5f;
	public float cursorPanBoundary = 2;

	private float yOffset;


	private void Awake()
	{
		yOffset = gameArea.bounds.extents.y;

	}

	// Update is called once per frame
	void Update()
	{

		Cursor.lockState = CursorLockMode.Confined;
		Pan();

	}

	void Pan()
	{
		//Pan by keys
		float horizontalInput = Input.GetAxis("Horizontal");
		float verticalInput = Input.GetAxis("Vertical");

		//this.transform.Translate(horizontalInput * panSpeed, 0, 0);

		if (horizontalInput != 0)
		{
			transform.Translate(transform.right * horizontalInput * Time.deltaTime * keyBoardPanSpeed);
		}

		if (verticalInput != 0)
		{
			transform.Translate(-transform.forward * verticalInput * Time.deltaTime * keyBoardPanSpeed);
		}



		if (enableEdgePan)
		{
			//Pan by mouse on sides
			if (Input.mousePosition.x < cursorPanBoundary)
			{
				this.transform.Translate(-mousePanSpeed, 0, 0);
			}
			if (Input.mousePosition.y < cursorPanBoundary)
			{
				this.transform.Translate(0, -mousePanSpeed, 0);
			}
			if (Input.mousePosition.x > Screen.width - cursorPanBoundary)
			{
				this.transform.Translate(mousePanSpeed, 0, 0);
			}
			if (Input.mousePosition.y > Screen.height - cursorPanBoundary)
			{
				this.transform.Translate(0, mousePanSpeed, 0);
			}
		}



		//We clamp the camera to stay inside the bounds at all times
		transform.position = gameArea.bounds.ClosestPoint(transform.position);
	}

	public void SnapToPosition(Vector3 pos)
	{
		transform.position = gameArea.bounds.ClosestPoint(new Vector3(pos.x, transform.position.y, pos.z));
	}

}
