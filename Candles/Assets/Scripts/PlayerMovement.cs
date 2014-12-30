using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {
	#region variables
	// Use this for initialization
	public Transform fpCam;
	public CapsuleCollider capCollider;
	public Animator animator;

	public bool grounded = false;
	public bool crouching = false;
	public bool sprinting = false;
	public float maxSlope = 45;	
	public float jumpHeight = 1.5f;

	public float moveSpeed	= 10;
	public float maxVelocityChange	= 10;
	public float movementMultiplyerBase	= 1;
	public float airRatio	= 0.1f;
	public float sprintRatio = 1.5f;
	float movementMultiplyer;
	float velocityDenom;
	public float horLookSpeed = 10f;
	public float vertLookSpeed = 10f;
	bool shouldCroutch = false;
	public float croutchTime = 0.5f;
	public float croutchHeight = 0.5f;
	float standHeight = 1.85f;
	float targHeight = 1.85f;
	public float croutchHeightAllowence = 0.1f;

	#endregion
	#region Start
	void Start ()
	{
		Screen.showCursor	= false;
		Screen.lockCursor	= true;
	}
	#endregion
	void Update ()
	{
		CamRotation ();
		MovementUnfixed ();
	}

	void FixedUpdate ()
	{
		MovementFixed ();
	}

	void CamRotation ()
	{
		Vector3 curEuler	= transform.localEulerAngles;
		curEuler.y += Input.GetAxis ("Mouse X") * horLookSpeed;
		transform.localEulerAngles = curEuler;

		Vector3 curCamEuler	= fpCam.localEulerAngles;
		curCamEuler.y += Input.GetAxis ("Mouse Y") * vertLookSpeed;
		fpCam.localEulerAngles = curCamEuler;
	}

	void MovementUnfixed ()
	{
		if (Input.GetButtonDown ("Jump") && grounded)
		{
			float jumpVel	= Mathf.Sqrt (2 * Mathf.Abs (Physics.gravity.magnitude) * jumpHeight);
			Vector3 newVel	= rigidbody.velocity;
			newVel.y		= jumpVel;
			rigidbody.velocity	= newVel;
		}
		if (Input.GetButton("Crouch"))
		{
			shouldCroutch = true;
		}
		else
		{
			shouldCroutch = false;
		}
		if (Input.GetButton ("Sprint"))
		{
			sprinting = true;
			movementMultiplyer	= movementMultiplyerBase * sprintRatio;
		}
		else
		{
			sprinting = false;
			movementMultiplyer = movementMultiplyerBase;
		}
		Vector3 curCapCenter = capCollider.center;
		float	heightTargDif = Mathf.Abs (capCollider.height - targHeight);
		if (shouldCroutch)
		{
			capCollider.height	= Mathf.Lerp (capCollider.height, croutchHeight, Time.deltaTime / croutchTime);

			curCapCenter.y	= capCollider.height / 2;
			capCollider.center = curCapCenter;
			targHeight = croutchHeight;

			if (heightTargDif < croutchHeightAllowence)
			{
				crouching	= true;
			}
		}
		else
		{
			capCollider.height	= Mathf.Lerp (capCollider.height, standHeight, Time.deltaTime / croutchTime);

			curCapCenter.y	= capCollider.height / 2;
			capCollider.center = curCapCenter;
			targHeight = standHeight;

			if (heightTargDif < croutchHeightAllowence)
			{
				crouching	= false;
			}
		}
	}
	#region MovementFixed
	void MovementFixed ()
	{
		if (grounded)
		{
			movementMultiplyer	= movementMultiplyerBase;
		}
		else
		{
			movementMultiplyer	= movementMultiplyerBase * airRatio;
		}
		Vector3 targetVelocity;
		targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		targetVelocity = Vector3.ClampMagnitude (targetVelocity, 1);
		targetVelocity = transform.TransformDirection(targetVelocity);
		targetVelocity *= moveSpeed;
		//targetVelocity *= movementMultiplyer;
		
		Vector3 moveVelocity;
		Vector3 moveVelocityChange;
		moveVelocity = rigidbody.velocity;
		moveVelocityChange = (targetVelocity - moveVelocity);
		moveVelocityChange.x = Mathf.Clamp(moveVelocityChange.x, -maxVelocityChange, maxVelocityChange);
		moveVelocityChange.z = Mathf.Clamp(moveVelocityChange.z, -maxVelocityChange, maxVelocityChange);
		moveVelocityChange.y = 0;
		rigidbody.AddForce (moveVelocityChange * movementMultiplyer, ForceMode.VelocityChange);

		//Debug.Log (movementMultiplyer);
	}
	#endregion
	void OnCollisionStay (Collision collision)
	{
		Vector3 contactPoint = collision.contacts [0].point;
		if (Vector3.Angle (-(contactPoint - (transform.position + transform.up)).normalized, Vector3.up) <= maxSlope)
		{
			grounded = true;
		}
//		for (ContactPoint contact in collision.contacts)
//		{

//		}
	}

	void OnCollisionExit ()
	{
		grounded = false;
	}
}
