using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D RB;
    public Animator AN;
    public SpriteRenderer SR;
    public PhotonView PV;
    public TMP_Text NicknameText;
    public Image HealthImage;

    bool isGround;
    Vector3 curPos;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }

    void Awake()
    {
        NicknameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NicknameText.color = PV.IsMine ? Color.green : Color.red;

        if (PV.IsMine)
        {
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine)
        {
            if ((transform.position - curPos).sqrMagnitude >= 100)
                transform.position = curPos;
            else
                transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            return;
        }

        float axis = Input.GetAxisRaw("Horizontal");
        RB.velocity = new Vector2(axis * 4, RB.velocity.y);

        if (axis != 0)
        {
            AN.SetBool("walk", true);
            PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
        }
        else
        {
            AN.SetBool("walk", false);
        }

        isGround = Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y - 0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
        AN.SetBool("jump", !isGround);
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGround)
        {
            PV.RPC("JumpRPC", RpcTarget.All);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(SR.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, SR.flipX ? -1 : 1);
            AN.SetTrigger("shot");
        }

    }

    public void Hit()
    {
        HealthImage.fillAmount -= 0.1f;
        if (HealthImage.fillAmount <= 0)
        {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void FlipXRPC(float axis)
    {
        SR.flipX = axis == -1;
    }

    [PunRPC]
    void JumpRPC()
    {
        RB.velocity = new Vector2(0, 0);
        RB.AddForce(Vector2.up * 700);
    }

    [PunRPC]
    void DestroyRPC()
    {
        Destroy(gameObject);
    }
}
