using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CitizenCarManager : MonoBehaviour {
NavMeshAgent agent;
private Transform NexWayPoint;
float velocityCache;
RaycastHit Hit;
[SerializeField] private GameObject WayPointsParent,CarMesh;
[SerializeField] float minSpeed = 15,maxSpeed = 30;
float timer=0,evadeRight =0 , evadeLeft = 0;
bool Obsticle = false;
	void Start () {
		agent = gameObject.GetComponent<NavMeshAgent>();
		GetNewWayPoint();
		agent.SetAreaCost(0,3);
		agent.SetAreaCost(5,0);
	}
	void Update () {
		if(agent.pathPending) return;

		if(agent.remainingDistance <= 2f)
		{
			GetNewWayPoint();
		}
            if (Physics.Linecast(gameObject.transform.position+Vector3.up,gameObject.transform.position+Vector3.up + (-CarMesh.transform.forward*6),out Hit))
			{
				Obsticle = true;
			}else if (Physics.Linecast(gameObject.transform.position+Vector3.up+gameObject.transform.right,gameObject.transform.position+Vector3.up + (-CarMesh.transform.forward*6-gameObject.transform.right*1.5f),out Hit))
			{
				Obsticle = true;
			}else if (Physics.Linecast(gameObject.transform.position+Vector3.up-gameObject.transform.right,gameObject.transform.position+Vector3.up + (-CarMesh.transform.forward*6+gameObject.transform.right*1.5f),out Hit))
			{
				Obsticle = true;
			}else if(Physics.CheckSphere(CarMesh.transform.position+Vector3.up*1.5f-CarMesh.transform.forward*5,.75f))
			{
				Obsticle = true;
			}
			else if(Physics.CheckSphere(CarMesh.transform.position+Vector3.up*1.5f-CarMesh.transform.forward*5-gameObject.transform.right*1.25f,.75f))
			{
				Obsticle = true;
				evadeRight = .25f;
			}
			else if(Physics.CheckSphere(CarMesh.transform.position+Vector3.up*1.5f-CarMesh.transform.forward*5+gameObject.transform.right*1.25f,.75f))
			{
				Obsticle = true;
				evadeLeft =.25f;
			}
			else Obsticle = false;

			if(evadeLeft > 0) {evadeLeft -= Time.deltaTime; agent.velocity += gameObject.transform.TransformVector(-Vector3.right*30f*Time.deltaTime);}
			if(evadeRight >0) {evadeRight-= Time.deltaTime;  agent.velocity += gameObject.transform.TransformVector(Vector3.right*30f*Time.deltaTime);/*agent.velocity = Vector3.RotateTowards(agent.desiredVelocity,gameObject.transform.TransformVector(Vector3.right*2f),1*Time.deltaTime,.0f);*/}

			Debug.DrawLine(gameObject.transform.position+Vector3.up,gameObject.transform.position+Vector3.up + (-CarMesh.transform.forward*6),Color.red);
			Debug.DrawLine(gameObject.transform.position+Vector3.up-gameObject.transform.right,gameObject.transform.position+Vector3.up + (-CarMesh.transform.forward*6-gameObject.transform.right*1.5f),Color.red);
			Debug.DrawLine(gameObject.transform.position+Vector3.up+gameObject.transform.right,gameObject.transform.position+Vector3.up + (-CarMesh.transform.forward*6+gameObject.transform.right*1.5f),Color.red);

		if(!Obsticle)
		{ 
			agent.speed = Mathf.Lerp(Random.Range(minSpeed,maxSpeed),Random.Range(minSpeed,maxSpeed),Random.Range(5,10));
		}
		else {
			timer += Time.deltaTime;
			agent.velocity += gameObject.transform.TransformVector(Vector3.back*30*Time.deltaTime);
			CarMesh.transform.LookAt(gameObject.transform.TransformVector(Vector3.forward));
		}
		if(agent.velocity.y - velocityCache <0.8f && agent.velocity.y - velocityCache >-0.08f )
		CarMesh.transform.rotation = Quaternion.Lerp(CarMesh.transform.rotation,Quaternion.LookRotation(-agent.velocity),5);
		velocityCache = agent.velocity.y;
	}
	void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(CarMesh.transform.position+Vector3.up*1.5f-CarMesh.transform.forward*5+gameObject.transform.right*1.25f,.75f);
		Gizmos.DrawWireSphere(CarMesh.transform.position+Vector3.up*1.5f-CarMesh.transform.forward*5-gameObject.transform.right*1.25f,.75f);
		Gizmos.DrawWireSphere(CarMesh.transform.position+Vector3.up*1.5f-CarMesh.transform.forward*5,.75f);
	}

	void GetNewWayPoint()
	{
		int i = Random.Range(0,WayPointsParent.transform.childCount);
		NexWayPoint = WayPointsParent.transform.GetChild(i);
		agent.destination = (NexWayPoint.position);
	}
}