using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class follower : MonoBehaviour 
{
	/*Liste de chemin*/
	public List<string> chemin = new List<string>();
	hermiteInterpol hermite;
	Vector3 dir;
	bool finish;
	public float vitesse;
	public byte m_brakesValue;
	public byte m_brakesPower;
	public float m_maxSpeedReduction = 3.0f;
	float m_speedReductionRation;
	public bool m_isRunning;
	bool m_firstSetPos;

	public follower()
	{
		m_isRunning = false;
		hermite = new hermiteInterpol ();
		dir = new Vector3 (0.0f, 0.0f, 1.0f);
		finish = false;
		vitesse = 0.0f;
		m_brakesValue = 0;
		m_brakesPower = 0;
		m_speedReductionRation = m_maxSpeedReduction / 100.0f;
	}
	
	//Update is called once per frame
	void Update () 
	{
		float angle;
		float velocity;
		float minVelocity;
		float gravityVelocity;
		Vector3 pos = new Vector3 ();
		Vector3 debug = new Vector3 (0.0f, 90.0f, 0.0f);
		Vector3 debug2 = new Vector3 (0.0f, 0.5f, 0.0f);
		GameObject path;
		int indexPath;
		objectBuilderScript pathScript;

		// if the spline is not running,
		if (m_isRunning == false) 
		{
			// start the spline
			if( finish == false && Input.anyKey == true )
			{
				m_isRunning = true;
			}
			// restart the app if the end is reached
			else if( finish == true && Input.anyKey == true )
			{
				Application.LoadLevel (0);
			}
			else
				return ;
	    }

		/*On récupère l'indice du chemin à suivre*/
		indexPath = hermite.getPathIndex ();
		
		/*On récupère le bon chemin*/
		if (chemin.Count - 1 >= indexPath && string.IsNullOrEmpty(chemin [indexPath]) == false) {
			path = GameObject.Find (chemin [indexPath]);
			pathScript = path.GetComponent <objectBuilderScript>();
			//if(path.GetComponent<Rigidbody>() == null)
			//return;
		} else if (chemin.Count >= 1 && string.IsNullOrEmpty(chemin [chemin.Count - 1]) == false) { //indexPath > nbChemin ==> FINI
			finish = true;
			m_isRunning = false;
			return;
		} else {
			return;
		}
		
		//if (finish == true)
		//	return;
		
		/*On calcule la vitesse*/
		velocity = 0.0f;//hermite.getVelocity(pathScript.node);
		gravityVelocity = hermite.getVelocityGravity (pathScript.gravityVelocity/*, pathScript.frottement*/);
		velocity += gravityVelocity - vitesse*pathScript.frottement;
		minVelocity = hermite.getMinVelocity(pathScript.node);
		vitesse += velocity * Time.deltaTime;
		//haptic brakes
		vitesse -= m_brakesPower * m_speedReductionRation;
		if (vitesse < minVelocity) 
		{
			vitesse = minVelocity;
			velocity = 0.0f;
		}

		/*On interpole la position*/
		pos = hermite.getUpdatedTrolleyPos(vitesse*Time.deltaTime, pathScript.node);
		/*On calcule la direction*/
		dir = hermite.getDirection ();
		
		/*On affecte la nouvelle position*/
		this.transform.position = pos + debug2;
		/*On fait face à la ligne*/
		this.transform.forward = dir;
		this.transform.Rotate (debug);
		/*On ajoute la rotation selon l'axe x (rouge) du chariot*/
		angle = hermite.getTrolleyRotationInterpolled (pathScript.node);
		this.transform.RotateAround(this.transform.position, dir, -angle);

	}
}

