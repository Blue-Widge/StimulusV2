using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(objectBuilderScript))]
public class objectBuildEditor : Editor 
{
	private const int lineSteps = 100;
	bool isRenderer = false;
	

	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
		
		/*Lien vers le script du game object principal*/
        objectBuilderScript myScript = (objectBuilderScript)target;
		
		/*Gestion des boutons*/
        if(GUILayout.Button("Add node to the end"))
        {
            myScript.buildObject(true);
        }
		if(GUILayout.Button("Add node in the middle"))
		{
			if(EditorUtility.DisplayDialog("Add node", "Vous allez ajouter un noeud à l'indice " +myScript.indexInList + ", (variable indexInList)", "Yolo", "Cancel"))
				myScript.buildObject(false);
		}
		if(GUILayout.Button("Delete last node"))
        {
            myScript.deleteObject(true);
        }
		if(GUILayout.Button("Delete node in the middle"))
		{
			if(EditorUtility.DisplayDialog("delete node", "Vous allez supprimer le noeud d'indice " + myScript.indexInList + ", (variable indexInList)", "Yolo", "Cancel"))
				myScript.deleteObject(false);
		}
		if(GUILayout.Button("Show/hide cubes"))
		{
			if(isRenderer)
				isRenderer = false;
			else
				isRenderer = true;
		}
		if(GUILayout.Button("Save path in a csv file"))
		{
			if(EditorUtility.DisplayDialog("Export path", "Attention: vous allez écraser le fichier sélectionné", "Yolo", "Cancel"))
				myScript.writeInCsv();
		}
		if(GUILayout.Button("Import path from a csv file"))
		{
			if(EditorUtility.DisplayDialog("Import path", "Attention: vous allez écraser le gameObject sélectionné", "Yolo", "Cancel"))
				myScript.loadFromCsv();
		}
		if(GUILayout.Button("Export rails as wavefront file (.obj)"))
		{
			myScript.meshesExporter();
		}
    }

	private void OnSceneGUI () 
	{	
		/*Lien vers le script du game object principal*/
        objectBuilderScript myScript = (objectBuilderScript)target;
		/*Classe d'interpollation*/
		hermiteInterpol hermite = new hermiteInterpol();

		Vector3 lineStart = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 lineEnd = new Vector3(0.0f, 0.0f, 0.0f);
		int nbNodes = 0;
		float angle;
		Matrix4x4 mat = new Matrix4x4();
		Vector3 dir = new Vector3 (0.0f, 0.0f, 0.0f);

		/*On parcour la liste de noeud*/
		foreach(GameObject goTmp in myScript.node)
        {
			/*On test si le noeud à été supprimé*/
			if(goTmp == null)
			{
				/*On enlève le noeud de la list*/
				myScript.node.Remove(goTmp);
				continue;
			}

			nbNodes++;

			/*On initialise le début du tracé au noeud courant*/
			lineStart = goTmp.transform.position;
			/*On dessine le trajet*/
			for(int i = 1; i <= lineSteps; i++) 
			{
				lineEnd = hermite.getPosOnPath((float)i/(float)lineSteps, myScript.node, nbNodes-1);
				Debug.DrawLine(lineStart, lineEnd, myScript.color, 0.0f, true);
				/*if(i%2 == 0){
					Debug.DrawLine(lineStart, lineEnd, Color.white, 0.0f, true);
				}else{
					Debug.DrawLine(lineStart, lineEnd, Color.blue, 0.0f, true);
				}*/

				/*Dessin des rails*/
				if(i%10 == 0 && myScript.helpDisplay)
				{
					/*On récupère l'angle*/
					angle = hermite.getRotationInterpolled(myScript.node, nbNodes-1, (float)i/(float)lineSteps);

					mat = hermite.getRotationMatrix(lineStart, lineEnd, angle);
					dir.x = mat.GetColumn(0).x;
					dir.y = mat.GetColumn(0).y;
					dir.z = mat.GetColumn(0).z;
					Debug.DrawRay(lineStart, dir, Color.red);
					dir.x = mat.GetColumn(1).x;
					dir.y = mat.GetColumn(1).y;
					dir.z = mat.GetColumn(1).z;
					Debug.DrawRay(lineStart, dir, Color.blue);
					dir.x = mat.GetColumn(2).x;
					dir.y = mat.GetColumn(2).y;
					dir.z = mat.GetColumn(2).z;
					Debug.DrawRay(lineStart, dir, Color.green);
				}

				lineStart = lineEnd;
			}

            //Debug.Log(goTmp.velocity);
			/*On déssine un repère*/
			goTmp.transform.position = Handles.DoPositionHandle(goTmp.transform.position, goTmp.transform.rotation);
			/*Gestion du rendu des cubes*/
			Renderer[] renderers = goTmp.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers)
			{
				r.enabled = isRenderer; 
			}
			/*Vector3 screenCoord = Camera.main.WorldToScreenPoint(goTmp.transform.position);
			GUI.Box(new Rect(screenCoord.x, screenCoord.y, 100, 20), goTmp.name);*/
        }
	}
}











