using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;


public enum ButtonBehaviourType
{
    HIDE_BUTTON,
    DISABLE_BUTTON,
    HIDE_BUTTON_ICON
}


public class AdsButtonWrapperController : MonoBehaviour
{
    public ButtonBehaviourType behaviourType;
    public GameObject AdsButton;
    public GameObject internalIcon;
    public bool continuousCheck;

    private Button buttonComp;

    public void Awake()
    {
        buttonComp = AdsButton.GetComponent<Button>();

        #if UNITY_EDITOR
        if (internalIcon == null)
            Debug.LogFormat("No se ha establecido imagenInterna en boton {0}>{1}", transform.parent.name, transform.name);
        #endif
    }

    public void OnEnable()
    {	
        AdsAvailability();
    }

    void Update()
    {
        if (continuousCheck)
        {
            AdsAvailability();
        }
    }

    void AdsAvailability()
    {
        if (GameManager.Instance)
        {
            switch (behaviourType)
            {
                case ButtonBehaviourType.DISABLE_BUTTON:
                    buttonComp.interactable = UnityAds.Instance.IsReady;
                    break;
                case ButtonBehaviourType.HIDE_BUTTON:
                    AdsButton.SetActive(UnityAds.Instance.IsReady);
                    break;
                case ButtonBehaviourType.HIDE_BUTTON_ICON:
                    internalIcon.SetActive(UnityAds.Instance.IsReady);
                    break;
            }
        }
    }
}
