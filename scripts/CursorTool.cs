using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace WasaaMP {
	public class CursorTool : MonoBehaviourPun {
		private bool caught ;
		public bool catching;
		public CubeManager target ;
		public int strength = 20;

		void Start () {
			caught = false ;
			catching = false;
		}
		
		public void Catch () 
		{
			if (target != null) 
			{
				if ((! caught) && (transform != target.transform))
				{
					target.Catch(this);
					catching = true;
					caught = true;
				}
				else
				{
					catching = false;
				}
				PhotonNetwork.SendAllOutgoingCommands () ;
			}
		}

		public void Release () {
			if (caught) {
				target.Release(this);
				caught = false ;
				catching = false;
				PhotonNetwork.SendAllOutgoingCommands () ;
			}
		}

		void OnTriggerEnter (Collider other) {
			if (! caught) {
				target = other.gameObject.GetComponent<CubeManager> () ;
				if (target != null) {
					PhotonNetwork.SendAllOutgoingCommands () ;
				}
			}
		}

		void OnTriggerExit (Collider other) {
			if (! caught) {
				if (target != null) {
					PhotonNetwork.SendAllOutgoingCommands () ;
					target = null ;
				}
			}
		}

	}

}