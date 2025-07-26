using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lets user toggle menu visibility by pressing the primary button.
/// </summary>
public class MenuToggle : MonoBehaviour
{
    public GameObject menuPanel;
    public InputActionReference toggleMenuAction;

    private void Awake()
    {
        if (!toggleMenuAction.action.enabled)
        {
            toggleMenuAction.action.Enable();
        }
        toggleMenuAction.action.performed += ToggleMenu;
    }

    private void OnDestroy()
    {
        toggleMenuAction.action.Disable();
        toggleMenuAction.action.performed -= ToggleMenu;
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }
}
