using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

namespace WasaaMP {
    public class CubeManager : MonoBehaviourPun
    {  
        private bool caught = false;

        private Color colorBeforeHighlight ;
        private Color oldColor ;
        private float oldAlpha ;

        public Dictionary<int, Vector3> positions;
        public Dictionary<int, int> strengths;
        public int catchable = 0; //0: idle 1:catchable 2:!catchable

        public int globalStrength;
        public int clientsCount;
        public Vector3 barycentre;

        public bool isMasterClient;

        public CursorTool client = null;
        public int weight = 20;

        
        void Start()
        {
            positions = new Dictionary<int, Vector3>();
            strengths = new Dictionary<int, int>();
        }

        void Update()
        {

            if (PhotonNetwork.IsConnected) 
            {
               ShowColor(); 
            }
            
    
            if(photonView.IsMine || !PhotonNetwork.IsConnected)
            {
                isMasterClient = true;
            }

            if(client && !isMasterClient)
            {
                photonView.RPC("UpdatePosition", RpcTarget.All, client.transform.position);
                photonView.RPC("UpdateStrength", RpcTarget.All, client.strength);
            }

            if (isMasterClient)
            {
                clientsCount = 0;
                clientsCount = positions.Count;
                globalStrength = 0;
                globalStrength = strengths.Sum(x => x.Value);

                barycentre = Vector3.zero;
                foreach(KeyValuePair<int, Vector3> entry in positions)
                {
                    barycentre += entry.Value;
                }

                if(client)
                {
                    clientsCount = clientsCount + 1;
                    globalStrength = globalStrength + client.strength;
                    barycentre = barycentre + client.transform.position;
                }

                print("clientsCount "+clientsCount);
                print("globalStrength "+globalStrength);

                if (globalStrength >= weight && clientsCount > 0)
                {
                    catchable = 1;
                    photonView.RPC("UpdateCatchable", RpcTarget.All, 1);
                    barycentre = barycentre / clientsCount;
                    transform.position = barycentre;
                }
                else if (clientsCount > 0)
                {
                    catchable = 2;
                    photonView.RPC("UpdateCatchable", RpcTarget.All, 2);
                }
                else if (PhotonNetwork.IsConnected)
                {
                    catchable = 0;
                    photonView.RPC("UpdateCatchable", RpcTarget.All, 0);
                }
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
                if (!isMasterClient)
                {
                    photonView.RPC("UpdateRelease", RpcTarget.All);
                }
                else
                {
                    clientsCount = clientsCount - 1;
                    globalStrength = globalStrength - client.strength;
                    barycentre = barycentre - client.transform.position;
                }
            }
        }

        [PunRPC] private void UpdateStrength(int strength, PhotonMessageInfo info) {
            strengths[info.Sender.ActorNumber] = strength;
        }

        [PunRPC] private void UpdatePosition(Vector3 position, PhotonMessageInfo info) {
            positions[info.Sender.ActorNumber] = position;
        }

        [PunRPC] private void UpdateRelease(PhotonMessageInfo info) {
            positions.Remove(info.Sender.ActorNumber);
            strengths.Remove(info.Sender.ActorNumber);
        }

        [PunRPC] private void UpdateCatchable(int value) {
            catchable = value;
        }

        [PunRPC] public void ShowColor () {
            if (catchable == 1) {
                Renderer renderer = GetComponentInChildren <Renderer> () ;
                oldAlpha = renderer.material.color.a ;
                colorBeforeHighlight = renderer.material.color ;
                Color c = Color.cyan ;
                renderer.material.color = new Color (c.r, c.g, c.b, 0.5f) ;
                PhotonNetwork.SendAllOutgoingCommands () ;
            }
            if (catchable == 2) {
                Renderer renderer = GetComponentInChildren <Renderer> () ;
                oldAlpha = renderer.material.color.a ;
                colorBeforeHighlight = renderer.material.color ;
                Color c = Color.red ;
                renderer.material.color = new Color (c.r, c.g, c.b, 0.5f) ;
                PhotonNetwork.SendAllOutgoingCommands () ;
            }
            if (catchable == 0){
                Renderer renderer = GetComponentInChildren <Renderer> () ;
                oldAlpha = renderer.material.color.a ;
                colorBeforeHighlight = renderer.material.color ;
                Color c = Color.yellow ;
                renderer.material.color = new Color (c.r, c.g, c.b, 0.5f) ;
                PhotonNetwork.SendAllOutgoingCommands () ;
            }
        }
        
    }
}
