using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

namespace Con.IgorGuriev.FPSMultiplayer
{
    public class Player : MonoBehaviourPunCallbacks
    {
        public float speed;
        public float sprintModifier;
        public float jumpForce;
        public int max_health;
        public Camera normalCam;
        public GameObject cameraParent;
        public Transform weaponParent;
        public Transform groundDetector;
        public LayerMask ground;

        private Transform ui_healthbar;
        private Text ui_ammo;

        private Rigidbody rig;

        private Vector3 targetWeaponBobPosition;
        private Vector3 weaponParentOrigin;

        private float movementCounter;
        private float idleCounter;

        public float baseFOV;
        public float sprintFOVModifier = 1.25f;

        private int current_health;

        private Manager manager;
        private Weapon weapon;

        private void Start()
        {
            manager = GameObject.Find("Manager").GetComponent<Manager>();
            weapon = GetComponent<Weapon>();

            current_health = max_health;

            cameraParent.SetActive(photonView.IsMine);
            if(!photonView.IsMine) gameObject.layer = 8;

            baseFOV = normalCam.fieldOfView;

            
            rig = GetComponent<Rigidbody>();
            weaponParentOrigin = weaponParent.localPosition;

            if (photonView.IsMine)
            {
                ui_healthbar = GameObject.Find("HUD/Bar").transform;
                ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
                RefreshHealthBar();
            }

        }

        private void Update()
        {
            if (!photonView.IsMine) return;

            // axles
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");

            // controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool pause = Input.GetKeyDown(KeyCode.P);

            // states
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;

            if(pause)
            {
                GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
            }

            if (Pause.paused)
            {
                t_hmove = 0f;
                t_vmove = 0f;
                sprint = false;
                jump = false;
                pause = false;
                isGrounded = false;
                isJumping = false;
                isSprinting = false;
            }

            // jumping
            if (isJumping)
            {
                rig.AddForce(Vector3.up * jumpForce);
            }

            if (Input.GetKeyDown(KeyCode.U)) TakeDamage(10);

            // head bob
            if (t_hmove == 0 && t_vmove == 0) 
            { 
                HeadBob(idleCounter, 0.02f, 0.02f); 
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else  if (!isSprinting)
            { 
                HeadBob(movementCounter, 0.02f, 0.02f); 
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else
            {
                HeadBob(movementCounter, 0.02f, 0.02f);
                movementCounter += Time.deltaTime * 5f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }


            
            
        }

        void FixedUpdate()
        {
            if (!photonView.IsMine) return;

            // axles
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");

            // controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool jump = Input.GetKeyDown(KeyCode.Space);

            // states
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;

            if (Pause.paused)
            {
                t_hmove = 0f;
                t_vmove = 0f;
                sprint = false;
                jump = false;
                isGrounded = false;
                isJumping = false;
                isSprinting = false;
            }

            // ui refresh
            RefreshHealthBar();
            weapon.RefreshAmmo(ui_ammo);

            // jumping
            if (isJumping)
            {
                rig.AddForce(Vector3.up * jumpForce);
            }

            // head bob
            if (t_hmove == 0 && t_vmove == 0)
            {
                HeadBob(idleCounter, 0.02f, 0.02f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if (!isSprinting)
            {
                HeadBob(movementCounter, 0.02f, 0.02f);
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else
            {
                HeadBob(movementCounter, 0.02f, 0.02f);
                movementCounter += Time.deltaTime * 5f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }


            // movement
            Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();

            float t_adjustedSpeed = speed;
            if (isSprinting) t_adjustedSpeed *= sprintModifier;

            Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustedSpeed * Time.deltaTime;
            t_targetVelocity.y = rig.velocity.y;
            rig.velocity = t_targetVelocity;

            // field of view
            if (isSprinting) { normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f); }
            else { normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f); ; }
        }

        void HeadBob(float p_z, float p_x_intensity, float p_y_intensity)
        {
            targetWeaponBobPosition = weaponParentOrigin + new Vector3(Mathf.Cos(p_z) * p_x_intensity, Mathf.Sin(p_z * 2) * p_y_intensity, weaponParentOrigin.z);
        }

        void RefreshHealthBar ()
        {
            float t_health_ratio = (float)current_health / (float)max_health;
            ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 10f);
        } 
        
        public void TakeDamage (int p_damage)
        {
            if(photonView.IsMine)
            {
                current_health -= p_damage;
                RefreshHealthBar();

                if(current_health <= 0)
                {
                    manager.Spawn();
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            
        }

    }
}