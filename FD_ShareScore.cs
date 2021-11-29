using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EasyMobile;
using I2.Loc;

namespace FlappyDragon
{
    public class FD_ShareScore : MonoBehaviour
    {
        [BoxGroup("Share System")] public RenderTexture shareRenderTexture;
        [BoxGroup("Share System")] public TextMeshProUGUI shareScoreTMP;
        [BoxGroup("Share System")] public Image shareDragon;
        [BoxGroup("Share System"), TextArea] public string shareKey;

        private void Awake()
        {
            shareKey = LocalizationManager.GetTranslation("S_POSTGAME.SHARE_MESSAGE");
        }

        private void Start()
        {
            UpdateSharingInfo();
        }

        public void UpdateSharingInfo()
        {
            shareScoreTMP.text = FD_GameMode.instance.currentScore.ToString();
            shareDragon.sprite = FD_GameMode.instance.currentDragon.dragonUp;
            shareDragon.transform.localScale = Vector3.one;
        }
        
        public void ShareScore()
        {
            StopAllCoroutines();
            StartCoroutine(ShareScoreCoroutine());
        }

        IEnumerator ShareScoreCoroutine()
        {
            yield return new WaitForEndOfFrame();
            
            Texture2D texture2D = new Texture2D(1024, 512, TextureFormat.RGB24, false);
            RenderTexture.active = shareRenderTexture;
            texture2D.ReadPixels(new Rect(0, 0, shareRenderTexture.width, shareRenderTexture.height), 0, 0);
            texture2D.Apply();  
            
            // Generates a PNG image from the given Texture2D object, saves it to persistentDataPath using
            // the given filename and then shares the image via the native sharing utility.
            Sharing.ShareTexture2D(texture2D, "shareScore", shareKey);
        }
        
    }

}
