using UnityEngine;
using UnityEngine.Networking;

public class PlayerShoot : NetworkBehaviour
{

    public PlayerWeapon weapon;
    
    [SerializeField] private Camera cam;

    [SerializeField] private LayerMask mask;
    void Start()
    {
        if (cam == null)
        {
            Debug.LogError("Pas de caméra référencée.");
            this.enabled = false;
        }
    }

    private void Update()
    {
        if (PauseMenu.GameIsPaused == true)
        {
            return;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
            InvokeRepeating("Shoot", 0f, 0.2f); //Arme automatique
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            CancelInvoke("Shoot");
        }
    }

    [Client] 
    private void Shoot()
    {
        Debug.Log("Tir effectué");
        //permet de récupérer des infos sur l'objet touché (type, distance ...)
        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, weapon.range, mask))
        {
            Player player = GetComponent<Player>();
            
            if (_hit.collider.tag == "Player")
            {
                CmdPlayerShot(_hit.collider.name, weapon.damage);
                player.score += 10;
                Debug.Log("score : "+player.score);
            }
            else if (_hit.collider.tag == "Enemy")
            {
                Enemy enemy = _hit.transform.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.DestroyTransform();
                    player.score++;
                    Debug.Log("score : "+player.score);
                }
            }
        }
    }

    [Command]
    private void CmdPlayerShot(string _playerID, int _damage)
    {
        Debug.Log(_playerID + " a été touché.");

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage);
    }
}
