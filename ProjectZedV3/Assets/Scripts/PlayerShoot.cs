using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;

    private PlayerWeapon currentWeapon;
    private WeaponManager weaponManager;

    public AudioSource testaudio;

	void Start () {
		if(cam == null)
        {
            Debug.LogError("Pas de caméra référencée.");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
        testaudio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (PauseMenu.isOn == true)
        {
            return;
        }

        if (currentWeapon.bullets < currentWeapon.maxBullets)
        {
            if (Input.GetButtonDown("Reload"))
            {
                weaponManager.Reload();
                return;
            }
        }

        if (currentWeapon.fireRate <= 0f)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Audio", 0f, 3f/currentWeapon.fireRate);
                InvokeRepeating("Shoot", 0f, 1f/currentWeapon.fireRate);
            }else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
                CancelInvoke("Audio");
            }
        }
    }

    // Fonction appelée sur le serveur lorsqu'un joueur tir
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    // Fait apparaitre les effets de tir sur tous les clients
    [ClientRpc]
    void RpcDoShootEffect()
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }
    
    [Command]
    void CmdOnHit2(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect2(_pos, _normal);
    }

    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 2f);
    }
    
    [ClientRpc]
    void RpcDoHitEffect2(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab2, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 2f);
    }

    [Client]
    private void Shoot()
    {
        if (!isLocalPlayer || weaponManager.isReloading)
        {
            return;
        }

        if(currentWeapon.bullets <= 0)
        {
            weaponManager.Reload();
            return;
        }
        

        currentWeapon.bullets--;

        Debug.Log("Bullets remaining: " + currentWeapon.bullets);

        // Fonction appelée lors d'un tir (du joueur local)
        CmdOnShoot();

        RaycastHit _hit;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
        {
            int money = 0;
            
            if(_hit.collider.tag == "Player")
            {
                CmdOnHit(_hit.point, _hit.normal);
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage, transform.name);
                money = 10;
            }
            else if (_hit.collider.tag == "Enemy")
            {
                CmdOnHit(_hit.point, _hit.normal);
                Enemy enemy = _hit.transform.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Player sourcePlayer = GameManager.GetPlayer(transform.name);
                    enemy.DieAnim(5.0f, currentWeapon.damage);
                    //enemy.DestroyTransform();
                    //sourcePlayer.kills++;
                    money = 5;
                }
            }

            Player player = GetComponent<Player>();
            if (player.currentMoney + money > player.maxMoney)
                player.currentMoney = player.maxMoney;
            else
                player.currentMoney += money;
            
            CmdOnHit2(_hit.point, _hit.normal);
        }

        if(currentWeapon.bullets <= 0)
        {
            weaponManager.Reload();
        }
    }
    
    private void Audio()
    {
        testaudio.Play();
    }
    

    [Command]
    void CmdPlayerShot(string _playerID, int _damage, string _sourceID)
    {
        Debug.Log(_playerID + " a été touché.");

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage, _sourceID);
    }

}
