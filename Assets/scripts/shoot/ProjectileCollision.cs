﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileCollision : MonoBehaviour
{    
    public float _areaOfEffectRadius = 5.0f;
	public float _explosiveRadius = 5.0f;
	public float _explosiveForce = 5.0f;

    private GameObject _explosionRadiusObj;
    private GameObject _projectileParent;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Terrain")
        {
            //decrease terrain height within radius of _explosionRadiusObj
            _projectileParent = GameObject.Find("ProjectileParent");

			CreateAreaOfEffectSphere ();

			int terrainLayer = (1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("PhysicsObject"));
            Collider[] hitColliders = Physics.OverlapSphere(_explosionRadiusObj.transform.position, _areaOfEffectRadius, terrainLayer);
					
            for (int i = 0; i < hitColliders.Length; i ++)
            {
				if (hitColliders[i].tag == "Terrain") {
					Mesh collisionMesh = hitColliders[i].GetComponent<MeshFilter> ().mesh;
					int[] verticesInBounds = GetVerticesInBounds (hitColliders[i], collisionMesh);

                    GameObject terrainControllerObject = GameObject.Find("TerrainController");
                    TerrainController terrainController = (TerrainController)terrainControllerObject.GetComponent<TerrainController>();
                    List<GameObject> localObjRef = terrainController.getObjRefArray();
                    int indexObjRef = localObjRef.IndexOf(hitColliders[i].gameObject);
                    //Debug.Log("indexObjRef: " + indexObjRef);

                    int localArrayLength = terrainController.getArrayLength();
                    int localMeshLimit = terrainController._meshLimit;
                    int meshTotalPerCol = (localArrayLength / localMeshLimit);
                    int meshColorIndex = -1;
                    int meshRows = (int)Mathf.Floor((indexObjRef * 1.0f) / (meshTotalPerCol * 1.0f));
                    int meshColRemainder = indexObjRef % meshTotalPerCol;

                    if (indexObjRef < meshTotalPerCol)
                    {
                        meshColorIndex = localMeshLimit * indexObjRef;
                    }
                    else
                    {                       
                        if (meshColRemainder == 0)
                        {
                            meshColorIndex = localArrayLength * localMeshLimit * meshRows;
                        }
                        else
                        {
                            meshColorIndex = (localArrayLength * localMeshLimit * meshRows) + (localMeshLimit * meshColRemainder);
                        }                        
                    }
                    

                    if (verticesInBounds.Length > 0){
						Vector3[] collisionMeshVertices = collisionMesh.vertices;

	                    float circleRadius = _areaOfEffectRadius * 0.6f;

						for (int j = 0; j < verticesInBounds.Length; j++) {

							float distanceToSphereCenter = Vector3.Distance(collisionMeshVertices [verticesInBounds[j]], _explosionRadiusObj.transform.position);
							//Debug.Log ("distanceToSphereCenter: " + distanceToSphereCenter);

							if (circleRadius > distanceToSphereCenter && collisionMeshVertices [verticesInBounds[j]].y < _explosionRadiusObj.transform.position.y) {
								collisionMeshVertices [verticesInBounds [j]] = Vector3.MoveTowards (collisionMeshVertices [verticesInBounds [j]], _explosionRadiusObj.transform.position, -1.0f * (circleRadius - distanceToSphereCenter));
							}

                            //TODO: update color of vertices to brown
                            if (meshColorIndex > -1)
                            {
                                terrainController.setMeshColorArray(meshColorIndex + verticesInBounds[j], Color.black);
                            }
                        }

						collisionMesh.vertices = collisionMeshVertices;
						MeshCollider collisionMeshCollider = hitColliders [i].GetComponent<MeshCollider> ();
						collisionMeshCollider.sharedMesh = null;
						collisionMeshCollider.sharedMesh = collisionMesh;

                        terrainController.updateTerrainTexture();
                        if (meshColRemainder == 0)
                        {
                            terrainController.updateMeshMaterials(indexObjRef, localMeshLimit * meshRows, 0);
                        }
                        else
                        {
                            terrainController.updateMeshMaterials(indexObjRef, localMeshLimit * meshRows, localMeshLimit * meshColRemainder);
                        }
                    }

					//create debris
					GetComponent<CreateDebris> ().createDebrisParticleSystem ();
				}

				if (hitColliders[i].tag == "PhysicsObject") {
					//create explosion
					Rigidbody rigidbody = hitColliders[i].attachedRigidbody;
					rigidbody.AddExplosionForce (_explosiveForce, transform.position, _explosiveRadius, 5, ForceMode.Impulse);
				}
            }

			Destroy (_explosionRadiusObj);
            Destroy (gameObject);
        }
    }

	void CreateAreaOfEffectSphere() {
		//show area of effect
		_explosionRadiusObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		_explosionRadiusObj.gameObject.name = "explosionRadiusSphere";
		_explosionRadiusObj.transform.localScale = new Vector3(_areaOfEffectRadius, _areaOfEffectRadius, _areaOfEffectRadius);
		_explosionRadiusObj.transform.position = transform.position;
		_explosionRadiusObj.transform.rotation = transform.rotation;
		_explosionRadiusObj.transform.parent = _projectileParent.transform;
		_explosionRadiusObj.GetComponent<MeshRenderer> ().enabled = false;
	}


	int[] GetVerticesInBounds(Collider collider, Mesh collisionMesh) {

		int[] matchedVerticesArray;
		List<int> matchedVerticesList = new List<int>();
		//Color color = new Color (Random.value, Random.value, Random.value, 1.0f);

		for (int iter = 0; iter < collisionMesh.vertexCount; iter++) {
			Vector3 convertedVertex = collider.transform.TransformPoint (collisionMesh.vertices [iter]);

			if (_explosionRadiusObj.GetComponent<SphereCollider>().bounds.Contains (convertedVertex)) {
				//Debug.DrawRay (convertedVertex, Vector3.up, color, 50.0f);
				//Debug.Log ("matched vert - iter: " + iter + " - vector: " + collisionMesh.vertices [iter]);
				matchedVerticesList.Add(iter);
			}
		}

		matchedVerticesArray = matchedVerticesList.ToArray();

		return matchedVerticesArray;
	}
}