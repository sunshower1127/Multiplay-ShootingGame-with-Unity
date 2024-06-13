using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

public class BulletScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    int dir;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 3.5f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * dir * 7 * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground")
        {
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }

        if (!PV.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<PlayerScript>().Hit();
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    void DirRPC(int _dir)
    {
        dir = _dir;
    }

    [PunRPC]
    void DestroyRPC()
    {
        Destroy(gameObject);
    }
}
