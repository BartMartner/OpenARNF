using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using System;

public class FileSlotUI : MonoBehaviour
{
    public Text slotDataText;
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        if (!canvasGroup)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public void SetSlotData(int slot)
    {
        SaveSlotData data;

        if (SaveGameManager.instance.saveFileData.saveSlots.TryGetValue(slot, out data) && data != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Started " + data.started.ToString());
            sb.AppendLine();
            string playTime;

            if(data.activeGameData != null && data.activeGameData.playerDead)
            {
                data.activeGameData = null;
                SaveGameManager.instance.Save(false, true);
            }

            if(data.activeGameData != null)
            {
                var t = TimeSpan.FromSeconds(data.activeGameData.playTime);
                playTime = t.Hours.ToString("00") + ":" + t.Minutes.ToString("00") + ":" + t.Seconds.ToString("00");
                sb.AppendLine("Play Time " + playTime);
                sb.AppendLine();
                var collectRate = (int)(data.activeGameData.collectRate * 100);
                sb.AppendLine("Collect Rate " + collectRate + "%");
            }
            else
            {
                sb.AppendLine("No Active Game");
            }
            
            slotDataText.text = sb.ToString();
        }
        else
        {
            slotDataText.text = "Empty";
        }       
    }
}
