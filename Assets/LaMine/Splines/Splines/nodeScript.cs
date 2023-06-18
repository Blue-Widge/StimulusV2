using UnityEngine;
using System.Collections;

public class nodeScript : MonoBehaviour 
{
	/*Vitesse du node*/
	public float velocity;
	public float minVelocity;
	/*Mode arrière*/
	public bool isReverse;
	/*Mode de dessin*/
	public bool isDraw;
	
	/*Constructeur*/
	public nodeScript () 
	{
		velocity = 15;
		minVelocity = 5.0f;
		isReverse = false;
		isDraw = true;
	}
}