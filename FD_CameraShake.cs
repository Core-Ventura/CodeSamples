using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

namespace FlappyDragon
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class FD_CameraShake : MonoBehaviour
    {
        private CinemachineVirtualCamera virtualCamera;
        private CinemachineTransposer transposer;
        private readonly Vector3 positionMask = new Vector3(1, 0, 1);
        private readonly Vector3 rotationMask = new Vector3(0, 0, 1);
        
        private void Awake()
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        }

        private void CameraShakePosition(float duration = 1f, float strength = 0.5f)
        {
            DOTween.Complete("CameraShakePosition");
            DOTween.Shake(() => transposer.m_FollowOffset, x => transposer.m_FollowOffset = x,
                    duration, positionMask * strength, fadeOut:true)
                .SetId("CameraShakePosition");

        }

        private void CameraShakeRotation(float duration = 1f, float strength = 0.5f)
        {
            DOTween.Complete("CameraShakeRotation");
            virtualCamera.transform.DOShakeRotation(duration, rotationMask * strength, fadeOut:true).SetId("CameraShakeRotation");
        }
        
        public void CameraOrthoShake(float duration = 1, float strength = 1)
        {
            if (DOTween.IsTweening("CameraOrthoSize")) return;
            
            DOTween.Complete("OrthoSize");
            DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x => virtualCamera.m_Lens.OrthographicSize = x,
                    virtualCamera.m_Lens.OrthographicSize, duration)
                .From(virtualCamera.m_Lens.OrthographicSize - strength)
                .SetEase(Ease.OutElastic)
                .SetId("OrthoSize");
        }
    }
}
