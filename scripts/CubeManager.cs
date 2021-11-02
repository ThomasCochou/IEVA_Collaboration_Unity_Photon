using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

namespace WasaaMP {
    public class CubeManager : MonoBehaviourPun
    {  
        private bool catchable = false;
        private bool caught = false;

        private Color colorBeforeHighlight ;
        private Color oldColor ;
        private float oldAlpha ;

        public Dictionary<int, Vector3> positions;
        public Dictionary<int, int> strengths;

        public int globalStrength;
        public int clientsCount;
        public Vector3 barycentre;

        public CursorTool client = null;
        private int weight = 20;

        
        void Start()
        {
            positions = new Dictionary<int, Vector3>();
            strengths = new Dictionary<int, int>();
        }

        void Update()
        {
            // masterClient
            if (photonView.IsMine || !PhotonNetwork.IsConnected)
            {

                clientsCount = positions.Count;
                globalStrength = strengths.Sum(x => x.Value);

                barycentre = Vector3.zero;
                foreach(KeyValuePair<int, Vector3> entry in positions)
                {
                    barycentre += entry.Value;
                }

                if(client){
                    clientsCount = clientsCount + 1;
                    globalStrength = globalStrength + client.strength;
                    barycentre = barycentre + client.transform.position;
                }

                if (globalStrength >= weight && clientsCount > 0)
                {
                    barycentre = barycentre / clientsCount;
                    transform.position = barycentre;
                }
            }


            // otherClient
            else if (client)
            {
                photonView.RPC("UpdatePosition", RpcTarget.MasterClient, client.transform.position);
                photonView.RPC("UpdateStrength", RpcTarget.MasterClient, client.strength);
            }
        }

        public void Catch(CursorTool client) {
            if (this.client == null)
            {
                this.client = client;
            }
        }

        public void Release(CursorTool client) {
            if (this.client == client)
            {
                this.client = null;
                if (!(photonView.IsMine || !PhotonNetwork.IsConnected))
                {
                    photonView.RPC("UpdateRelease", RpcTarget.MasterClient);
                }
            }
        }

        [PunRPC] private void UpdateStrength(int strength, PhotonMessageInfo info) {
            strengths[info.Sender.ActorNumber] = strength;
        }

        [PunRPC] private void UpdatePosition(Vector3 pos, PhotonMessageInfo info) {
            positions[info.Sender.ActorNumber] = pos;
        }

        [PunRPC] private void UpdateRelease(PhotonMessageInfo info) {
            positions.Remove(info.Sender.ActorNumber);
            strengths.Remove(info.Sender.ActorNumber);
        }


        [PunRPC] public void ShowCaught () {
            if (! caught) {
                var rb = GetComponent<Rigidbody> () ;
                rb.isKinematic = true ;
                Renderer renderer = GetComponentInChildren <Renderer> () ;
                oldColor = renderer.material.color ;
                renderer.material.color = Color.yellow ;
                caught = true ;
            }
        }

        [PunRPC] public void ShowReleased () {
            if (caught) {
                var rb = GetComponent<Rigidbody> () ;
                rb.isKinematic = false ;
                Renderer renderer = GetComponentInChildren <Renderer> () ;
                renderer.material.color = oldColor ;
                caught = false ;
            }
        }

        [PunRPC] public void ShowCatchable () {
            if (! caught) {
                if (! catchable) {
                    Renderer renderer = GetComponentInChildren <Renderer> () ;
                    oldAlpha = renderer.material.color.a ;
                    colorBeforeHighlight = renderer.material.color ;
                    //Color c = renderer.material.color ;
                    Color c = Color.cyan ;
                    renderer.material.color = new Color (c.r, c.g, c.b, 0.5f) ;
                    catchable = true ;
                }
            }
        }
        
        [PunRPC] public void HideCatchable () {
            if (! caught) {
                if (catchable) {
                    Renderer renderer = GetComponentInChildren <Renderer> () ;
                    //Color c = renderer.material.color ;
                    Color c = colorBeforeHighlight ;
                    renderer.material.color = new Color (c.r, c.g, c.b, oldAlpha) ;
                    catchable = false ;
                }
            }
        }
        
    }
}
