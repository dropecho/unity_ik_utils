using UnityEngine;

namespace Dropecho {
  [RequireComponent(typeof(Animator))]
  public class LookAtIK : MonoBehaviour {
    [Tooltip("Should the IK run")]
    public bool runIK;
    [Tooltip("The amount of time it takes to look at or away from the target.")]
    public float timeToLook = 1;
    [Tooltip("The game object to look at.")]
    public GameObject target;
    [Range(0, 360), Tooltip("The maximum angle to turn the head to look at.")]
    public float angle;
    [Range(0, 1), Tooltip("The IK weight of the body.")]
    public float bodyWeight;
    [Range(0, 1), Tooltip("The IK weight of the head.")]
    public float headWeight;
    [Range(0, 1), Tooltip("The IK weight of the eyes.")]
    public float eyesWeight;
    [Range(0, 1), Tooltip("The maximum amount of weight applied.")]
    public float weightClamp;

    [HideInInspector]
    public Vector3 currentLookAtDir { get; private set; }

    private Animator _animator;
    private float _ikWeight = 0;

    void Awake() {
      _animator = GetComponent<Animator>();
    }

    void Start() {
      currentLookAtDir = transform.forward;
      _ikWeight = 1;
    }

    void OnAnimatorIK() {
      if (_ikWeight <= 0 && !runIK) {
        return;
      }
      var headBonePos = _animator.GetBoneTransform(HumanBodyBones.Head).position;
      currentLookAtDir = Vector3.RotateTowards(currentLookAtDir, GetDesiredLookAt(), Time.deltaTime / timeToLook, 0);
      _animator.SetLookAtPosition(headBonePos + currentLookAtDir);

      _ikWeight = Mathf.MoveTowards(_ikWeight, runIK ? 1 : 0, Time.deltaTime / timeToLook);
      _animator.SetLookAtWeight(_ikWeight, bodyWeight, headWeight, eyesWeight, weightClamp);
    }

    Vector3 GetDesiredLookAt() {
      if (target == null || !runIK) {
        return transform.forward;
      }

      var headBonePos = _animator.GetBoneTransform(HumanBodyBones.Head).position;

      var desiredLookAt = target.transform.position - headBonePos;
      if (Vector3.Angle(transform.forward, desiredLookAt) < angle / 2) {
        return desiredLookAt;
      }

      return transform.forward;
    }
  }
}