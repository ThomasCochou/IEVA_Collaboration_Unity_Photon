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
        public int weight;

        public int strengthSum;
        public int clientsCount;
        public Vector3 barycenter;

        public bool isMasterClient;

        public CursorTool client = null;
        

        
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
               if(barycenter != Vector3.zero && catchable == 1)
               {
                    transform.position = barycenter;
               }
            }
            
    
            if(photonView.IsMine)
            {
                isMasterClient = true;
            }

            if(client && !isMasterClient)
            {
                print("client "+barycenter);
                photonView.RPC("UpdatePosition", RpcTarget.All, client.transform.position);
                photonView.RPC("UpdateStrength", RpcTarget.All, client.strength);
            }

            if (isMasterClient)
            {
                clientsCount = 0;
                clientsCount = positions.Count;
                strengthSum = 0;
                strengthSum = strengths.Sum(x => x.Value);

                barycenter = Vector3.zero;
                foreach(KeyValuePair<int, Vector3> entry in positions)
                {
                    barycenter = barycenter + entry.Value;
                }

                if(client)
                {
                    clientsCount = clientsCount + 1;
                    strengthSum = strengthSum + client.strength;
                    barycenter = barycenter + client.transform.position;
                }

                if (strengthSum >= weight && clientsCount > 0)
                {
                    catchable = 1;
                    photonView.RPC("UpdateCatchable", RpcTarget.All, 1);

                    barycenter = barycenter / clientsCount;
                    photonView.RPC("UpdateBarycenter", RpcTarget.All, barycenter);
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
            this.client = client;
        }

        public void Release(CursorTool client) {
            this.client = null;
            if (!isMasterClient)
            {
                photonView.RPC("UpdateRelease", RpcTarget.All);
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

        [PunRPC] private void UpdateBarycenter(Vector3 value) {
            barycenter = value;
        }

        [PunRPC] public void ShowColor () {
            if (catchable == 1) {
                Renderer renderer = GetComponentInChildren <Renderer> () ;
                oldAlpha = renderer.material.color.a ;
                colorBeforeHighlight = renderer.material.color ;
                Color c = Color.cyan ;
                renderer.material.color = new Color (c.r, c.g, c.b, 0.5f) ;
            }
            if (catchable == 2) {
                Renderer renderer = GetComponentInChildren <Renderer> () ;
                oldAlpha = renderer.material.color.a ;
                colorBeforeHighlight = renderer.material.color ;
                Color c = Color.red ;
                renderer.material.color = new Color (c.r, c.g, c.b, 0.5f) ;
            }
            if (catchable == 0){
                Renderer renderer = GetComponentInChildren <Renderer> () ;
                oldAlpha = renderer.material.color.a ;
                colorBeforeHighlight = renderer.material.color ;
                Color c = Color.yellow ;
                renderer.material.color = new Color (c.r, c.g, c.b, 0.5f) ;
            }
        }
        
    }
}
