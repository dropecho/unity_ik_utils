using UnityEngine;

namespace Dropecho {
  [RequireComponent(typeof(Animator))]
  public class ReachIK : MonoBehaviour {
    public GameObject target;
    public AvatarIKGoal bone;
    [Range(0, 1)]
    public float positionWeight = 0;
    [Range(0, 1)]
    public float rotationWeight = 0;

    private Animator _animator;
    private Vector3 pos;
    private Quaternion rot;

    void Awake() {
      _animator = GetComponent<Animator>();
    }



    void LateUpdate() {
      pos = target.transform.position;
      rot = target.transform.rotation;
    }

    void OnAnimatorIK() {

      _animator.SetIKPosition(bone, pos);
      _animator.SetIKPositionWeight(bone, positionWeight);

      // _animator.SetIKPosition(bone, rot.eulerAngles);
      // _animator.SetIKRotationWeight(bone, rotationWeight);
    }
  }
}