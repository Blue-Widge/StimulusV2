using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class csvhandlePath {

	// Update is called once per frame
	public void write (List<GameObject> path, string fileName) {
		string test = "";
		nodeScript script = new nodeScript ();

		/*On parcour la liste de noeud*/
		foreach (GameObject goTmp in path) {
			script = goTmp.GetComponent<nodeScript>();

			test += goTmp.transform.position.x.ToString() + ";" + goTmp.transform.position.y.ToString() + ";" + goTmp.transform.position.z.ToString() + ";";
			test += goTmp.transform.rotation.eulerAngles.x.ToString() + ";" + goTmp.transform.rotation.eulerAngles.y.ToString() + ";" + goTmp.transform.rotation.eulerAngles.z.ToString() + ";";
			test += script.velocity.ToString();
			test += "\n";
		}

		File.WriteAllText (fileName, test);
	}

	public List<GameObject> read (objectBuilderScript parent, string fileName) {
		List<GameObject> list = new List<GameObject>();

		string[] lignesDurAll = {""};
		string[] ligneTab = {""};
		Vector3 vectorTmp = new Vector3 (0.0f, 0.0f, 0.0f);
		int index;

		lignesDurAll = File.ReadAllLines(fileName);

		foreach (string ligneDur in lignesDurAll) {
			GameObject goTmp = new GameObject ();
			GameObject cube;
			float velocity;
			Vector3 scale = new Vector3 (1.0f, 1.0f, 1.0f);
			nodeScript nodeTmp;
			ligneTab = ligneDur.Split(';');

			/*On le définit comme fils*/
			goTmp.transform.parent = parent.transform;
			goTmp.transform.name = "node" + list.Count.ToString();
			/*On lui affecte son script*/
			goTmp.AddComponent<nodeScript>();

			index = 0;
			foreach (string word in ligneTab) {
				Debug.Log (word);
				if(index == 0){
					vectorTmp.x = float.Parse(word);
				}else if(index == 1){
					vectorTmp.y = float.Parse(word);
				}else if(index == 2){
					vectorTmp.z = float.Parse(word);
					goTmp.transform.position = vectorTmp;
				}else if(index == 3){
					vectorTmp.x = float.Parse(word);
				}else if(index == 4){
					vectorTmp.y = float.Parse(word);
				}else if(index == 5){
					vectorTmp.z = float.Parse(word);
					goTmp.transform.rotation = Quaternion.Euler(vectorTmp);
				}else if(index == 6){
					velocity = float.Parse(word);
					nodeTmp = goTmp.GetComponent<nodeScript> ();
					nodeTmp.velocity = velocity;
				}

				index++;
			}

			/*On créé un cube fils*/
			cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.transform.parent = goTmp.transform;
			cube.transform.position = goTmp.transform.position;
			cube.transform.rotation = goTmp.transform.rotation;
			cube.transform.localScale = scale;

			/*On l'ajoute à la liste*/
			list.Add(goTmp);
		}
		return list;
	}
	
}





