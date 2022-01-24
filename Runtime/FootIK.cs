using Dropecho;
using UnityEngine;

namespace Dropecho {
  public class FootIK : MonoBehaviour {
    [Tooltip("The layers to use as ground.")]
    public LayerMask mask;
    [Range(0, 1)]
    [Tooltip("How high from the foot to start the raycast.")]
    public float footRaycastOriginOffset = 0.25f;
    [Range(0, 1)]
    [Tooltip("How far from start can the pelvis go down to adjust for foot placement.")]
    public float maxPelvisAdjustment = 0.5f;

    public float pelvisAdjustmentSpeed = 8f;
    public float ikWeightActivateSpeed = 8f;
    public float ikWeightOffSpeed = 10f;

    private Animator _animator;
    private bool _ikActive;

    void Awake() {
      _animator = GetComponent<Animator>();
    }

    void Update() {
      if (!_animator) {
        return;
      }
      _ikActive = _animator.GetFloat("forward") < 0.15f;
    }

    // Vars for smoothing/lerping.
    float _weight = 0;
    float _lastPelvisPositionY = 0;
    float _lastLeftTargetY = 0;
    float _lastRightTargetY = 0;

    void OnAnimatorIK() {
      if (!_animator) {
        return;
      }

      if (_ikActive) {
        _weight = Mathf.Lerp(_weight, 1, Time.deltaTime * ikWeightActivateSpeed);
      } else {
        _weight = Mathf.Lerp(_weight, 0, Time.deltaTime * ikWeightOffSpeed);
      }

      _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, _weight);
      _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _weight);
      _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, _weight);
      _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, _weight);

      if (!_ikActive) {
        _lastPelvisPositionY = 0; // Prevent bounce on ik start.
        return;
      }

      var leftHit = CastFootIK(AvatarIKGoal.LeftFoot);
      var rightHit = CastFootIK(AvatarIKGoal.RightFoot);

      var missingHitsFromFootRay = leftHit.point == Vector3.zero && rightHit.point == Vector3.zero;

      var diffToGround = Mathf.Abs(leftHit.point.y - rightHit.point.y);
      if (missingHitsFromFootRay) {
        return;
      }

      var groundAdjustment = Mathf.Min(maxPelvisAdjustment, diffToGround);
      if (_lastPelvisPositionY == 0) {
        _lastPelvisPositionY = _animator.bodyPosition.y;
      }

      var newPelvisPosition = _animator.bodyPosition + (Vector3.up * -groundAdjustment);
      _lastPelvisPositionY = newPelvisPosition.y = Mathf.Lerp(_lastPelvisPositionY, newPelvisPosition.y, Time.deltaTime * pelvisAdjustmentSpeed);
      _animator.bodyPosition = newPelvisPosition;
    }

    RaycastHit CastFootIK(AvatarIKGoal iKGoal) {
      var footHeight = iKGoal == AvatarIKGoal.LeftFoot ? _animator.leftFeetBottomHeight : _animator.rightFeetBottomHeight;
      var bone = iKGoal == AvatarIKGoal.LeftFoot ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;

      var ray = new Ray(_animator.GetIKPosition(iKGoal) + Vector3.up * footRaycastOriginOffset, Vector3.down);
      if (Physics.Raycast(ray, out var hit, footHeight + maxPelvisAdjustment + footRaycastOriginOffset, mask)) {

        var footTransform = _animator.GetBoneTransform(bone);
        _animator.SetIKRotation(iKGoal, Quaternion.LookRotation(footTransform.forward, hit.normal));

        var lastY = iKGoal == AvatarIKGoal.LeftFoot ? _lastLeftTargetY : _lastRightTargetY;

        var newTarget = hit.point + new Vector3(0, footHeight, 0);
        newTarget.y = Mathf.Lerp(lastY, newTarget.y, Time.deltaTime * 5f);

        if (iKGoal == AvatarIKGoal.LeftFoot) {
          _lastLeftTargetY = newTarget.y;
        } else {
          _lastRightTargetY = newTarget.y;
        }
        _animator.SetIKPosition(iKGoal, newTarget);
      }

      return hit;
    }
  }
}