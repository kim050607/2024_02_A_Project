using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //플레이어의 움직임 속도를 설정하는 변수
    [Header("Player Movement")]
    public float moveSpeed = 5.0f;          //이동 속도
    public float jumpForce = 5.0f;          //점프 힘
    public float rotationSpeed = 10.0f;     //ȸ�� �ӵ�
    //카메라 설정 변수
    [Header("camera Settings")]
    public Camera firstPersonCamera;         //1인칭 카메라
    public Camera thirdPersonCamera;         //3인칭 카메라
    public float mouseSenesitivity = 2.0f;   //마우스 감도

    public float radius = 5.0f;             //3인칭 카메라와 플레이어 간의 거리
    public float minRadius = 1.0f;          //카메라 최소 거리
    public float maxRadius = 10.0f;         //카메라 최대 거리

    public float yMinLimit = -90;           //카메라 수직 회전 최소각
    public float yMaxLimit = 90;            //카메라 수직 회전 최대각

    private float theta = 0.0f;                    //카메라의 수평 회전 각도
    private float phi = 0.0f;                      //카메라의 수직 회전 각도
    private float targetVericalRotation = 0;        //목표 수직 회전 각도
    private float verticalRotationSpeed = 240f;      //수직 푀전 각도

    //내부 변수들
    private bool isFirstPerson = true;     //1인칭 모드 인지 여부
    private bool isGround;                 //플레이어가 땅에 있는지 여부
    private Rigidbody rb;                  //플레이어의 rigidbody
     
    //플레이어가 땅에 닿아 있는지 감지

    private void OnCollisionStay(Collision collision)
    {
        isGround = true;           //충돌 중이면 플레이어는 땅에 있다.
    }
        // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();         //RigidBody 컴포넌트를 가져온다.

        Cursor.lockState = CursorLockMode.Locked;    //마우스 커서를 잠그고 숨긴다.
        SetupCameras();
        SetActiveCamera();

    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation(); 
        HandleJump();
        HandleCameraToggle();
    }

     //카메라 초기 위치 및 회전을 설정하는 함수

     void SetupCameras() 
    {
        firstPersonCamera.transform.localPosition = new Vector3(0.0f, 0.6f, 0.0f);     //1인칭 카메라 위치
        firstPersonCamera.transform.localRotation = Quaternion.identity;                //1인칭 카메라 회전 초기화
    }

    //플레이어 점플를 처리하는 함수
    void HandleJump()
    {
        //점프 버튼을 누르고 땅에 있을때
        if (Input.GetKeyDown(KeyCode.Space) && isGround)       
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);       //위쪽으로 힘을 가해 점프
            isGround = false;
        }
    }
    
    //활성화할 카메라를 설정하는 함수
    void SetActiveCamera()
    {
        firstPersonCamera.gameObject.SetActive(isFirstPerson);            //1��Ī ī�޶� Ȱ��ȭ ����
        firstPersonCamera.gameObject.SetActive(!isFirstPerson);           //3��Ī ī�޶� Ȱ���� ����
    }

    //카메라 및 캐릭터 회전 처리하는 함수
    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSenesitivity;                //마우스 좌우 입력
        float mouseY = Input.GetAxis("Mouse Y") * mouseSenesitivity;                //마우스 상하 입력

        //수평 회전(theta 값)
        theta += mouseX;       //마우스 입력값 추가
        theta = Mathf.Repeat(theta, 360.0f);       //각도 값이 360를 넘지 않도록 종정 (0 ~ 360 | 361 -> 1)  

        //수직 회전 처리
        targetVericalRotation -= mouseY;
        targetVericalRotation = Mathf.Clamp(targetVericalRotation, yMinLimit, yMaxLimit);   //수직 회전 제한
        phi = Mathf.MoveTowards(phi, targetVericalRotation, verticalRotationSpeed * Time.deltaTime);

       
        if (isFirstPerson)
        {
          firstPersonCamera.transform.localRotation = Quaternion.Euler(phi, 0.0f, 0.0f);    //1인칭 카메라 수직 회전
        }
        else
        {
            //3인칭 카메라 구면 좌표계에서 위치 및 회전 계산
            float x = radius * Mathf.Sin(Mathf.Deg2Rad * phi) * Mathf.Cos(Mathf.Deg2Rad * theta);
            float y = radius * Mathf.Cos(Mathf.Deg2Rad * phi);
            float z = radius * Mathf.Sin(Mathf.Deg2Rad * phi) * Mathf.Sin(Mathf.Deg2Rad * theta);

            thirdPersonCamera.transform.position = transform.position + new Vector3(x, y, z);
            thirdPersonCamera.transform.LookAt(transform);          //카메라가 항상 플레이어를 바라보도록 설정

            //마우스 스크롤을 사용하여 카메라 줌 조정
            radius = Mathf.Clamp(radius - Input.GetAxis("Mouse ScrollWheel") * 5, minRadius, maxRadius);
        }
    }
     
    //1��Ī�� 3��Ī ī�޶� ��ȯ�ϴ� �Լ�

    void HandleCameraToggle()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isFirstPerson = !isFirstPerson;     //카메라 모드 전환
            SetActiveCamera(); 
        }
    }
    
    //플레이어의 이동을 처리하는 함수

    void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");                  //좌우 입력 (-1 ~ 1)
        float moveVertical = Input.GetAxis("Vertical");                      //앞뒤 입력 (1 ~ -1)

        if (!isFirstPerson)//3인칭 모드 일때, 카메라 방향으로 이동 처리
        {
            Vector3 cameraForward = thirdPersonCamera.transform.forward;       //카메라 앞 방향
            cameraForward.y = 0.0f;
            cameraForward.Normalize();  //방향 벡터 정규화 (0~1) 사이 값으로 만들어준다.

            Vector3 cameraRight = thirdPersonCamera.transform.right;            //카메라 오른쪽 방향
            cameraRight.y = 0.0f;
            cameraRight.Normalize();

            Vector3 movement = cameraRight * moveHorizontal + cameraForward * moveVertical ;
            rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);       //물리 기반 이동
        }
        else
        {
            //캐릭터 기준으로 이동
            Vector3 movement = transform.right * moveHorizontal + transform.forward * moveVertical;
            rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);     //물리 기반 이동
        }
    }
}   


