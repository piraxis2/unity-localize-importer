using UnityEngine;
using TMPro;
using System;

namespace Simple.Localize
{
    /// <summary>
    /// 런타임 로컬라이즈 설정을 위한 유틸리티 클래스
    /// </summary>
    public static class LocalizeUtil
    {
        private static string _overrideTextTable;
        private static string _overrideFontTable;

        /// <summary>
        /// 프로젝트 기본 텍스트 테이블 이름 (에셋 설정값 또는 런타임 오버라이드)
        /// </summary>
        public static string DefaultTextTable 
        { 
            get => !string.IsNullOrEmpty(_overrideTextTable) ? _overrideTextTable : SimpleLocalizeSettings.Instance.defaultTextTable;
            set => _overrideTextTable = value;
        }

        /// <summary>
        /// 프로젝트 기본 폰트 테이블 이름 (에셋 설정값 또는 런타임 오버라이드)
        /// </summary>
        public static string DefaultFontTable 
        { 
            get => !string.IsNullOrEmpty(_overrideFontTable) ? _overrideFontTable : SimpleLocalizeSettings.Instance.defaultFontTable;
            set => _overrideFontTable = value;
        }

        /// <summary>
        /// 기본 폰트 테이블을 사용하여 로컬라이즈를 설정합니다.
        /// </summary>
        public static void Set(TextMeshProUGUI tmp, string textKey, string fontKey, params object[] args)
        {
            Set(tmp, textKey, DefaultFontTable, fontKey, args);
        }

        /// <summary>
        /// TextMeshProUGUI에 로컬라이즈 텍스트와 폰트를 설정합니다.
        /// </summary>
        /// <param name="tmp">대상 TextMeshProUGUI 컴포넌트</param>
        /// <param name="textKey">로컬라이즈 텍스트 키</param>
        /// <param name="fontTable">폰트 에셋 테이블 이름</param>
        /// <param name="fontKey">폰트 키</param>
        /// <param name="args">Smart String용 인자 (선택)</param>
        public static void Set(TextMeshProUGUI tmp, string textKey, string fontTable, string fontKey, params object[] args)
        {
            if (tmp == null)
            {
                Debug.LogWarning("[SimpleLocalize] 대상 TextMeshProUGUI가 null입니다.");
                return;
            }

            var localizedComp = GetOrAddComponent(tmp);
            string[] stringArgs = args != null && args.Length > 0 
                ? Array.ConvertAll(args, x => x?.ToString() ?? "") 
                : null;

            localizedComp.SetFull(DefaultTextTable, textKey, fontTable, fontKey, stringArgs);
        }

        /// <summary>
        /// 텍스트 테이블까지 명시적으로 지정하여 설정합니다.
        /// </summary>
        public static void SetFull(TextMeshProUGUI tmp, string textTable, string textKey, string fontTable, string fontKey, params object[] args)
        {
            if (tmp == null) return;

            var localizedComp = GetOrAddComponent(tmp);
            string[] stringArgs = args != null && args.Length > 0 
                ? Array.ConvertAll(args, x => x?.ToString() ?? "") 
                : null;

            localizedComp.SetFull(textTable, textKey, fontTable, fontKey, stringArgs);
        }

        private static SimpleLocalizedText GetOrAddComponent(TextMeshProUGUI tmp)
        {
            if (!tmp.TryGetComponent<SimpleLocalizedText>(out var comp))
            {
                comp = tmp.gameObject.AddComponent<SimpleLocalizedText>();
            }
            return comp;
        }

        #region Extension Methods
        /// <summary>
        /// 기본 폰트 테이블을 사용하는 확장 메서드 스타일 설정
        /// </summary>
        public static void SetLocalize(this TextMeshProUGUI tmp, string textKey, string fontKey, params object[] args)
        {
            Set(tmp, textKey, DefaultFontTable, fontKey, args);
        }

        /// <summary>
        /// 폰트 테이블을 직접 지정하는 확장 메서드 스타일 설정
        /// </summary>
        public static void SetLocalize(this TextMeshProUGUI tmp, string textKey, string fontTable, string fontKey, params object[] args)
        {
            Set(tmp, textKey, fontTable, fontKey, args);
        }
        #endregion
    }
}
