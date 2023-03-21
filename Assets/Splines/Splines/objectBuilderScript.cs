using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class objectBuilderScript : MonoBehaviour 
{
	/*Liste de game object*/
	public List<GameObject> node = new List<GameObject>();
	/*Couleur*/
	public Color color;
	/*Help display*/
	public bool helpDisplay;
	public string csv_FileName;
	public string obj_FileName;
	public int indexInList;
	public float gravityVelocity;
	public float frottement;

	public objectBuilderScript()
	{
		color = Color.white;
		helpDisplay = true;
		csv_FileName = "path.csv";
		indexInList = 0;
		gravityVelocity = 10.0f;
		frottement = 0.02f;
	}

	public void renameAll()
	{
		int i = 0;
		foreach (GameObject goTmp in node) {
			goTmp.transform.name = "node" + i.ToString();
			i++;
		}
	}
	
	public void buildObject(bool addToTheEnd)
	{
		/*On déclare un game object*/
		GameObject tmp;
		GameObject cube;
		Vector3 scale = new Vector3 (1.0f, 1.0f, 1.0f);

		if(addToTheEnd == false && (indexInList < 0 || indexInList >= node.Count))
			return;
		/*On l'alloue*/
		tmp = new GameObject();

		/*On le définit comme fils*/
		tmp.transform.parent = this.transform;
		tmp.transform.position = this.transform.position;
		tmp.transform.name = "node" + node.Count.ToString();

		/*On créé un cube fils*/
		cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.parent = tmp.transform;
		cube.transform.position = tmp.transform.position;
		cube.transform.localScale = scale;

		/*On initialise sa position/rotation*/
		if (node.Count < 1) 
		{
			tmp.transform.position = this.transform.position;
		} 
		else 
		{
			tmp.transform.position = node[node.Count - 1].transform.position;//this.transform.position;
			//tmp.velocity = node[node.Count - 1].velocity;
		}
		tmp.transform.rotation = this.transform.rotation;
		/*On lui affecte son script*/
		tmp.AddComponent<nodeScript>();

		/*On l'ajoute à la liste*/
		if (addToTheEnd) {
			node.Add (tmp);
		} else {
			node.Insert (indexInList, tmp);
			renameAll();
		}

		/*On affecte au noeud la vitesse de son parent*/
		if (node.Count >= 2) 
		{
			nodeScript nodeTmp;
			nodeScript nodeTmp1;
			nodeTmp = node [node.Count - 1].GetComponent<nodeScript> ();
			nodeTmp1 = node [node.Count - 2].GetComponent<nodeScript> ();
			nodeTmp.velocity = nodeTmp1.velocity;
		} 
		else 
		{
			nodeScript nodeTmp;
			nodeTmp = node [node.Count - 1].GetComponent<nodeScript> ();
			nodeTmp.velocity = 5.0f;
		}
	}
	
	public void deleteObject(bool deleteToTheEnd)
	{
		int id;

		if(deleteToTheEnd == false && (indexInList < 0 || indexInList >= node.Count))
			return;

		if (deleteToTheEnd) 
			id = node.Count-1;
		else
			id = indexInList;

		if (id >= 0) 
		{
			/*On détruit l'objet*/
			DestroyImmediate(node [id]);
			/*On enlève le node de la list*/
			node.RemoveAt(id);
			if (deleteToTheEnd == false)
				renameAll();
		}
	}

	public void writeInCsv()
	{
		csvhandlePath csv = new csvhandlePath();
		csv.write(node, csv_FileName);
	}

	public void loadFromCsv()
	{
		csvhandlePath csv = new csvhandlePath();
		node = csv.read(this, csv_FileName);
	}

	public void meshesExporter()
	{
		morphExportMeshes mesh = new morphExportMeshes();

		mesh.meshTransform(node, obj_FileName);
	}
}






