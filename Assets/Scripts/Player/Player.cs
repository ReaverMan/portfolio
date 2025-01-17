using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    /// <summary>
    /// 플레이어 이동속도
    /// </summary>
    public float moveSpeed = 3f;

    /// 플레이어 점프력
    /// </summary>
    public float jumpPower = 5f;

    // 인벤토리 및 플레이어 UI 하면서 건든거 ----------------------------------------------------------------------/

    Inventory inven;

    public Inventory Inventory => inven;

    /// <summary>
    /// 플레이어의 최대 체력
    /// </summary>
    float maxHp = 100;

    public float MaxHp => maxHp;

    /// <summary>
    /// 플레이어 체력
    /// </summary>
    float hp = 100;
    public float Hp
    {
        get => hp;
        set
        {
            if (hp != value)
            {
                hp = value;
                hp = Mathf.Clamp(hp, 0, maxHp);
                onHealthChange?.Invoke(hp);
                if (hp < 0.1)
                {
                    Die();
                }
            }
        }
    }


    /// <summary>
    /// 플레이어의 무게
    /// </summary>
    float weight = 0;

    public float Weight
    {
        get => weight;
        set
        {
            if(weight != value)
            {
                weight = value;
                onWeightChange?.Invoke(weight);
                // 현재 무게에 따라 이속속도 가변하는 것 추가하기
            }
        }
    }

    PlayerUI playerUI;
    
    public PlayerUI PlayerUI => playerUI;

    SlotNumber slotNum;

    public SlotNumber SlotNumber => slotNum;

    // ---------------------------------------------------------------------------------------------------------/

    /// <summary>
    /// 달릴시 빨라지는 속도 
    /// </summary>
    public float runningSpeed = 2.0f;

    /// <summary>
    /// 캐릭터 컴포넌트 참조를 위한 변수 선언
    /// </summary>
    private CharacterController cc;

    /// <summary>
    /// 인풋 시스템 참조용 변수
    /// </summary>
    private PlayerMove inputActions;



    /// <summary>
    /// 움직임 좌표계산을 위한? 변수
    /// </summary>
    private Vector2 movementInput;

    /// <summary>
    /// 아래로 떨어지는 중력
    /// </summary>
    private float gravity = -20f;

    /// <summary>
    /// 캐릭터의 수직 속도 고정
    /// </summary>
    private float yVelocity = 0;

    /// <summary>
    /// 달리기 상태
    /// </summary>
    private bool isRunning = false;
    private Vector3 moveDirection = Vector3.zero;
    public float limitWeight = 20f; // 무게가 이 값보다 클 때 속도가 줄어드는 지점
    public float MaxWeight = 40f; // 무게가 이 값보다 클 때 이동이 멈추는 지점
    public float currentWeight = 10f; // 현재 무게

    /// <summary>
    /// 플레이어의 사망을 알리는 델리게이트
    /// </summary>
    public Action onDie;
    public Action<float> onHealthChange { get; set; }

    public Action<float> onWeightChange { get; set; }

    public float defense = 1.0f;

    public Animator animator;

    bool isMove = false;

    PlayerNoiseSystem noise;
    WaitForSeconds LangingSoundInterval = new WaitForSeconds(0.1f);
    const float runSoundRange = 5.0f;
    const float landingSoundRange = 6.0f;

    /// <summary>
    /// 플레이어가 공중에 있었는지 확인하는 변수
    /// </summary>
    private bool wasInAir = false;


    private void Awake()
    {
        inputActions = new PlayerMove();


        noise = transform.GetComponentInChildren<PlayerNoiseSystem>(true);
        noise.gameObject.SetActive(false);

        firePosition = transform.GetChild(1);

        slotNum = GetComponentInChildren<SlotNumber>();
    }

    // 점프시 레이를 이용해 점프할 수 있는 환경인지 확인
    private void OnEnable()
    {
        noise.gameObject.SetActive(false);
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;  // 이동 시작
        inputActions.Player.Move.canceled += OnMoveCanceled;    // 이동 중지
        inputActions.Player.Jump.performed += OnJump;           // 점프
        inputActions.Player.Run.performed += OnRunPerformed;    // 달리기 시작
        inputActions.Player.Run.canceled += OnRunCanceled;      // 달리기 중지
        //inputActions.Player.InteractAction.performed += OnInteract;            // 상호작용
        inputActions.Player.InventoryAction.performed += OnInventoryAction;    // 인벤토리 사용
        inputActions.Player.InventoryAction.canceled += OnInventoryAction;     // 인벤토리 사용
        inputActions.Player.Exit.performed += OnExit;                          // 종료(일시정지)
        inputActions.Player.LeftMouse.performed += OnLeftMouse;                // 왼쪽 마우스 입력
        inputActions.Player.RightMouse.performed += OnRightMouse;              // 오른쪽 마우스 입력
        inputActions.Player.HotbarKey.performed += OnHotbarKey;                // 핫바키 사용


    }

    private void OnDisable()
    {


        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Run.performed -= OnRunPerformed;
        inputActions.Player.Run.canceled -= OnRunCanceled;
        //inputActions.Player.InteractAction.performed -= OnInteract;
        inputActions.Player.InventoryAction.performed -= OnInventoryAction;
        inputActions.Player.InventoryAction.canceled -= OnInventoryAction;
        inputActions.Player.Exit.performed -= OnExit;
        inputActions.Player.LeftMouse.performed -= OnLeftMouse;
        inputActions.Player.RightMouse.performed -= OnRightMouse;
        inputActions.Player.HotbarKey.performed -= OnHotbarKey;
        inputActions.Player.Disable();
    }

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        playerUI = GameManager.Instance.PlayerUI;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
        isMove = true;
        UpdateNoise();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        movementInput = Vector2.zero;
        isMove = false;
        UpdateNoise();
    }
    private void UpdateNoise()
    {
        if (isMove && isRunning)
        {
            noise.Radius = runSoundRange;
            noise.gameObject.SetActive(true);
        }
        else
        {
            noise.gameObject.SetActive(false);
        }
    }
    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (IsGrounded())
        {
            yVelocity = jumpPower;
        }
    }
    private bool IsGrounded()
    {
        float distanceToGround = cc.bounds.extents.y;
        bool isGrounded = Physics.Raycast(transform.position, -Vector3.up, distanceToGround + 0.5f);

        // 레이 시각화를 위한 코드
        if (isGrounded)
        {
            Debug.DrawLine(transform.position, transform.position + (-Vector3.up * (distanceToGround + 0.5f)), Color.green);
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + (-Vector3.up * (distanceToGround + 0.5f)), Color.red);
        }

        return isGrounded;
    }

    private void OnRunPerformed(InputAction.CallbackContext ctx)
    {
        isRunning = true;   // 달리는 중
        if (isMove) 
        {
            noise.Radius = runSoundRange;
            noise.gameObject.SetActive(true);
        }
    }

    private void OnRunCanceled(InputAction.CallbackContext ctx)
    {
        isRunning = false;  // 달리기 종료
        noise.gameObject.SetActive(false);
    }

    private void OnInventoryAction(InputAction.CallbackContext ctx)
    {
    }

    private void OnExit(InputAction.CallbackContext ctx)
    {
    }

    private void OnLeftMouse(InputAction.CallbackContext ctx)
    {
    }

    private void OnRightMouse(InputAction.CallbackContext ctx)
    {
    }

    private void OnHotbarKey(InputAction.CallbackContext ctx)
    {
        // int hotbarIndex = /* 여기에서 핫바 키 번호를 가져오는 코드 작성 */;
        // HotberKey(hotbarIndex);
    }




    private void FixedUpdate()
    {
        CalculateMovement();
        ApplyGravity();
        cc.Move(moveDirection * Time.deltaTime);
    }

    private void CalculateMovement()
    {
        if (Camera.main != null)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0;
            Vector3 cameraRight = Camera.main.transform.right;
            cameraRight.y = 0;
            moveDirection = movementInput.x * cameraRight + movementInput.y * cameraForward;
        }
        AdjustSpeedBasedOnWeight();
    }
    private void AdjustSpeedBasedOnWeight()
    {
        float currentSpeed = moveSpeed;  // 기본 이동 속도를 시작 속도로 설정

        // 현재 무게가 제한 무게를 초과하고 최대 무게 이하일 때
        if (currentWeight > limitWeight && currentWeight <= MaxWeight)
        {
            // 무게가 증가함에 따라 이동 속도 감소
            currentSpeed -= (currentWeight - limitWeight) / (MaxWeight - limitWeight) * (moveSpeed - 0f);
        }
        else if (currentWeight > MaxWeight)
        {
            // 최대 무게를 초과하면 이동 속도를 0으로 설정하여 이동 불가
            currentSpeed = 0f;
        }

        // 달리는 상태일 때 추가 속도 적용
        currentSpeed = isRunning ? currentSpeed + runningSpeed : currentSpeed;
        moveDirection *= currentSpeed;  // 최종 이동 방향과 속도 적용
    }

    private void ApplyGravity()
    {
        if (!IsGrounded())
        {
            yVelocity += gravity * Time.deltaTime;
        }
        else
        {
            yVelocity = Mathf.Max(0, yVelocity);
        }
        moveDirection.y = yVelocity;
    }


    public void Damege(float damege) 
    {
        Hp -= (damege/defense);
        Debug.Log($"{damege}받음 / {Hp}");
    }
    public void Heal(float rate)
    {
        Hp += (rate);
    }

    public void Die()
    {
        // 조종이 불가하도록 만든다.
        inputActions.Player.Disable();


        // 델리게이트에 죽었음을 알리기(onDie 델리게이트)
        onDie?.Invoke();
    }

    //IEnumerator LandingNoise()
    //{
    //    noise.Radius = landingSoundRange;
    //    noise.gameObject.SetActive(true);
    //    yield return LangingSoundInterval;
    //    noise.gameObject.SetActive(false);
    //}


    // Weapon_Equip 관련 ------------------------------------------------------------------------

    Transform firePosition;

    Equipment equipment = Equipment.None;

    public Equipment Equipment
    {
        get => equipment;
        set
        {
            equipment = value;
        }
    }

    public bool IsEquip => firePosition.childCount == 0;

    public void Equipped(Equipment type)
    {
        if (IsEquip)
        {
            slotNum.SwapItem(type, firePosition, false);
        }
        else
        {
            slotNum.SwapItem(type, firePosition, true);
        }
    }

    public void UseItem(ItemSlot slot)
    {

    }

    public bool UnEquipped()
    {
        bool result = false;

        WeaponBase weapon;
        weapon = GetComponentInChildren<WeaponBase>();

        if (weapon != null)
        {
            Destroy(weapon.gameObject);
            result = true;
        }
        return result;
    }


    // ----------------------------------------------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (UnityEditor.Selection.Contains(gameObject))
        {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(transform.position, transform.up, 5.0f);
            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, transform.up, 5.0f * 0.6f);
        }
    }

#endif
}
