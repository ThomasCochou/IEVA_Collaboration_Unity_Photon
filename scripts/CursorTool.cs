using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace WasaaMP {
	public class CursorTool : MonoBehaviourPun {
		private bool caught ;
		public CubeManager target ;
		public int strength = 20;

		void Start () {
			caught = false ;
		}
		
		public void Catch () 
		{
			if (target != null) 
			{
				if ((! caught) && (transform != target.transform))
				{
					target.Catch(this);
					caught = true;
				}
				PhotonNetwork.SendAllOutgoingCommands () ;
			}
		}

		public void Release () {
			if (caught) {
				target.Release(this);
				caught = false ;
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
					target.Release(this);
					PhotonNetwork.SendAllOutgoingCommands () ;
					target = null ;
				}
			}
		}

	}

}