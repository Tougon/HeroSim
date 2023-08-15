using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    RawImage backgroundImage;


    // Start is called before the first frame update
    void Awake()
    {
        backgroundImage = GetComponent<RawImage>();

        var backgroundMat = backgroundImage.material;
        backgroundImage.material = Instantiate(backgroundMat);
    }


    #region Background Tween Classes

    public class BackgroundTweenFrame
    {
        public enum TweenType { HitEffect }
    }

    #endregion
}
