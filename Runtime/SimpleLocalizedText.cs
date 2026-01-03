using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Simple.Localize
{
    // 에디터에서도 미리보기가 가능하도록 설정
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SimpleLocalizedText : MonoBehaviour
    {
        [Tooltip("엑셀(테이블)에 정의된 키 값")]
        public string localizationKey;

        [Tooltip("사용할 스트링 테이블 선택")]
        public LocalizedStringTable stringTable = new LocalizedStringTable { TableReference = "Localize" };

        private TextMeshProUGUI _textMeshPro;

        private void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            UpdateText();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
        
        private void OnValidate()
        {
            if (_textMeshPro == null) _textMeshPro = GetComponent<TextMeshProUGUI>();
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // 에디터 갱신 타이밍 문제 해결을 위해 delayCall 사용
                UnityEditor.EditorApplication.delayCall += UpdateText;
            }
#endif
        }

        private void OnLocaleChanged(Locale locale)
        {
            UpdateText();
        }

        public void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey)) return;
            if (_textMeshPro == null) return;
            // stringTable 유효성 체크
            if (stringTable == null || stringTable.TableReference == default) return;

            try
            {
                var op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(stringTable.TableReference, localizationKey);
            
                if (op.IsDone)
                {
                    _textMeshPro.text = op.Result;
                    MarkDirtyInEditor();
                }
                else
                {
                    op.Completed += (handle) =>
                    {
                        if (_textMeshPro != null)
                        {
                            _textMeshPro.text = handle.Result;
                            MarkDirtyInEditor();
                        }
                    };
                }
            }
            catch (System.Exception)
            {
                // 에디터 초기화 이슈 무시
            }
        }

        private void MarkDirtyInEditor()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && _textMeshPro != null)
            {
                // 변경 사항을 저장하고 씬 뷰를 갱신
                UnityEditor.EditorUtility.SetDirty(_textMeshPro);
            }
#endif
        }

        public void SetKey(string key)
        {
            localizationKey = key;
            UpdateText();
        }
    }
}
