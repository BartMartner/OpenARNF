using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class Preprocess { }
/*
public class Preprocess : IPreprocessBuild
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        var morningstarLogo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SplashScreens/MorningstarGameStudio.png");
        var hitcentsArnfLogo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SplashScreens/ARobotNamedFightHitcents.png");
        var arnfLogo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SplashScreens/ARobotNamedFightLogo.png");
        var controllerScreen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SplashScreens/Controller.png");

        if (target == BuildTarget.Switch)
        {
            Debug.Log("Switch Build Preprocess! Wooooo!");
            var logos = new PlayerSettings.SplashScreenLogo[2];
            logos[0] = PlayerSettings.SplashScreenLogo.Create(3, morningstarLogo);
            logos[1] = PlayerSettings.SplashScreenLogo.Create(3, hitcentsArnfLogo);
            PlayerSettings.SplashScreen.logos = logos;
        }
        else
        {
            Debug.Log("Normal Ass Build Preprocess! Wooooo!");
            var logos = new PlayerSettings.SplashScreenLogo[3];
            logos[0] = PlayerSettings.SplashScreenLogo.Create(3, morningstarLogo);
            logos[1] = PlayerSettings.SplashScreenLogo.Create(3, arnfLogo);
            logos[2] = PlayerSettings.SplashScreenLogo.Create(3, controllerScreen);
            PlayerSettings.SplashScreen.logos = logos;
        }
    }
}
*/
