﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitController : MonoBehaviour
{
    [Header("Stats")]
    public bool inControl = false;
    public int health;
    public int speed;
    public int strenght;
    public int defense;

    private Animator anim;
    private CharacterController cc;
    private bool canJump = true;
    private bool attackReady = true;

    [Header("CameraRef")]
    public Text timerText;
    public GameObject cameraObject;
    public bool rotateToCamera;

    [Header("Fortify")]
    public GameObject shield;
    public bool fortified;

    [Header("Weapon")]
    public int weaponDamage;
    public int weaponRange;
    public weapons currentWeapon;
    public enum weapons { noWeapon, powerPunch, knife, warHammer, gun }

    [Header("TeamTag")]
    public string teamName;

    private void Start()
    {
        anim = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (inControl == true)
        {
            Movement();
            Attack();
            Jump();
            if (rotateToCamera)
            {
                RotateTowardsCamera();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                shield.SetActive(true);
                fortified = true;
                StopCoroutine("Timer");
                TimersUp();
            }
        }
    }

    public void GainControl()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        timerText = GameObject.Find("TimerText").GetComponent<Text>();
        anim.speed = 1 + (speed / 10);
        attackReady = true;
        fortified = false;
        shield.SetActive(false);
        inControl = true;
        cameraObject.GetComponent<Camera>().enabled = true;
        rotateToCamera = true;
        StartCoroutine("Timer");
    }

    IEnumerator Timer()
    {
        int timeLeft = 10;
        timerText.text = timeLeft.ToString();
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(1f);
            timeLeft--;
            timerText.text = timeLeft.ToString();
        }
        TimersUp();
    }

    public void TimersUp()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        inControl = false;
        cameraObject.GetComponent<Camera>().enabled = false;
        rotateToCamera = false;
        anim.SetFloat("Horizontal", 0);
        anim.SetFloat("Vertical", 0);
        GameManager.instance.TurnEnded();
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canJump && attackReady)
        {
            anim.Play("Jump");
            canJump = false;
        }
    }
    
    void ResetJump()
    {
        canJump = true;
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && attackReady && canJump)
        {
            anim.speed = 1;
            switch (currentWeapon)
            {
                case weapons.noWeapon:
                    anim.Play("Punch");
                    weaponDamage = 2 + strenght;
                    weaponRange = 1;
                    break;
                case weapons.powerPunch:
                    anim.Play("PowerKick");
                    weaponDamage = 8 + strenght;
                    weaponRange = 1;
                    break;
                case weapons.knife:
                    anim.Play("Knife");
                    weaponDamage = 15 + strenght;
                    weaponRange = 1;
                    break;
                case weapons.warHammer:
                    anim.Play("Hammer");
                    weaponDamage = 40 + strenght;
                    weaponRange = 2;
                    break;
                case weapons.gun:
                    anim.Play("Shoot");
                    weaponDamage = 10 + strenght;
                    weaponRange = 10;
                    break;
                default:
                    break;
            }
            attackReady = false;
        }
    }

    public void AttackRay()
    {
        RaycastHit hit;

        Vector3 rayOrigin = transform.position + transform.forward * 0.2f + transform.up * 0.5f;
        Debug.DrawRay(rayOrigin, transform.forward * weaponRange, Color.blue, 1);
        if (Physics.Raycast(rayOrigin, transform.forward, out hit, weaponRange))
        {
            if (hit.collider.tag != teamName)
            {
                Debug.Log("Raycast hit " + hit.collider.name);
                UnitController ut = hit.collider.GetComponent<UnitController>();
                if (ut != null)
                {
                    ut.TakeDamage(weaponDamage);
                }
            }
        }
    }

    void ResetAttack()
    {
        anim.speed = 1 + (speed / 10);
        Invoke("AttackReset", 0.5f);
    }

    void AttackReset()
    {
        attackReady = true;
    }

    void Movement()
    {
        var x = Input.GetAxis("Horizontal");
        var z = Input.GetAxis("Vertical");

        anim.SetFloat("Horizontal", x);
        anim.SetFloat("Vertical", z);
    }

    void RotateTowardsCamera()
    {
        var CharacterRotation = cameraObject.transform.rotation;
        CharacterRotation.x = 0;
        CharacterRotation.z = 0;
        transform.rotation = CharacterRotation;
    }

    public void TakeDamage(int damage)
    {
        if (fortified == false)
        {
            health -= damage;
        }
        else
        {
            int calcDamaged = Mathf.RoundToInt(damage / defense);
            health -= calcDamaged;
        }
        if (health < 1)
        {
            anim.Play("Death");
            cc.enabled = false;
            if (teamName == "Blue Team")
            {
                GameManager.instance.teamBlue--;
                GameManager.instance.CheckIfWon();
            }
            else if (teamName == "Red Team")
            {
                GameManager.instance.teamRed--;
                GameManager.instance.CheckIfWon();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Death"))
        {
            TakeDamage(1000);
        }
    }
}
