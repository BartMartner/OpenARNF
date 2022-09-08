using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using Rewired.UI.ControlMapper;

public class RewiredSetControlsScreenState : ScreenState
{
    public ScreenState previousState;
    public ControlMapper controllerMapper;
    public GameObject conventionWarning;

    public override void AppearFinished()
    {
        //InputHelper.instance.AssignPlayer1LastActiveController();
        if (SaveGameManager.instance.saveFileData != null && SaveGameManager.instance.saveFileData.conventionMode)
        {
            conventionWarning.SetActive(true);
        }
        else
        {
            controllerMapper.Open();
            controllerMapper.ScreenClosedEvent += ScreenClose;
        }
    }

    public override void ReadyUpdate()
    {
        base.ReadyUpdate();

        if (SaveGameManager.instance.saveFileData != null && SaveGameManager.instance.saveFileData.conventionMode)
        {
            if (_controller.GetButtonDown("UICancel"))
            {
                ScreenClose();
            }
        }
    }

    public void ScreenClose()
    {
        if (SaveGameManager.instance.saveFileData != null && SaveGameManager.instance.saveFileData.conventionMode) conventionWarning.SetActive(false);
        UISounds.instance.Cancel();
        GoToState(previousState);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (SaveGameManager.instance &&
            (SaveGameManager.instance.saveFileData == null || 
            !SaveGameManager.instance.saveFileData.conventionMode))
        {
            if (controllerMapper.isOpen) { controllerMapper.Close(false); }
        }
    }
}
