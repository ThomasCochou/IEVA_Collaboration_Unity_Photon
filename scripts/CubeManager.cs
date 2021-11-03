using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

namespace WasaaMP {
    public class CubeManager : MonoBehaviourPun
    {  
        private bool catchable;
        private bool caught = false;

        private Color colorBeforeHighlight ;
        private Color oldColor ;
        private float oldAlpha ;

        public Dictionary<int, Vector3> positions;
        public Dictionary<int, int> strengths;

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
            if(photonView.IsMine || !PhotonNetwork.IsConnected)
            {
                isMasterClient = true;
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

                if (globalStrength >= weight && clientsCount > 0)
                {
                    catchable = true;
                    barycentre = barycentre / clientsCount;
                    transform.position = barycentre;
                    PhotonNetwork.SendAllOutgoingCommands () ;
                }
                else if (!(globalStrength >= weight && clientsCount > 0))
                {
                    catchable = false;
                }
            }

            else if (client)
            {
                photonView.RPC("UpdatePosition", RpcTarget.All, client.transform.position);
                photonView.RPC("UpdateStrength", RpcTarget.All, client.strength);
                PhotonNetwork.SendAllOutgoingCommands () ;
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

        [PunRPC] public void ShowColor () {
            if (catchable) {
                Renderer renderer = GetComponentInChildren <Renderer> () ;
                oldAlpha = renderer.material.color.a ;
                colorBeforeHighlight = renderer.material.color ;
                Color c = Color.cyan ;
                renderer.material.color = new Color (c.r, c.g, c.b, 0.5f) ;
                PhotonNetwork.SendAllOutgoingCommands () ;
            }
            else if (!catchable) {
                Renderer renderer = GetComponentInChildren <Renderer> () ;
                oldAlpha = renderer.material.color.a ;
                colorBeforeHighlight = renderer.material.color ;
                Color c = Color.red ;
                renderer.material.color = new Color (c.r, c.g, c.b, 0.5f) ;
                PhotonNetwork.SendAllOutgoingCommands () ;
            }
        }

        [PunRPC] public void HideColor () {
            Renderer renderer = GetComponentInChildren <Renderer> () ;
            oldAlpha = renderer.material.color.a ;
            colorBeforeHighlight = renderer.material.color ;
            Color c = Color.yellow ;
            renderer.material.color = new Color (c.r, c.g, c.b, 0.5f) ;
            PhotonNetwork.SendAllOutgoingCommands () ;
        }
        
    }
}
