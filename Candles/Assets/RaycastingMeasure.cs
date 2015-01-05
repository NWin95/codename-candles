using UnityEngine;
using System.Collections;

public class RaycastingMeasure : MonoBehaviour {

	public float offsetY = 5;
	public LayerMask mask;
	public int amountBase = 100;
	int amount;
	public int length = 1;
	public Vector3 direction = Vector3.down;
	
	void Update ()
	{
		RayFunc ();
	}

	void RayFunc ()
	{
		amount = amountBase;
	//	Debug.Log (amount);
		while (amount > 0)
		{
			if (Physics.Raycast (transform.position + (Vector3.up * offsetY), direction, length, mask))
			{

			}
			amount --;
		}
	//	Debug.Log (amount);
	}
}
