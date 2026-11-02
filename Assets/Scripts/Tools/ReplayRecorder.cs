using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ReplayRecorder : MonoBehaviour
{
    [Header("Capture Source")]
    [SerializeField] private Camera captureCamera;

    [Header("Buffer")]
    [SerializeField] private int captureWidth = 640;
    [SerializeField] private int captureHeight = 360;
    [SerializeField] private int captureFps = 15;
    [SerializeField] private float bufferSeconds = 10f;

    [Header("Output")]
    [SerializeField] private string outputFolder = "Assets/Art/Tutorial/Videos/Replays";
    [Tooltip("Full path to ffmpeg.exe. Leave empty to use 'ffmpeg' from PATH.")]
    [SerializeField] private string ffmpegPath = "";

    [Header("Manual Trigger (Editor/dev builds)")]
    [SerializeField] private KeyCode saveHotkey = KeyCode.F9;

    private RenderTexture captureRT;
    private Texture2D readTex;
    private Queue<byte[]> frameBuffer;
    private int maxFrames;
    private float captureInterval;
    private float captureTimer;
    private bool isSaving;
    private bool warnedMissingCamera;

    private void Awake()
    {
        maxFrames = Mathf.Max(1, Mathf.RoundToInt(captureFps * bufferSeconds));
        frameBuffer = new Queue<byte[]>(maxFrames);
        captureInterval = 1f / captureFps;

        captureRT = new RenderTexture(captureWidth, captureHeight, 24);
        readTex = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(saveHotkey))
        {
            SaveLastSeconds("Replay_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        }

        if (!captureCamera)
        {
            if (!warnedMissingCamera)
            {
                Debug.LogWarning("[ReplayRecorder] 'Capture Camera' is not assigned — no frames are being captured.", this);
                warnedMissingCamera = true;
            }
            return;
        }

        if (isSaving)
        {
            return;
        }

        captureTimer += Time.unscaledDeltaTime;
        if (captureTimer < captureInterval)
        {
            return;
        }

        captureTimer = 0f;
        CaptureFrame();
    }

    private void CaptureFrame()
    {
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture prevTarget = captureCamera.targetTexture;

        captureCamera.targetTexture = captureRT;
        captureCamera.Render();
        RenderTexture.active = captureRT;

        readTex.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        readTex.Apply(false);

        captureCamera.targetTexture = prevTarget;
        RenderTexture.active = prevActive;

        frameBuffer.Enqueue(readTex.GetRawTextureData());

        if (frameBuffer.Count > maxFrames)
        {
            frameBuffer.Dequeue();
        }
    }

    /// <summary>
    /// Encodes the buffered last `bufferSeconds` of frames into
    /// {outputFolder}/{clipName}.mp4 (no audio track).
    /// </summary>
    public void SaveLastSeconds(string clipName)
    {
        if (isSaving)
        {
            Debug.LogWarning("[ReplayRecorder] Already saving a clip, ignoring request.");
            return;
        }

        if (frameBuffer.Count == 0)
        {
            Debug.LogWarning("[ReplayRecorder] No frames captured yet — nothing to save. Check that 'Capture Camera' is assigned and enough time has passed.", this);
            return;
        }

        StartCoroutine(SaveRoutine(clipName));
    }

    private IEnumerator SaveRoutine(string clipName)
    {
        isSaving = true;

        byte[][] frames = frameBuffer.ToArray();
        string tempDir = Path.Combine(Application.temporaryCachePath, "ReplayFrames_" + clipName);
        Directory.CreateDirectory(tempDir);

        for (int i = 0; i < frames.Length; i++)
        {
            Texture2D frameTex = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
            frameTex.LoadRawTextureData(frames[i]);
            frameTex.Apply(false);

            byte[] png = frameTex.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(tempDir, $"frame_{i:D5}.png"), png);
            Destroy(frameTex);

            if (i % 10 == 0)
            {
                yield return null;
            }
        }

        Directory.CreateDirectory(outputFolder);
        string outputPath = Path.Combine(outputFolder, clipName + ".mp4");

        bool ok = RunFfmpeg(tempDir, outputPath);

        Directory.Delete(tempDir, true);

        if (ok)
        {
            Debug.Log($"[ReplayRecorder] Saved replay to {outputPath}");
        }

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        isSaving = false;
    }

    private bool RunFfmpeg(string frameDir, string outputPath)
    {
        string exe = string.IsNullOrEmpty(ffmpegPath) ? "ffmpeg" : ffmpegPath;

        // No audio (-an). Matches the H.264/yuv420p/BT.709 profile confirmed to
        // play cleanly through Unity's VideoPlayer on this project.
        string args =
            $"-y -framerate {captureFps} -i \"{frameDir}/frame_%05d.png\" " +
            "-an -c:v libx264 -pix_fmt yuv420p " +
            "-color_primaries bt709 -color_trc bt709 -colorspace bt709 " +
            $"\"{outputPath}\"";

        var startInfo = new ProcessStartInfo(exe, args)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
        };

        try
        {
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Debug.LogError($"[ReplayRecorder] ffmpeg exited with code {process.ExitCode}: {process.StandardError.ReadToEnd()}");
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ReplayRecorder] Failed to run ffmpeg ('{exe}'). Is it installed and on PATH, or set Ffmpeg Path explicitly? {e}");
            return false;
        }

        return true;
    }
}
