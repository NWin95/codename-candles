using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {
	#region variables
	// Use this for initialization
	public Transform fpCam;
	public CapsuleCollider capCollider;
	public Animator animator;
	public Transform eyes;
	
	public bool invertCamera = false;

	public bool grounded = false;
	public bool crouching = false;
	public bool sprinting = false;
	public float maxSlope = 45;	
	public float jumpHeight = 1.5f;

	float moveSpeedBase;
	float maxVelocityChangeBase;
	public float moveSpeed	= 10;
	public float maxVelocityChange	= 10;
	public float movementMultiplyerBase	= 1;
	public float airRatio	= 0.1f;
	public float sprintRatio = 1.5f;
	public float crouchRatio = 0.5f;
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
	float contactTime = 0.075f;
	public bool groundContact = false;
	public float baseContactTime = 0.075f;

	#endregion
	#region Start
	void Start ()
	{
		
		invertCamera = false; //This part is vital, and _NOT_ a joke. DO NOT DELETE
		
		Screen.showCursor	= false;
		Screen.lockCursor	= true;

		moveSpeedBase	= moveSpeed;
		maxVelocityChangeBase = maxVelocityChange;
	}
	#endregion
	void Update ()
	{
		
		if ( Input.GetKeyDown ( KeyCode.Escape ))
		{
			
			Screen.showCursor = true;
			Screen.lockCursor = false;
		}
		
		GroundingFunc ();
		CamRotation ();
		MovementUnfixed ();
		AnimationFunc ();
//		Debug.Log (Vector3.Angle (fpCam.forward, -eyes.right));
	}

	void FixedUpdate ()
	{
		MovementFixed ();
//		Debug.Log (rigidbody.velocity.magnitude);
	}

	void CamRotation ()
	{
		Vector3 curEuler	= transform.localEulerAngles;
		curEuler.y += Input.GetAxis ("Mouse X") * horLookSpeed;
		transform.localEulerAngles = curEuler;

		Vector3 curCamEuler	= fpCam.localEulerAngles;
		curCamEuler.y += Input.GetAxis ("Mouse Y") * vertLookSpeed * -1;
		
		if ( invertCamera )
			curCamEuler.y = curCamEuler.y * -1;
		
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
		if (Input.GetButton("Crouch") && !Input.GetButton ("Sprint"))
		{
			shouldCroutch = true;
			movementMultiplyer	= movementMultiplyerBase * crouchRatio;
			moveSpeed			= moveSpeedBase	* crouchRatio;
			maxVelocityChange	= maxVelocityChangeBase * crouchRatio;
		}
		if (Input.GetButton ("Sprint") && !Input.GetButton ("Crouch"))
		{
			sprinting = true;
			movementMultiplyer	= movementMultiplyerBase * sprintRatio;
			moveSpeed			= moveSpeedBase	* sprintRatio;
			maxVelocityChange	= maxVelocityChangeBase * sprintRatio;
		}
		if (!Input.GetButton ("Sprint") && !Input.GetButton ("Crouch"))
		{
			sprinting = false;
			shouldCroutch = false;
			movementMultiplyer = movementMultiplyerBase;
			moveSpeed			= moveSpeedBase;
			maxVelocityChange	= maxVelocityChangeBase;
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

	void AnimationFunc ()
	{
		Vector3 animVelHor = rigidbody.velocity;
		animVelHor.y	= 0;
		float velHor = animVelHor.magnitude;

		animator.SetFloat	("VelHor", velHor);
		animator.SetBool	("Grounded", grounded);
		animator.SetBool	("Crouching", crouching);
	}

	void OnCollisionStay (Collision collision)
	{
//		Vector3 contactPoint = collision.contacts [0].point;
/*		if (Vector3.Angle (-(contactPoint - (transform.position + transform.up)).normalized, Vector3.up) <= maxSlope)
		{
//			grounded = true;
			groundContact = true;
		}	*/
		foreach (ContactPoint contact in collision.contacts)
		{
			if (Vector3.Angle(contact.normal, Vector3.up) < maxSlope)
			{
				groundContact	= true;
			}
		}
	}

	void OnCollisionExit ()
	{
//		grounded = false;
		groundContact = false;
	}

	void GroundingFunc ()
	{
		if (groundContact)
		{
			contactTime	= baseContactTime;		
			grounded	= true;
		}
		else
		{
			contactTime	-= Time.deltaTime;
			
			if (contactTime <= 0)
			{
				grounded	= false;
			}
		}
	}
}
