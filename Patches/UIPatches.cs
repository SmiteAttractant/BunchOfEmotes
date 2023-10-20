using HarmonyLib;
using Reptile;
using TMPro;
using UnityEngine;

namespace BunchOfEmotes.Patches
{
    public class UI : MonoBehaviour
    {
        public static UI Instance = null;
        private TextMeshProUGUI m_label = null;
        private TextMeshProUGUI m_label1 = null;
        private TextMeshProUGUI m_label2 = null;
        private float m_notificationTimer = 5f;
        private bool m_active;



        private void Awake()
        {
            Instance = this;

            SetupLabel();
            SetupLabelPause();
        }

        public void ShowNotification(string textbef, string textmid, string textaft)
        {
            m_label1.text = textbef;
            m_label.text = textmid;
            m_label2.text = textaft;
            m_notificationTimer = 5f;
            m_label.gameObject.SetActive(true);
            m_label1.gameObject.SetActive(true);
            m_label2.gameObject.SetActive(true);
        }


        public void HideNotification()
        {
            m_label1.gameObject.SetActive(false);
            m_label.gameObject.SetActive(false);
            m_label2.gameObject.SetActive(false);
        }

        private void SetupLabel()
        {
            m_label = new GameObject("Emote").AddComponent<TextMeshProUGUI>();
            m_label1 = new GameObject("EmotePrev").AddComponent<TextMeshProUGUI>();
            m_label2 = new GameObject("EmoteAft").AddComponent<TextMeshProUGUI>();
            var uiManager = Core.Instance.UIManager;
            var gameplay = Traverse.Create(uiManager).Field<GameplayUI>("gameplay").Value;
            var rep = gameplay.graffitiNewLabel;
            m_label.font = rep.font;
            m_label1.font = rep.font;
            m_label2.font = rep.font;
            m_label2.alpha = 0.3f;
            m_label1.alpha = 0.3f;
            m_label.fontSize = 32;
            m_label1.fontSize = 32;
            m_label2.fontSize = 32;
            m_label.fontMaterial = rep.fontMaterial;
            m_label1.fontMaterial = rep.fontMaterial;
            m_label2.fontMaterial = rep.fontMaterial;
            m_label.alignment = TextAlignmentOptions.TopRight;
            m_label1.alignment = TextAlignmentOptions.TopRight;
            m_label2.alignment = TextAlignmentOptions.TopRight;
            var rect = m_label.rectTransform;
            var rect1 = m_label1.rectTransform;
            var rect2 = m_label2.rectTransform;
            rect.anchorMin = new Vector2(0.1f, 0.5f);
            rect1.anchorMin = new Vector2(0.1f, 0.5f);
            rect2.anchorMin = new Vector2(0.1f, 0.5f);
            //1f = hug right, 0f = hug bottom
            rect.anchorMax = new Vector2(0.868f, 0.90f);
            rect1.anchorMax = new Vector2(0.868f, 0.95f);
            rect2.anchorMax = new Vector2(0.868f, 0.85f);
            rect.pivot = new Vector2(0, 1);
            rect1.pivot = new Vector2(0, 1);
            rect2.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(1f, 0.2f);
            rect1.anchoredPosition = new Vector2(1f, 0.2f);
            rect2.anchoredPosition = new Vector2(1f, 0.2f);
            m_label.rectTransform.SetParent(gameplay.gameplayScreen.GetComponent<RectTransform>(), false);
            m_label1.rectTransform.SetParent(gameplay.gameplayScreen.GetComponent<RectTransform>(), false);
            m_label2.rectTransform.SetParent(gameplay.gameplayScreen.GetComponent<RectTransform>(), false);
            //BunchOfEmotesPlugin.m_label = m_label;
        }


        private void SetupLabelPause()
        {

        }
    }

}
