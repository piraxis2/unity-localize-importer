using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System;

namespace Simple.Localize
{
    // LocalizedAsset<T>를 상속받아 폰트 에셋을 다루는 클래스 정의
    [Serializable]
    public class LocalizedFont : LocalizedAsset<TMP_FontAsset> {}

    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SimpleLocalizedText : MonoBehaviour
    {
        [Header("Text Settings")]
        [Tooltip("Select Table and Key for Text")]
        public LocalizedString localizedString = new LocalizedString();

        [Tooltip("Arguments for Smart Strings (e.g. {0}, {name})")]
        public List<string> smartArguments = new List<string>();

        [Header("Font Settings (Optional)")]
        [Tooltip("Select Table and Key for Font Asset")]
        public LocalizedFont localizedFont = new LocalizedFont();

        private TextMeshProUGUI _textMeshPro;

        private void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (_textMeshPro == null) _textMeshPro = GetComponent<TextMeshProUGUI>();

            // 1. 텍스트 이벤트 구독
            localizedString.StringChanged += UpdateText;
            
            // 2. 폰트 이벤트 구독
            localizedFont.AssetChanged += UpdateFont;

            // 3. 초기화 및 갱신
            Refresh();
        }

        private void OnDisable()
        {
            localizedString.StringChanged -= UpdateText;
            localizedFont.AssetChanged -= UpdateFont;
        }



        public void UpdateText()
        {
            Refresh();
        }

        public void Refresh()
        {
            // --- 텍스트 갱신 ---
            if (smartArguments != null && smartArguments.Count > 0)
            {
                localizedString.Arguments = smartArguments.ToArray();
            }
            else
            {
                localizedString.Arguments = null;
            }
            
            localizedString.RefreshString();

            // --- 폰트 갱신 ---
            // 테이블이나 키가 설정되어 있을 때만 로드 시도
            if (!localizedFont.IsEmpty)
            {
                // LoadAssetAsync()가 내부적으로 캐싱 및 로딩 처리
                 var op = localizedFont.LoadAssetAsync();
                 if (op.IsDone) UpdateFont(op.Result);
            }

            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // 에디터 비동기 처리 (텍스트)
                var opText = localizedString.GetLocalizedStringAsync();
                if (opText.IsDone) UpdateText(opText.Result);
                else opText.Completed += (handle) => UpdateText(handle.Result);
            }
#endif
        }

        private void UpdateText(string text)
        {
            if (_textMeshPro != null)
            {
                // 텍스트가 다를 때만 변경
                if (_textMeshPro.text != text)
                {
                    _textMeshPro.text = text;
                    MarkDirtyInEditor();
                }
            }
        }

        private void UpdateFont(TMP_FontAsset font)
        {
            if (_textMeshPro != null && font != null)
            {
                // 폰트가 다를 때만 변경
                if (_textMeshPro.font != font)
                {
                    _textMeshPro.font = font;
                    MarkDirtyInEditor();
                }
            }
        }

        private void MarkDirtyInEditor()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && _textMeshPro != null)
            {
                UnityEditor.EditorUtility.SetDirty(_textMeshPro);
            }
#endif
        }

        // 런타임에서 인자 변경
        public void SetArgs(params string[] args)
        {
            if (smartArguments == null) smartArguments = new List<string>();
            smartArguments.Clear();
            smartArguments.AddRange(args);
            Refresh();
        }

        // 런타임에서 텍스트 키 변경
        public void SetKey(string tableName, string key)
        {
            localizedString.SetReference(tableName, key);
            Refresh();
        }

        // 런타임에서 폰트 키 변경
        public void SetFontKey(string tableName, string key)
        {
            localizedFont.SetReference(tableName, key);
            Refresh();
        }
    }
}