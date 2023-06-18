using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class hermiteInterpol
{
	int pathIndex;
	int trolleyNodeIndex;
	float S;
	float distanceTotal;
	Vector3 direction;

	public hermiteInterpol()
	{
		pathIndex = 0;
		trolleyNodeIndex = 1;
		distanceTotal = 0.0f;
		direction = new Vector3 (1.0f, 0.0f, 0.0f);
	}

	public Matrix4x4 getRotationMatrix(Vector3 lineStart, Vector3 lineEnd, float angle)
	{
		Vector3 forwardAxis = new Vector3 (0.0f, 0.0f, 0.0f);
		Vector3 sideAxis = new Vector3 (0.0f, 0.0f, 0.0f);
		Vector3 upAxis = new Vector3 (0.0f, 0.0f, 0.0f);
		Matrix4x4 mat = new Matrix4x4();

		/*On calcule la direction*/
		forwardAxis = (lineEnd - lineStart).normalized;
		mat.SetColumn(0, forwardAxis);
		/*On calcule la normal*/
		upAxis.Set(0.0f, 1.0f, 0.0f); 
		sideAxis = Vector3.Cross(upAxis, forwardAxis).normalized;
		sideAxis *= -1.0f;
		upAxis = Vector3.Cross(sideAxis, forwardAxis).normalized;

		sideAxis = Vector3.RotateTowards(sideAxis, upAxis, angle*(3.14159f/180.0f), 0.0f);
		mat.SetColumn(2, sideAxis.normalized);
		upAxis = Vector3.Cross(sideAxis, forwardAxis);
		mat.SetColumn(1, upAxis.normalized);
		
		/*Translation*/
		mat.SetColumn(3, (lineEnd + lineStart)/2.0f);

		return mat;
	}

	public bool getDrawStat(List<GameObject> path, int index)
	{
		nodeScript tmp;

		/*On récupère le script du noeud en cours de traitement*/
		tmp = path[index].GetComponent<nodeScript>();

		return tmp.isDraw;
	}
	
	public float getMinVelocity(List<GameObject> path)
	{
		nodeScript tmp;
		float nextVelocity;
		float prevVelocity;
		
		if ((trolleyNodeIndex + 1) <= (path.Count - 1))
		{
			/*On récupère le script du noeud en cours de traitement*/
			tmp = path[trolleyNodeIndex+1].GetComponent<nodeScript>();
			nextVelocity = tmp.minVelocity;
		}
		else 
		{
			tmp = path [trolleyNodeIndex].GetComponent<nodeScript>();
			nextVelocity = tmp.minVelocity;
		}
		
		tmp = path [trolleyNodeIndex].GetComponent<nodeScript>();
		prevVelocity = tmp.minVelocity;
		
		return ((prevVelocity * (1.0f-S)) + (nextVelocity * S));
	}

	public float getVelocityGravity(float gravityVelocity/*, float frottement*/)
	{
		Vector3 g = new Vector3 (0.0f, -1.0f, 0.0f);
		float angle;

		angle = Vector3.Dot(direction, g);

		return angle*gravityVelocity;
	}

	public Vector3 getDirection()
	{
		return direction;
	}

	public int getPathIndex()
	{
		return pathIndex;
	}

	public void reset()
	{
		trolleyNodeIndex = 1;
		distanceTotal = 0;
		S = 0.0f;
	}

	public void goToNextPath()
	{
		pathIndex++;
		reset();
	}

	public void finish(List<GameObject> path)
	{
		if(path.Count - 2 >= 0) //-2 car de 1 à n+1 et le dernier neud est l'avant dernier
			trolleyNodeIndex = path.Count - 2;
		else if(path.Count - 1 >= 0) //Si il y a que un noeud
			trolleyNodeIndex = path.Count - 1;
		else //Si la liste est vide
			trolleyNodeIndex = 0;
	}

	public float getVelocity(List<GameObject> path)
	{
		nodeScript tmp;
		float nextVelocity;
		float prevVelocity;

		if ((trolleyNodeIndex + 1) <= (path.Count - 1))
		{
			/*On récupère le script du noeud en cours de traitement*/
			tmp = path[trolleyNodeIndex+1].GetComponent<nodeScript>();
			nextVelocity = tmp.velocity;
		}
		else 
		{
			tmp = path [trolleyNodeIndex].GetComponent<nodeScript>();
			nextVelocity = tmp.velocity;
		}

		tmp = path [trolleyNodeIndex].GetComponent<nodeScript>();
		prevVelocity = tmp.velocity;

		return ((prevVelocity * (1.0f-S)) + (nextVelocity * S));
	}

	public float getDegre (float val)
	{
		float degre;

		degre = val;

		if(degre > 360.0f){
			degre -= 360.0f*(float)((int)(degre/360.0f));
		}
		else if(degre < -360.0f){
			degre += 360.0f*(float)((int)(degre/(-360.0f)));
		}

		if (degre > 180.0f) {
			degre = degre - 360.0f;
		}
		else if(degre < -180.0f){
			degre = degre + 360.0f;
		}

		return degre;
	}

	public float getTrolleyRotationInterpolled(List<GameObject> path)
	{
		return getRotationInterpolled(path, trolleyNodeIndex, S);
	}

	public float getRotationInterpolled(List<GameObject> path, int index, float dp)
	{
		float angle = 0.0f;

		if ((path.Count - 1) >= index + 1) 
		{
			angle = (dp * getDegre(path [index + 1].transform.eulerAngles.x)) + ((1.0f-dp) * getDegre(path [index].transform.eulerAngles.x));
		}

		return angle;
	}

	/*Donne la position du chariot après avoir parcouru distParcouru depuis sa dernière position*/
	public Vector3 getUpdatedTrolleyPos(float distParcouru, List<GameObject> path)
	{
		nodeScript node;
		Vector3 tmp = new Vector3();
		float dist;
		Vector3 lineStart = new Vector3 (0.0f, 0.0f, 0.0f);
		Vector3 lineEnd = new Vector3 (0.0f, 0.0f, 0.0f);
		int lineSteps = 100;
		int lineSteps2 = 100;
		
		/*Vérifie qu'on ne soit pas sur le dernier noeud*/
		if ((trolleyNodeIndex + 1) < (path.Count - 1)) 
		{
			/*Calcule la distance entre le noeud actuel et le noeud suivant*/
			dist = 0.0f;
			lineStart = path [trolleyNodeIndex].transform.position;
			for(int i = 1; i <= lineSteps; i++) 
			{
				lineEnd = getPosOnPath((float)i/(float)lineSteps, path, trolleyNodeIndex);
				tmp = lineEnd - lineStart;
				dist += tmp.magnitude;
				lineStart = lineEnd;
			}
			lineSteps2 = (int)(dist*50.0f);

			/*Calcule de la distance à avancer (entre 0 et 1)*/
			distanceTotal += distParcouru;
			S = distanceTotal / dist;

			if (S > 1.0f) //Si on dépasse le noeud suivant
			{
				S = 0.0f; //S - 1.0f
				trolleyNodeIndex++;
				distanceTotal = 0.0f;
			}
			else if (S < 0.0f) //Si on dépasse le noeud précédent
			{
				S = 1.0f;

				/*Calcule la distance entre le noeud actuel et le noeud precedent*/
				if(trolleyNodeIndex >= 1)
				{
					dist = 0.0f;
					lineStart = path [trolleyNodeIndex-1].transform.position;
					for(int i = 1; i <= lineSteps; i++) 
					{
						lineEnd = getPosOnPath((float)i/(float)lineSteps, path, trolleyNodeIndex-1);
						tmp = lineEnd - lineStart;
						dist += tmp.magnitude;
						lineStart = lineEnd;
					}
					trolleyNodeIndex--;
				}
				else
				{
					dist = 0.0f;
				}

				distanceTotal = dist;
			}
		}
		else //Si on dépasse le dernier noeud
		{
			int tmptmp = trolleyNodeIndex;
			goToNextPath();
			/*on retourne la position du dernier noeud*/
			return path[tmptmp].transform.position;
		}

		/*Place le cube*/
		dist = 0.0f;
		lineStart = path [trolleyNodeIndex].transform.position;
		for(int i = 1; i <= lineSteps2; i++) 
		{
			lineEnd = getPosOnPath((float)i/(float)lineSteps2, path, trolleyNodeIndex);
			tmp = lineEnd - lineStart;
			dist += tmp.magnitude;
			if(dist >= distanceTotal){
				break;
			}
			lineStart = lineEnd;
		}
		/*On calcule la direction*/
		direction = (lineEnd - lineStart).normalized;

		/*On récupère le script du noeud en cours de traitement*/
		node = path[trolleyNodeIndex].GetComponent<nodeScript>();
		if (node.isReverse)
			direction = direction * -1.0f; //On inverse la direction

		return lineEnd;
	}

	/*Donne la position entre le noeud nodeIndex et le noeud suivant interpolé suivant s (0-1)*/
	public Vector3 getPosOnPath(float s, List<GameObject> path, int nodeIndex)
	{
		Vector3 pos1 = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 pos2 = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 pos3 = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 pos4 = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 res  = new Vector3(0.0f, 0.0f, 0.0f);

		/*pos2 est la position du noeud de départ de l'interpolation (pos3 est le noeud de fin)*/
		if(nodeIndex <= (path.Count-1))
			pos2 = path[nodeIndex].transform.position;
		else //Out of range
			pos2 = path[path.Count-1].transform.position;

		/*Si pos2 n'est pas le premier noeud*/
		if(nodeIndex > 0)
			pos1 = path[nodeIndex-1].transform.position;
		else
			pos1 = pos2; //Le noeud précédent pos2 est pos2

		/*Si pos2 n'est pas le dernier noeud*/
		if ((path.Count - 1) >= (nodeIndex + 1))
			pos3 = path [nodeIndex + 1].transform.position;
		else
			return pos2; //On retourne la position du dernier noeud (pos2)

		/*Si pos3 n'est pas le dernier noeud*/
		if((path.Count-1) >= (nodeIndex+2))
			pos4 = path[nodeIndex+2].transform.position;
		else
			pos4 = pos3; //Le noeud suivant pos3 est pos3

		/*On interpole*/
		res = hermiteInterpolation(s, pos1, pos2, pos3, pos4);

		return res;
	}

	public Vector3 hermiteInterpolation(float s, Vector3 precPoint, Vector3 startPoint, Vector3 endPoint, Vector3 nextPoint)
	{
		Vector3 t1, t2;
		Vector3 tmp1, tmp2;
		
		tmp1 = startPoint - precPoint;  
		tmp2 = endPoint - startPoint;   
		t1 = (tmp1 + tmp2) / 2f;        //Tangente à startPoint

		tmp1 = endPoint - startPoint;
		tmp2 = nextPoint - endPoint;
		t2 = (tmp1 + tmp2) / 2f;        //Tangente à nextPoint
		
		return hermiteInterpolation_(s, startPoint, t1, endPoint, t2);
	}
	
	
	public Vector3 hermiteInterpolation_(float s, Vector3 p1, Vector3 t1, Vector3 p2, Vector3 t2)
	{
		float h1, h2, h3, h4;
		float s2, s3;
		Vector3 p;
		
		s2 = s*s;
		s3 = s2*s;
		
		h1 = 2.0f*s3 - 3.0f*s2 + 1f;     // calculate basis function 1
		h2 = -2.0f*s3 + 3.0f*s2;         // calculate basis function 2
		h3 = s3 - 2.0f*s2 + s;           // calculate basis function 3
		h4 = s3 - s2;				     // calculate basis function 4
		
		/*multiply and sum all funtions together to build the interpolated point along the curve.*/
		p = (((p1 * h1) + (p2 * h2)) + ((t1 * h3) + (t2 * h4)));
		
		return p;
	}
}
