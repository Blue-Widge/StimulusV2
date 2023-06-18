using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class Faces
{
	public int[] v = new int[3];

	public int[] t = new int[3];

	public int[] n = new int[3];

	public Faces()
	{
		for (int i=0; i<3; ++i) 
		{
			v[i] = -1;
			t[i] = -1;
			n[i] = -1;
		}
	}
}

public class morphExportMeshes {

	public Vector3[] vertex = new Vector3[15000];
	public int[] vertexFlag = new int[15000];
	int nbVertex = 0;
	public Vector3[] normals = new Vector3[15000];
	int nbNormals = 0;
	public string[] textures = new string[15000];
	int nbTextures = 0;
	public List<Faces> faces = new List<Faces>();

	public Vector3[] vertex2 = new Vector3[15000];
	int nbVertexEnTrop = 0;

	float distRails = 0.0f;
	int nbTest = 0;


	public void readObj()
	{
		string[] lignesDurAll = {""};
		string[] ligneTab = {""};
		int i;
		int j;
		int l;
		int nor = 0;
		int tex = 0;
		int fac = 0;

		lignesDurAll = File.ReadAllLines("rail0.obj");

		i = 0;
		foreach (string ligneDur in lignesDurAll) 
		{

			ligneTab = ligneDur.Split(' ');
			if(ligneTab[0].Equals("v"))
			{
				j = 0;
				foreach (string word in ligneTab) 
				{
					if(j == 1)
						vertex[i].x = float.Parse(word);
					else if(j == 2)
						vertex[i].y = float.Parse(word);
					else if (j== 3)
						vertex[i].z = float.Parse(word);

					j++;
				}

				i++;
			}

			/*if(ligneDur[0].Equals('v') && ligneDur[1].Equals('n') && ligneDur[2].Equals(' '))
			{
				//normals[nor] = ligneDur;
				nor++;
			}*/
			else if(ligneDur[0].Equals('v') && ligneDur[1].Equals('t') && ligneDur[2].Equals(' '))
			{
				textures[tex] = ligneDur;
				tex++;
			}
			else if(ligneDur[0].Equals('f') && ligneDur[1].Equals(' '))
			{
				string[] ligneTab3 = {""};
				Faces fTmp = new Faces();

				ligneTab3 = ligneDur.Split(' ');

				for(j=1;j<4;++j)
				{
					string[] ligneTab2 = {""};

					ligneTab2 = ligneTab3[j].Split('/');
					l = 0;
					foreach (string word in ligneTab2) 
					{
						if(l == 0 && string.IsNullOrEmpty(word) == false)
							fTmp.v[j-1] = int.Parse(word);
						else if(l == 1 && string.IsNullOrEmpty(word) == false)
							fTmp.t[j-1] = int.Parse(word);
						else if(l == 2 && string.IsNullOrEmpty(word) == false)
							fTmp.n[j-1] = int.Parse(word);

						l++;
					}
				}
				faces.Add(fTmp);

				fac++;
			}
		}

		nbVertex = i;
		nbNormals = nor;
		nbTextures = tex;
		//nbfaces = fac;
	}



	public string writeObj()
	{
		string[] vw = new string[15000];
		string test = "";
		string face_s = "";

		for(int i=0;i<nbVertex;i++)
		{
			vw[i] = "v" + " " + vertex2[i].x.ToString() + " " + vertex2[i].y.ToString() + " " + vertex2[i].z.ToString() + "\n";
			test += vw[i];
		}
		test += "\n";

		face_s = "";
		foreach (Faces f in faces) 
		{
			face_s += "f";
			for(int i=0;i<3;++i)
			{
				face_s += " ";
				if(f.v[i] != -1)
				{
					face_s += f.v[i].ToString();
				}
				face_s += "/";
				if(f.t[i] != -1)
				{
					face_s += f.t[i].ToString();
				}
				face_s += "/";
			}
			face_s += "\n";
		}
	
		test += face_s;
		test += "\n";

		return test;
	}

	public void meshTransform(List<GameObject> path, string obj_FileName)
	{
		/*Classe d'interpollation*/
		hermiteInterpol hermite = new hermiteInterpol();
		float dist;
		Vector3 tmp;
		Vector3 lineStart = new Vector3 (0.0f, 0.0f, 0.0f);
		Vector3 lineEnd = new Vector3 (0.0f, 0.0f, 0.0f);
		int lineSteps;
		float distanceParcouru = 0.0f;
		float distanceParcouru_d = 0.0f;
		string obj_s = "";

		float angle;
		
		Matrix4x4 mat = new Matrix4x4();
		Vector3 vectSansOffset = new Vector3 (0.0f, 0.0f, 0.0f);


		readObj();

		/*On écrit les textures*/
		for(int i=0;i<nbTextures;++i)
		{
			obj_s += textures[i] + "\n";
		}
		
		
		/*On parcour la liste de noeud*/
		for(int i = 1;i<path.Count-2;i++)
		{
			/*Si on ne dessine pas le rail*/
			if(hermite.getDrawStat(path, i) == false)
			{
				for(int g=0;g<nbVertex; ++g)
				{
					if(vertexFlag[g] == 0)
					{
						vertexFlag[g] = 1;
						vectSansOffset = vertex[g];
						vectSansOffset.x = 0.0f;
						vertex2[g] = tramsform(mat, vectSansOffset);
					}
					vertexFlag[g] = 0;
				}
				obj_s += writeObj();
				continue;
			}

			/*Calcule la distance entre le noeud actuel et le noeud suivant*/
			dist = 0.0f;
			lineStart = path[i].transform.position;
			for(int j = 1; j <= 100; j++) 
			{
				lineEnd = hermite.getPosOnPath((float)j/(float)100, path, i);
				tmp = lineEnd - lineStart;
				dist += tmp.magnitude;
				lineStart = lineEnd;
			}
			lineSteps = (int)(dist*30.0f);

			/*On interpole*/
			lineStart = path[i].transform.position;
			for(int k=1;k<lineSteps;k++)
			{
				lineEnd = hermite.getPosOnPath((float)k/(float)lineSteps, path, i);
				tmp = lineEnd - lineStart;
				distanceParcouru += tmp.magnitude;
				distanceParcouru_d += tmp.magnitude;
				
				if(distanceParcouru_d >= 0.05f)
				{
					distanceParcouru_d = 0.0f;

					/*On récupère l'angle*/
					angle = hermite.getRotationInterpolled(path, i, (float)k/(float)lineSteps);
					/*Calcule de la matrice de rotation*/
					mat = hermite.getRotationMatrix(lineStart, lineEnd, angle);
					
					if(distanceParcouru > distRails)
						distRails = distanceParcouru;

					int ttmpp;
					float ttmpp2;
					ttmpp = (int)distanceParcouru;
					ttmpp2 = distanceParcouru - (float)ttmpp;
					if(ttmpp2 > 0.5f)
						ttmpp2 -= 0.5f;
					if(ttmpp > nbTest)
					{
						nbTest = ttmpp;
						for(int g=0;g<nbVertex; ++g)
						{
							if(vertexFlag[g] == 0)
							{
								vertexFlag[g] = 1;
								vectSansOffset = vertex[g];
								vectSansOffset.x = 0.0f;
								vertex2[g] = tramsform(mat, vectSansOffset);
							}
							vertexFlag[g] = 0;
						}
						obj_s += writeObj();
						
						foreach(Faces ff in faces)
						{
							for(int tt=0;tt<3;++tt)
							{
								ff.v[tt] += nbVertex;
								ff.n[tt] += nbNormals;
							}
						}
						nbVertexEnTrop += nbVertex;
					}

					for(int g=0;g<nbVertex; ++g)
					{
						if(vertex[g].x < ttmpp2 && vertexFlag[g] == 0)
						{
							vertexFlag[g] = 1;
							vectSansOffset = vertex[g];
							vectSansOffset.x = 0.0f;
							vertex2[g] = tramsform(mat, vectSansOffset);
						}
					}
				}
				lineStart = lineEnd;
			}
		}
	
		File.WriteAllText(obj_FileName, obj_s);
	}


	public Vector3 tramsform(Matrix4x4 mat, Vector3 posVertex)
	{
		Vector3 tmp = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 pos = new Vector3(mat.GetColumn(3).x, mat.GetColumn(3).y, mat.GetColumn(3).z);
		mat.SetColumn(3, tmp);
		tmp = mat.MultiplyVector(posVertex) + pos;
		tmp.x *= -1.0f;
		return tmp;
	}
}



























