using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class pPd : MonoBehaviour
{
    public Texture2D warningImage;
    public AudioClip warningSound;

    private Canvas canvas;
    private RawImage rawImage;
    private AudioSource audioSource;

    void Start()
    {
        try
        {
            string gameRootPath = System.IO.Directory.GetParent(Application.dataPath).FullName;

            string winhttpPath = System.IO.Path.Combine(gameRootPath, "winhttp.dll");
            string versionPath = System.IO.Path.Combine(gameRootPath, "version.dll");

            bool cheatDetected = false;

            if (System.IO.File.Exists(winhttpPath))
            {
                UnityEngine.Debug.LogError("winhttp.dll detected.");
                cheatDetected = true;
            }

            if (System.IO.File.Exists(versionPath))
            {
                UnityEngine.Debug.LogError("version.dll detected.");
                cheatDetected = true;
            }

            if (DetectInjectedDLL())
            {
                UnityEngine.Debug.LogError("Injected DLL detected.");
                cheatDetected = true;
            }

            if (cheatDetected)
            {
                StartCoroutine(ShowImageAndPlaySoundAndQuit(warningImage, warningSound));
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error while checking for cheat files: {e.Message}");
            Application.Quit();
        }
    }

    private bool DetectInjectedDLL()
    {
        Process currentProcess = Process.GetCurrentProcess();
        var modules = currentProcess.Modules.Cast<ProcessModule>();

        foreach (var module in modules)
        {
            if (module.ModuleName.Contains("Unity") || module.ModuleName.Contains("mono") || module.ModuleName.Contains("kernel32"))
            {
                continue;
            }

            if (module.ModuleName.ToLower().EndsWith(".dll"))
            {
                UnityEngine.Debug.LogError($"Injected DLL detected: {module.ModuleName}");
                return true;
            }
        }

        return false;
    }

    private IEnumerator ShowImageAndPlaySoundAndQuit(Texture2D image, AudioClip sound)
    {
        if (image == null)
        {
            UnityEngine.Debug.LogError("No warning image assigned.");
            Application.Quit();
            yield break;
        }

        canvas = new GameObject("Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("WarningImage");
        imageObject.transform.SetParent(canvas.transform);
        rawImage = imageObject.AddComponent<RawImage>();
        rawImage.texture = image;

        float imageRatio = (float)image.width / image.height;
        float screenRatio = (float)Screen.width / Screen.height;
        if (imageRatio > screenRatio)
        {
            rawImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.width / imageRatio);
        }
        else
        {
            rawImage.rectTransform.sizeDelta = new Vector2(Screen.height * imageRatio, Screen.height);
        }

        rawImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rawImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rawImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rawImage.rectTransform.anchoredPosition = Vector2.zero;

        audioSource = canvas.gameObject.AddComponent<AudioSource>();
        audioSource.clip = sound;
        audioSource.Play();

        yield return new WaitForSeconds(5);

        Process.Start("https://davuksdev.github.io/DMenuWEB/DPAntiCheat");   
        Application.Quit();
    }
}
