using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    AudioSource m_AudioSource;
    Animator m_Animator;
    public InputAction MoveAction;

    public float turnSpeed = 20f;

    Rigidbody m_Rigidbody;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        MoveAction.Enable();
        m_Animator = GetComponent<Animator>();
        m_AudioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        var pos = MoveAction.ReadValue<Vector2>();

        m_Movement.Set(pos.x, 0f, pos.y);
        m_Movement.Normalize();

        // 衝突でソルバーが与えた速度が残留すると入力なしでも滑走するため、毎ステップ打ち消す
        m_Rigidbody.linearVelocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;

        bool isWalking = m_Movement.sqrMagnitude > 0f;
        m_Animator.SetBool("IsWalking", isWalking);

        if (isWalking)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
            }

            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
            m_Rotation = Quaternion.LookRotation(desiredForward);
        }
        else
        {
            m_AudioSource.Stop();
        }
    }

    void OnAnimatorMove()
    {
        // 移動量はルートモーション由来。アニメの歩幅と実移動が常に一致するので足滑りしない
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude);
        m_Rigidbody.MoveRotation(m_Rotation);
    }
}
